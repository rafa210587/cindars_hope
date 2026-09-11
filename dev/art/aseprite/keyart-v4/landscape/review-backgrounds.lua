-- Inspection-only composite; the isolated export is never overwritten.
local src=assert(app.open(app.params.source))
local flat=Image(src.width,src.height,ColorMode.RGB);flat:clear();flat:drawSprite(src,1)
local result=Sprite(src.width*2+12,src.height,ColorMode.RGB)
local pixels=Image(result.width,result.height,ColorMode.RGB)
local light=app.pixelColor.rgba(228,217,192,255)
local dark=app.pixelColor.rgba(26,35,38,255)
for y=0,result.height-1 do for x=0,result.width-1 do
 pixels:drawPixel(x,y,x<src.width+6 and light or dark)
end end
for y=0,src.height-1 do for x=0,src.width-1 do
 local p=flat:getPixel(x,y)
 if app.pixelColor.rgbaA(p)>0 then pixels:drawPixel(x,y,p);pixels:drawPixel(x+src.width+12,y,p) end
end end
result.cels[1].image=pixels
result:saveCopyAs(app.params.output)
result:close();src:close()
