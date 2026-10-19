"""Exercise the real stdio proxy against a fake backend; mutate external snapshots only."""
import json
import os
from pathlib import Path
import queue
import subprocess
import sys
import threading
import time
import uuid

HERE = Path(__file__).resolve().parent
RUNTIME = Path(r"D:\Projetos\Cindars_Hope\.serena-runtime")
EVIDENCE = HERE.parents[1] / "docs" / "validation" / "serena_unity_20260912" / "protocolo-evidence.txt"
ALLOWED = ["read_file", "get_symbols_overview", "find_symbol", "find_referencing_symbols"]


def backend():
    case = os.environ["PROTOCOL_CASE"]
    marker = Path(os.environ["PROTOCOL_MARKER"])
    snapshot = Path(os.environ["PROTOCOL_SNAPSHOT"])
    for line in sys.stdin:
        request = json.loads(line)
        marker.write_text("forwarded", encoding="utf-8")
        if case == "hang":
            time.sleep(10)
            continue
        if case == "midquery_drift":
            source = snapshot / "Assets/_Game/Scripts/Foundation/StableHash32.cs"
            source.write_bytes(source.read_bytes() + b"\n// fake backend drift\n")
        if request.get("method") == "tools/list":
            result = {"tools": [{"name": name} for name in ALLOWED]}
        elif case == "oversize":
            result = {"content": [{"type": "text", "text": "X" * 31000}]}
        else:
            result = {"content": [{"type": "text", "text": "ok"}], "isError": False}
        sys.stdout.write(json.dumps({"jsonrpc": "2.0", "id": request["id"], "result": result}) + "\n")
        sys.stdout.flush()


def proxy_case():
    import serena_unity_proxy as proxy

    actual_popen = subprocess.Popen
    case = os.environ["PROTOCOL_CASE"]
    marker = os.environ["PROTOCOL_MARKER"]
    if case == "hang":
        proxy.REQUEST_TIMEOUT_SECONDS = 1

    def patched_popen(args, **kwargs):
        if "--transport" in args:
            env = kwargs.get("env", os.environ.copy()).copy()
            env.update(PROTOCOL_CASE=case, PROTOCOL_MARKER=marker,
                       PROTOCOL_SNAPSHOT=args[args.index("--project") + 1])
            kwargs["env"] = env
            return actual_popen([sys.executable, str(Path(__file__).resolve()), "--backend"], **kwargs)
        return actual_popen(args, **kwargs)

    proxy.subprocess.Popen = patched_popen
    sys.argv = [sys.argv[0]]
    proxy.main()


def run_case(case, request):
    marker_dir = RUNTIME / "protocol-test" / uuid.uuid4().hex[:8]
    marker_dir.mkdir(parents=True)
    marker = marker_dir / "forwarded.txt"
    env = os.environ.copy()
    env.update(PROTOCOL_CASE=case, PROTOCOL_MARKER=str(marker))
    process = subprocess.Popen(
        [sys.executable, str(Path(__file__).resolve()), "--proxy-case"],
        stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        text=True, encoding="utf-8", env=env,
    )
    output = queue.Queue()
    errors = queue.Queue()
    threading.Thread(target=lambda: [output.put(line) for line in process.stdout], daemon=True).start()
    threading.Thread(target=lambda: [errors.put(line) for line in process.stderr], daemon=True).start()
    process.stdin.write(json.dumps(request) + "\n")
    process.stdin.flush()
    deadline = time.monotonic() + 60
    lines = []
    while time.monotonic() < deadline:
        try:
            lines.append(output.get(timeout=0.2).strip())
            break
        except queue.Empty:
            if process.poll() is not None:
                break
    if process.poll() is None:
        process.stdin.close()
        try:
            process.wait(timeout=8)
        except subprocess.TimeoutExpired:
            process.kill()
            process.wait(timeout=5)
    stderr = "".join(list(errors.queue))
    return {"case": case, "exit": process.returncode, "forwarded": marker.exists(),
            "response": lines, "stderr_tail": stderr[-600:]}


def main():
    def call(name="find_symbol", arguments=None):
        return {"jsonrpc": "2.0", "id": 1, "method": "tools/call",
                "params": {"name": name, "arguments": arguments or {"name_path_pattern": "StableHash32"}}}
    cases = [
        ("valid_list", {"jsonrpc": "2.0", "id": 1, "method": "tools/list", "params": {}}),
        ("normal", call()),
        ("denied_tool", call(name="execute_shell_command")),
        ("denied_path", call(arguments={"relative_path": "../outside.cs"})),
        ("oversize", call()),
        ("midquery_drift", call()),
        ("hang", call()),
    ]
    only = os.environ.get("PROTOCOL_ONLY")
    if only:
        cases = [entry for entry in cases if entry[0] == only]
    results = []
    for case, request in cases:
        result = run_case(case, request)
        payload = json.loads(result["response"][0]) if result["response"] else {}
        if case == "valid_list":
            passed = result["exit"] == 0 and result["forwarded"] and {x["name"] for x in payload.get("result", {}).get("tools", [])} == set(ALLOWED)
        elif case == "normal":
            passed = result["exit"] == 0 and result["forwarded"] and payload.get("result", {}).get("isError") is False
        elif case in ("denied_tool", "denied_path"):
            passed = result["exit"] != 0 and not result["forwarded"] and payload.get("error", {}).get("code") == -32001
        else:
            passed = result["exit"] != 0 and result["forwarded"] and payload.get("error", {}).get("code") == -32001
        result["verdict"] = "PASS" if passed else "FAIL"
        results.append(result)
        print(json.dumps(result, ensure_ascii=False), flush=True)
    with EVIDENCE.open("a", encoding="utf-8") as evidence:
        evidence.write("\n".join(json.dumps(row, ensure_ascii=False) for row in results) + "\n")
    if any(row["verdict"] != "PASS" for row in results):
        raise SystemExit(1)


if __name__ == "__main__":
    if "--backend" in sys.argv:
        backend()
    elif "--proxy-case" in sys.argv:
        proxy_case()
    else:
        main()
