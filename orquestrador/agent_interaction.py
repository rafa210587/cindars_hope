"""
Agent interaction - Allow user to send messages to agent mid-execution
"""

import json
import time
from datetime import datetime
from pathlib import Path
from typing import Optional, List


class CommandQueue:
    """Manages command/message queue for agent interaction"""

    def __init__(self, item_log_dir: Path):
        self.cmd_dir = item_log_dir / "commands"
        self.cmd_dir.mkdir(parents=True, exist_ok=True)
        self.processed = set()

    def get_new_commands(self) -> List[dict]:
        """Get all unprocessed commands"""
        commands = []

        if not self.cmd_dir.exists():
            return commands

        try:
            for cmd_file in sorted(self.cmd_dir.glob("command_*.txt")):
                if str(cmd_file) in self.processed:
                    continue

                try:
                    content = cmd_file.read_text(encoding="utf-8")
                    cmd = self._parse_command(content)
                    if cmd:
                        commands.append(cmd)
                        self.processed.add(str(cmd_file))
                except Exception as e:
                    print(f"[WARN] Failed to read command {cmd_file.name}: {e}")

        except Exception as e:
            print(f"[WARN] Failed to scan commands: {e}")

        return commands

    def _parse_command(self, content: str) -> Optional[dict]:
        """Parse command file"""
        lines = content.strip().split("\n")
        cmd = {}

        for line in lines:
            if ":" in line:
                key, value = line.split(":", 1)
                cmd[key.strip()] = value.strip()

        return cmd if cmd else None

    def write_response(self, message: str):
        """Write agent response to interaction log"""
        resp_file = self.cmd_dir / "responses.log"
        with open(resp_file, "a", encoding="utf-8") as f:
            timestamp = datetime.now().isoformat()
            f.write(f"{timestamp} | {message}\n")


def check_and_log_commands(logger, cmd_queue: CommandQueue):
    """Check for new commands and log them as events"""
    commands = cmd_queue.get_new_commands()

    for cmd in commands:
        logger.log_event("user_command", {
            "timestamp": cmd.get("TIMESTAMP", "unknown"),
            "message": cmd.get("MESSAGE", "")
        })
        logger.update_timeline("user_command", cmd.get("MESSAGE", ""))
        print(f"\n[USER COMMAND] {cmd.get('MESSAGE', '')}\n")


def get_interaction_prompt_addendum(cmd_queue: CommandQueue) -> str:
    """Generate prompt addendum with pending commands"""
    commands = cmd_queue.get_new_commands()

    if not commands:
        return ""

    addendum = "\n===NOVAS INSTRUÇÕES DO USUÁRIO===\n"
    for i, cmd in enumerate(commands, 1):
        addendum += f"{i}. {cmd.get('MESSAGE', '')}\n"
    addendum += "===FIM DAS INSTRUÇÕES===\n"

    return addendum
