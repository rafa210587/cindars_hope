"""Deterministic sync guard fixtures; never mutate the real Unity checkout."""
from pathlib import Path
import subprocess
import uuid
from serena_unity_proxy import safe_relative

root = Path(r"D:\Projetos\Cindars_Hope\.serena-runtime\guard_test") / uuid.uuid4().hex
repo = root / "fixture"
repo.mkdir(parents=True)
(repo / "Input.cs").write_text("class Input {}", encoding="utf-8")
(repo / "metadata.dll").write_bytes(b"hash-only fixture, not loaded by LSP")
(repo / "analyzer.dll").write_bytes(b"hash-only analyzer fixture")
for name in ("Foundation", "Gameplay", "Runtime"):
    (repo / f"CindarsHope.{name}.csproj").write_text(
        '<Project><ItemGroup><Compile Include="Input.cs"/>'
        '<Reference Include="Metadata"><HintPath>metadata.dll</HintPath></Reference>'
        '<Analyzer Include="analyzer.dll"/></ItemGroup></Project>', encoding="utf-8")
sync = Path(__file__).with_name("Sync-UnitySnapshot.ps1")

def run(snapshot, check=False, expected=0):
    args = ["pwsh", "-NoProfile", "-File", str(sync), "-Repository", str(repo), "-Snapshot", str(snapshot)]
    result = subprocess.run(args + (["-CheckOnly"] if check else []), capture_output=True, text=True, timeout=60)
    assert (result.returncode == 0) == (expected == 0), result.stdout + result.stderr

for value in ("../outside.cs", "C:/outside.cs", "Assets\\..\\outside.cs"):
    assert not safe_relative(value)
assert safe_relative("Assets/Input.cs")
print("PASS path argument checks", flush=True)

for case, relative, copied in (
    ("source original", "Input.cs", False),
    ("source snapshot", "Input.cs", True),
    ("project snapshot", "CindarsHope.Foundation.csproj", True),
    ("metadata DLL", "metadata.dll", False),
    ("analyzer DLL", "analyzer.dll", False),
):
    snapshot = root / uuid.uuid4().hex
    run(snapshot)
    run(snapshot, check=True)
    run(snapshot, expected=1)
    target = (snapshot if copied else repo) / relative
    target.write_bytes(target.read_bytes() + b"\n<!-- fixture drift -->")
    run(snapshot, check=True, expected=1)
    print(f"PASS {case} drift and existing-target rejection", flush=True)
print("PASS 6 guard groups; synthetic filesystem fixtures, not semantic Unity validation", flush=True)
