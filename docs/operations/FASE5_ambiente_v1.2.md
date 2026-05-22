# Cindar's Hope — Fase 5: Estruturação do Ambiente v1.2

> **Fase:** 5 de 13
> **Status:** ✅ Documento pronto — execução local deve ser validada pelo checklist
> **Última atualização:** 2026-05-16
> **Depende de:** ARCH_fase4_v2.2.md, GDD_v2.6.md
> **Alimenta:** Fase 6 (Histórias), Fase 7 (Spec Kit), Fase 8 (Implementação MVP)

---

## HANDOFF PARA OUTRA LLM

### Plano macro

| # | Fase | Status |
|---|---|---|
| 1–4 | Ideação, Refinamento, Identidade, Arquitetura | ✅ Concluídas |
| 5 | **Estruturação do Ambiente** | ✅ Documento pronto / execução local a validar |
| 6 | Quebra de Histórias | ✅ FARM detalhado / demais épicos esboçados |
| 7 | Spec Kit MVP Fazenda | ✅ Specs completas |
| 8 | MVP — Core Loop Fazenda | 🔄 Próxima execução |
| 9–13 | Execução, Polish, Testes, Build, Iteração | ⏳ Pendente |
| 9–13 | Execução, Polish, Testes, Build, Iteração | ⏳ Pendente |

### Contexto técnico (Windows, VSCode, Codex)
- SO: Windows
- IDE: VS Code
- Agente de código: Codex (app + VS Code extension)
- Pixel art: **Aseprite** como ferramenta principal; Pixelorama/LibreSprite como fallback open-source
- IA de arte: DALL-E 3 via ChatGPT Plus
- Git: instalar via winget ou site (ver seção 2)
- Unity: instalar via Unity Hub (ver seção 3)
- SpecKit: instalar via `uv` (ver seção 6)

### O que este documento entrega
- [x] Script PowerShell de setup automático do ambiente (seção 1)
- [x] Instalação Git + Git LFS (seção 2)
- [x] Instalação Unity Hub + Unity LTS (seção 3)
- [x] Criação e configuração do projeto Unity (seção 4)
- [x] Estrutura de pastas completa (seção 4.2)
- [x] Configuração Git LFS + .gitignore (seção 5)
- [x] Instalação SpecKit (seção 6)
- [x] CLAUDE.md pronto (seção 7)
- [x] AGENTS.md pronto (seção 8)
- [x] constitution.md do SpecKit (seção 9)
- [x] Checklist de validação (seção 10)


---

## 0. Guia micro de configuração do ambiente

Esta seção é o passo a passo operacional para preparar a máquina do zero no Windows.

### 0.1 Premissas

| Item | Decisão |
|---|---|
| Sistema operacional | Windows 10/11 |
| Pasta padrão | `C:\dev\cindars-hope` |
| Engine | Unity LTS, template 2D URP |
| IDE | VS Code |
| Código | C# |
| Agente de código | Codex no VS Code + Claude como revisor/documentador |
| Sprite/Pixel art | Aseprite principal; Pixelorama/LibreSprite como fallback |
| Versionamento | Git + Git LFS |
| Spec-driven | GitHub SpecKit |

### 0.2 Preparar Windows

1. Criar a pasta base:

```powershell
mkdir C:\dev
mkdir C:\dev\cindars-hope
```

2. Abrir PowerShell como Administrador.
3. Validar `winget`:

```powershell
winget --version
```

4. Se `winget` não existir, instalar **App Installer** pela Microsoft Store.
5. Permitir execução do script local somente para esta sessão:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### 0.3 Instalar Git e Git LFS

```powershell
winget install --id Git.Git -e --source winget
winget install --id GitHub.GitLFS -e --source winget
```

Fechar e abrir o terminal novamente. Validar:

```powershell
git --version
git lfs version
git lfs install
```

### 0.4 Instalar VS Code e extensões

```powershell
winget install --id Microsoft.VisualStudioCode -e --source winget
```

Abrir novo terminal e instalar extensões:

```powershell
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension Unity.unity-debug
code --install-extension tobiah.unity-tools
code --install-extension kleber-swf.unity-code-snippets
code --install-extension eamodio.gitlens
```

Extensões opcionais:

```powershell
code --install-extension GitHub.copilot
code --install-extension GitHub.copilot-chat
```

### 0.5 Instalar Unity Hub e Unity LTS

1. Baixar Unity Hub pelo site oficial da Unity.
2. Instalar e logar com conta Unity.
3. Abrir **Installs → Install Editor**.
4. Selecionar a versão **Unity LTS mais recente**.
5. Marcar módulos:
   - Windows Build Support (IL2CPP)
   - WebGL Build Support, opcional
   - Visual Studio Code Editor
6. Não instalar Android/iOS agora.

### 0.6 Criar o projeto Unity

1. Unity Hub → **New Project**.
2. Template: **2D URP**.
3. Nome: `CindarsHope`.
4. Location: `C:\dev\cindars-hope`.
5. Abrir o projeto.
6. Confirmar que existe a pasta `Assets/` na raiz do repositório.

### 0.7 Configurar projeto Unity

Em **Project Settings → Player**:

| Campo | Valor |
|---|---|
| Company Name | Rafa ou nome do estúdio |
| Product Name | Cindar's Hope |
| Default Screen Width | 1280 |
| Default Screen Height | 720 |
| Fullscreen Mode | Windowed para desenvolvimento |

Em **Project Settings → Editor**:

- Asset Serialization: `Force Text`
- Version Control Mode: `Visible Meta Files`

Em **Project Settings → Input System Package**:

- Active Input Handling: `Input System Package (New)`

### 0.8 Instalar packages Unity

Abrir **Window → Package Manager** e instalar:

| Package | Uso |
|---|---|
| Input System | Controles |
| Cinemachine | Câmera 2D com follow |
| TextMeshPro | UI |
| 2D Tilemap Extras | Rule Tiles e Animated Tiles |
| SuperTiled2Unity | Importação de mapas Tiled, se usado |

### 0.9 Configurar Aseprite

**Decisão:** Aseprite é a ferramenta principal de sprites.

Instalação recomendada:

1. Comprar/instalar pela Steam ou pelo site oficial.
2. Abrir Aseprite.
3. Criar um arquivo de teste:
   - Sprite: 32x32px
   - Color Mode: RGBA
   - Background: Transparent
4. Ativar grid:
   - View → Grid → Show Grid
   - Grid size: 32x32 para tiles; 32x48 para personagem
5. Criar paleta do projeto com as cores base:
   - Fazenda: `#D4832A`, `#8B6914`, `#6B3A2A`, `#4A6741`
   - Cidade: `#4A5E7A`, `#7A8A9A`, `#D4A850`
   - Caverna: `#2A2A2A`, `#3A1F4A`, `#4A4A5A`
   - Outline: `#0A0A0A`
6. Exportar teste:
   - File → Export
   - Format: PNG
   - Sem scaling
   - Fundo transparente

Fallback gratuito/open-source:

- Pixelorama: bom para edição simples e prototipagem.
- LibreSprite: fork open-source do Aseprite antigo.

Regra de produção: qualquer ferramenta é aceita se exportar PNG/spritesheet padronizado para Unity.

### 0.10 Configurar importação pixel art no Unity

Para qualquer PNG:

| Campo | Valor |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single para ícones; Multiple para spritesheets |
| Pixels Per Unit | 32 |
| Filter Mode | Point |
| Compression | None |
| Generate Mip Maps | false |

Depois criar preset:

1. Selecionar um PNG.
2. Configurar campos acima.
3. Inspector → Preset icon → Save Current to Preset.
4. Nome: `PixelArt_Sprite`.
5. Project Settings → Preset Manager.
6. Adicionar preset para `TextureImporter` com filtro `t:Texture2D`.

### 0.11 Configurar Git LFS no repositório

Dentro de `C:\dev\cindars-hope`:

```powershell
git init
git checkout -b main
git lfs install
git lfs track "*.png"
git lfs track "*.aseprite"
git lfs track "*.ase"
git lfs track "*.wav"
git lfs track "*.mp3"
git lfs track "*.ogg"
git lfs track "*.unity"
git lfs track "*.prefab"
git lfs track "*.asset"
git lfs track "*.controller"
git add .gitattributes
git commit -m "chore: configurar git lfs"
```

### 0.12 Instalar Python e SpecKit

```powershell
winget install --id Python.Python.3.12 -e --source winget
python --version
pip install uv
uv --version
uv tool install specify-cli --from "git+https://github.com/github/spec-kit.git"
specify --version
```

Dentro da raiz do projeto:

```powershell
specify init .
```

Depois criar/validar:

```text
.specify/memory/constitution.md
.specify/memory/context.md
.github/prompts/
specs/
```

### 0.13 Criar arquivos de agente

Na raiz do repositório:

```text
CLAUDE.md
AGENTS.md
```

Ambos devem conter as regras invioláveis do projeto: sem `GameObject.Find()`, comunicação via `GameEventBus`, dados em ScriptableObject, unsubscribe obrigatório, commits em português e importação correta de sprites.

### 0.14 Smoke test obrigatório

Antes de iniciar a Fase 8:

1. Unity abre sem erros no Console.
2. `Assets/_Game/` existe com a estrutura padrão.
3. `PixelArt_Sprite` existe.
4. Um PNG 32x32 importado fica nítido, sem blur.
5. Git está inicializado.
6. Git LFS rastreia assets binários.
7. VS Code abre o projeto.
8. SpecKit responde `specify --version`.
9. Aseprite abre e exporta PNG transparente.
10. Commit inicial criado em português.

---

## 1. Script de Setup — setup_cindars_hope.ps1

Salvar como `setup_cindars_hope.ps1` e executar no PowerShell como Administrador.

> **Importante:** antes de executar, instalar manualmente o Unity Hub (ver seção 3) pois requer login. O script cuida do resto.

```powershell
# setup_cindars_hope.ps1
# Cindar's Hope — Setup completo do ambiente de desenvolvimento
# Executar como Administrador no PowerShell
# Pré-requisito: Unity Hub instalado e logado

param(
    [string]$ProjectPath = "C:\dev\cindars-hope",
    [string]$RepoUrl = ""  # preencher com URL do repo GitHub
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step { param($msg) Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok   { param($msg) Write-Host "    [OK] $msg" -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "    [AVISO] $msg" -ForegroundColor Yellow }

# ─── 1. Winget ───────────────────────────────────────────────────────────────
Write-Step "Verificando winget..."
if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
    Write-Error "winget não encontrado. Instale o App Installer pela Microsoft Store."
}
Write-Ok "winget disponível"

# ─── 2. Git ──────────────────────────────────────────────────────────────────
Write-Step "Instalando Git..."
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    winget install --id Git.Git -e --source winget --silent
    $env:PATH += ";C:\Program Files\Git\cmd"
    Write-Ok "Git instalado"
} else {
    Write-Ok "Git já instalado: $(git --version)"
}

# ─── 3. Git LFS ──────────────────────────────────────────────────────────────
Write-Step "Instalando Git LFS..."
if (-not (Get-Command git-lfs -ErrorAction SilentlyContinue)) {
    winget install --id GitHub.GitLFS -e --source winget --silent
    Write-Ok "Git LFS instalado"
} else {
    Write-Ok "Git LFS já instalado"
}
git lfs install
Write-Ok "Git LFS inicializado globalmente"

# ─── 4. Python + uv (para SpecKit) ───────────────────────────────────────────
Write-Step "Instalando Python e uv..."
if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    winget install --id Python.Python.3.12 -e --source winget --silent
    $env:PATH += ";$env:LOCALAPPDATA\Programs\Python\Python312\Scripts"
    Write-Ok "Python 3.12 instalado"
} else {
    Write-Ok "Python já instalado: $(python --version)"
}

if (-not (Get-Command uv -ErrorAction SilentlyContinue)) {
    pip install uv --quiet
    Write-Ok "uv instalado"
} else {
    Write-Ok "uv já disponível"
}

# ─── 5. VS Code Extensions ───────────────────────────────────────────────────
Write-Step "Instalando extensões do VS Code..."
if (Get-Command code -ErrorAction SilentlyContinue) {
    $extensions = @(
        "ms-dotnettools.csharp",           # C# (OmniSharp)
        "Unity.unity-debug",               # Unity Debugger
        "tobiah.unity-tools",              # Unity Tools
        "kleber-swf.unity-code-snippets",  # Unity Snippets
        "GitHub.copilot",                  # GitHub Copilot (Codex)
        "GitHub.copilot-chat",             # Copilot Chat
        "eamodio.gitlens"                  # GitLens
    )
    foreach ($ext in $extensions) {
        code --install-extension $ext --force 2>$null
        Write-Ok "Extensão: $ext"
    }
} else {
    Write-Warn "VS Code não encontrado no PATH. Instale manualmente e reexecute."
}

# ─── 6. Ferramentas de sprite ───────────────────────────────────────────────
Write-Step "Registrando ferramenta de sprites..."
Write-Warn "Aseprite é a ferramenta principal, mas a instalação é manual via Steam/site oficial."
Write-Warn "Fallback open-source opcional: Pixelorama ou LibreSprite."
Write-Warn "Após instalar, validar: abrir app, criar canvas 32x32, exportar PNG transparente."

# ─── 7. Criar estrutura do projeto ───────────────────────────────────────────
Write-Step "Criando estrutura de pastas em $ProjectPath ..."
$dirs = @(
    "docs",
    "specs\farm",
    "specs\cave",
    "specs\combat",
    "specs\craft",
    "specs\companion",
    "specs\npc",
    "specs\ui",
    ".specify\memory"
)
foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Force -Path "$ProjectPath\$dir" | Out-Null
}
Write-Ok "Estrutura de pastas criada"

# ─── 8. Git init + LFS ───────────────────────────────────────────────────────
Write-Step "Inicializando repositório Git..."
Set-Location $ProjectPath
if (-not (Test-Path ".git")) {
    git init
    git checkout -b main
    Write-Ok "Git init concluído"
} else {
    Write-Ok "Repositório já inicializado"
}

# Configurar LFS
$lfsPatterns = @("*.png", "*.jpg", "*.jpeg", "*.gif", "*.psd",
                 "*.aseprite", "*.ase",
                 "*.wav", "*.mp3", "*.ogg",
                 "*.fbx", "*.obj",
                 "*.unity", "*.unitypackage", "*.asset",
                 "*.prefab", "*.mat", "*.controller")
foreach ($pattern in $lfsPatterns) {
    git lfs track $pattern 2>$null
}
Write-Ok "Git LFS configurado para assets binários"

# ─── 9. Copiar arquivos de configuração ──────────────────────────────────────
Write-Step "Criando arquivos de configuração..."

# .gitignore para Unity
$gitignore = @"
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
crashlytics-build.properties
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj

# VS Code
.vscode/*
!.vscode/settings.json
!.vscode/extensions.json

# OS
.DS_Store
Thumbs.db
*.bak

# Saves de desenvolvimento
AppData/LocalLow/<Company>/Cindar's Hope/saves/  # gerado via Application.persistentDataPath

# SpecKit output temporário
.specify/cache/
"@
$gitignore | Out-File -FilePath ".gitignore" -Encoding UTF8
Write-Ok ".gitignore criado"

# Copiar docs de referência
$docsSource = @(
    "GDD_v2.6.md",
    "ARCH_fase4_v2.2.md",
    "FASE5_ambiente_v1.2.md"
)
foreach ($doc in $docsSource) {
    if (Test-Path "$PSScriptRoot\$doc") {
        Copy-Item "$PSScriptRoot\$doc" "docs\" -Force
        Write-Ok "Copiado: $doc → docs\"
    }
}

# ─── 10. SpecKit ─────────────────────────────────────────────────────────────
Write-Step "Instalando SpecKit..."
try {
    uv tool install specify-cli --from "git+https://github.com/github/spec-kit.git" 2>$null
    Write-Ok "SpecKit (specify-cli) instalado"
    Write-Warn "Execute 'specify init .' dentro do projeto Unity após criá-lo"
} catch {
    Write-Warn "Falha ao instalar SpecKit automaticamente. Execute manualmente:"
    Write-Warn "  uv tool install specify-cli --from git+https://github.com/github/spec-kit.git"
}

# ─── 11. Primeiro commit ─────────────────────────────────────────────────────
Write-Step "Commit inicial..."
git add .
git commit -m "feat: setup inicial do projeto Cindar's Hope"
Write-Ok "Commit inicial criado"

if ($RepoUrl -ne "") {
    git remote add origin $RepoUrl
    git push -u origin main
    Write-Ok "Push para $RepoUrl concluído"
} else {
    Write-Warn "RepoUrl não informado. Configure o remote manualmente:"
    Write-Warn "  git remote add origin <url-do-repo>"
    Write-Warn "  git push -u origin main"
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Setup concluído!" -ForegroundColor Green
Write-Host "  Próximos passos:" -ForegroundColor Green
Write-Host "  1. Criar projeto Unity (ver FASE5_ambiente_v1.2.md seção 3)" -ForegroundColor Green
Write-Host "  2. Rodar 'specify init .' dentro de Assets/" -ForegroundColor Green
Write-Host "  3. Criar branch dev: git checkout -b dev" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
```

---

## 2. Git — Instalação Manual (se necessário)

Se o winget falhar:
1. Baixar em https://git-scm.com/download/win
2. Instalar com opções padrão + "Git from the command line and also from 3rd-party software"
3. Abrir novo terminal e verificar: `git --version`
4. Instalar Git LFS: https://git-lfs.com → instalar → `git lfs install`

---

## 3. Unity Hub + Unity LTS

**Instalação manual obrigatória** (requer login com conta Unity):

1. Baixar Unity Hub em https://unity.com/download
2. Instalar Unity Hub
3. Fazer login com conta Unity (criar se não tiver — gratuito para Personal)
4. No Unity Hub: **Installs → Install Editor**
5. Selecionar a versão **LTS mais recente** (verificar em https://unity.com/releases/lts)
6. Módulos a incluir na instalação:
   - ✅ Windows Build Support (IL2CPP)
   - ✅ WebGL Build Support (opcional, para futuro)
   - ✅ Visual Studio Code Editor
   - ❌ Android / iOS (não precisa agora)

---

## 4. Criação do Projeto Unity

### 4.1 Criar projeto

1. Unity Hub → **New Project**
2. Template: **2D (URP)** — não usar "2D Core" simples
3. Nome: `CindarsHope`
4. Location: `C:\dev\cindars-hope\` (mesmo diretório do repo Git)
5. Clicar em **Create project**

### 4.2 Packages a instalar (Package Manager)

Abrir **Window → Package Manager** e instalar:

| Package | Source | Uso |
|---|---|---|
| Input System | Unity Registry | Controles de teclado/gamepad |
| Cinemachine | Unity Registry | Câmera 2D com follow suave |
| TextMeshPro | Unity Registry | Textos de UI com qualidade |
| 2D Extras | Unity Registry | Rule Tiles para tilemaps automáticos |
| SuperTiled2Unity | OpenUPM | Importar mapas do Tiled Editor |

**Instalar SuperTiled2Unity via OpenUPM:**
```
Window → Package Manager → + → Add package by name
Nome: com.seanba.super-tiled2unity
```

### 4.3 Configurações do projeto

**Project Settings → Player:**
- Company Name: [seu nome]
- Product Name: Cindar's Hope
- Default Icon: deixar vazio por enquanto

**Project Settings → Player → Resolution:**
- Default Screen Width: 1280
- Default Screen Height: 720
- Fullscreen Mode: Windowed (para desenvolvimento)

**Project Settings → Graphics:**
- Render Pipeline Asset: já vem configurado no URP template

**Project Settings → Input System:**
- Active Input Handling: Input System Package (New)

### 4.4 Configurar TextureImporterPreset (CRÍTICO)

Cria um preset de importação de sprite para garantir que todos os sprites pixel art fiquem corretos:

1. Selecionar qualquer PNG no projeto
2. No Inspector: configurar assim:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single (mudar para Multiple no Sprite Editor quando necessário)
   - Pixels Per Unit: 32
   - Filter Mode: **Point** ← não esquecer, é o que evita blur
   - Compression: **None** ← não esquecer
   - Max Size: 2048
   - Generate Mip Maps: **false**
3. No topo do Inspector: clicar no ícone de preset → **Save Current to Preset**
4. Nomear: `PixelArt_Sprite`
5. Ir em **Edit → Project Settings → Preset Manager**
6. Adicionar o preset para tipo `TextureImporter`
7. Filter: `t:Texture2D` → agora todo PNG importado usa essas configurações automaticamente

### 4.5 Sorting Layers

**Edit → Project Settings → Tags and Layers → Sorting Layers:**

Adicionar nesta ordem (ordem = profundidade visual):
1. Background
2. Ground
3. Decoration
4. Characters
5. TreeTops
6. Items
7. UI_World
8. UI

### 4.6 Estrutura de pastas dentro de Assets/

Criar manualmente ou via script no Unity Console:

```csharp
// CreateFolderStructure.cs — colocar em Assets/Editor/ e executar uma vez
using UnityEditor;
using System.IO;

public class CreateFolderStructure
{
    [MenuItem("Cindar's Hope/Criar Estrutura de Pastas")]
    public static void Create()
    {
        string[] folders = {
            "Assets/_Game/Data/Items",
            "Assets/_Game/Data/Seeds",
            "Assets/_Game/Data/Creatures",
            "Assets/_Game/Data/Recipes",
            "Assets/_Game/Data/Companions",
            "Assets/_Game/Data/NPCs",
            "Assets/_Game/Data/Workshops",
            "Assets/_Game/Data/Biomes",
            "Assets/_Game/Data/LootTables",
            "Assets/_Game/Data/Quests",
            "Assets/_Game/Data/Config",
            "Assets/_Game/Data/Dialogues",
            "Assets/_Game/Scripts/Core/Events",
            "Assets/_Game/Scripts/Player",
            "Assets/_Game/Scripts/Farm",
            "Assets/_Game/Scripts/Cave",
            "Assets/_Game/Scripts/Combat",
            "Assets/_Game/Scripts/Craft",
            "Assets/_Game/Scripts/Companion",
            "Assets/_Game/Scripts/NPC",
            "Assets/_Game/Scripts/UI",
            "Assets/_Game/Scripts/Utils",
            "Assets/_Game/Scripts/Save",
            "Assets/_Game/Scenes",
            "Assets/_Game/Prefabs/Characters",
            "Assets/_Game/Prefabs/Creatures",
            "Assets/_Game/Prefabs/Farm",
            "Assets/_Game/Prefabs/UI",
            "Assets/_Game/Prefabs/VFX",
            "Assets/_Game/Sprites/Characters",
            "Assets/_Game/Sprites/Creatures",
            "Assets/_Game/Sprites/Items",
            "Assets/_Game/Sprites/Tilesets",
            "Assets/_Game/Sprites/UI",
            "Assets/_Game/Sprites/VFX",
            "Assets/_Game/Sprites/Placeholders",
            "Assets/_Game/Animations/Characters",
            "Assets/_Game/Animations/Creatures",
            "Assets/_Game/Tilemaps",
            "Assets/_Game/Audio",
            "Assets/ThirdParty",
            "Assets/Editor"
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(folder + "/.gitkeep", "");
            }
        }
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Estrutura de pastas criada com sucesso!");
    }
}
```

---

## 5. Git LFS — Configuração no Projeto Unity

Após criar o projeto Unity, dentro da pasta do projeto:

```bash
# Verificar que .gitattributes foi criado pelo setup script
cat .gitattributes

# Se precisar adicionar padrões manualmente:
git lfs track "*.png"
git lfs track "*.aseprite"
git lfs track "*.wav"
git lfs track "*.mp3"
git lfs track "*.ogg"
git lfs track "*.unity"
git lfs track "*.prefab"
git lfs track "*.mat"
git lfs track "*.controller"
git lfs track "*.asset"

# Branches de trabalho
git checkout -b dev    # branch de desenvolvimento
# main = sempre estável e testado
# dev = desenvolvimento ativo
# feature/nome = novas features (merge para dev)
```

**Convenção de commits (português):**
```
feat: adicionar sistema de plantio
fix: corrigir bug de colheita dupla
refactor: extrair lógica de fome para HungerSystem
docs: atualizar CLAUDE.md com novos padrões
chore: configurar Git LFS para sprites
test: adicionar testes do TimeManager
```

---

## 6. SpecKit — Setup

Após instalar pelo script (ou manualmente):

```bash
# Dentro da pasta raiz do projeto (onde está Assets/)
specify init .

# Escolher integração: GitHub Copilot + VS Code
# Isso cria:
#   .github/prompts/speckit.*.md
#   .specify/memory/
```

**Após o init, criar manualmente:**

```bash
# .specify/memory/context.md
# Aponta para os documentos de referência do projeto
```

O conteúdo do `constitution.md` está na seção 9 deste documento.

---

## 7. CLAUDE.md

Criar na raiz do repositório (mesmo nível de `Assets/`):

```markdown
# CLAUDE.md — Cindar's Hope

## Contexto do projeto
Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, região Dornecia.
Arte: Aseprite como ferramenta principal, sprites 32x32px, resolução 1280x720.
IA de arte: DALL-E 3 via ChatGPT Plus + retoque manual no Aseprite.
Geração de código: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify→Plan→Tasks→Implement.

## Documentos de referência (ler antes de qualquer tarefa)
- docs/design/GDD_v2.6.md          — design completo do jogo
- docs/architecture/ARCH_fase4_v2.2.md   — arquitetura técnica, padrões, eventos
- docs/operations/FASE5_ambiente_v1.2.md     — setup do ambiente (este contexto)
- specs/                     — specs por sistema (geradas na Fase 7)

## Modelo de LLM padrão
claude-sonnet-4-6

## Regras INVIOLÁVEIS de código

1. NUNCA usar GameObject.Find() ou FindObjectOfType()
   → Usar injeção via [SerializeField] no Inspector ou eventos
2. NUNCA criar comunicação direta entre sistemas
   → Sempre via GameEventBus.Publish() e Subscribe()
3. NUNCA hardcodar dados de jogo (HP, dano, preços, nomes)
   → Sempre em ScriptableObject no Assets/_Game/Data/
4. SEMPRE fazer Unsubscribe em OnDisable ou OnDestroy
   → void OnDisable() => GameEventBus.Unsubscribe<XEvent>(OnX);
5. NUNCA escrever lógica de negócio em MonoBehaviour
   → MonoBehaviour só faz ponte entre Unity e classes C# puras
6. SEMPRE prefixar ScriptableObjects: ItemDataSO, SeedDataSO, etc.
7. SEMPRE prefixar eventos: DayStartedEvent, PlayerDiedEvent, etc.
8. SEMPRE commits em português
9. NUNCA implementar feature sem spec aprovada (Fase 7+)
10. Sprites: SEMPRE importar com Filter Mode Point + Compression None

## Convenções de nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Classes | PascalCase | PlayerController, FarmSystem |
| Eventos | [Acao][Substantivo]Event | DayStartedEvent, ItemCraftedEvent |
| ScriptableObjects | [Tipo]DataSO | ItemDataSO, SeedDataSO |
| Prefabs | [Categoria]_[Nome] | Creature_Slime, NPC_Brumdar |
| Sprites | [Cat]_[Nome]_[Tamanho].png | Item_SwordIron_32x32.png |
| Variáveis private | _camelCase | _currentHP, _isGrounded |
| Variáveis public/[SerializeField] | PascalCase | MaxHP, MoveSpeed |
| Constantes | UPPER_SNAKE | MAX_COMPANIONS, BASE_HUNGER_RATE |
| Cenas | PascalCase | FarmScene, TownScene, CaveScene |

## Estrutura de pastas

```
Assets/_Game/
├── Data/          ← ScriptableObjects (nunca .cs aqui)
├── Scripts/
│   ├── Core/      ← GameEventBus, TimeManager, SaveManager, todos os Events/
│   ├── Player/
│   ├── Farm/
│   ├── Cave/
│   ├── Combat/
│   ├── Craft/
│   ├── Companion/
│   ├── NPC/
│   ├── UI/
│   ├── Save/
│   └── Utils/
├── Scenes/
├── Prefabs/
├── Sprites/
│   └── Placeholders/  ← retângulos coloridos para MVP
├── Animations/
├── Tilemaps/
└── Audio/         ← vazio até Fase 10 (polish)
```

## Padrão de MonoBehaviour mínimo

```csharp
public class NomeDoSistema : MonoBehaviour
{
    [SerializeField] private NomeDataSO _data; // dados do SO

    void OnEnable()
    {
        GameEventBus.Subscribe<XEvent>(OnX);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<XEvent>(OnX);
    }

    private void OnX(XEvent e)
    {
        // reagir ao evento
    }
}
```

## Padrão de ScriptableObject mínimo

```csharp
[CreateAssetMenu(fileName = "X_NomeDoItem", menuName = "CindarsHope/Categoria/Tipo")]
public class TipoDataSO : ScriptableObject
{
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon; // 32x32, Filter Point, Compression None
}
```

## Paleta de cores (hex) — referenciar em todos os sprites

Fazenda: laranja #D4832A, bronze #8B6914, terra #6B3A2A, verde musgo #4A6741
Cidade: azul ardósia #4A5E7A, pedra #7A8A9A, âmbar de lanterna #D4A850
Caverna: cinza carvão #2A2A2A, roxo escuro #3A1F4A, cinza úmido #4A4A5A
Outline padrão de personagens: #0A0A0A (1px)

## Placeholder visual (MVP — Fase 8)

Para o MVP, usar retângulos coloridos:
- Jogador: cubo azul 32x48
- Inimigo: cubo vermelho 32x32
- Planta: cubo verde 32x32 (muda de tom conforme estágio)
- Tile de chão: cinza claro #CCCCCC
- Tile de parede: cinza escuro #555555
- Item dropado: cubo amarelo 16x16

Criar em: Assets/_Game/Sprites/Placeholders/
```

---

## 8. AGENTS.md

Idêntico ao CLAUDE.md — criar na raiz com o mesmo conteúdo. Serve para agentes diferentes (Copilot, Codex, outros).

```bash
# Na raiz do projeto
cp CLAUDE.md AGENTS.md
```

---

## 9. constitution.md (SpecKit)

Criar em `.specify/memory/constitution.md`:

```markdown
# Cindar's Hope — Constitution (SpecKit)

Estas regras são verificáveis e invioláveis. O agente deve self-review
contra esta constitution antes de qualquer output de código ou spec.

## Regras de código (verificáveis)

- [ ] Todo dado de jogo ESTÁ em ScriptableObject (não em MonoBehaviour)
- [ ] Toda comunicação entre sistemas USA GameEventBus
- [ ] Nenhum script USA GameObject.Find() ou FindObjectOfType()
- [ ] Todo Subscribe TEM Unsubscribe correspondente em OnDisable/OnDestroy
- [ ] Commits ESTÃO em português
- [ ] Nenhuma feature FOI implementada sem spec aprovada

## Regras de arte (verificáveis)

- [ ] Sprites TÊM Filter Mode: Point
- [ ] Sprites TÊM Compression: None
- [ ] Sprites SÃO 32x32px (ou múltiplos: 32x48, 64x64)
- [ ] Paleta ESTÁ dentro das cores definidas no CLAUDE.md
- [ ] Outline de personagens É 1px #0A0A0A

## Regras de spec (verificáveis)

- [ ] Spec foca em WHAT e WHY, não em HOW
- [ ] Edge cases ESTÃO documentados
- [ ] Critérios de aceite SÃO testáveis

## O que NÃO é verificável (não incluir na constitution)

- "Código de alta qualidade" → vago, não verificável
- "Boa performance" → sem métrica, não verificável
- "UI intuitiva" → subjetivo, não verificável
```

---

## 10. Checklist de Validação — Fase 5 concluída

Marcar cada item antes de avançar para a Fase 6:

**Git:**
- [ ] `git --version` retorna versão instalada
- [ ] `git lfs version` retorna versão instalada
- [ ] `.gitattributes` existe com os padrões de LFS
- [ ] `.gitignore` existe com padrões Unity
- [ ] Branch `main` e `dev` criadas
- [ ] Commit inicial feito

**Unity:**
- [ ] Unity Hub instalado e com Unity LTS ativo
- [ ] Projeto `CindarsHope` criado com template 2D (URP)
- [ ] Packages instalados: Input System, Cinemachine, TextMeshPro, 2D Extras
- [ ] TextureImporterPreset `PixelArt_Sprite` criado e configurado como padrão
- [ ] Sorting Layers criados na ordem correta
- [ ] Estrutura de pastas criada via script Unity

**Ambiente:**
- [ ] VS Code com extensões C# e Unity instaladas
- [ ] Codex acessível via VS Code
- [ ] Aseprite instalado e abrindo; fallback Pixelorama/LibreSprite validado se necessário
- [ ] SpecKit instalado (`specify --version`)

**Documentos:**
- [ ] `CLAUDE.md` na raiz do projeto
- [ ] `AGENTS.md` na raiz do projeto
- [ ] `.specify/memory/constitution.md` criado
- [ ] `docs/` com GDD e ARCH copiados

---

## 11. Próximos Passos — Fase 6 (Quebra de Histórias)

Com o ambiente pronto, a Fase 6 quebra o GDD em user stories por épico:

**Épicos previstos:**
- FARM-001 a FARM-XXX → sistema de fazenda (plantar, colher, pesca, árvores)
- CITY-001 a CITY-XXX → cidade, NPCs, comércio
- CAVE-001 a CAVE-XXX → caverna procedural, combate, checkpoints
- CRAFT-001 a CRAFT-XXX → workshops, crafting
- COMP-001 a COMP-XXX → companions, jobs, morte
- CHAR-001 a CHAR-XXX → personagem, atributos, skill tree
- SAVE-001 a SAVE-XXX → sistema de save

**Formato de cada história:**
```
ID: FARM-001
Como: jogador
Quero: plantar uma semente no canteiro da fazenda
Para que: ela cresça ao longo dos dias e eu possa colhê-la
Critérios de aceite:
  - Dado que tenho uma semente no inventário
  - Quando seleciono o canteiro vazio e uso a semente
  - Então a semente é plantada e aparece no estágio 0
  - E o canteiro mostra o sprite correto da semente no estágio 0
  - E após X dias (conforme SeedDataSO.GrowthDays) a planta está pronta
Prioridade: Must Have
```

---

*Fase 5 — Versão 1.1. Ambiente Windows detalhado em nível micro, com Aseprite como ferramenta principal de sprites e Pixelorama/LibreSprite como fallback. Próxima fase prática: Fase 8 — Implementação do MVP Fazenda.*


---

## 11. Setup micro — Codex para implementação local

### 11.1 Objetivo

Preparar o ambiente para que o Codex consiga alterar código com segurança, rodar testes e respeitar as regras do projeto.

### 11.2 Checklist antes de abrir uma tarefa no Codex

- [ ] Repositório aberto no VS Code na raiz correta, no mesmo nível de `Assets/`.
- [ ] Unity abre o projeto sem erros no Console.
- [ ] Branch de trabalho criada: `feature/fase8-pr-xxx-nome`.
- [ ] `CLAUDE_v1.2.md` e `AGENTS.md` estão na raiz.
- [ ] Docs atualizados em `/docs`.
- [ ] Specs atualizadas em `/specs`.
- [ ] Git limpo antes da tarefa: `git status` sem mudanças não commitadas.
- [ ] O prompt do Codex cita exatamente a spec e o PR alvo.

### 11.3 Configuração recomendada de permissões

Para tarefas iniciais:

- Permitir leitura/escrita no workspace.
- Não permitir comandos destrutivos sem revisão.
- Não permitir instalar dependências externas sem aprovação.
- Não permitir acesso irrestrito à rede para tarefas de código do MVP.

### 11.4 Prompt base para qualquer tarefa Codex

```md
Leia primeiro:
- CLAUDE_v1.2.md
- docs/design/GDD_v2.6.md
- docs/architecture/ARCH_fase4_v2.2.md
- docs_old/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
- docs_old/FASE8_EXECUTION_PLAN_CODEX_v1.0.md

Implemente somente o PR: <ID e nome>.

Arquivos permitidos:
- <lista>

Arquivos proibidos:
- <lista>

Regras:
- Não implemente V2/FULL.
- Não use GameObject.Find, FindObjectOfType ou FindObjectsByType em runtime.
- Não salve referências Unity no JSON.
- Não use StreamingAssets para save.
- Dados de jogo ficam em ScriptableObject.
- Comunicação entre sistemas via GameEventBus.

Ao final, entregue:
- arquivos alterados;
- teste manual;
- riscos/pendências;
- sugestão do próximo PR.
```

### 11.5 Validação depois de cada PR Codex

```powershell
git diff --stat
git diff
```

No Unity:

- Abrir Console.
- Corrigir erros de compilação antes de seguir.
- Rodar cena alvo.
- Executar teste manual definido.
- Fazer commit pequeno em português.

Exemplo:

```bash
git add .
git commit -m "feat: adicionar contrato inicial de eventos core"
```

---

## 12. Setup micro — ferramentas de sprites e IA

### 12.1 Camadas de ferramenta

| Camada | Ferramenta | Obrigatória? | Uso |
|---|---|---:|---|
| Conceito rápido | ChatGPT/DALL-E | Não | Ideias, ícones simples, variações |
| Geração pixel-art focada | PixelLab | Opcional | Sprites, personagens, tilesets, variações 32x32/32x48 |
| Consistência em lote / modelo próprio | Scenario | Opcional | Art bible, assets em massa, estilo persistente |
| Edição final | Aseprite | Sim | Limpeza, paleta, outline, animação, export |
| Fallback gratuito | Pixelorama/LibreSprite | Opcional | Substituir Aseprite se necessário |

### 12.2 Estrutura local de arte

Criar fora de `Assets/` uma pasta de trabalho para arquivos brutos:

```text
art_workbench/
├── references/
├── ai_raw/
├── aseprite_source/
├── exports_png/
├── rejected/
└── notes/
```

Somente entram em `Assets/_Game/Sprites/` arquivos PNG aprovados.
Arquivos `.aseprite/.ase` entram no Git LFS.

### 12.3 Checklist de sprite aprovado

- [ ] Tamanho correto: 32x32, 32x48, 64x64 ou outro definido na spec.
- [ ] Fundo transparente.
- [ ] Sem antialiasing/blur.
- [ ] Outline consistente quando aplicável.
- [ ] Paleta próxima da paleta do projeto.
- [ ] Silhueta legível em zoom 1x.
- [ ] Nome segue convenção.
- [ ] Import no Unity usa `PixelArt_Sprite`.
- [ ] Sprite referenciado pelo ScriptableObject correto.

---

## 13. Smoke test expandido antes da Fase 8

Além do checklist anterior:

- [ ] `Application.persistentDataPath` foi identificado no Console com um log temporário.
- [ ] Pasta `saves/` é criada dentro de `persistentDataPath`, não dentro de `StreamingAssets`.
- [ ] Um `ItemDataSO` de teste foi criado e referenciado por um registry.
- [ ] Um evento `DayStartedEvent` foi publicado e recebido em teste simples.
- [ ] Codex conseguiu alterar um arquivo pequeno e a mudança foi revisada por diff.
- [ ] Aseprite exportou um PNG 32x32 com transparência.
- [ ] Unity importou o PNG sem blur.
- [ ] Git LFS rastreia `.png`, `.aseprite`, `.ase`, `.unity`, `.prefab`, `.asset`.


