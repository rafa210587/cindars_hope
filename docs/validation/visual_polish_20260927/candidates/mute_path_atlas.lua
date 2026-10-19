local source = assert(app.params.source, "source required")
local output = assert(app.params.output, "output required")
assert(source:lower() ~= output:lower(), "output must not replace source")

local sprite = assert(app.open(source), "cannot open source")
assert(sprite.width == 64 and sprite.height == 64 and sprite.colorMode == ColorMode.RGB,
  "expected 64x64 RGB path atlas")
assert(#sprite.frames == 1 and #sprite.layers == 1, "expected one flat source frame")
local original = sprite.layers[1]
local cel = assert(original:cel(sprite.frames[1]), "source cel missing")
assert(cel.position.x == 0 and cel.position.y == 0, "source cel moved")

local corrected = Image(64, 64, sprite.colorMode)
local function clamp(value)
  return math.max(0, math.min(255, value))
end
for y = 0, 63 do
  for x = 0, 63 do
    local pixel = cel.image:getPixel(x, y)
    corrected:drawPixel(x, y, app.pixelColor.rgba(
      clamp(app.pixelColor.rgbaR(pixel) - 12),
      clamp(app.pixelColor.rgbaG(pixel) - 5),
      clamp(app.pixelColor.rgbaB(pixel) + 12),
      app.pixelColor.rgbaA(pixel)))
  end
end

app.transaction(function()
  original.name = "Original farm path atlas (preserved)"
  original.isVisible = false
  local polish = sprite:newLayer()
  polish.name = "Muted warm earth -12R -5G +12B"
  sprite:newCel(polish, sprite.frames[1], corrected, Point(0, 0))
end)
sprite:saveAs(output)
sprite:close()

local reopened = assert(app.open(output), "cannot reopen candidate")
assert(reopened.width == 64 and reopened.height == 64 and #reopened.layers == 2,
  "candidate structure changed")
reopened:close()
