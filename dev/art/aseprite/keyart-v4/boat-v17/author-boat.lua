local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local out=root..'dev/art/aseprite/keyart-v4/boat-v17/'
local pc=app.pixelColor
local function rgb(r,g,b) return pc.rgba(r,g,b,255) end
local original=assert(app.open(root..'Assets/_Game/Art/Generated/World/props/boat_keyart_v4.png'))
assert(original.width==76 and original.height==68)
local base=Image(76,68,ColorMode.RGB); base:drawSprite(original,1); original:close()
local hull=Image(76,68,ColorMode.RGB); hull:clear()
local rim=Image(76,68,ColorMode.RGB); rim:clear()
local function line(img,x0,y0,x1,y1,c)
 local dx,dy=math.abs(x1-x0),-math.abs(y1-y0)
 local sx,sy=x0<x1 and 1 or -1,y0<y1 and 1 or -1; local err=dx+dy
 while true do img:drawPixel(x0,y0,c); if x0==x1 and y0==y1 then break end
  local e=2*err; if e>=dy then err=err+dy; x0=x0+sx end; if e<=dx then err=err+dx; y0=y0+sy end
 end
end
local function chain(img,pts,c)
 for i=1,#pts-1 do line(img,pts[i][1],pts[i][2],pts[i+1][1],pts[i+1][2],c) end
end
-- Complete the occluded stern inside the existing left margin. The upper lip turns
-- inward, and the hull continues beneath it rather than ending at the old x8 cut.
local spans={{36,7,8},{37,6,9},{38,5,9},{39,4,9},{40,3,10},{41,3,10},
 {42,2,10},{43,2,10},{44,2,10},{45,2,10},{46,3,10},{47,3,10},
 {48,4,10},{49,4,10},{50,5,10},{51,5,10},{52,6,10},{53,8,11},{54,10,12}}
for _,a in ipairs(spans) do
 for x=a[2],a[3] do
  if pc.rgbaA(base:getPixel(x,a[1]))==0 then
   local c=a[1]<45 and rgb(29,13,1) or (a[1]<50 and rgb(73,35,3) or rgb(43,17,1))
   hull:drawPixel(x,a[1],c)
  end
 end
end
-- Three structural bands follow the same curvature. No added noise, ropes or props.
chain(hull,{{3,46},{4,49},{6,52},{9,54},{12,55}},rgb(27,10,1))
chain(hull,{{4,48},{6,50},{9,52},{11,52}},rgb(88,40,3))
chain(hull,{{5,50},{7,52},{10,53}},rgb(55,22,1))
chain(rim,{{8,35},{6,36},{4,38},{2,41},{1,43},{2,46},{4,48},{8,49},{10,49}},rgb(41,23,3))
chain(rim,{{8,36},{6,37},{4,39},{3,41},{2,43},{3,45},{5,47},{8,48},{10,48}},rgb(145,85,4))
chain(rim,{{8,36},{6,38},{4,40},{3,42},{3,44},{4,46},{7,47},{9,47}},rgb(171,110,14))
chain(rim,{{8,38},{6,40},{5,42},{5,44},{6,45},{9,46}},rgb(69,36,2))
-- A short dark inner transom defines the boat cavity at the stern instead of a flat cap.
chain(rim,{{6,40},{6,43},{8,45}},rgb(20,7,0))
local s=Sprite(76,68,ColorMode.RGB); s.layers[1].name='Original boat - preserved'
s:newCel(s.layers[1],1,base,Point(0,0))
local l=s:newLayer(); l.name='Reconstructed stern hull'; s:newCel(l,1,hull,Point(0,0))
l=s:newLayer(); l.name='Continuous wooden rim and inner transom'; s:newCel(l,1,rim,Point(0,0))
local slice=s:newSlice(Rectangle(0,0,76,68)); slice.name='Existing support'; slice.pivot=Point(35,44)
s:saveAs(out..'boat_contact_v17.aseprite'); s:saveCopyAs(out..'boat_contact_v17.png'); s:close()
local reopened=assert(app.open(out..'boat_contact_v17.aseprite')); assert(#reopened.layers==3)
assert(reopened.slices[1].pivot==Point(35,44))
local result=Image(76,68,ColorMode.RGB); result:drawSprite(reopened,1); reopened:close()
local changes=0
for y=0,67 do for x=0,75 do
 if result:getPixel(x,y)~=base:getPixel(x,y) then changes=changes+1; assert(x<=12 and x>=1 and y>=35 and y<=55) end
end end
local board=Sprite(680,390,ColorMode.RGB); local b=Image(680,390,ColorMode.RGB); b:clear(rgb(31,53,54))
local function draw(img,ox,oy,scale,bg)
 for y=0,68*scale-1 do for x=0,76*scale-1 do
  local p=img:getPixel(math.floor(x/scale),math.floor(y/scale)); b:drawPixel(ox+x,oy+y,pc.rgbaA(p)==0 and bg or p)
 end end
end
draw(base,16,16,4,rgb(24,47,53)); draw(result,356,16,4,rgb(24,47,53))
draw(base,16,308,1,rgb(200,202,177)); draw(result,132,308,1,rgb(200,202,177))
draw(base,356,308,1,rgb(24,47,53)); draw(result,472,308,1,rgb(24,47,53))
board:newCel(board.layers[1],1,b,Point(0,0)); board:saveCopyAs(out..'boat-before-after-board.png'); board:close()
print('PASS boat76x68,3layers,1frame; pivot top35,44/bottom35,24; only stern x1..12 y35..55 changed: '..changes..'pixels')
