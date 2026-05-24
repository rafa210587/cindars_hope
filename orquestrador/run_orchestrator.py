#!/usr/bin/env python3
r"""
[DEPRECATED] Legacy wrapper - Use: python .\orquestrador\run_orquestrador.py

This file is maintained for backward compatibility only.
All functionality has moved to run_orquestrador.py (v2.0)
"""

from pathlib import Path
import runpy

target = Path(__file__).resolve().parent / "run_orquestrador.py"
runpy.run_path(str(target), run_name="__main__")
