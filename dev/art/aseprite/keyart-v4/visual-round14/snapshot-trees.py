"""Read-only comparison of authored tree names and transform ancestry across regeneration."""
import json, re, sys
from pathlib import Path

scene, output = map(Path, sys.argv[1:])
names, transforms, by_object = {}, {}, {}
for kind, ident, body in re.findall(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- !u!|\Z)', scene.read_text(encoding='utf-8'), re.M | re.S):
    if kind == '1':
        name = re.search(r'^  m_Name: (.*)$', body, re.M)
        if name:
            names[ident] = name.group(1)
    elif kind == '4':
        obj = re.search(r'm_GameObject: \{fileID: (\d+)\}', body).group(1)
        parent = re.search(r'm_Father: \{fileID: (\d+)\}', body).group(1)
        values = [re.search(r'^  ' + field + r': (.*)$', body, re.M).group(1)
                  for field in ('m_LocalPosition', 'm_LocalRotation', 'm_LocalScale')]
        transforms[ident] = (obj, parent, values)
        by_object[obj] = ident
result = {}
for obj, name in names.items():
    if not re.fullmatch(r'TreeNode_\d+', name):
        continue
    chain, ident = [], by_object[obj]
    while ident != '0':
        owner, ident, values = transforms[ident]
        chain.append([names[owner], *values])
    result[name] = chain
output.write_text(json.dumps(result, sort_keys=True, indent=2), encoding='utf-8')
print(f'{len(result)} tree transforms recorded: {output}')
