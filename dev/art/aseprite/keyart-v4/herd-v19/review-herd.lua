local out='D:/Projetos/Cindars_Hope/cindars_hope/dev/art/aseprite/keyart-v4/herd-v19/'
local pc=app.pixelColor
local board=Sprite(960,576,ColorMode.RGB);local b=Image(960,576,ColorMode.RGB);b:clear(pc.rgba(42,63,38,255))
for row,kind in ipairs({'cow','sheep','goat'}) do
 local s=assert(app.open(out..kind..'_v19.aseprite'))
 for f=1,19 do
  local img=Image(48,48,ColorMode.RGB);img:clear();img:drawSprite(s,f)
  local ox=((f-1)%10)*96;local oy=(row-1)*192+math.floor((f-1)/10)*96
  for y=0,47 do for x=0,47 do local p=img:getPixel(x,y)
   if pc.rgbaA(p)>0 then for dy=0,1 do for dx=0,1 do b:drawPixel(ox+x*2+dx,oy+y*2+dy,p) end end end
  end end
  -- Contact guide belongs only to the review board, never to sprite art.
  for x=0,95 do if x%8<4 then b:drawPixel(ox+x,oy+80,pc.rgba(99,104,65,255)) end end
 end;s:close()
end
board:newCel(board.layers[1],1,b,Point(0,0));board:saveCopyAs(out..'herd-contact-board-2x.png');board:close()
