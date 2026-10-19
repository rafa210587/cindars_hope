local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local s=app.open(root..'Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png')
local src=Image(s.width,s.height,ColorMode.RGB); src:drawSprite(s,1)
local img=Image(314,314,ColorMode.RGB)
for y=0,313 do for x=0,313 do img:drawPixel(x,y,src:getPixel(math.floor((x+.5)*1254/314),math.floor((y+.5)*1254/314))) end end
local out=Sprite(314,314,ColorMode.RGB); out:newCel(out.layers[1],1,img,Point(0,0))
out:saveCopyAs(root..'dev/art/aseprite/keyart-v4/contact-v16/art/grass-nearest-baseline.png')
out:close(); s:close()
