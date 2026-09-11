local source = assert(app.params.source)
local output = assert(app.params.output)
local preview = assert(app.params.preview)
local sprite = assert(app.open(source))
assert(sprite.width == 64 and sprite.height == 64 and #sprite.frames == 1)
assert(sprite.colorMode == ColorMode.RGB)
local baseline = sprite.layers[1]
baseline.name = 'Original dirt - preserved'
baseline.isVisible = false
local function rgba(r,g,b) return app.pixelColor.rgba(r,g,b,255) end
local soil = {rgba(129,99,46),rgba(141,107,49),rgba(151,115,54),rgba(161,123,61),rgba(169,132,68)}
local original = sprite.cels[1].image
local ground = Image(64,64,ColorMode.RGB)
-- Broad irregular wear patches replace the directional stripe in the source texture.
local wear={{12,15,9,6,1},{40,34,11,7,-1},{24,52,8,5,1}}
for y=0,63 do for x=0,63 do
  local pixel = original:getPixel(math.floor(x/2)*2, math.floor(y/2)*2)
  local value = (app.pixelColor.rgbaR(pixel)+app.pixelColor.rgbaG(pixel)+app.pixelColor.rgbaB(pixel))/3
  local index=3
  for _,p in ipairs(wear) do
    local d=((x-p[1])/p[3])^2+((y-p[2])/p[4])^2
    if d<1+0.18*math.sin(x*1.3+y) then index=index+p[5] end
  end
  if (math.floor(x/2)*17+math.floor(y/2)*13)%23==0 then index=index+(value>108 and 1 or -1) end
  index=math.max(1,math.min(5,index))
  ground:drawPixel(x,y,soil[index])
end end
local earth = sprite:newLayer(); earth.name='Packed earth - clustered palette'
sprite:newCel(earth,1,ground,Point(0,0))
local details=Image(64,64,ColorMode.RGB); details:clear()
-- Authored sparse irregular clusters; wrapping keeps the tile repeat continuous.
local points={{5,9},{24,4},{47,13},{15,33},{38,29},{59,41},{31,53},{8,59}}
for i,p in ipairs(points) do
  local x,y=p[1],p[2]
  local dark=rgba(101,85,49); local mid=rgba(141,127,81); local light=rgba(176,155,104)
  details:drawPixel(x,y,dark); details:drawPixel((x+1)%64,y,mid)
  details:drawPixel(x,(y+1)%64,mid)
  details:drawPixel((x+1)%64,(y+1)%64,light)
  if i%2==0 then
    details:drawPixel((x+2)%64,y,mid)
    details:drawPixel((x+2)%64,(y+1)%64,dark)
    details:drawPixel(x,(y+2)%64,dark)
  end
end
local pebble=sprite:newLayer(); pebble.name='Sparse embedded pebbles'
sprite:newCel(pebble,1,details,Point(0,0))
sprite:saveAs(output)
sprite:close()
local check=assert(app.open(output))
assert(check.width==64 and check.height==64 and #check.layers==3 and #check.frames==1)
assert(not check.layers[1].isVisible)
check:saveCopyAs(preview)
check:close()
print('ASEPRITE_ROAD_ROUNDTRIP_PASS 64x64 layers=3 frames=1')
