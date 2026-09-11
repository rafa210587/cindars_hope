"""Read-only corpus comparison; reference encodings, not Astra/session billing.
Run from repository root with the bundled Python runtime after installing tiktoken
in the task-local .claude/.runtime/token-benchmark directory.
"""
import hashlib
import json
from pathlib import Path
import sys

root = Path.cwd()
sys.path.insert(0, str(root / '.claude/.runtime/token-benchmark'))
import tiktoken

artifacts = root / 'docs/validation/artifacts/progressive-harness-20260909'
before = json.loads((artifacts / 'english-pilot-before.json').read_text(encoding='utf-8-sig'))
encodings = {name: tiktoken.get_encoding(name) for name in ('o200k_base', 'cl100k_base')}
for encoding in encodings.values():
    assert len(encoding.encode('hello')) == 1
    probe = 'Ação → action; {{field}} /spec/path'
    assert encoding.decode(encoding.encode(probe)) == probe

rows = []
for item in before:
    old = item['text']
    new = (root / item['path']).read_text(encoding='utf-8-sig')
    # Normalize line endings in both inputs so CRLF changes do not masquerade as language savings.
    old = old.replace('\r\n', '\n')
    new = new.replace('\r\n', '\n')
    row = {'path': item['path'], 'before_sha256': hashlib.sha256(old.encode()).hexdigest(),
           'after_sha256': hashlib.sha256(new.encode()).hexdigest(),
           'before_bytes_normalized': len(old.encode()), 'after_bytes_normalized': len(new.encode()),
           'tokens': {}}
    for name, encoding in encodings.items():
        row['tokens'][name] = {'before': len(encoding.encode(old)), 'after': len(encoding.encode(new))}
    rows.append(row)

groups = {}
for label, selected in [('all_pilot_files', rows),
                        ('skills_only', [r for r in rows if r['path'].startswith('.claude/skills/')]),
                        ('skill_entries_only', [r for r in rows if r['path'].endswith('/SKILL.md')])]:
    totals = {}
    for name in encodings:
        old = sum(r['tokens'][name]['before'] for r in selected)
        new = sum(r['tokens'][name]['after'] for r in selected)
        totals[name] = {'before': old, 'after': new, 'reduction_percent': round(100 * (old-new)/old, 2)}
    groups[label] = {'files': len(selected), 'tokens': totals}

result = {'tiktoken_version': tiktoken.__version__,
          'scope': 'reference-encoding corpus comparison; no inference calls or session usage measurement',
          'exact_astra_tokenizer': 'NOT CONFIRMED',
          'groups': groups, 'files': rows}
(artifacts / 'english-pilot-tokens.json').write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding='utf-8')
print(json.dumps({'tiktoken_version': tiktoken.__version__, 'groups': groups}, indent=2))
