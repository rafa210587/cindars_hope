# CLI and Lua execution

Use this reference for repeatable candidate edits. Check installed `--help` and version before adapting these hypothetical examples; paths and edit coordinates are task inputs, not project defaults.

## Locate and preserve

Use `Get-Command aseprite -ErrorAction SilentlyContinue` or the known installation path; do not recursively search the whole disk. Set absolute paths to the observed executable, approved source, new candidate and reviewed Lua file. Create a fresh task output directory if needed. Do not place candidates over live imported assets.

```powershell
$asepriteExe = 'C:\Program Files\Aseprite\Aseprite.exe' # replace with observed path
$sourceArt = 'D:\Art\approved\prop.aseprite'
$candidateArt = 'D:\Art\review\prop-v01.aseprite'
$editScript = 'D:\Art\review\edit.lua'
if (!(Test-Path -LiteralPath $asepriteExe)) { throw 'Aseprite unavailable' }
if (Test-Path -LiteralPath $candidateArt) { throw 'Choose a new candidate path' }
& $asepriteExe --version
& $asepriteExe --help
$baselineHash = (Get-FileHash -LiteralPath $sourceArt -Algorithm SHA256).Hash
& $asepriteExe --batch --script-param "source=$sourceArt" --script-param "output=$candidateArt" --script $editScript
if ($LASTEXITCODE -ne 0) { throw 'Aseprite candidate creation failed' }
if (!(Test-Path -LiteralPath $candidateArt)) { throw 'Candidate missing' }
if ((Get-FileHash -LiteralPath $sourceArt -Algorithm SHA256).Hash -ne $baselineHash) {
    throw 'Source changed; investigate before continuing'
}
```

Pass arguments directly through PowerShell's call operator; do not build an expression string or use `Invoke-Expression`. Review scripts before running: Lua is code with filesystem access. Capture errors and resulting files; an exit code alone does not prove a useful edit.

## Minimal layered edit

This hypothetical RGB-only script adds one opaque pixel on an independent correction layer in frame 1. Replace the coordinate/color with a visually justified correction; it is a smoke example, not an art recipe. For indexed sprites, preserve palette indices/transparency and implement a matching color-mode operation; do not silently convert to RGB. Linked cels, groups, tilemaps and per-frame palettes require explicit handling when affected.

```lua
local source = assert(app.params.source, "source required")
local output = assert(app.params.output, "output required")
assert(source:lower() ~= output:lower(), "separate output required")
local sprite = assert(app.open(source), "cannot open source")
assert(sprite.colorMode == ColorMode.RGB, "example requires RGB")
local x, y = 2, 2 -- hypothetical; select after viewing source
assert(x >= 0 and y >= 0 and x < sprite.width and y < sprite.height)
app.transaction(function()
  local corrections = sprite:newLayer()
  corrections.name = "Candidate correction"
  local patch = Image(sprite.width, sprite.height, sprite.colorMode)
  patch:clear()
  patch:drawPixel(x, y, app.pixelColor.rgba(110, 80, 50, 255))
  sprite:newCel(corrections, sprite.frames[1], patch, Point(0, 0))
end)
sprite:saveAs(output)
sprite:close()
local reopened = assert(app.open(output), "cannot reopen candidate")
assert(reopened.layers[#reopened.layers].name == "Candidate correction")
reopened:close()
```

An overlay can add/cover pixels, but cannot erase underlying opaque pixels. For silhouette removal, duplicate the affected layer/cel, preserve the baseline hidden, then modify the duplicate; account for cel-local coordinates and linked cel sharing. Do not paint a fake checkerboard as transparency.

## Review export

For existing cels, pixel coordinates are local to the cel image: account for `cel.position`
and observed image bounds when mapping canvas coordinates. `Image:drawPixel()` does not create
undo history by itself; wrapping direct image writes in `app.transaction` does not change that.
For undoable edits to an existing cel, clone its image, modify the clone and assign it through
the documented cel API inside the transaction. Recheck intended linked-cel sharing. The example
above draws a new image before attaching it and does not depend on undo of direct pixel writes.

For a simple all-frame review strip, use fresh output paths:

```powershell
$reviewSheet = 'D:\Art\review\prop-v01-strip.png'
$reviewData = 'D:\Art\review\prop-v01-strip.json'
if ((Test-Path -LiteralPath $reviewSheet) -or (Test-Path -LiteralPath $reviewData)) {
    throw 'Choose new review output paths'
}
& $asepriteExe --batch --list-layers --list-tags --list-slices $candidateArt --sheet-type horizontal --sheet $reviewSheet --format json-array --data $reviewData
if ($LASTEXITCODE -ne 0) { throw 'Review export failed' }
```

This strip is review-only; it does not change the production sheet contract. Do not use `--all-layers` when hidden baseline layers would contaminate the preview. Avoid trim, duplicate merging and empty-frame skipping unless explicitly required. CLI options can depend on order. Check exported JSON and reopened native data; JSON is not a full-fidelity substitute for the layered source or Unity metadata.

## Official references

Verified 2026-09-09; local runtime execution must be reported separately. [CLI](https://www.aseprite.org/docs/cli/) documents ordered batch/script parameters and sheet export. [App API](https://www.aseprite.org/api/app) documents parameters, opening and transactions. [Sprite API](https://www.aseprite.org/api/sprite) documents layers, cels and saving. [Image API](https://www.aseprite.org/api/image) documents pixel operations. Consult only the API needed by the actual edit and check installed-version compatibility.
