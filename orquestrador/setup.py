#!/usr/bin/env python3
"""
Setup script for Spec Orchestrator - initializes environment and validates configuration
"""

import os
import sys
from pathlib import Path
import json


def check_dependencies():
    """Verify Python version and pip is available"""
    if sys.version_info < (3, 9):
        print("❌ Python 3.9+ required")
        return False
    print("✅ Python version OK")
    return True


def setup_env_file():
    """Create .env file if it doesn't exist"""
    env_file = Path(".env")
    if env_file.exists():
        print("✅ .env file exists")
        return True

    env_example = Path(".env.example")
    if not env_example.exists():
        print("⚠️  .env.example not found, creating minimal .env")
        with open(".env", "w") as f:
            f.write("ANTHROPIC_API_KEY=\n")
    else:
        # Copy example to .env
        with open(env_example, "r") as src:
            content = src.read()
        with open(".env", "w") as dst:
            dst.write(content)
        print("✅ Created .env from .env.example")

    print("⚠️  Please set ANTHROPIC_API_KEY in .env file")
    return True


def validate_config():
    """Validate config.json structure"""
    config_file = Path("config.json")
    if not config_file.exists():
        print("❌ config.json not found")
        return False

    try:
        with open(config_file, "r") as f:
            config = json.load(f)

        required_keys = ["project_name", "project_root", "api_config"]
        if not all(k in config for k in required_keys):
            print(f"❌ config.json missing required keys: {required_keys}")
            return False

        print("✅ config.json valid")
        return True
    except json.JSONDecodeError as e:
        print(f"❌ config.json invalid JSON: {e}")
        return False


def check_project_structure():
    """Verify project structure is accessible"""
    project_root = Path("../")
    required_dirs = ["docs/specs", "Assets/_Game/Scripts"]

    for directory in required_dirs:
        full_path = project_root / directory
        if not full_path.exists():
            print(f"⚠️  Missing directory: {directory}")
            return False

    print("✅ Project structure OK")
    return True


def install_dependencies():
    """Install Python dependencies"""
    print("\n📦 Installing Python dependencies...")
    import subprocess
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "-q", "-r", "requirements.txt"])
        print("✅ Dependencies installed")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ Failed to install dependencies: {e}")
        return False


def test_orchestrator_import():
    """Test that orchestrator can be imported"""
    try:
        from spec_orchestrator import SpecOrchestrator
        print("✅ Orchestrator imports OK")
        return True
    except ImportError as e:
        print(f"❌ Failed to import orchestrator: {e}")
        return False


def initialize_state():
    """Create initial orchestrator state if it doesn't exist"""
    from spec_orchestrator import SpecOrchestrator

    state_file = Path("orchestrator_state.json")
    if state_file.exists():
        print("✅ Orchestrator state already initialized")
        return True

    try:
        orchestrator = SpecOrchestrator("../")
        print("✅ Orchestrator state initialized")
        return True
    except Exception as e:
        print(f"⚠️  Failed to initialize state: {e}")
        return False


def main():
    print("=" * 70)
    print("SPEC ORCHESTRATOR SETUP")
    print("=" * 70)

    checks = [
        ("Python version", check_dependencies),
        ("Environment file", setup_env_file),
        ("Configuration", validate_config),
        ("Project structure", check_project_structure),
        ("Installing dependencies", install_dependencies),
        ("Orchestrator import", test_orchestrator_import),
        ("Initializing state", initialize_state),
    ]

    results = []
    for name, check_func in checks:
        print(f"\n{name}...")
        try:
            result = check_func()
            results.append(result)
        except Exception as e:
            print(f"❌ Error: {e}")
            results.append(False)

    print("\n" + "=" * 70)
    if all(results):
        print("✅ SETUP COMPLETE")
        print("\nNext steps:")
        print("1. Edit .env and add your ANTHROPIC_API_KEY")
        print("2. Run: python run_orchestrator.py --command status")
        print("3. Run: python run_orchestrator.py --command next")
        print("\nSee README.md for full usage guide.")
        return 0
    else:
        print("❌ SETUP INCOMPLETE")
        print("Please fix the issues above and run setup.py again")
        return 1


if __name__ == "__main__":
    sys.exit(main())
