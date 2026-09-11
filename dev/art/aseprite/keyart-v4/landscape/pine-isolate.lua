-- Single candidate only: source pixels and manually traced contour, no reconstruction.
local sprite=assert(app.open(assert(app.params.source)))
assert(sprite.colorMode==ColorMode.RGB)
local source=Image(sprite.cels[1].image)
local polygon={44,10,46,12,45,18,47,21,47,24,51,26,49,28,54,32,50,33,48,31,49,36,54,39,58,44,54,45,52,43,52,47,56,50,60,55,56,57,54,54,56,59,61,63,60,66,58,65,61,70,65,73,64,76,61,75,64,80,68,83,71,83,72,87,69,88,72,91,76,94,74,97,69,97,72,101,69,104,66,103,63,105,58,104,56,108,50,106,47,111,46,115,50,121,55,125,53,128,48,127,43,124,39,125,35,128,32,127,33,124,39,116,40,110,36,106,32,109,27,106,24,108,20,106,16,106,16,102,12,102,10,99,12,96,16,94,19,90,15,91,13,88,17,85,20,82,18,80,22,76,20,74,24,71,28,68,24,69,22,66,27,62,30,60,28,58,32,54,30,52,33,48,31,46,34,43,31,41,35,38,37,35,33,34,32,31,36,28,35,26,39,23,39,20,42,18,42,14}
local function inside(x,y,p)
 local hit=false;local j=#p-1
 for i=1,#p,2 do
  local xi,yi=p[i],p[i+1];local xj,yj=p[j],p[j+1]
  if ((yi>y)~=(yj>y)) and x<(xj-xi)*(y-yi)/(yj-yi)+xi then hit=not hit end
  j=i
 end
 return hit
end
local masked=Image(sprite.width,sprite.height,ColorMode.RGB);masked:clear()
local count=0;local minX,minY,maxX,maxY=sprite.width,sprite.height,-1,-1
for y=0,sprite.height-1 do for x=0,sprite.width-1 do
 if inside(x+0.5,y+0.5,polygon) then
  masked:drawPixel(x,y,source:getPixel(x,y));count=count+1
  minX=math.min(minX,x);minY=math.min(minY,y);maxX=math.max(maxX,x);maxY=math.max(maxY,y)
 end
end end
sprite.layers[1].name='Approved keyart crop - original hidden';sprite.layers[1].isVisible=false
local layer=sprite:newLayer();layer.name='Pine candidate - manual true alpha'
sprite:newCel(layer,1,masked,Point(0,0))
sprite:saveAs(assert(app.params.output));sprite:saveCopyAs(assert(app.params.preview));sprite:close()
local reopened=assert(app.open(app.params.output))
assert(#reopened.layers==2 and #reopened.frames==1 and not reopened.layers[1].isVisible)
reopened:close()
print(string.format('PINE_ALPHA_PASS canvas=%dx%d opaque=%d transparent=%d bounds=%d,%d..%d,%d reopened=true',masked.width,masked.height,count,masked.width*masked.height-count,minX,minY,maxX,maxY))
