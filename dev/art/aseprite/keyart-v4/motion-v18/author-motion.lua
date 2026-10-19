local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local out=root..'dev/art/aseprite/keyart-v4/motion-v18/'
local pc=app.pixelColor
local function rgb(r,g,b) return pc.rgba(r,g,b,255) end
local function fresh(w,h) local i=Image(w,h,ColorMode.RGB); i:clear(); return i end
local function tag(s,n,a,b) local t=s:newTag(a,b); t.name=n; t.aniDir=AniDir.FORWARD end
local src=assert(app.open(root..'dev/art/aseprite/keyart-v4/boat-v17/boat_contact_v17.png'))
local original=fresh(76,68); original:drawSprite(src,1); src:close()
local boat=Sprite(76,68,ColorMode.RGB); boat.layers[1].name='Complete v17 hull - one pixel buoyancy'
local keep=boat:newLayer(); keep.name='Preserved v17 source - hidden'; keep.isVisible=false
boat:newCel(keep,1,original,Point(0,0))
for f,dy in ipairs({0,-1,0,1}) do
 if f>1 then boat:newEmptyFrame() end; boat.frames[f].duration=1.25
 local img=fresh(76,68); img:drawImage(original,Point(0,dy)); boat:newCel(boat.layers[1],f,img,Point(0,0))
end
tag(boat,'boat_bob',1,4); local sl=boat:newSlice(Rectangle(0,0,76,68)); sl.name='Existing water support'; sl.pivot=Point(35,44)
boat:saveAs(out..'boat_v18.aseprite'); boat:saveCopyAs(out..'boat_v18.gif'); boat:close()

-- Original compact hen: warm ivory feathers, shaded wings, ochre feet and red comb.
local C={outline=rgb(64,51,31),shadow=rgb(139,129,99),mid=rgb(195,184,147),light=rgb(235,226,194),
 white=rgb(248,240,210),red=rgb(173,47,27),redlight=rgb(214,70,35),beak=rgb(224,165,46),leg=rgb(163,103,26),eye=rgb(31,27,20)}
local function dot(im,x,y,c) if x>=0 and y>=0 and x<32 and y<32 then im:drawPixel(x,y,c) end end
local function rect(im,x,y,w,h,c) for yy=y,y+h-1 do for xx=x,x+w-1 do dot(im,xx,yy,c) end end end
local function ellipse(im,cx,cy,rx,ry,c)
 for y=math.floor(cy-ry),math.ceil(cy+ry) do for x=math.floor(cx-rx),math.ceil(cx+rx) do
  if ((x-cx)/rx)^2+((y-cy)/ry)^2<=1 then dot(im,x,y,c) end
 end end
end
local function foot(im,x,y,side)
 rect(im,x,y-2,1,3,C.leg); rect(im,x-1,y,3,1,C.beak)
 if side then dot(im,x+2,y,C.leg) end
end
local function bodyFront(im,bob,back)
 ellipse(im,16,18+bob,7,6,C.outline); ellipse(im,16,17+bob,6,5,C.mid)
 ellipse(im,16,16+bob,4,5,C.light)
 -- Wings hold distinct shadow edges instead of speckled feather texture.
 rect(im,10,17+bob,2,4,C.shadow); rect(im,21,17+bob,2,4,C.shadow)
 rect(im,12,21+bob,2,1,C.light); rect(im,19,21+bob,2,1,C.light)
 if back then
  rect(im,14,19+bob,5,2,C.white); rect(im,15,21+bob,3,2,C.mid)
 else rect(im,14,22+bob,5,1,C.mid) end
end
local function frontHead(im,bob,back,blink)
 ellipse(im,16,12+bob,3,4,C.shadow); ellipse(im,16,12+bob,2,3,C.light)
 rect(im,15,8+bob,2,2,C.red); dot(im,15,7+bob,C.redlight); dot(im,17,8+bob,C.redlight)
 if not back then
  dot(im,14,12+bob,C.eye); dot(im,18,12+bob,C.eye)
  if blink then dot(im,14,12+bob,C.shadow); dot(im,18,12+bob,C.shadow) end
  rect(im,15,14+bob,3,1,C.beak); dot(im,16,15+bob,C.leg); dot(im,16,16+bob,C.red)
 else rect(im,15,11+bob,3,2,C.white) end
end
local function sideBody(im,bob,rest)
 -- Tail fan sits behind the oval body and neck; stable left-facing tail silhouette.
 rect(im,7,13+bob,2,5,C.outline); rect(im,8,14+bob,3,5,C.mid)
 dot(im,7,12+bob,C.light); rect(im,9,13+bob,2,4,C.light)
 ellipse(im,16,19+bob,8,rest and 4 or 5,C.outline)
 ellipse(im,16,18+bob,7,rest and 3 or 4,C.mid)
 ellipse(im,17,17+bob,5,3,C.light)
 ellipse(im,15,19+bob,4,2,C.shadow); rect(im,13,18+bob,5,1,C.light); rect(im,14,20+bob,3,1,C.mid)
end
local function sideHead(im,x,y,sleep)
 rect(im,x-2,y+1,3,5,C.mid); ellipse(im,x,y,3,3,C.shadow); ellipse(im,x,y-1,2,2,C.light)
 rect(im,x-1,y-4,3,1,C.red); dot(im,x-1,y-5,C.redlight); dot(im,x+1,y-5,C.red)
 rect(im,x+3,y,2,1,C.beak); dot(im,x+3,y+1,C.leg)
 dot(im,x+1,y-1,sleep and C.shadow or C.eye); dot(im,x+1,y+3,C.red)
end
local chicken=Sprite(32,32,ColorMode.RGB); chicken.layers[1].name='Feet - contact and passing'
local body=chicken:newLayer(); body.name='Body wings and tail'
local head=chicken:newLayer(); head.name='Head comb beak and expression'
local durations={.4,.4,.14,.14,.14,.14,.14,.14,.14,.14,.14,.14,.14,.14,.18,.18,.18,.6,.6}
for f=1,19 do
 if f>1 then chicken:newEmptyFrame() end; chicken.frames[f].duration=durations[f]
 local feet,feathers,face=fresh(32,32),fresh(32,32),fresh(32,32)
 if f<=2 then
  foot(feet,13,25,false); foot(feet,19,25,false); bodyFront(feathers,0,false); frontHead(face,0,false,f==2)
 elseif f<=10 then
  local back=f>=7; local phase=(f-(back and 7 or 3))%4
  local bob=(phase==1 or phase==3) and -1 or 0
  local ly=(phase==0 or phase==1) and 25 or phase==2 and 23 or 24
  local ry=(phase==2 or phase==3) and 25 or phase==0 and 23 or 24
  foot(feet,13,ly,false); foot(feet,19,ry,false)
  bodyFront(feathers,bob,back); frontHead(face,bob,back,false)
 elseif f<=14 then
  local phase=f-11; local bob=(phase==1 or phase==3) and -1 or 0
  foot(feet,phase==0 and 12 or phase==2 and 19 or 16,25,true)
  foot(feet,phase==0 and 20 or phase==2 and 13 or 17,phase==1 and 23 or 24,true)
  sideBody(feathers,bob,false); sideHead(face,23,12+bob,false)
 elseif f<=17 then
  foot(feet,14,25,true); foot(feet,20,25,true); sideBody(feathers,0,false)
  local pose=f-15
  if pose==1 then
   -- Neck folds forward; head rotates down so beak, not throat, meets the ground.
   rect(face,22,19,3,3,C.mid); rect(face,24,21,2,2,C.light)
   ellipse(face,26,22,2,2,C.shadow); ellipse(face,26,22,1,1,C.light)
   dot(face,24,20,C.redlight); dot(face,25,20,C.red); dot(face,27,22,C.eye)
   rect(face,27,24,1,2,C.beak); dot(face,27,26,C.leg)
  else sideHead(face,25,pose==2 and 16 or 18,false) end
 else
  sideBody(feathers,2,true); sideHead(face,22,f==18 and 17 or 16,true)
  rect(feathers,13,24,8,1,C.shadow) -- settled breast reaches y25, immediately above ground contact y26.
 end
 chicken:newCel(chicken.layers[1],f,feet,Point(0,0)); chicken:newCel(body,f,feathers,Point(0,0)); chicken:newCel(head,f,face,Point(0,0))
end
for _,t in ipairs({{'idle',1,2},{'walk_down',3,6},{'walk_up',7,10},{'walk_side',11,14},{'peck',15,17},{'rest',18,19}}) do tag(chicken,t[1],t[2],t[3]) end
sl=chicken:newSlice(Rectangle(0,0,32,32)); sl.name='Feet support'; sl.pivot=Point(16,26)
chicken:saveAs(out..'chicken_v18.aseprite'); chicken:saveCopyAs(out..'chicken_v18.gif'); chicken:close()
for _,d in ipairs({{'boat_v18',76,68,4},{'chicken_v18',32,32,19}}) do
 local s=assert(app.open(out..d[1]..'.aseprite')); assert(s.width==d[2] and s.height==d[3] and #s.frames==d[4]); s:close()
end
print('PASS boat4x76x68/1250ms and chicken19x32x32; layered natives reopened; supports fixed')
