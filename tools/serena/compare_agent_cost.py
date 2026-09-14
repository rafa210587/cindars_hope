"""Two bounded, independent Codex investigations; native usage, not output estimates.
Never changes provider/global/project configuration. CLI overrides are per invocation.
"""
import hashlib
import json
from pathlib import Path
import subprocess
import time
import uuid

REPO = Path(__file__).resolve().parents[2]
SOURCE = Path(r"D:\Projetos\Cindars_Hope\.serena-runtime\comparison\3dd585d76c\source")
RUNTIME = Path(r"D:\Projetos\Cindars_Hope\.serena-runtime")
NODE = r"C:\Program Files\nodejs\node.exe"
CODEX = r"C:\Users\Rafa\AppData\Roaming\npm\node_modules\@openai\codex\bin\codex.js"
OUT = REPO / "docs/validation/serena_unity_20260912" / ("agent-cost-" + uuid.uuid4().hex[:10])
PROMPT = """Read-only investigation on the captured Unity source graph in your working directory.
You are not alone: do not edit any files or settings, install tools, launch other agents,
build/restore projects, browse the web, or read outside this source tree. Only Assets C#
is in scope. Do not read benchmark artifacts or previous answers. Use at most 16 tool
calls and produce a final answer under 800 words; disclose gaps instead of inventing.
Investigate from source, choose your own searches and reads:
A. Identify every concrete implementation of IGameplayRandomSource, including indirect
inheritance, and its direct production construction sites. Distinguish world/visual RNG.
B. Identify every concrete implementation of IPreparableSkillEffectExecutor. Explain the
production prepare/commit call chain, behavior on failed preparation and which concrete
ISkillEffectExecutor classes do not support preparation. Do not assume API names from this
question. Give exact source file:line evidence and state scope/completeness limitations.
"""

def run_arm(arm):
    args = [NODE, CODEX, "exec", "--json", "--ephemeral", "--skip-git-repo-check", "--ignore-user-config",
            "--sandbox", "read-only", "-C", str(SOURCE), "-m", "gpt-5.6-sol",
            "-c", 'model_reasoning_effort="medium"', "-c", 'approval_policy="never"',
            "-c", 'windows.sandbox="elevated"']
    arm_instruction = "\nUse available shell tools with rg and bounded source reads. Serena is unavailable."
    if arm == "serena":
        settings = {
            "command": str(RUNTIME / "venv/Scripts/python.exe"),
            "args": [str(REPO / "tools/serena/serena_unity_proxy.py"), "--repo", str(SOURCE), "--runtime-root", str(RUNTIME)],
            "enabled": True, "required": True, "startup_timeout_sec": 180, "tool_timeout_sec": 120,
            "enabled_tools": ["read_file", "get_symbols_overview", "find_symbol", "find_referencing_symbols"]}
        for key, value in settings.items():
            args += ["-c", "mcp_servers.serena_unity_readonly." + key + "=" + json.dumps(value)]
        arm_instruction = "\nUse Serena as your primary source navigator. Shell rg/bounded reads may fill gaps; disclose fallback and tool failures."
    args += [PROMPT + arm_instruction]
    (OUT / f"{arm}-invocation.json").write_text(json.dumps(args, indent=2), encoding="utf-8")
    start = time.perf_counter()
    with (OUT / f"{arm}.jsonl").open("w", encoding="utf-8") as output, (OUT / f"{arm}.stderr.txt").open("w", encoding="utf-8") as errors:
        child = subprocess.Popen(args, stdout=output, stderr=errors)
        try:
            code = child.wait(timeout=600)
        except subprocess.TimeoutExpired:
            subprocess.run(["taskkill", "/PID", str(child.pid), "/T", "/F"], capture_output=True, timeout=15)
            code = "TIMEOUT"
    events = []
    for line in (OUT / f"{arm}.jsonl").read_text(encoding="utf-8").splitlines():
        try:
            events.append(json.loads(line))
        except json.JSONDecodeError:
            pass
    usage = [e["usage"] for e in events if e.get("type") == "turn.completed" and "usage" in e]
    items = [e["item"] for e in events if e.get("type") == "item.completed"]
    result = {"arm": arm, "exit": code, "wall_seconds": time.perf_counter() - start,
              "usage": usage, "item_types": {}, "errors": [e for e in events if e.get("type") in ("error", "turn.failed")]}
    for item in items:
        kind = item.get("type", "unknown")
        result["item_types"][kind] = result["item_types"].get(kind, 0) + 1
    (OUT / f"{arm}-summary.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps(result), flush=True)
    return result

if __name__ == "__main__":
    OUT.mkdir(parents=True)
    sandbox = [NODE, CODEX, "sandbox", "-P", ":read-only", "--include-managed-config",
               "-c", 'windows.sandbox="elevated"', "-C", str(SOURCE), "--",
               r"C:\Users\Rafa\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe",
               "-NoProfile"]
    preflight = []
    for suffix in (["-Command", "rg -n IGameplayRandomSource Assets/_Game/Scripts/Foundation/Random/IRandomSource.cs"],
                   ["-File", str(REPO / "tools/serena/Test-ReadonlyBoundary.ps1")]):
        checked = subprocess.run(sandbox + suffix, capture_output=True, text=True, timeout=30)
        preflight.append({"command": sandbox + suffix, "exit": checked.returncode, "stdout": checked.stdout, "stderr": checked.stderr})
        if checked.returncode:
            (OUT / "preflight.json").write_text(json.dumps(preflight, indent=2), encoding="utf-8")
            raise SystemExit("Preflight failed; no model runs launched")
    (OUT / "preflight.json").write_text(json.dumps(preflight, indent=2), encoding="utf-8")
    manifest = json.loads((SOURCE / "source-manifest.json").read_text(encoding="utf-8-sig"))
    for row in manifest["sources"]:
        assert hashlib.sha256((SOURCE / row["Path"]).read_bytes()).hexdigest().upper() == row["Sha256"]
    (OUT / "protocol.txt").write_text(
        "Predeclared: same source, model and reasoning; independent CLI sessions; two questions per arm; "
        "one pair only, no automatic retries. Sequential baseline then candidate; cache order disclosed. Accuracy judged against independent key. "
        "No cost acceptance without usage and no regression in answer quality. "
        "Compare input/cached/output separately; no quota or billing guarantee inferred. "
        "Timeout 600s each; requested 16 tool calls and 800 words each (soft prompt limits).\n" + PROMPT,
        encoding="utf-8")
    print(json.dumps({"evidence": str(OUT), "status": "started"}), flush=True)
    baseline = run_arm("baseline")
    results = [baseline]
    baseline_stderr = (OUT / "baseline.stderr.txt").read_text(encoding="utf-8")
    if baseline["exit"] == 0 and baseline["usage"] and baseline["item_types"].get("command_execution", 0) > 0 and "blocked by policy" not in baseline_stderr:
        results.append(run_arm("serena"))
    else:
        print("Baseline access/usage gate failed; candidate NOT RUN", flush=True)
    (OUT / "summary.json").write_text(json.dumps(results, indent=2), encoding="utf-8")
