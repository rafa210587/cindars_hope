-- Builds editable NpcWalkAnimator candidates from the accepted 5x5 PNG.
-- Each frame is a 150x150 native cell; no trim/repack/rescale is performed.
local source = assert(app.params.source, "source required")
local output = assert(app.params.output, "output required")
local id = assert(app.params.id, "id required")
local src = assert(app.open(source), "cannot open source PNG")
assert(src.width == 750 and src.height == 750, "expected 750x750 source sheet")
local srcImage = src.cels[1].image
local spr = Sprite(150, 150, ColorMode.RGB)
spr.filename = output
local render = spr.layers[1]
render.name = "Approved Detailed Render"
local notes = spr:newLayer()
notes.name = "Identity and Direction Notes"
local directions = { "down", "downleft", "right", "up", "upleft" }
for n = 1, 25 do
  local row = math.floor((n - 1) / 5)
  local col = (n - 1) % 5
  local frame = (n == 1) and spr.frames[1] or spr:newEmptyFrame()
  local cell = Image(150, 150, ColorMode.RGB)
  cell:clear()
  cell:drawImage(srcImage, Point(-col * 150, -row * 150))
  spr:newCel(render, frame, cell, Point(0, 0))
  frame.duration = (col == 0) and 0.14 or 0.10
end
for row = 0, 4 do
  local tag = spr:newTag(row * 5 + 1, row * 5 + 5)
  tag.name = directions[row + 1]
  tag.aniDir = AniDir.FORWARD
end
local meta = spr:newLayer()
meta.name = "Contract: 5x5 / BottomCenter / PPU234"
spr:saveAs(output)
src:close()
spr:close()
