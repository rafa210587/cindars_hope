"""
Execution Queue - Build and order spec/prompt execution queues
(Renamed from queue.py to avoid stdlib collision)
"""

import re
from pathlib import Path
from typing import Optional, List, Tuple


def natural_sort_key(path: Path) -> tuple:
    """
    Natural sort key with letter-suffix handling.
    SPEC_17A sorts BEFORE SPEC_17 (letter suffix = prerequisite version).

    Examples:
      SPEC_17A_scale → ((1,"spec_"), (0,17,"a"), (1,"_scale"))
      SPEC_17_ui    → ((1,"spec_"), (0,17,"~"), (1,"_ui"))
    """
    text = path.stem.lower()
    parts = []
    i = 0

    while i < len(text):
        # Consume non-digit characters
        j = i
        while j < len(text) and not text[j].isdigit():
            j += 1
        if j > i:
            parts.append((1, text[i:j]))

        if j >= len(text):
            break

        # Consume digit characters
        k = j
        while k < len(text) and text[k].isdigit():
            k += 1
        num = int(text[j:k])

        # Check for letter suffix immediately after digits
        m = k
        while m < len(text) and text[m].isalpha():
            m += 1

        if m > k:
            # Has letter suffix: sort BEFORE same num without suffix
            suffix = text[k:m]
        else:
            # No letter suffix: sort AFTER any letter suffix
            suffix = "~"

        parts.append((0, num, suffix))
        i = m

    return tuple(parts)


def build_agnostic_queue(
    input_dir: Path,
    ignored_patterns: List[str] = None,
    sort_mode: str = "natural",
    include_all_md: bool = False
) -> Tuple[List[Path], List[Path]]:
    """
    Build execution queue from any folder (folder-agnostic).
    Returns: (queue, ignored_list)
    """
    DEFAULT_IGNORE_PATTERNS = [
        "00_INDEX_ORDEM_USO",
        "00_PROMPT_MESTRE",
        "00B_PROMPT_AUXILIAR",
        "99_TEMPLATE",
        "README",
    ]

    if not input_dir.exists():
        return [], []

    # Read only direct .md files (not recursive)
    all_md = [f for f in input_dir.iterdir() if f.is_file() and f.suffix.lower() == ".md"]

    if include_all_md:
        eligible = all_md
        skipped = []
    else:
        # Filter by ignored patterns (case-insensitive)
        effective_patterns = list(DEFAULT_IGNORE_PATTERNS) + (ignored_patterns or [])
        eligible = []
        skipped = []

        for f in all_md:
            if any(pat.lower() in f.name.lower() for pat in effective_patterns):
                skipped.append(f)
            else:
                eligible.append(f)

    # Sort by mode
    if sort_mode == "natural":
        queue = sorted(eligible, key=natural_sort_key)
    elif sort_mode == "alpha":
        queue = sorted(eligible, key=lambda p: p.name.lower())
    elif sort_mode == "index":
        index_path = input_dir / "00_INDEX_ORDEM_USO.md"
        if not index_path.exists():
            print(f"ERROR: --sort index requires {index_path} but it does not exist.")
            import sys
            sys.exit(1)

        index_order = parse_prompt_index(index_path)
        index_dict = {name: i for i, (_, name) in enumerate(index_order)}

        def sort_key(p):
            if p.name in index_dict:
                return (0, index_dict[p.name])
            else:
                return (1, natural_sort_key(p))

        queue = sorted(eligible, key=sort_key)
    else:
        # Default to natural
        queue = sorted(eligible, key=natural_sort_key)

    return queue, skipped


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
    Handles markdown table format:
    | Ordem | Spec | Status | Depende de | Bloqueia | Risco se antecipar |
    | 12 | [spec_player_combat...](a_implementar/spec_player_combat...md) | A implementar | ...

    Returns dict mapping spec_number -> {order, filename, path, status}
    """
    if not spec_order_path.exists():
        return {}

    content = spec_order_path.read_text(encoding="utf-8")
    order_dict = {}

    for line in content.split("\n"):
        # Split by pipe character for markdown table
        cols = [c.strip() for c in line.split("|")]

        # Need at least: |order|spec|status|... (4 cols after splitting)
        if len(cols) < 4:
            continue

        # Column 1 (index 1): order number
        order_col = cols[1].strip()
        if not order_col.isdigit():
            continue  # Skip header rows

        order_num = int(order_col)

        # Column 2 (index 2): spec with markdown link [name](path)
        spec_col = cols[2].strip()

        # Column 3 (index 3): status
        status_col = cols[3].strip()

        # Extract link: [spec_player_combat...](a_implementar/spec_player_combat...)
        link_match = re.search(r'\[([^\]]+)\]\(([^)]+)\)', spec_col)
        if link_match:
            link_path = link_match.group(2)  # e.g. "a_implementar/spec_player_combat..."
            filename = Path(link_path).name  # e.g. "spec_player_combat_weapons..."
            full_path = f"docs/specs/{link_path}"  # e.g. "docs/specs/a_implementar/spec_player..."

            order_dict[order_num] = {
                "order": order_num,
                "filename": filename,
                "path": full_path,
                "status": status_col
            }

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
    Warns if files not found in execution order.
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
    warned = set()

    def sort_key(path: Path) -> tuple:
        num = extract_spec_number(path.name)
        if num is not None and num in order_dict:
            return (0, order_dict[num].get("order", 9999), path.name)
        elif num is not None:
            if num not in warned:
                print(f"WARNING: SPEC_{num} not found in SPEC_EXECUTION_ORDER.md")
                warned.add(num)
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

    # Always ignore index file itself and templates
    always_ignore = ["00_INDEX_ORDEM_USO.md", "00_PROMPT_MESTRE", "00B_PROMPT_AUXILIAR", "99_TEMPLATE"]

    # Filter ignored patterns
    filtered = []
    for file in all_files:
        should_ignore = False

        # Check always-ignore patterns
        for pattern in always_ignore:
            if pattern in file.name:
                should_ignore = True
                break

        # Check user-provided patterns
        if not should_ignore:
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
    1. Explicit path in content (docs/specs/a_implementar/... or docs/specs/implementados/...)
    2. SPEC_XX number from prompt filename + lookup in execution order
    3. Returns None if can't infer
    """
    # Try to find explicit spec path in content
    match = re.search(
        r"docs/specs/(a_implementar|implementados)/(\S+\.md)",
        prompt_content,
        re.IGNORECASE
    )
    if match:
        subdir = match.group(1)
        spec_filename = match.group(2)
        spec_path = Path("docs/specs") / subdir / spec_filename
        if spec_path.exists():
            return spec_path

    # Try to extract spec number from prompt filename
    spec_num = extract_spec_number(prompt_path.name)
    if spec_num is None:
        return None

    # Look up spec file in execution order
    order_dict = parse_spec_execution_order(spec_execution_order_path)

    # Use execution order map to get real filename
    if spec_num in order_dict:
        spec_info = order_dict[spec_num]
        spec_path = Path(spec_info["path"])

        # Check if exists in a_implementar
        if spec_path.exists():
            return spec_path

        # Check if exists in implementados
        impl_path = Path("docs/specs/implementados") / spec_info["filename"]
        if impl_path.exists():
            return impl_path

    # Fallback: try common filename patterns
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


def print_dry_run_report(queue: List[Path], mode: str, ignored_patterns: List[str] = None, skipped_items: List[Path] = None):
    """
    Print dry run report showing queue, ignored patterns, and skipped items.
    """
    print(f"\n{'='*70}")
    print(f"DRY RUN - {mode.upper()} MODE")
    print(f"{'='*70}\n")

    if not queue:
        print("Queue is EMPTY\n")
    else:
        print(f"Queue ({len(queue)} items):")
        for i, path in enumerate(queue, 1):
            spec_num = extract_spec_number(path.name)
            spec_label = f" [SPEC_{spec_num:02d}]" if spec_num else ""
            print(f"  {i}. {path.name}{spec_label}")

    if skipped_items:
        print(f"\nIgnored ({len(skipped_items)} items):")
        for path in skipped_items:
            print(f"  - {path.name}")

    if ignored_patterns:
        print(f"\nIgnored patterns:")
        for pattern in ignored_patterns:
            print(f"  - {pattern}")

    print(f"\n{'='*70}\n")
