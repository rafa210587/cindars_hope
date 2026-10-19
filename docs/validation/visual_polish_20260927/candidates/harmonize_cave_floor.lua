local source = assert(app.params.source, "source required")
local output = assert(app.params.output, "output required")
local dr = tonumber((assert(app.params.dr, "dr required")))
local dg = tonumber((assert(app.params.dg, "dg required")))
local db = tonumber((assert(app.params.db, "db required")))
assert(source:lower() ~= output:lower(), "output must not replace source")

local sprite = assert(app.open(source), "cannot open source")
assert(sprite.width == 128 and sprite.height == 128 and sprite.colorMode == ColorMode.RGB,
  "expected 128x128 RGB cave tile")
assert(#sprite.frames == 1 and #sprite.layers == 1, "expected flat single-frame source")
local base = sprite.layers[1]
local cel = assert(base:cel(sprite.frames[1]), "source cel missing")
assert(cel.position.x == 0 and cel.position.y == 0, "source cel moved")

local corrected = Image(128, 128, sprite.colorMode)
local function clamp(value)
  return math.max(0, math.min(255, value))
end
for y = 0, 127 do
  for x = 0, 127 do
    local pixel = cel.image:getPixel(x, y)
    local alpha = app.pixelColor.rgbaA(pixel)
    corrected:drawPixel(x, y, app.pixelColor.rgba(
      clamp(app.pixelColor.rgbaR(pixel) + dr),
      clamp(app.pixelColor.rgbaG(pixel) + dg),
      clamp(app.pixelColor.rgbaB(pixel) + db), alpha))
  end
end

app.transaction(function()
  base.name = "Original floor tile (preserved)"
  base.isVisible = false
  local polish = sprite:newLayer()
  polish.name = string.format("Cave palette harmony %+dR %+dG %+dB", dr, dg, db)
  sprite:newCel(polish, sprite.frames[1], corrected, Point(0, 0))
end)
sprite:saveAs(output)
sprite:close()

local reopened = assert(app.open(output), "cannot reopen candidate")
assert(reopened.width == 128 and reopened.height == 128 and #reopened.layers == 2,
  "candidate structure changed")
reopened:close()
