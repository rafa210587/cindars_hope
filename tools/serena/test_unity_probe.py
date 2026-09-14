"""Bounded production-source navigation probe; writes machine-readable evidence to stdout."""
import asyncio
import argparse
from datetime import timedelta
import json
from pathlib import Path
import sys
import time

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

HERE = Path(__file__).resolve().parent


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo")
    args = parser.parse_args()
    server_args = [str(HERE / "serena_unity_proxy.py")]
    if args.repo:
        server_args.extend(["--repo", args.repo])
    server = StdioServerParameters(
        command=sys.executable,
        args=server_args,
    )
    start = time.perf_counter()
    async with stdio_client(server) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            print(json.dumps({"event": "initialize", "seconds": round(time.perf_counter()-start, 3)}), flush=True)
            tools = await session.list_tools()
            names = sorted(t.name for t in tools.tools)
            print(json.dumps({"event": "tools/list", "names": names}), flush=True)
            for name, arguments in [
                ("find_symbol", {"name_path_pattern": "StableHash32/Compute", "relative_path": "Assets/_Game/Scripts/Foundation/StableHash32.cs", "include_info": True}),
                ("find_referencing_symbols", {"name_path": "StableHash32/Compute", "relative_path": "Assets/_Game/Scripts/Foundation/StableHash32.cs"}),
                ("find_symbol", {"name_path_pattern": "GameplayInputBlocker/Acquire", "relative_path": "Assets/_Game/Scripts/Gameplay/GameplayInputBlocker.cs", "include_info": True}),
                ("find_referencing_symbols", {"name_path": "GameplayInputBlocker/Acquire", "relative_path": "Assets/_Game/Scripts/Gameplay/GameplayInputBlocker.cs"}),
            ]:
                start = time.perf_counter()
                result = await session.call_tool(name, arguments, read_timeout_seconds=timedelta(seconds=300))
                print(json.dumps({"event": name, "arguments": arguments, "seconds": round(time.perf_counter()-start,3), "result": result.model_dump(mode="json")}, ensure_ascii=False), flush=True)


asyncio.run(main())
