#!/usr/bin/env python3
import os
import glob
import re

os.chdir("D:\\Projetos\\Jogos\\Cindars_hope\\cindars_hope")

# Check 1: Count refinamento_init files
init_refs = glob.glob("docs/refinements/a_implementar/pre_refinamentos/refinamento_init_*.md")
print(f"Refinamento_init count: {len(init_refs)} (expected 18)")

# Check 2: Check future specs with spec_ prefix
future_specs = glob.glob("docs/specs/a_implementar/spec_*.md")
print(f"Future specs with spec_ prefix: {len(future_specs)}")

# Check 3: Check markers in future specs
for spec_file in future_specs:
    with open(spec_file, 'r', encoding='utf-8') as f:
        content = f.read()

    missing_markers = []
    for marker in ["# /speckit.specify", "# /speckit.plan", "# /speckit.tasks"]:
        if marker not in content:
            missing_markers.append(marker)

    if missing_markers:
        print(f"SPEC missing markers: {os.path.basename(spec_file)}")
        for m in missing_markers:
            print(f"  - {m}")
    else:
        print(f"OK: {os.path.basename(spec_file)}")

# Check 4: Check for bad naming in a_implementar
bad_specs = []
for f in os.listdir("docs/specs/a_implementar"):
    if f.endswith('.md') and f != 'README.md' and not f.startswith('spec_') and not f.startswith('DEPRECATED_'):
        bad_specs.append(f)

if bad_specs:
    print("\nSpecs without spec_ prefix (excluding README, DEPRECATED, etc):")
    for s in bad_specs:
        print(f"  - {s}")
else:
    print("\nNo specs without spec_ prefix (OK)")
