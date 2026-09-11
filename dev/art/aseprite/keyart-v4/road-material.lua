local source=assert(app.open(assert(app.params.source)))
local sampled=Image(source.cels[1].image)
source:close()
local sprite=Sprite(64,64,ColorMode.RGB)
sprite.layers[1].name='Earth clusters - sampled approved road'
local ground=Image(64,64,ColorMode.RGB)
local function rgba(r,g,b) return app.pixelColor.rgba(r,g,b,255) end
local palette={rgba(124,100,45),rgba(133,106,48),rgba(141,112,51),rgba(148,117,54),rgba(157,124,59),rgba(168,135,68)}
for y=0,31 do for x=0,31 do
 local sx=(x+math.floor(y/8)*7)%32
 local sy=(y+math.floor(x/8)*5)%20
 local pixel=sampled:getPixel(sx,sy)
 local r=app.pixelColor.rgbaR(pixel); local g=app.pixelColor.rgbaG(pixel)
 local idx=math.max(1,math.min(6,math.floor((r-103)/15)+1))
 if r<g*1.12 then idx=3 end
 for dy=0,1 do for dx=0,1 do ground:drawPixel(x*2+dx,y*2+dy,palette[idx]) end end
end end
sprite:newCel(sprite.layers[1],1,ground,Point(0,0))
local details=Image(64,64,ColorMode.RGB); details:clear()
-- Sparse inset stone clusters; no directional wheel marks or seam blend.
local points={{8,6},{26,17},{47,7},{57,31},{17,42},{38,36},{49,55},{7,59}}
for i,p in ipairs(points) do
 local x,y=p[1],p[2]
 for dy=0,2 do for dx=0,3 do
  if not (dx==3 and dy==0) then
   details:drawPixel(x+dx,y+dy,dy==2 and rgba(112,96,48) or rgba(157,134,72))
  end
 end end
 details:drawPixel(x+1,y,rgba(174,148,88))
end
local stones=sprite:newLayer(); stones.name='Sparse embedded stones'
sprite:newCel(stones,1,details,Point(0,0))
sprite:saveAs(assert(app.params.output)); sprite:saveCopyAs(assert(app.params.preview)); sprite:close()
print('KEYART_ROAD_MATERIAL_PASS 64x64 opaque 2 layers')
