local source=assert(app.open(assert(app.params.source)))
local original=Image(source.cels[1].image)
source:close()
local sprite=Sprite(64,16,ColorMode.RGB)
sprite.layers[1].name='Grass transition - sampled source palette'
local pixels=Image(64,16,ColorMode.RGB); pixels:clear()
for x=0,63 do
  local depth=6+math.floor(2*math.sin(x*0.7)+2*math.sin(x*0.27))
  for y=0,15 do
    local edge=depth
    if x%13<3 then edge=edge+4 end
    if y<=edge then
      pixels:drawPixel(x,y,original:getPixel((x*4+72)%original.width,(y*4+40)%original.height))
    end
  end
end
sprite:newCel(sprite.layers[1],1,pixels,Point(0,0))
sprite:saveAs(assert(app.params.output))
sprite:saveCopyAs(assert(app.params.preview))
sprite:close()
print('ASEPRITE_GRASS_EDGE_PASS 64x16 transparent')
