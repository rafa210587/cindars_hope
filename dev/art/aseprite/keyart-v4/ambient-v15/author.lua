local out='dev/art/aseprite/keyart-v4/ambient-v15/'
local root='Assets/_Game/Art/Generated/World/props/'
local pc=app.pixelColor
local function rgba(r,g,b) return pc.rgba(r,g,b,255) end
local function round(v) return math.floor(v+0.5) end
local function createAmbient(name,sourceName,isFountain)
 local src=assert(app.open(root..sourceName..'.png')); local original=Image(src.cels[1].image)
 local w,h=src.width,src.height; local mask={}; local palette={}
 local function inWater(x,y)
  if x<0 or y<0 or x>=w or y>=h then return false end
  return mask[y*w+x]==true
 end
 for y=0,h-1 do for x=0,w-1 do
  local p=original:getPixel(x,y); local r,g,b=pc.rgbaR(p),pc.rgbaG(p),pc.rgbaB(p)
  local selected=pc.rgbaA(p)>0 and g>r*(isFountain and 1.22 or 1.06) and b>r*(isFountain and 1.28 or 1.05) and b>g*0.80
  if isFountain then
   local central=x>=71 and x<=89 and y>=3 and y<=80
   local arms=(x>=51 and x<=70 or x>=89 and x<=110) and y>=37 and y<=88
   local pool=((x-80)/40)^2+((y-83)/24)^2<=1
   selected=selected and (central or arms or pool)
  end
  mask[y*w+x]=selected
  if selected then palette[#palette+1]=p end
 end end
 table.sort(palette,function(a,b) return pc.rgbaR(a)+pc.rgbaG(a)+pc.rgbaB(a)<pc.rgbaR(b)+pc.rgbaG(b)+pc.rgbaB(b) end)
 local highlight=palette[math.floor(#palette*0.88)]; local soft=palette[math.floor(#palette*0.70)]
 local s=Sprite(w,h,ColorMode.RGB); s.layers[1].name='Structure - invariant'
 local structure=Image(original); local baseWater=Image(w,h,ColorMode.RGB); baseWater:clear()
 for y=0,h-1 do for x=0,w-1 do if inWater(x,y) then structure:drawPixel(x,y,0); baseWater:drawPixel(x,y,original:getPixel(x,y)) end end end
 local water=s:newLayer(); water.name=isFountain and 'Water jets glow and pool ripples' or 'Descending water and foam'
 local baseline=s:newLayer(); baseline.name='Approved source - preserved hidden'; baseline.isVisible=false
 s:newCel(baseline,1,Image(original),Point(0,0))
 local streams=isFountain and {{79,19,73,1},{60,48,81,-1},{98,48,83,-1}} or {{13,12,27,-1},{23,17,32,-1},{32,21,36,-1},{42,24,40,-1},{49,24,41,-1}}
 for f=1,5 do
  if f>1 then s:newEmptyFrame() end
  s.frames[f].duration=0.14
  s:newCel(s.layers[1],f,Image(structure),Point(0,0))
  local img=Image(baseWater); local phase=f-1
  for _,stream in ipairs(streams) do
   for y=stream[2],stream[3] do
    local travel=(y-stream[2]+phase*2*stream[4])%10
    if travel<2 then for x=stream[1]-1,stream[1]+1 do
     if inWater(x,y) then img:drawPixel(x,y,x==stream[1] and highlight or soft) end
    end end
   end
  end
  if isFountain then
   -- Three anchored impacts emit shallow widening arcs, never moving the basin.
   for _,center in ipairs({{60,82},{80,74},{98,83}}) do
    local radius=3+phase
    for side=-1,1,2 do for dx=-radius,radius do
     local dy=round(side*1.5*math.sqrt(math.max(0,1-(dx/radius)^2)))
     local x,y=center[1]+dx,center[2]+dy
     if inWater(x,y) and math.abs(dx)>radius-3 then img:drawPixel(x,y,phase<3 and highlight or soft) end
    end end
   end
   -- The existing star silhouette holds; only its native cyan interior breathes.
   if phase==1 or phase==2 then for y=7,16 do for x=76,84 do
    if inWater(x,y) and (x==79 or x==80) then img:drawPixel(x,y,phase==2 and highlight or soft) end
   end end end
  else
   -- Foam accents travel right/down along the fixed lip; rocks outside the mask never change.
   for i,center in ipairs({{13,27},{24,33},{35,38},{46,40}}) do
    local dx=(phase+i)%5-2
    for k=0,2 do local x,y=center[1]+dx+k,center[2]+math.floor((phase+i)%3/2)
     if inWater(x,y) then img:drawPixel(x,y,k==1 and highlight or soft) end
    end
   end
  end
  s:newCel(water,f,img,Point(0,0))
 end
 local tag=s:newTag(1,5); tag.name=isFountain and 'fountain_loop' or 'descending_flow'; tag.aniDir=AniDir.FORWARD
 s:saveAs(out..name..'.aseprite'); s:saveCopyAs(out..name..'.gif'); s:close(); src:close()
 local reopened=assert(app.open(out..name..'.aseprite')); assert(#reopened.frames==5 and reopened.width==w and reopened.height==h)
 for f=1,5 do
  assert(math.abs(reopened.frames[f].duration-.14)<.00001)
  local rendered=Image(w,h,ColorMode.RGB); rendered:clear(); rendered:drawSprite(reopened,f)
  for y=0,h-1 do for x=0,w-1 do
   if not inWater(x,y) then assert(rendered:getPixel(x,y)==original:getPixel(x,y),'Structure changed') end
   assert(pc.rgbaA(rendered:getPixel(x,y))==pc.rgbaA(original:getPixel(x,y)),'Alpha silhouette changed')
  end end
 end
 local maskPreview=Sprite(w,h,ColorMode.RGB); maskPreview:newCel(maskPreview.layers[1],1,baseWater,Point(0,0)); maskPreview:saveCopyAs(out..name..'-water-mask.png'); maskPreview:close()
 reopened:close(); print('PASS '..name..':5frames140ms, structure RGBA and alpha invariant, source mask pixels '..#palette)
end
createAmbient('fountain_ambient_v15','fountain_keyart_v3',true)
createAmbient('cascade_ambient_v15','river_cascade_keyart_v4',false)

local fish=Sprite(48,48,ColorMode.RGB); fish.layers[1].name='Fish jump - body and tail'
local splash=fish:newLayer(); splash.name='Launch impact and dissipating splash'
local outline=rgba(18,55,61); local back=rgba(48,94,87); local body=rgba(106,155,122); local belly=rgba(185,204,157)
local foam=rgba(180,224,224); local water=rgba(68,139,157)
local poses={{13,29,-65},{18,18,-45},{25,12,5},{31,19,48},{35,31,78}}
for f=1,8 do
 if f>1 then fish:newEmptyFrame() end
 fish.frames[f].duration=.12
 local img=Image(48,48,ColorMode.RGB); img:clear(); local fx=Image(48,48,ColorMode.RGB); fx:clear()
 if f<=5 then
  local pose=poses[f]; local angle=pose[3]*math.pi/180
  local function dot(x,y,c)
   local px=round(pose[1]+x*math.cos(angle)-y*math.sin(angle)); local py=round(pose[2]+x*math.sin(angle)+y*math.cos(angle))
   if px>=0 and px<48 and py>=0 and py<48 then img:drawPixel(px,py,c) end
  end
  for x=-3,2 do dot(x,-1,back); dot(x,0,body); dot(x,1,belly) end
  for x=-2,1 do dot(x,-2,outline) end
  dot(3,0,body); dot(2,-1,outline); dot(3,1,outline)
  dot(-4,0,back); dot(-5,-1,body); dot(-5,1,body); dot(-5,-2,outline); dot(-5,2,outline)
  if f==1 then for _,p in ipairs({{10,33},{11,32},{15,33},{17,34}}) do fx:drawPixel(p[1],p[2],water) end end
 elseif f==6 then
  for _,p in ipairs({{30,32},{31,31},{32,30},{34,28},{35,27},{37,29},{38,31},{40,32},{31,34},{33,35},{36,35},{39,34}}) do fx:drawPixel(p[1],p[2],foam) end
  for x=32,38 do fx:drawPixel(x,34,water) end
 elseif f==7 then
  for _,p in ipairs({{28,34},{29,33},{30,33},{33,36},{34,36},{38,35},{41,33},{42,34}}) do fx:drawPixel(p[1],p[2],water) end
 end -- frame8 is deliberately completely transparent; Unity owns the20second wait.
 fish:newCel(fish.layers[1],f,img,Point(0,0)); fish:newCel(splash,f,fx,Point(0,0))
end
local tag=fish:newTag(1,8); tag.name='jump_splash'; tag.aniDir=AniDir.FORWARD
local slice=fish:newSlice(Rectangle(0,0,48,48)); slice.name='Water contact'; slice.pivot=Point(24,34)
fish:saveAs(out..'fish_jump_v15.aseprite'); fish:saveCopyAs(out..'fish_jump_v15.gif'); fish:close()
print('PASS fish authored:8frames120ms, finalframe transparent, contact24,34top')
