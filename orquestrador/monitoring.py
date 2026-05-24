"""
Monitoring utilities for real-time orchestrator observability
"""

import json
import time
from datetime import datetime
from pathlib import Path
from typing import Optional


def start_monitor(state_dir: Path):
    """Monitor current execution status in real-time"""
    state_dir.mkdir(parents=True, exist_ok=True)
    current_status_path = state_dir / "current_status.json"

    print("Starting monitor mode. Press Ctrl+C to exit.\n")
    try:
        while True:
            if current_status_path.exists():
                try:
                    status_json = current_status_path.read_text(encoding="utf-8")
                    status = json.loads(status_json)
                    elapsed = (datetime.now() - datetime.fromisoformat(
                        status.get("started_at", datetime.now().isoformat())
                    )).total_seconds()
                    print(
                        f"\r[{status['status']:8}] {status['item_id']:40} | "
                        f"{status['agent']:6} | Phase: {status.get('current_phase', 'N/A'):12} | "
                        f"Silence: {status.get('seconds_since_last_output', 0):3}s | "
                        f"Files: {status.get('changed_files_count', 0):2}",
                        end="",
                        flush=True
                    )
                except json.JSONDecodeError:
                    print("Waiting for valid status...", end="\r", flush=True)
            else:
                print("Waiting for execution to start...", end="\r", flush=True)
            time.sleep(2)
    except KeyboardInterrupt:
        print("\n\nMonitor stopped.")
        return


def check_stop_signal(state_dir: Path) -> bool:
    """Check if STOP signal file exists"""
    stop_file = state_dir / "STOP"
    return stop_file.exists()


def check_pause_signal(state_dir: Path) -> bool:
    """Check if PAUSE signal file exists"""
    pause_file = state_dir / "PAUSE"
    return pause_file.exists()


def clear_pause_signal(state_dir: Path):
    """Remove PAUSE signal file"""
    pause_file = state_dir / "PAUSE"
    if pause_file.exists():
        pause_file.unlink()


def write_status(state_dir: Path, status_json: str):
    """Write status.json to state directory"""
    state_dir.mkdir(parents=True, exist_ok=True)
    status_path = state_dir / "current_status.json"
    status_path.write_text(status_json, encoding="utf-8")
