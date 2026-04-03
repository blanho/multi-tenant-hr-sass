#!/usr/bin/env python3
"""
Removes all // comments from all C# files in the project.

Removes:
  - /// XML doc comments
  - // TODO: tags
  - Full-line // comments
  - Trailing // comments appended to code lines

Usage:
  python3 scripts/remove-inline-comments.py            # dry-run (preview only)
  python3 scripts/remove-inline-comments.py --apply    # apply changes
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).parent.parent
SRC_DIRS = [ROOT / "src"]
APPLY = "--apply" in sys.argv


def _advance_string(line: str, i: int) -> int:
    i += 1
    while i < len(line):
        if line[i] == '\\':
            i += 2
        elif line[i] == '"':
            return i + 1
        else:
            i += 1
    return i


def _advance_verbatim(line: str, i: int) -> int:
    i += 2
    while i < len(line):
        if line[i] == '"' and i + 1 < len(line) and line[i + 1] == '"':
            i += 2
        elif line[i] == '"':
            return i + 1
        else:
            i += 1
    return i


def strip_trailing_comment(line: str) -> str:
    """
    Removes trailing // comment from a code line.
    Skips removal when // appears inside a string literal.
    """
    i = 0
    while i < len(line):
        ch = line[i]
        if ch == '@' and i + 1 < len(line) and line[i + 1] == '"':
            i = _advance_verbatim(line, i)
        elif ch == '"':
            i = _advance_string(line, i)
        elif ch == '/' and i + 1 < len(line) and line[i + 1] == '/':
            return line[:i].rstrip()
        else:
            i += 1
    return line


def process_file(path: Path) -> tuple[str, str]:
    original = path.read_text(encoding="utf-8")
    lines = original.splitlines(keepends=True)
    result = []

    for line in lines:
        stripped = line.strip()

        if stripped.startswith("//"):
            continue

        if "//" in line:
            new_line = strip_trailing_comment(line)
            if new_line != line.rstrip("\n"):
                ending = "\n" if line.endswith("\n") else ""
                result.append(new_line + ending)
                continue

        result.append(line)

    cleaned = "".join(result)
    return original, cleaned


def main():
    cs_files = []
    for src_dir in SRC_DIRS:
        cs_files.extend(src_dir.rglob("*.cs"))

    changed = []
    for path in sorted(cs_files):
        original, cleaned = process_file(path)
        if original != cleaned:
            changed.append(path)
            rel = path.relative_to(ROOT)
            if APPLY:
                path.write_text(cleaned, encoding="utf-8")
                print(f"  [updated] {rel}")
            else:
                print(f"  [preview] {rel}")

    print()
    if not changed:
        print("No inline comments found.")
        return

    if APPLY:
        print(f"Removed inline comments from {len(changed)} file(s).")
    else:
        print(f"Found inline comments in {len(changed)} file(s).")
        print("Run with --apply to remove them.")


if __name__ == "__main__":
    main()
