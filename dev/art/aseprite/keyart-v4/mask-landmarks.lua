-- Individual landmark extraction from the user-approved keyart.
-- Preserve the unmodified crop hidden; all coordinates are crop-local.
local sprite=assert(app.open(assert(app.params.source)))
assert(sprite.colorMode == ColorMode.RGB)
local original=Image(sprite.cels[1].image)
local shapes={
 fountain={
  {45,49,57,44,60,38,66,38,70,43,75,41,75,19,72,15,75,7,80,3,85,8,89,14,84,20,85,43,91,42,96,38,102,39,106,46,119,49,128,61,129,76,123,91,111,100,91,107,84,116,64,116,60,108,43,100,36,88,34,72,37,59},
  {23,13,30,13,34,18,33,29,32,33,32,39,35,46,31,51,23,51,19,45,21,36,21,22},
  {131,13,138,13,142,18,141,32,143,42,144,47,140,52,130,50,126,45,129,33,128,20},
  {23,70,29,68,35,73,35,89,38,100,36,107,30,112,23,111,19,107,19,99,22,90},
  {131,72,138,73,143,78,142,96,145,102,142,110,134,112,126,108,126,101,129,93,129,80},
  {68,119,77,118,83,122,81,128,71,130,63,127,63,123},
  {88,110,95,108,103,111,104,116,97,120,89,118},
  {88,125,98,125,105,130,103,134,94,137,86,134},
  {64,130,73,130,78,134,75,139,64,139,58,136},
 },
 well={
  {21,7,27,6,63,15,67,26,65,35,60,36,60,58,66,61,65,72,60,81,47,85,31,84,21,80,16,73,16,60,19,55,19,32,12,29},
 },
 stall={
  {10,13,16,11,59,11,64,16,65,30,63,34,61,59,13,59,9,53,11,30},
 },
 waterfall={
  {15,2,26,0,51,0,58,9,64,18,63,31,63,52,69,64,72,78,64,91,61,104,50,110,35,108,26,100,16,88,9,80,10,63,12,45,11,30,6,22},
 },
 bridge={
  {8,15,13,10,19,12,20,17,40,14,67,14,82,17,83,10,91,9,97,14,95,36,100,51,96,57,86,56,83,53,23,53,20,59,9,59,6,52,10,34},
 }
}
local function inside(x,y,p)
 local hit=false; local j=#p-1
 for i=1,#p,2 do
  local xi,yi=p[i],p[i+1]; local xj,yj=p[j],p[j+1]
  if ((yi>y) ~= (yj>y)) and (x<(xj-xi)*(y-yi)/(yj-yi)+xi) then hit=not hit end
  j=i
 end
 return hit
end
local polygons=assert(shapes[app.params.kind])
local masked=Image(sprite.width,sprite.height,ColorMode.RGB); masked:clear()
local count=0
for y=0,sprite.height-1 do
 for x=0,sprite.width-1 do
  local keep=false
  for _,poly in ipairs(polygons) do if inside(x+0.5,y+0.5,poly) then keep=true; break end end
  if keep then
   local color=original:getPixel(x,y)
   if app.params.kind=='waterfall' and y>88 then
    local a=math.max(0,255-(y-88)*12)
    color=app.pixelColor.rgba(app.pixelColor.rgbaR(color),app.pixelColor.rgbaG(color),app.pixelColor.rgbaB(color),a)
   end
   masked:drawPixel(x,y,color); count=count+1
  end
 end
end
sprite.layers[1].isVisible=false
sprite.layers[1].name='Approved crop - unmodified hidden source'
local layer=sprite:newLayer(); layer.name='Isolated landmark - true alpha'
sprite:newCel(layer,1,masked,Point(0,0))
sprite:saveAs(assert(app.params.output))
sprite:saveCopyAs(assert(app.params.preview))
sprite:close()
print('KEYART_LANDMARK_ALPHA_PASS '..app.params.kind..' opaque='..count)
