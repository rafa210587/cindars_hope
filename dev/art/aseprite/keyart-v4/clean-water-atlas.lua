local sprite=assert(app.open(assert(app.params.source)))
local original=Image(sprite.cels[1].image)
local clean=Image(original)
for y=0,63 do for x=0,63 do
 local c=original:getPixel(x,y)
 local r,g,b=app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c)
 if b<=g or g<=r then
  local replacement=nil
  for radius=1,12 do
   if replacement then break end
   for dy=-radius,radius do
    if replacement then break end
    for dx=-radius,radius do
     local xx,yy=x+dx,y+dy
     if xx>=0 and xx<64 and yy>=0 and yy<64 then
      local test=original:getPixel(xx,yy)
      local tr,tg,tb=app.pixelColor.rgbaR(test),app.pixelColor.rgbaG(test),app.pixelColor.rgbaB(test)
      if tb>tg and tg>tr then replacement=test; break end
     end
    end
   end
  end
  if replacement then clean:drawPixel(x,y,replacement) end
 end
end end
sprite.layers[1].isVisible=false; sprite.layers[1].name='Original approved lake sample'
local layer=sprite:newLayer(); layer.name='Water tile - isolated bank flecks removed'
sprite:newCel(layer,1,clean,Point(0,0))
sprite:saveAs(assert(app.params.output)); sprite:saveCopyAs(assert(app.params.preview)); sprite:close()
