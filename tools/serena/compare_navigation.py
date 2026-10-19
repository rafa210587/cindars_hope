"""Matched navigation probe: real captured Unity sources, Serena vs targeted rg.

Output tokens use o200k_base as a reproducible proxy, NOT billed/account tokens.
No gameplay files or provider configuration are changed.
"""
import asyncio
from datetime import timedelta
import hashlib
import json
from pathlib import Path
import statistics
import subprocess
import sys
import time
import uuid

import tiktoken
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
RUNTIME = Path(r"D:\Projetos\Cindars_Hope\.serena-runtime")
TASKS = [
    ("method_body", "Implementation of StableHash32.Compute", "find_symbol",
     {"name_path_pattern": "StableHash32/Compute", "relative_path": "Assets/_Game/Scripts/Foundation/StableHash32.cs", "include_body": True},
     ["rg", "-n", "-A", "15", "-F", "public static int Compute", "Assets/_Game/Scripts/Foundation/StableHash32.cs"]),
    ("hash_references", "Files/lines referencing StableHash32.Compute, distinguishing XML cref", "find_referencing_symbols",
     {"name_path": "StableHash32/Compute", "relative_path": "Assets/_Game/Scripts/Foundation/StableHash32.cs"},
     ["rg", "-n", "-C", "2", "-F", "StableHash32.Compute", "Assets", "-g", "*.cs"]),
    ("input_references", "Files/lines referencing GameplayInputBlocker.Acquire", "find_referencing_symbols",
     {"name_path": "GameplayInputBlocker/Acquire", "relative_path": "Assets/_Game/Scripts/Gameplay/GameplayInputBlocker.cs"},
     ["rg", "-n", "-C", "2", "-F", "GameplayInputBlocker.Acquire", "Assets", "-g", "*.cs"]),
]

async def main():
    run_id = uuid.uuid4().hex[:10]
    source = RUNTIME / "comparison" / run_id / "source"
    evidence = REPO / "docs/validation/serena_unity_20260912" / ("comparison-" + run_id)
    evidence.mkdir(parents=True)
    start = time.perf_counter()
    synced = subprocess.run(["pwsh", "-NoProfile", "-File", str(HERE / "Sync-UnitySnapshot.ps1"),
                             "-Repository", str(REPO), "-Snapshot", str(source)],
                            capture_output=True, text=True, timeout=90)
    if synced.returncode:
        raise RuntimeError(synced.stderr)
    encoding = tiktoken.get_encoding("o200k_base")
    report = {"scope": "Three known-symbol queries on identical captured Unity Compile sources",
              "source": str(source), "capture_seconds": time.perf_counter() - start,
              "token_metric": "o200k_base output-content proxy; not billed tokens or full agent cost",
              "manifest_sha256": hashlib.sha256((source / "source-manifest.json").read_bytes()).hexdigest(),
              "rounds": 2, "results": [], "evidence": str(evidence)}

    def measure_text(text):
        return {"chars": len(text), "utf8_bytes": len(text.encode()), "output_tokens_proxy": len(encoding.encode(text))}

    params = StdioServerParameters(command=sys.executable,
                                  args=[str(HERE / "serena_unity_proxy.py"), "--repo", str(source)])
    start = time.perf_counter()
    async with stdio_client(params) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            report["serena_startup_seconds"] = time.perf_counter() - start
            listed = await session.list_tools()
            assert {t.name for t in listed.tools} == {"read_file", "find_symbol", "find_referencing_symbols", "get_symbols_overview"}
            report["serena_tool_list_tokens_proxy"] = len(encoding.encode(listed.model_dump_json()))
            for round_number in (1, 2):
                for task_id, question, tool, arguments, command in TASKS:
                    # Alternate execution order by round. Keep both raw outputs, no trimming.
                    for engine in (("rg", "serena") if round_number == 1 else ("serena", "rg")):
                        start = time.perf_counter()
                        if engine == "rg":
                            result = subprocess.run(command, cwd=source, capture_output=True, text=True, timeout=30)
                            assert result.returncode == 0, result.stderr
                            text = result.stdout
                        else:
                            result = await session.call_tool(tool, arguments, read_timeout_seconds=timedelta(seconds=180))
                            assert not result.isError, result
                            text = "\n".join(c.text for c in result.content if c.type == "text")
                        elapsed = time.perf_counter() - start
                        output_file = evidence / f"{round_number}-{task_id}-{engine}.txt"
                        output_file.write_text(text, encoding="utf-8")
                        row = {"round": round_number, "task": task_id, "question": question,
                               "engine": engine, "seconds": elapsed, **measure_text(text),
                               "output": output_file.name}
                        report["results"].append(row)
                        print(json.dumps(row), flush=True)
    report["summary"] = {}
    for engine in ("rg", "serena"):
        rows = [r for r in report["results"] if r["engine"] == engine]
        report["summary"][engine] = {"query_seconds_total": sum(r["seconds"] for r in rows),
                                      "query_seconds_median": statistics.median(r["seconds"] for r in rows),
                                      "output_tokens_proxy_total": sum(r["output_tokens_proxy"] for r in rows)}
    report["correctness"] = "Pending independent review of raw outputs; exit success alone is not correctness."
    (evidence / "results.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({"complete": True, "summary": report["summary"], "evidence": str(evidence)}), flush=True)

asyncio.run(main())
