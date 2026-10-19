local sprite=assert(app.open(assert(app.params.source)))
local original=Image(sprite.cels[1].image)
local ripples=Image(sprite.width,sprite.height,ColorMode.RGB); ripples:clear()
for y=0,sprite.height-1 do for x=0,sprite.width-1 do
 local pixel=original:getPixel(x,y)
 local r=app.pixelColor.rgbaR(pixel); local g=app.pixelColor.rgbaG(pixel); local b=app.pixelColor.rgbaB(pixel)
 -- Keep only blue reflection clusters, with a feathered alpha at their dark edge.
 if b>g and g>68 and r<110 then
  local a=math.min(210,math.max(0,(g-68)*8))
  ripples:drawPixel(x,y,app.pixelColor.rgba(r,g,b,a))
 end
end end
sprite.layers[1].isVisible=false; sprite.layers[1].name='Approved water sample - hidden source'
local layer=sprite:newLayer(); layer.name='Separated reflection clusters'
sprite:newCel(layer,1,ripples,Point(0,0))
sprite:saveAs(assert(app.params.output)); sprite:saveCopyAs(assert(app.params.preview)); sprite:close()
