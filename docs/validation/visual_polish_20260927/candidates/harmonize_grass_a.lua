local source = assert(app.params.source, "source required")
local output = assert(app.params.output, "output required")
assert(source:lower() ~= output:lower(), "output must not replace source")

local sprite = assert(app.open(source), "cannot open source")
assert(sprite.width == 64 and sprite.height == 64, "expected native 64x64 tile")
assert(sprite.colorMode == ColorMode.RGB and #sprite.frames == 1, "expected single-frame RGBA tile")
assert(#sprite.layers == 1, "expected one flat source layer")

local original = sprite.layers[1]
local originalCel = assert(original:cel(sprite.frames[1]), "source cel missing")
assert(originalCel.position.x == 0 and originalCel.position.y == 0, "source cel moved")
local originalImage = originalCel.image
local corrected = Image(sprite.width, sprite.height, sprite.colorMode)

local function clamp(value)
  return math.max(0, math.min(255, value))
end

-- The dark Town material-pocket tile averages RGB(62,77,6), whereas its
-- neighboring base grass averages RGB(84,100,3). Preserve every pixel cluster
-- and alpha; shift only the palette to remove large dark checkerboard blocks.
for y = 0, sprite.height - 1 do
  for x = 0, sprite.width - 1 do
    local pixel = originalImage:getPixel(x, y)
    local alpha = app.pixelColor.rgbaA(pixel)
    if alpha == 0 then
      corrected:drawPixel(x, y, pixel)
    else
      local red = clamp(app.pixelColor.rgbaR(pixel) + 18)
      local green = clamp(app.pixelColor.rgbaG(pixel) + 18)
      local blue = clamp(app.pixelColor.rgbaB(pixel) - 2)
      corrected:drawPixel(x, y, app.pixelColor.rgba(red, green, blue, alpha))
    end
  end
end

app.transaction(function()
  original.name = "Original ground_grass_a (preserved)"
  original.isVisible = false
  local polish = sprite:newLayer()
  polish.name = "Town palette harmony +18R +18G -2B"
  sprite:newCel(polish, sprite.frames[1], corrected, Point(0, 0))
end)
sprite:saveAs(output)
sprite:close()

local reopened = assert(app.open(output), "cannot reopen candidate")
assert(reopened.width == 64 and reopened.height == 64 and #reopened.frames == 1, "candidate geometry changed")
assert(#reopened.layers == 2 and reopened.layers[2].isVisible, "candidate layers invalid")
reopened:close()
