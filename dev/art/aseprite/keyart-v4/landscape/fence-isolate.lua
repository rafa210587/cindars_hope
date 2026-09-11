-- Source-colored wood only. Separate polygons preserve open air between rails.
local sprite=assert(app.open(assert(app.params.source)))
assert(sprite.colorMode==ColorMode.RGB)
local source=Image(sprite.cels[1].image)
local shapes={
 horizontal={
  {6,6,9,6,11,8,11,10,10,12,10,24,9,26,6,26,5,24,6,12,5,10,5,8},
  {34,6,36,6,38,8,38,10,37,12,37,24,38,26,36,27,33,26,33,12,32,10,32,8},
  {10,11,33,11,33,14,10,14},
  {10,19,33,19,33,22,10,22}
 },
 diagonal={
  {7,5,10,5,12,7,12,9,11,11,11,25,10,27,7,27,6,25,6,11,5,10,5,7},
  {34,15,37,15,39,17,39,20,38,22,38,35,36,37,33,36,33,21,32,20,32,17},
  {11,12,16,13,23,16,33,19,33,22,23,19,16,16,11,15},
  {11,21,19,23,27,26,33,27,33,30,27,29,19,26,11,24}
 }
}
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
 local keep=false
 for _,poly in ipairs(assert(shapes[app.params.kind])) do if inside(x+0.5,y+0.5,poly) then keep=true;break end end
 if keep then
  masked:drawPixel(x,y,source:getPixel(x,y));count=count+1
  minX=math.min(minX,x);minY=math.min(minY,y);maxX=math.max(maxX,x);maxY=math.max(maxY,y)
 end
end end
sprite.layers[1].name='Approved fence crop - original hidden';sprite.layers[1].isVisible=false
local layer=sprite:newLayer();layer.name='Wood rails and posts - manual alpha'
sprite:newCel(layer,1,masked,Point(0,0))
sprite:saveAs(assert(app.params.output));sprite:saveCopyAs(assert(app.params.preview));sprite:close()
local reopened=assert(app.open(app.params.output))
assert(#reopened.layers==2 and #reopened.frames==1 and not reopened.layers[1].isVisible)
reopened:close()
print(string.format('FENCE_ALPHA_PASS kind=%s canvas=%dx%d opaque=%d transparent=%d bounds=%d,%d..%d,%d reopened=true',app.params.kind,masked.width,masked.height,count,masked.width*masked.height-count,minX,minY,maxX,maxY))
