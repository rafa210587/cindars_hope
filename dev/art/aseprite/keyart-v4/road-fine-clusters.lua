local original=assert(app.open(assert(app.params.source)))
local sample=Image(original.cels[1].image)
original:close()
local sprite=Sprite(64,64,ColorMode.RGB)
sprite.layers[1].name='Approved earth - fine native clusters'
local material=Image(64,64,ColorMode.RGB)
for y=0,63 do for x=0,63 do
  local sx=(x+math.floor(y/12)*7)%sample.width
  local sy=(y+math.floor(x/16)*5)%12
  material:drawPixel(x,y,sample:getPixel(sx,sy))
end end
sprite:newCel(sprite.layers[1],1,material,Point(0,0))
local baseline=sprite:newLayer()
baseline.name='Unmodified approved sample - hidden source'
sprite:newCel(baseline,1,sample,Point(0,0))
baseline.isVisible=false
sprite:saveAs(assert(app.params.output))
sprite:saveCopyAs(assert(app.params.preview))
sprite:close()
print('ROAD_FINE_CLUSTERS_PASS 64x64, no doubled source pixels')
