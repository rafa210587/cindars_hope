local sourcePath='Assets/_Game/Art/Generated/World/building/farmhouse_keyart_v4.png'
local targetPath='Assets/_Game/Art/Generated/World/building/farmhouse_opening_keyart_v4.png'
local candidatePath='dev/art/aseprite/keyart-v4/animation/farmhouse_opening_keyart_v4.aseprite'
local house=assert(app.open(sourcePath))
local original=Image(house.cels[1].image)
local baseline=house.layers[1]; baseline.name='Approved farmhouse - preserved hidden'; baseline.isVisible=false
local facade=house:newLayer(); facade.name='Facade with measured door aperture'
local edited=Image(original)
for y=146,164 do for x=111,131 do edited:drawPixel(x,y,0) end end
house:newCel(facade,1,edited,Point(0,0))
local aperture=house:newSlice(Rectangle(111,146,21,19)); aperture.name='Door aperture only'
house:saveAs(candidatePath); house:close()
local reopened=assert(app.open(candidatePath)); assert(reopened.width==212 and reopened.height==205)
assert(not reopened.layers[1].isVisible and reopened.layers[2].isVisible)
reopened:saveCopyAs(targetPath); reopened:close()
local exported=assert(app.open(targetPath)); local ei=exported.cels[1].image
local door=assert(app.open('Assets/_Game/Art/Generated/World/props/farmhouse_door_open_v4.png')); local di=door.cels[1].image
local count=0
for y=0,204 do for x=0,211 do
 local hole=x>=111 and x<=131 and y>=146 and y<=164
 if hole then
   assert(app.pixelColor.rgbaA(ei:getPixel(x,y))==0)
   assert(di:getPixel(x-108,y-143)==original:getPixel(x,y))
   count=count+1
 else assert(ei:getPixel(x,y)==original:getPixel(x,y)) end
end end
assert(count==399)
print('PASS: exported212x205 facade has exactly399 cleared aperture pixels; all other pixels unchanged; closed leaf restores every removed source RGBA pixel')
exported:close(); door:close()
