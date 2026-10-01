#!/usr/bin/env python3
"""Validate the repository's canonical agent-content layout."""

from __future__ import annotations

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CANONICAL_SKILLS = ROOT / ".ai" / "skills"
CANONICAL_PROMPTS = ROOT / ".ai" / "prompts"

DOCUMENTATION_REQUIREMENTS = {
    "docs/getting-started.md": (
        "# Quick guide to using the template",
        "## Prerequisites",
        "## Step-by-step to set up the environment",
        "## Configuration file checklist",
        "dotnet restore AppProject.slnx",
        "do not let Copilot, Codex, or any other generator automatically create",
    ),
    "docs/architecture.md": (
        "# Project structure",
        "# Project specifications",
        "src/Directory.Build.props",
        "src/Stylecop.json",
    ),
    "docs/integrations.md": (
        "# External integrations",
        "## Auth0",
        "## SendGrid",
        "## GitHub AI Models",
        "## Administrator user",
    ),
    "docs/development-guide.md": (
        "# CRUD example",
        "## Backend",
        "## Frontend",
        "### 1. Identify the module",
        "### 7. Creating controller classes",
        "dotnet ef migrations add MigrationName",
        "IDatabaseRepository",
        "IPermissionService.ValidateCurrentUserPermissionAsync",
        "Resource.pt-BR.resx",
        "Resource.es-ES.resx",
    ),
    "docs/testing.md": (
        "# Tests",
        "NUnit",
        "Moq",
        "Shouldly",
        "Bogus",
        "dotnet test AppProject.slnx",
    ),
    "docs/production.md": (
        "# Preparing for production",
        "dotnet publish -c Release",
        "environment variables",
    ),
}


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
        ROOT / ".ai" / "instructions" / "configuration.md",
        ROOT / ".ai" / "instructions" / "completion-checklist.md",
        ROOT / "docs" / "README.md",
    ]
    for path in required:
        if not path.is_file():
            errors.append(f"missing required file: {path.relative_to(ROOT)}")


def validate_documentation_coverage(errors: list[str]) -> None:
    """Protect the topics migrated from the original bilingual README."""
    for relative_path, required_fragments in DOCUMENTATION_REQUIREMENTS.items():
        path = ROOT / relative_path
        if not path.is_file():
            errors.append(f"missing documentation file: {relative_path}")
            continue

        text = path.read_text(encoding="utf-8")
        for fragment in required_fragments:
            if fragment not in text:
                errors.append(f"{relative_path} is missing migrated content: {fragment}")


def main() -> int:
    errors: list[str] = []
    validate_required_files(errors)
    validate_documentation_coverage(errors)
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
