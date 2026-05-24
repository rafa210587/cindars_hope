"""
Queue - Build and order spec/prompt execution queues
"""

import re
from pathlib import Path
from typing import Optional, List


def extract_spec_number(filename: str) -> Optional[int]:
    """
    Extract spec number from filename.
    Examples: spec_player_combat_weapons_spells_skill_actions_runtime.md → 12
              SPEC_12_player-combat_PROMPT.md → 12
    """
    # Try SPEC_XX pattern first
    match = re.search(r"SPEC_(\d+)", filename, re.IGNORECASE)
    if match:
        return int(match.group(1))

    # Try spec_xxx pattern (count of spec_ in filename)
    # This is a fallback for the generic spec naming
    match = re.search(r"spec_(\d+)_", filename, re.IGNORECASE)
    if match:
        return int(match.group(1))

    return None


def parse_spec_execution_order(spec_order_path: Path) -> dict:
    """
    Parse SPEC_EXECUTION_ORDER.md to get spec ordering and dependencies.
    Returns dict mapping spec_number -> spec info
    """
    if not spec_order_path.exists():
        return {}

    content = spec_order_path.read_text(encoding="utf-8")
    order_dict = {}

    # Simple parser: look for lines with SPEC_XX
    lines = content.split("\n")
    order = 0
    for line in lines:
        if match := re.search(r"SPEC_(\d+)", line):
            spec_num = int(match.group(1))
            order_dict[spec_num] = {"order": order}
            order += 1

    return order_dict


def parse_prompt_index(index_path: Path) -> list:
    """
    Parse 00_INDEX_ORDEM_USO.md to get prompt execution order.
    Returns list of (spec_number, filename) tuples in order
    """
    if not index_path.exists():
        return []

    content = index_path.read_text(encoding="utf-8")
    order = []

    # Simple parser: look for numbered items with SPEC_XX
    lines = content.split("\n")
    for line in lines:
        if match := re.search(r"SPEC_(\d+).*?(\S+\.md)", line):
            spec_num = int(match.group(1))
            filename = match.group(2)
            order.append((spec_num, filename))

    return order


def build_spec_queue(
    input_dir: Path,
    spec_execution_order_path: Path
) -> List[Path]:
    """
    Build spec execution queue from directory.
    Orders by SPEC_EXECUTION_ORDER.md, fallback by filename number, then alphabetic.
    """
    if not input_dir.exists():
        return []

    # Get all .md files
    spec_files = sorted(input_dir.glob("spec_*.md"))

    if not spec_files:
        return []

    # Parse execution order
    order_dict = parse_spec_execution_order(spec_execution_order_path)

    # Sort by execution order
    def sort_key(path: Path) -> tuple:
        num = extract_spec_number(path.name)
        if num is not None and num in order_dict:
            return (0, order_dict[num].get("order", 9999), path.name)
        elif num is not None:
            return (1, num, path.name)
        else:
            return (2, 9999, path.name)

    sorted_files = sorted(spec_files, key=sort_key)
    return sorted_files


def build_prompt_queue(
    input_dir: Path,
    ignored_patterns: List[str],
    index_path: Optional[Path] = None
) -> List[Path]:
    """
    Build prompt execution queue from directory.
    Filters ignored patterns, orders by index or filename number.
    """
    if not input_dir.exists():
        return []

    # Get all .md files
    all_files = list(input_dir.glob("*.md"))

    # Filter ignored patterns
    filtered = []
    for file in all_files:
        should_ignore = False
        for pattern in ignored_patterns:
            if pattern in file.name:
                should_ignore = True
                break
        if not should_ignore:
            filtered.append(file)

    if not filtered:
        return []

    # Try to parse index for ordering
    if index_path and index_path.exists():
        index_order = parse_prompt_index(index_path)
        index_dict = {name: i for i, (_, name) in enumerate(index_order)}

        def sort_key(path: Path) -> tuple:
            if path.name in index_dict:
                return (0, index_dict[path.name])
            else:
                num = extract_spec_number(path.name)
                if num is not None:
                    return (1, num, path.name)
                else:
                    return (2, 9999, path.name)

        return sorted(filtered, key=sort_key)

    # Fallback: sort by spec number, then alphabetically
    def sort_key(path: Path) -> tuple:
        num = extract_spec_number(path.name)
        if num is not None:
            return (0, num, path.name)
        else:
            return (1, 9999, path.name)

    return sorted(filtered, key=sort_key)


def infer_target_spec(
    prompt_path: Path,
    prompt_content: str,
    spec_execution_order_path: Path
) -> Optional[Path]:
    """
    Infer the target spec for a prompt.
    Tries:
    1. Explicit path in content (docs/specs/a_implementar/...)
    2. SPEC_XX number from prompt filename + lookup in execution order
    3. Returns None if can't infer
    """
    # Try to find explicit spec path in content
    match = re.search(
        r"docs/specs/a_implementar/(\S+\.md)",
        prompt_content,
        re.IGNORECASE
    )
    if match:
        spec_filename = match.group(1)
        spec_path = Path("docs/specs/a_implementar") / spec_filename
        if spec_path.exists():
            return spec_path

    # Try to extract spec number from prompt filename
    spec_num = extract_spec_number(prompt_path.name)
    if spec_num is None:
        return None

    # Look up spec file in execution order
    order_dict = parse_spec_execution_order(spec_execution_order_path)

    # Try common filename patterns
    patterns = [
        f"spec_{spec_num}_*.md",
        f"*SPEC_{spec_num}*.md",
    ]

    specs_dir = Path("docs/specs/a_implementar")
    if specs_dir.exists():
        for pattern in patterns:
            matches = list(specs_dir.glob(pattern))
            if matches:
                return matches[0]

    return None


def print_dry_run_report(queue: List[Path], mode: str, ignored: List[str] = None):
    """
    Print dry run report showing queue and ignored items.
    """
    print(f"\n{'='*70}")
    print(f"DRY RUN - {mode.upper()} MODE")
    print(f"{'='*70}\n")

    if not queue:
        print("Queue is EMPTY\n")
        return

    print(f"Queue ({len(queue)} items):")
    for i, path in enumerate(queue, 1):
        spec_num = extract_spec_number(path.name)
        spec_label = f" [SPEC_{spec_num}]" if spec_num else ""
        print(f"  {i}. {path.name}{spec_label}")

    if ignored:
        print(f"\nIgnored patterns ({len(ignored)} patterns):")
        for pattern in ignored:
            print(f"  - {pattern}")

    print(f"\n{'='*70}\n")
