local original=assert(app.open(assert(app.params.source)))
local sample=Image(original.cels[1].image)
original:close()
local sprite=Sprite(64,64,ColorMode.RGB)
sprite.layers[1].name='Approved earth - original color clusters'
local material=Image(64,64,ColorMode.RGB)
for y=0,31 do for x=0,31 do
  local sx=(x+math.floor(y/8)*7)%sample.width
  local sy=(y+math.floor(x/8)*5)%12
  local pixel=sample:getPixel(sx,sy)
  for dy=0,1 do for dx=0,1 do
    material:drawPixel(x*2+dx,y*2+dy,pixel)
  end end
end end
sprite:newCel(sprite.layers[1],1,material,Point(0,0))
local baseline=sprite:newLayer()
baseline.name='Unmodified approved sample - hidden source'
sprite:newCel(baseline,1,sample,Point(0,0))
baseline.isVisible=false
sprite:saveAs(assert(app.params.output))
sprite:saveCopyAs(assert(app.params.preview))
sprite:close()
print('ROAD_NATIVE_COLORS_PASS 64x64 two layers preserved source')
