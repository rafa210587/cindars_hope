local base='dev/art/aseprite/keyart-v4/animation/'
local assets='Assets/_Game/Art/Generated/World/'
local water=assert(app.open(assets..'tiles/ground_water_loop_v4.png')); local wi=water.cels[1].image
local source=assert(app.open(assets..'tiles/ground_water_keyart_v4.png')); local si=source.cels[1].image
local palette={}; for y=0,63 do for x=0,63 do palette[si:getPixel(x,y)]=true; assert(wi:getPixel(x,y)==si:getPixel(x,y)) end end
print('PASS water frame 0: 4096 exact RGBA source pixels')
for f=0,5 do
 local changed=0
 for y=0,63 do for x=0,63 do
   local p=wi:getPixel(f*64+x,y); assert(palette[p]); assert(app.pixelColor.rgbaA(p)==255)
   if p~=wi:getPixel(((f+1)%6)*64+x,y) then changed=changed+1 end
 end end
 print('Water frame '..f..' -> '..((f+1)%6)..': '..changed..' changed pixels; native palette and opaque PASS')
end
water:close(); source:close()
local house=assert(app.open(assets..'building/farmhouse_keyart_v4.png')); local hi=house.cels[1].image
local door=assert(app.open(assets..'props/farmhouse_door_open_v4.png')); local di=door.cels[1].image
local threshold=assert(app.open(assets..'props/farmhouse_door_threshold_v4.png')); local ti=threshold.cels[1].image
for y=0,26 do for x=0,26 do
 local inAperture=x>=3 and x<=23 and y>=3 and y<=21
 if inAperture then
  assert(di:getPixel(x,y)==hi:getPixel(108+x,143+y))
  assert(app.pixelColor.rgbaA(ti:getPixel(x,y))==255)
 else assert(app.pixelColor.rgbaA(ti:getPixel(x,y))==0) end
end end
print('PASS closed leaf: 399 exact source RGBA pixels; threshold opaque only inside 21x19 aperture')
local context=assert(app.open(base..'farmhouse-door-context.aseprite'))
local strip=Sprite(52*5,51,ColorMode.RGB); local sheet=Image(52*5,51,ColorMode.RGB); sheet:clear()
for f=1,5 do
 local rendered=Image(212,205,ColorMode.RGB); rendered:clear(); rendered:drawSprite(context,f)
 for y=0,50 do for x=0,51 do sheet:drawPixel((f-1)*52+x,y,rendered:getPixel(96+x,132+y)) end end
end
strip:newCel(strip.layers[1],1,sheet,Point(0,0)); strip:saveCopyAs(base..'door-context-poses.png')
strip:close(); context:close(); house:close(); door:close(); threshold:close()
