-- Review-only image. Left: existing asset at matched height. Right: native candidate.
local old=assert(app.open(app.params.existing))
local candidate=assert(app.open(app.params.candidate))
local result=Sprite(216,151,ColorMode.RGB)
local pixels=Image(result.width,result.height,ColorMode.RGB)
pixels:clear(app.pixelColor.rgba(30,38,40,255))
local function blit(sprite,ox,oy)
 local src=Image(sprite.width,sprite.height,ColorMode.RGB);src:clear();src:drawSprite(sprite,1)
 for y=0,src.height-1 do for x=0,src.width-1 do
  local p=src:getPixel(x,y)
  if app.pixelColor.rgbaA(p)>0 then pixels:drawPixel(x+ox,y+oy,p) end
 end end
end
blit(old,16,11)
blit(candidate,111,0)
result.cels[1].image=pixels
result:saveCopyAs(app.params.output)
old:close();candidate:close();result:close()
