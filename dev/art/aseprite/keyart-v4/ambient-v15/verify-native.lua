local base='D:/Projetos/Cindars_Hope/cindars_hope/dev/art/aseprite/keyart-v4/ambient-v15/'
for _,def in ipairs({{'fountain_ambient_v15',170,159,5,.14},{'cascade_ambient_v15',68,56,5,.14},{'fish_jump_v15',48,48,8,.12}}) do
 local s=assert(app.open(base..def[1]..'.aseprite'))
 assert(s.width==def[2] and s.height==def[3] and #s.frames==def[4])
 assert(#s.layers>=2 and #s.tags==1)
 for _,f in ipairs(s.frames) do assert(math.abs(f.duration-def[5])<.00001) end
 if def[1]=='fish_jump_v15' then
  local img=Image(48,48,ColorMode.RGB); img:clear(); img:drawSprite(s,8)
  for y=0,47 do for x=0,47 do assert(app.pixelColor.rgbaA(img:getPixel(x,y))==0) end end
  assert(s.slices[1].pivot.x==24 and s.slices[1].pivot.y==34)
 end
 print('PASS reopened '..def[1]..': canvas, frames, layers, tag, timing')
 s:close()
end
