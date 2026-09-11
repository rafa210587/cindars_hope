local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local out=root..'dev/art/aseprite/keyart-v4/herd-v19/'
local pc=app.pixelColor
local function rgb(r,g,b) return pc.rgba(r,g,b,255) end
local C={edge=rgb(51,42,28),dark=rgb(67,58,40),darklit=rgb(91,82,58),cream=rgb(214,202,162),light=rgb(240,226,183),
 shade=rgb(161,148,113),wool=rgb(224,213,175),woolshade=rgb(184,172,133),horn=rgb(185,165,116),
 tan=rgb(166,130,80),tanlight=rgb(196,161,105),tanshade=rgb(120,90,51),muzzle=rgb(170,128,100),eye=rgb(26,23,17)}
local function img() local i=Image(48,48,ColorMode.RGB); i:clear(); return i end
local function dot(i,x,y,c) if x>=0 and y>=0 and x<48 and y<48 then i:drawPixel(x,y,c) end end
local function rect(i,x,y,w,h,c) for yy=y,y+h-1 do for xx=x,x+w-1 do dot(i,xx,yy,c) end end end
local function ellipse(i,cx,cy,rx,ry,c)
 for y=cy-ry,cy+ry do for x=cx-rx,cx+rx do if ((x-cx)/rx)^2+((y-cy)/ry)^2<=1 then dot(i,x,y,c) end end end
end
local function line(i,x0,y0,x1,y1,c)
 local dx,dy=math.abs(x1-x0),-math.abs(y1-y0); local sx,sy=x0<x1 and 1 or -1,y0<y1 and 1 or -1; local err=dx+dy
 while true do dot(i,x0,y0,c); if x0==x1 and y0==y1 then break end; local e=err*2
  if e>=dy then err=err+dy;x0=x0+sx end;if e<=dx then err=err+dx;y0=y0+sy end
 end
end
local function leg(i,x,top,bottom,wide,color)
 rect(i,x,top,wide,bottom-top,C.edge); rect(i,x,top,math.max(1,wide-1),bottom-top-1,color)
 rect(i,x-1,bottom,wide+1,1,C.dark)
end
local function woolBody(i,cx,cy,rx,ry)
 ellipse(i,cx,cy,rx,ry,C.edge); ellipse(i,cx,cy-1,rx-1,ry-1,C.woolshade)
 for _,d in ipairs({{-7,-5,4},{0,-7,5},{7,-5,4},{-9,0,3},{8,0,4},{-5,3,4},{2,2,5}}) do
  ellipse(i,cx+d[1],cy+d[2],d[3],3,C.wool)
 end
 rect(i,cx-3,cy-7,4,1,C.light); rect(i,cx+5,cy-4,3,1,C.light)
end
local function cowHead(i,x,y,back,blink)
 -- Broad bovine forehead, short ivory horns and a wide muzzle.
 line(i,x-5,y-5,x-8,y-9,C.horn); dot(i,x-8,y-10,C.light)
 line(i,x+5,y-5,x+8,y-9,C.horn); dot(i,x+8,y-10,C.light)
 ellipse(i,x-7,y-1,3,2,C.dark); ellipse(i,x+7,y-1,3,2,C.dark)
 dot(i,x-8,y-1,C.shade);dot(i,x+8,y-1,C.shade)
 ellipse(i,x,y,6,8,C.edge); ellipse(i,x,y,5,7,C.darklit)
 rect(i,x-1,y-6,3,7,C.cream);rect(i,x,y-7,2,2,C.light)
 if not back then
  dot(i,x-3,y,blink and C.shade or C.eye);dot(i,x+3,y,blink and C.shade or C.eye)
  ellipse(i,x,y+5,6,3,C.muzzle);rect(i,x-3,y+5,2,1,C.dark);rect(i,x+2,y+5,2,1,C.dark)
 else ellipse(i,x,y+3,4,4,C.dark);rect(i,x-1,y-5,2,5,C.shade) end
end
local function smallHead(i,kind,x,y,back,blink,side,graze)
 if kind=='goat' then
  -- Tall swept horns and pointed beard, distinct from the sheep's wool cap.
  if not back and not side then
   -- Frontal foreshortening must not hide the long-horned identity visible from back/side.
   line(i,x-2,y-5,x-5,y-17,C.horn);dot(i,x-5,y-18,C.light)
   line(i,x+2,y-5,x+5,y-17,C.horn);dot(i,x+5,y-18,C.light)
  else
   line(i,x-2,y-5,x-5,y-12,C.horn);dot(i,x-5,y-13,C.light)
   line(i,x+2,y-5,x,y-12,C.horn)
  end
  rect(i,x-7,y-3,5,2,C.tanshade);rect(i,x+3,y-3,4,2,C.tanshade)
  if not back and not side then rect(i,x-7,y-3,2,1,C.tanlight);rect(i,x+5,y-3,2,1,C.tanlight) end
  ellipse(i,x,y,4,6,C.edge);ellipse(i,x,y-1,3,5,C.tanlight)
  if not back then
   if side then dot(i,x+1,y-2,blink and C.tanshade or C.eye)
   else dot(i,x-2,y-2,blink and C.tanshade or C.eye);dot(i,x+2,y-2,blink and C.tanshade or C.eye) end
   ellipse(i,x+(side and 2 or 0),y+3,3,2,C.tanshade)
   rect(i,x,y+5,2,4,C.dark);dot(i,x,y+9,C.darklit)
  else rect(i,x-1,y-3,2,6,C.tanshade) end
 else
  ellipse(i,x-4,y-2,3,2,C.shade);ellipse(i,x+4,y-2,3,2,C.shade)
  ellipse(i,x,y,4,5,C.edge);ellipse(i,x,y-1,3,4,C.shade)
  ellipse(i,x,y-4,4,2,C.wool)
  if not back then
   if side then dot(i,x+1,y-1,blink and C.woolshade or C.eye)
   else dot(i,x-2,y-1,blink and C.woolshade or C.eye);dot(i,x+2,y-1,blink and C.woolshade or C.eye) end
   ellipse(i,x+(side and 2 or 0),y+3,3,2,C.tanshade)
  else ellipse(i,x,y-1,2,3,C.woolshade) end
 end
end
local function cowSideBody(i,bob,rest)
 local cy=rest and 33 or 24+bob; local ry=rest and 6 or 9
 ellipse(i,23,cy,14,ry,C.edge);ellipse(i,23,cy-1,13,ry-1,C.cream)
 ellipse(i,15,cy-2,5,math.max(2,ry-3),C.dark);ellipse(i,28,cy-3,5,math.max(2,ry-4),C.dark)
 rect(i,28,cy-4,4,2,C.darklit);rect(i,21,cy-ry+1,4,1,C.light)
 ellipse(i,21,cy+ry-2,4,2,C.shade)
 if not rest then
  line(i,10,21+bob,7,29+bob,C.shade);line(i,7,29+bob,6,34+bob,C.dark);rect(i,5,33+bob,2,3,C.dark)
 end
end
local function cowSideHead(i,bob,graze,rest,blink)
 if graze==1 then
  -- Lowered muzzle meets top-origin groundline40; horns stay folded above it.
  rect(i,34,27,4,7,C.dark);ellipse(i,38,34,4,5,C.darklit);ellipse(i,40,38,5,2,C.muzzle)
  dot(i,39,33,C.eye);line(i,36,29,33,25,C.horn);line(i,40,29,39,24,C.horn)
 else
  local y=rest and 27 or graze==0 and 27 or graze==2 and 22 or 19+bob
  rect(i,32,y,5,9,C.dark);ellipse(i,37,y,5,7,C.edge);ellipse(i,37,y-1,4,6,C.darklit)
  rect(i,36,y-5,2,6,C.cream);ellipse(i,40,y+4,5,3,C.muzzle);dot(i,40,y-1,blink and C.shade or C.eye)
  ellipse(i,32,y-4,3,2,C.dark)
  line(i,35,y-6,33,y-8,C.horn);dot(i,33,y-9,C.light)
  line(i,40,y-5,42,y-7,C.horn);dot(i,42,y-8,C.light)
  dot(i,43,y+4,C.dark)
 end
end
local function smallSideBody(i,kind,bob,rest)
 local cy=rest and 34 or (kind=='sheep' and 25 or 29)+bob
 if kind=='sheep' then
  if rest then
   ellipse(i,23,34,12,5,C.edge);ellipse(i,23,33,11,4,C.woolshade)
   for _,d in ipairs({{-7,0},{-2,-1},{4,-1},{8,1}}) do ellipse(i,23+d[1],32+d[2],4,3,C.wool) end
  else woolBody(i,23,cy,12,10) end
 else
  ellipse(i,23,cy,11,rest and 5 or 6,C.edge);ellipse(i,23,cy-1,10,rest and 4 or 5,C.tan)
  ellipse(i,21,cy-3,7,2,C.tanlight);rect(i,14,cy+2,5,2,C.tanshade)
  if not rest then line(i,13,cy-3,9,cy-7,C.tanlight);dot(i,9,cy-8,C.dark) end
 end
end
local function frontBody(i,kind,bob,back,rest)
 if kind=='cow' then
  ellipse(i,24,21+bob,10,14,C.edge);ellipse(i,24,20+bob,9,13,C.cream)
  ellipse(i,19,18+bob,4,7,C.dark);ellipse(i,29,25+bob,4,6,C.dark)
  rect(i,22,8+bob,3,2,C.light)
  if back then line(i,24,26+bob,24,35+bob,C.shade);rect(i,23,34+bob,3,3,C.dark) end
 elseif kind=='sheep' then
  woolBody(i,24,25+bob,11,10)
  if back then ellipse(i,24,32+bob,2,3,C.woolshade);rect(i,23,32+bob,2,2,C.light) end
 else
  ellipse(i,24,27+bob,8,9,C.edge);ellipse(i,24,26+bob,7,8,C.tan)
  rect(i,23,18+bob,2,12,C.tanlight);ellipse(i,20,29+bob,3,4,C.tanshade)
  if back then rect(i,23,31+bob,2,5,C.tanshade);dot(i,23,36+bob,C.dark) end
 end
end
local durationsBase={.4,.4,0,0,0,0,0,0,0,0,0,0,0,0,.22,.22,.22,.65,.65}
local kinds=app.params.animal and {app.params.animal} or {'cow','sheep','goat'}
for _,kind in ipairs(kinds) do
 local s=Sprite(48,48,ColorMode.RGB);s.layers[1].name='Legs and planted hooves'
 local body=s:newLayer();body.name=kind=='sheep' and 'Wool lobes and tail' or 'Coat silhouette and markings'
 local head=s:newLayer();head.name='Head muzzle ears and species features'
 for f=1,19 do
  if f>1 then s:newEmptyFrame() end;s.frames[f].duration=durationsBase[f]>0 and durationsBase[f] or (kind=='cow' and .18 or .16)
  local legs,coat,face=img(),img(),img();local phase=0;local bob=0
  local side=f>=11;local back=f>=7 and f<=10;local rest=f>=18;local graze=f>=15 and f<=17
  if f>=3 and f<=14 then phase=(f-3)%4;bob=(phase==1 or phase==3) and -1 or 0 end
  if not rest then
   local color=kind=='cow' and C.cream or kind=='sheep' and C.shade or C.tanlight
   if side then
    local moving=f<=14;local nearA=moving and (phase<2 and 0 or 2) or 0;local nearB=moving and (phase<2 and 2 or 0) or 0
    leg(legs,15+nearA,kind=='cow' and 28 or 31,39,kind=='cow' and 3 or 2,color)
    leg(legs,29-nearA,kind=='cow' and 28 or 31,39,kind=='cow' and 3 or 2,color)
    leg(legs,18-nearB,kind=='cow' and 28 or 31,moving and (phase==0 and 37 or 38) or 38,2,C.tanshade)
    leg(legs,31+nearB,kind=='cow' and 28 or 31,moving and (phase==2 and 37 or 38) or 38,2,C.tanshade)
   else
    local ly=(phase==0 or phase==1) and 39 or 37;local ry=(phase==2 or phase==3) and 39 or 37
    if f<=2 then ly=39;ry=39 end
    leg(legs,18,29,ly,kind=='cow' and 3 or 2,color);leg(legs,28,29,ry,kind=='cow' and 3 or 2,color)
    leg(legs,20,27,36,2,C.tanshade);leg(legs,27,27,36,2,C.tanshade)
   end
  end
  if side then
   if kind=='cow' then cowSideBody(coat,bob,rest);cowSideHead(face,bob,graze and f-15 or nil,rest,f==19)
   else
    smallSideBody(coat,kind,bob,rest)
    if graze and f==16 then
     -- Bent grazing neck remains within canvas and touches ground only at the muzzle.
     rect(face,32,29,3,7,C.shade);ellipse(face,36,35,3,4,kind=='goat' and C.tanlight or C.shade)
     ellipse(face,38,38,4,2,C.tanshade);dot(face,37,34,C.eye)
     if kind=='goat' then line(face,34,31,30,26,C.horn);line(face,36,31,34,25,C.horn);rect(face,35,38,1,2,C.dark)
     else ellipse(face,34,31,3,2,C.wool) end
    else
     local y=rest and (kind=='goat' and 30 or 32) or graze and (f==15 and 31 or 27) or (kind=='goat' and 25 or 28)+bob
     smallHead(face,kind,35,y,false,f==19,true,false)
    end
   end
  else
   frontBody(coat,kind,bob,back,false)
   if kind=='cow' then cowHead(face,24,back and 17+bob or 26+bob,back,f==2)
   else smallHead(face,kind,24,back and 24+bob or 29+bob,back,f==2,false,false) end
  end
  s:newCel(s.layers[1],f,legs,Point(0,0));s:newCel(body,f,coat,Point(0,0));s:newCel(head,f,face,Point(0,0))
 end
 for _,t in ipairs({{'idle',1,2},{'walk_down',3,6},{'walk_up',7,10},{'walk_side',11,14},{'graze',15,17},{'rest',18,19}}) do local tag=s:newTag(t[2],t[3]);tag.name=t[1];tag.aniDir=AniDir.FORWARD end
 local sl=s:newSlice(Rectangle(0,0,48,48));sl.name='Feet support';sl.pivot=Point(24,40)
 s:saveAs(out..kind..'_v19.aseprite');s:saveCopyAs(out..kind..'_v19.gif');s:close()
 local r=assert(app.open(out..kind..'_v19.aseprite'));assert(r.width==48 and r.height==48 and #r.frames==19 and #r.layers==3)
 for _,f in ipairs(r.frames) do assert(f.duration>0) end;r:close()
 print('PASS '..kind..'48x48,19frames,3layers,support24,40top')
end
