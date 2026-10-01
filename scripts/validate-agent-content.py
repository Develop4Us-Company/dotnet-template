#!/usr/bin/env python3
"""Validate the repository's canonical agent-content layout."""

from __future__ import annotations

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CANONICAL_SKILLS = ROOT / ".ai" / "skills"
CANONICAL_PROMPTS = ROOT / ".ai" / "prompts"


def validate_link(path: Path, expected: Path, errors: list[str]) -> None:
    if not path.is_symlink():
        errors.append(f"{path.relative_to(ROOT)} must be a symbolic link")
        return

    if path.resolve() != expected.resolve():
        errors.append(
            f"{path.relative_to(ROOT)} resolves to {path.resolve()}, expected {expected.resolve()}"
        )


def frontmatter(path: Path, errors: list[str]) -> str | None:
    text = path.read_text(encoding="utf-8")
    match = re.match(r"^---\n(.*?)\n---\n", text, re.DOTALL)
    if match is None:
        errors.append(f"{path.relative_to(ROOT)} has no YAML frontmatter")
        return None

    return match.group(1)


def validate_skills(errors: list[str]) -> None:
    for path in sorted(CANONICAL_SKILLS.glob("*/SKILL.md")):
        metadata = frontmatter(path, errors)
        if metadata is None:
            continue

        name = re.search(r"^name:\s*(.+)$", metadata, re.MULTILINE)
        description = re.search(r"^description:\s*(.+)$", metadata, re.MULTILINE)
        if name is None:
            errors.append(f"{path.relative_to(ROOT)} has no skill name")
        elif name.group(1).strip(' "') != path.parent.name:
            errors.append(f"{path.relative_to(ROOT)} name does not match its directory")
        if description is None:
            errors.append(f"{path.relative_to(ROOT)} has no description")


def validate_prompts(errors: list[str]) -> None:
    for path in sorted(CANONICAL_PROMPTS.glob("*.prompt.md")):
        metadata = frontmatter(path, errors)
        if metadata is None:
            continue

        if re.search(r"^mode:\s*agent\s*$", metadata, re.MULTILINE) is None:
            errors.append(f"{path.relative_to(ROOT)} must declare mode: agent")
        if re.search(r"^description:\s*.+$", metadata, re.MULTILINE) is None:
            errors.append(f"{path.relative_to(ROOT)} has no description")


def validate_required_files(errors: list[str]) -> None:
    required = [
        ROOT / "AGENTS.md",
        ROOT / "CLAUDE.md",
        ROOT / ".github" / "copilot-instructions.md",
        ROOT / ".ai" / "README.md",
        ROOT / ".ai" / "instructions" / "repository.md",
        ROOT / ".ai" / "instructions" / "completion-checklist.md",
        ROOT / "docs" / "README.md",
    ]
    for path in required:
        if not path.is_file():
            errors.append(f"missing required file: {path.relative_to(ROOT)}")


def main() -> int:
    errors: list[str] = []
    validate_required_files(errors)
    validate_link(ROOT / ".github" / "skills", CANONICAL_SKILLS, errors)
    validate_link(ROOT / ".agents" / "skills", CANONICAL_SKILLS, errors)
    validate_link(ROOT / ".claude" / "skills", CANONICAL_SKILLS, errors)
    validate_link(ROOT / ".github" / "prompts", CANONICAL_PROMPTS, errors)
    validate_skills(errors)
    validate_prompts(errors)

    if errors:
        print("Agent-content validation failed:", file=sys.stderr)
        for error in errors:
            print(f"- {error}", file=sys.stderr)
        return 1

    skill_count = len(list(CANONICAL_SKILLS.glob("*/SKILL.md")))
    prompt_count = len(list(CANONICAL_PROMPTS.glob("*.prompt.md")))
    print(f"Validated {skill_count} canonical skills and {prompt_count} canonical prompts.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
