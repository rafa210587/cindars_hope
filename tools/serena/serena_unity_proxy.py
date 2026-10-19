"""Read-only Serena stdio bridge over one immutable-per-session Unity source snapshot."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
import threading
import uuid
import shutil
import hashlib

ALLOWED = {"read_file", "get_symbols_overview", "find_symbol", "find_referencing_symbols"}
REQUEST_TIMEOUT_SECONDS = 90


def safe_relative(value):
    if not isinstance(value, str) or not value or Path(value).is_absolute():
        return False
    normalized = value.replace("\\", "/")
    return not (":" in normalized or any(part in ("", "..") for part in normalized.split("/")))


def stop_tree(child):
    if child.poll() is None:
        try:
            subprocess.run(["taskkill", "/PID", str(child.pid), "/T", "/F"], capture_output=True, timeout=10)
            child.wait(timeout=5)
        except subprocess.TimeoutExpired:
            child.kill()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", default=r"D:\Projetos\Cindars_Hope\cindars_hope")
    parser.add_argument("--runtime-root", default=r"D:\Projetos\Cindars_Hope\.serena-runtime")
    parser.add_argument("--serena-exe", default=None)
    parser.add_argument("--roslyn-cache", default=None)
    args = parser.parse_args()
    session = Path(args.runtime_root) / "s" / uuid.uuid4().hex[:8]
    snapshot = session / "src"
    profile = Path.home()
    child_env = os.environ.copy()
    child_env.setdefault("USERPROFILE", str(profile))
    child_env.setdefault("APPDATA", str(profile / "AppData" / "Roaming"))
    child_env.setdefault("LOCALAPPDATA", str(profile / "AppData" / "Local"))
    child_env.setdefault("TEMP", str(profile / "AppData" / "Local" / "Temp"))
    child_env.setdefault("TMP", child_env["TEMP"])
    child_env.setdefault("ProgramFiles", r"C:\Program Files")
    child_env.setdefault("ProgramFiles(x86)", r"C:\Program Files (x86)")
    child_env.setdefault("ProgramW6432", r"C:\Program Files")
    child_env["DOTNET_CLI_HOME"] = str(session)
    child_env["NUGET_PACKAGES"] = str(Path(args.runtime_root) / "nuget_packages")
    sync = Path(__file__).with_name("Sync-UnitySnapshot.ps1")
    protected = {sync: hashlib.sha256(sync.read_bytes()).digest()}
    def check(check_only=False):
        for path, digest in protected.items():
            if hashlib.sha256(path.read_bytes()).digest() != digest:
                raise RuntimeError(f"Launcher/config drift: {path.name}; reconnect")
        command = ["pwsh", "-NoProfile", "-File", str(sync), "-Repository", args.repo, "-Snapshot", str(snapshot)]
        if check_only:
            command.append("-CheckOnly")
        result = subprocess.run(command, capture_output=True, text=True, timeout=60, env=child_env)
        if result.returncode:
            raise RuntimeError((result.stderr or result.stdout).strip()[:1000])
    check()
    restored = None
    for _ in range(2):
        restored = subprocess.run(
            ["dotnet", "restore", "CindarsHope.Runtime.csproj", "--ignore-failed-sources", "-v:minimal"],
            cwd=snapshot, capture_output=True, text=True, timeout=60, env=child_env,
        )
        if restored.returncode == 0:
            break
    if restored.returncode:
        raise RuntimeError(f"Snapshot restore failed: {(restored.stderr or restored.stdout)[-2000:]}")
    serena_home = session / "h"
    project_data = session / "p" / snapshot.name
    serena_home.mkdir(parents=True)
    project_data.mkdir(parents=True)
    binary_cache = Path(args.roslyn_cache) if args.roslyn_cache else Path(args.runtime_root) / "roslyn-cache"
    if not binary_cache.is_dir():
        raise RuntimeError(f"Roslyn cache missing: {binary_cache}")
    shutil.copytree(binary_cache, serena_home / "language_servers")
    (serena_home / "serena_config.yml").write_text(
        f"projects:\n- {snapshot.as_posix()}\n"
        "web_dashboard: false\nweb_dashboard_open_on_launch: false\n"
        f"project_serena_folder_location: '{(session / 'p' / '$projectFolderName').as_posix()}'\n"
        f"trusted_project_path_patterns:\n- '{snapshot.as_posix()}'\n"
        "fixed_tools:\n- read_file\n- get_symbols_overview\n- find_symbol\n"
        "- find_referencing_symbols\nbase_modes: []\ndefault_modes: []\n"
        "default_max_tool_answer_chars: 12000\ntool_timeout: 120\n"
        "language_backend: LSP\n",
        encoding="utf-8",
    )
    (project_data / "project.yml").write_text(
        "project_name: cindars_hope\nlanguage_servers:\n- csharp\nread_only: true\n"
        "fixed_tools:\n- read_file\n- get_symbols_overview\n- find_symbol\n"
        "- find_referencing_symbols\ndefault_modes: []\nadded_modes: []\n"
        "ignored_paths:\n- '**/obj/**'\n- '**/bin/**'\n",
        encoding="utf-8",
    )
    env = child_env.copy()
    env["SERENA_HOME"] = str(serena_home)
    # Serena fills missing defaults on load. Normalize with the pinned package before
    # recording immutable config hashes, rather than accepting arbitrary later changes.
    normalized = subprocess.run(
        [sys.executable, "-c", "from serena.config.serena_config import SerenaConfig; c = SerenaConfig.from_config_file(generate_if_missing=False); assert len(c.projects) == 1"],
        env=env, cwd=snapshot, capture_output=True, text=True, timeout=30,
    )
    if normalized.returncode:
        raise RuntimeError(f"Config normalization failed: {normalized.stderr[-1000:]}")
    for path in (serena_home / "serena_config.yml", project_data / "project.yml"):
        protected[path] = hashlib.sha256(path.read_bytes()).digest()
    child = subprocess.Popen(
        [str(args.serena_exe or (Path(args.runtime_root) / "venv" / "Scripts" / "serena.exe")), "start-mcp-server", "--project", str(snapshot),
         "--transport", "stdio", "--enable-web-dashboard", "false", "--open-web-dashboard", "false"],
        stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=sys.stderr, env=env, bufsize=0, cwd=snapshot,
    )
    pending = {}
    lock = threading.Lock()
    output_lock = threading.Lock()
    closing = threading.Event()
    def send(packet):
        with output_lock:
            sys.stdout.buffer.write((json.dumps(packet) + "\n").encode())
            sys.stdout.buffer.flush()
    def fail(error, request_id=None):
        print(f"Serena proxy fail-closed: {error}", file=sys.stderr, flush=True)
        with lock:
            failed_ids = [request_id] if request_id is not None else list(pending)
        for failed_id in failed_ids:
            send({"jsonrpc": "2.0", "id": failed_id, "error": {"code": -32001, "message": str(error)[:1000]}})
        stop_tree(child)
        os._exit(1)
    def output_loop():
        try:
            while True:
                line = child.stdout.readline(30001)
                if not line:
                    if not closing.is_set():
                        fail("Backend exited; reconnect or use rg")
                    break
                request_id = None
                try:
                    if len(line) > 30000:
                        raise RuntimeError("Tool response exceeds 30000-byte cap")
                    packet = json.loads(line)
                    request_id = packet.get("id")
                    if "id" in packet and packet["id"] in pending:
                        check(True)
                        with lock:
                            timer = pending.pop(packet["id"], None)
                            if timer:
                                timer.cancel()
                    if packet.get("id") is not None and packet.get("result", {}).get("tools") is not None:
                        names = {tool["name"] for tool in packet["result"]["tools"]}
                        if names != ALLOWED:
                            raise RuntimeError(f"Unsafe tools/list: {sorted(names)}")
                    send(packet)
                except Exception as error:
                    fail(error, request_id)
        finally:
            stop_tree(child)
    relay = threading.Thread(target=output_loop, daemon=True)
    relay.start()
    request_id = None
    try:
        for line in sys.stdin.buffer:
            packet = json.loads(line)
            request_id = packet.get("id")
            if packet.get("method") == "tools/call":
                name = packet.get("params", {}).get("name")
                if name not in ALLOWED:
                    raise RuntimeError(f"Tool not allowlisted: {name}")
                arguments = packet.get("params", {}).get("arguments", {})
                if not isinstance(arguments, dict):
                    raise RuntimeError("Tool arguments must be an object")
                for key in ("relative_path", "path"):
                    if key in arguments and not safe_relative(arguments[key]):
                        raise RuntimeError(f"Unsafe {key} argument")
                check(True)
                with lock:
                    if request_id is None or request_id in pending:
                        raise RuntimeError("Missing or duplicate request id")
                    timer = threading.Timer(REQUEST_TIMEOUT_SECONDS, fail, args=("Tool timed out; reconnect or use rg", request_id))
                    timer.daemon = True
                    pending[request_id] = timer
                    timer.start()
            child.stdin.write(line)
            child.stdin.flush()
    except Exception as error:
        fail(error, request_id)
    finally:
        closing.set()
        with lock:
            for timer in pending.values():
                timer.cancel()
        stop_tree(child)
        relay.join(timeout=10)


if __name__ == "__main__":
    main()
