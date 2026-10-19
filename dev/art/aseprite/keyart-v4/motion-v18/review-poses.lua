local out='D:/Projetos/Cindars_Hope/cindars_hope/dev/art/aseprite/keyart-v4/motion-v18/'
local s=assert(app.open(out..'chicken_v18.aseprite'))
local board=Sprite(640,192,ColorMode.RGB); local img=Image(640,192,ColorMode.RGB); img:clear(app.pixelColor.rgba(42,63,38,255))
for f=1,19 do
 local frame=Image(32,32,ColorMode.RGB); frame:clear(); frame:drawSprite(s,f)
 local ox=((f-1)%10)*64; local oy=math.floor((f-1)/10)*96
 for y=0,31 do for x=0,31 do local p=frame:getPixel(x,y)
  if app.pixelColor.rgbaA(p)>0 then for dy=0,1 do for dx=0,1 do img:drawPixel(ox+x*2+dx,oy+y*2+dy,p) end end end
 end end
end
board:newCel(board.layers[1],1,img,Point(0,0)); board:saveCopyAs(out..'chicken-poses-2x.png'); board:close(); s:close()
