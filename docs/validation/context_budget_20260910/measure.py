"""Read-only local instruction measurements; not model usage or billing telemetry."""
import json
import re
import sys
from pathlib import Path

root = Path.cwd()
sys.path.insert(0, str(root / '.claude/.runtime/token-benchmark'))
import tiktoken
enc = tiktoken.get_encoding('o200k_base')
assert enc.decode(enc.encode('Ação → {{path}}')) == 'Ação → {{path}}'

def read(p):
    return Path(p).read_text(encoding='utf-8-sig').replace('\r\n', '\n')

def stats(text):
    return {'characters': len(text), 'o200k_base': len(enc.encode(text))}

def metadata(p):
    text = read(p)
    header = text.split('---', 2)[1]
    def field(key):
        match = re.search(r'^' + key + r':\s*(.+)$', header, re.M)
        return match.group(1).strip().strip('"\'') if match else p.parent.name
    return field('name'), field('description')

paths = sorted((root / '.agents/skills').glob('*/SKILL.md'))
rows = []
for p in paths:
    name, desc = metadata(p)
    rows.append({'name': name, 'path': p.relative_to(root).as_posix(),
                 'full_entry': stats(read(p)), 'description_characters': len(desc)})

def catalog(selected, short):
    lines = []
    for p in selected:
        name, desc = metadata(p)
        if short:
            desc = desc[:110]
        # The session uses the r8 alias; this is a reconstruction, not raw host telemetry.
        location = 'r8/' + p.parent.name + '/SKILL.md'
        lines.append(f'- {name}: {desc} (file: {location})')
    return stats('\n'.join(lines))

files = ['AGENTS.md', 'CLAUDE.md', 'docs/project/CURRENT_STATE.md', '.claude/HARNESS_INDEX.md']
file_stats = {p: stats(read(root / p)) for p in files}
new = [p for p in paths if p.parent.name in ('unity-mcp-operations', 'unity-performance-profiling')]
scenario_paths = {
    'mcp_operation': ['unity-mcp-operations/SKILL.md'],
    'mcp_validation': ['unity-mcp-operations/SKILL.md', 'unity-validation/SKILL.md'],
    'aseprite_edit': ['aseprite-authoring/SKILL.md', 'aseprite-authoring/references/cli-lua.md', 'aseprite-authoring/references/acceptance.md'],
    'animation_extra': ['pixel-art-animator/SKILL.md', 'pixel-art-animator/references/operations.md', 'sprite-animation-review/SKILL.md', 'sprite-animation-review/references/motion-diagnostics.md'],
    'hud_binding': ['hud-canvas-binding/SKILL.md'],
    'profiling': ['unity-performance-profiling/SKILL.md'],
}
scenarios = {name: {'paths': group, **stats('\n'.join(read(root / '.agents/skills' / p) for p in group))}
             for name, group in scenario_paths.items()}
agent_rows = []
for p in sorted((root / '.claude/agents').glob('*.md')):
    name, desc = metadata(p)
    agent_rows.append({'name': name, 'description': desc, 'full_entry': stats(read(p))})
out = {'method': 'o200k_base reference tokenizer; normalized LF; catalog reconstructed with 110-character description cap and r8 paths, not actual serialized prompts',
       'project_skill_count': len(paths), 'project_catalog_110': catalog(paths, True),
       'project_catalog_full_descriptions': catalog(paths, False),
       'two_new_catalog_110': catalog(new, True), 'files': file_stats,
       'all_skill_entries_NOT_automatic': stats('\n'.join(read(p) for p in paths)),
       'scenarios_additional_reads': scenarios,
       'skill_entries': rows,
       'largest_entries': sorted(rows, key=lambda x: x['full_entry']['o200k_base'], reverse=True)[:12],
       'legacy_command_entries': [r for r in rows if r['name'] in ('implement-spec','resolve-spec-dependency-chain','audit-spec','start-spec')],
       'agent_metadata_reconstruction': stats('\n'.join('- '+r['name']+': '+r['description'] for r in agent_rows)),
       'agents': agent_rows}
external = [
 ('r1', 'C:/Users/Rafa/.codex/skills/.system', x+'/SKILL.md')
 for x in ['imagegen','openai-docs','plugin-creator','skill-creator','skill-installer']]
external += [('r0','C:/Users/Rafa/.codex/skills',x+'/SKILL.md') for x in ['ai-memory-intent-retrieval','medium-series-editor']]
external += [('r5','C:/Users/Rafa/.codex/plugins/cache/openai-curated-remote/google-drive/0.1.16/skills',x+'/SKILL.md') for x in ['google-docs','google-drive','google-drive-comments','google-sheets','google-slides']]
external += [
 ('r2','C:/Users/Rafa/.codex/plugins/cache/openai-bundled','computer-use/26.903.61454/skills/computer-use/SKILL.md'),
 ('r2','C:/Users/Rafa/.codex/plugins/cache/openai-bundled','visualize/1.0.32/skills/visualize/SKILL.md'),
 ('r4','C:/Users/Rafa/.codex/plugins/cache/openai-curated-remote','deep-research-work/0.1.15/skills/deep-research/SKILL.md'),
 ('r4','C:/Users/Rafa/.codex/plugins/cache/openai-curated-remote','plugin-management/0.1.0/skills/plugin-management/SKILL.md'),
]
external += [('r6','C:/Users/Rafa/.codex/plugins/cache/openai-primary-runtime',x+'/26.905.11957/skills/'+x+'/SKILL.md') for x in ['documents','pdf','presentations','template-creator']]
external += [('r3','C:/Users/Rafa/.codex/plugins/cache/openai-bundled/sites/0.1.66/skills',x+'/SKILL.md') for x in ['sites-building','sites-hosting']]
external += [('r7','C:/Users/Rafa/.codex/plugins/cache/openai-primary-runtime/spreadsheets/26.905.11957/skills',x+'/SKILL.md') for x in ['spreadsheets','excel-live-control']]
external_lines = []
for alias, base, suffix in external:
    p = Path(base) / suffix
    name, desc = metadata(p)
    external_lines.append(f'- {name}: {desc[:110]} (file: {alias}/{suffix})')
out['external_catalog_reconstruction'] = {'entries': len(external_lines), **stats('\n'.join(external_lines))}
dest = root / 'docs/validation/context_budget_20260910/measurements.json'
dest.write_text(json.dumps(out, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps({k:v for k,v in out.items() if k not in ('agents',)}, ensure_ascii=False, indent=2))
