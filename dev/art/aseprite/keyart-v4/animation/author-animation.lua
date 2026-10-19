local base='dev/art/aseprite/keyart-v4/animation/'
local assets='Assets/_Game/Art/Generated/World/'
local house=assert(app.open(assets..'building/farmhouse_keyart_v4.png'))
local original=Image(house.cels[1].image)
local door=Sprite(27,27,ColorMode.RGB)
local leaf=door.layers[1]; leaf.name='Hinged painted timber leaf'
local baseline=door:newLayer(); baseline.name='Source aperture - preserved hidden'; baseline.isVisible=false
local sourcePatch=Image(27,27,ColorMode.RGB); sourcePatch:clear()
for y=0,18 do for x=0,20 do sourcePatch:drawPixel(x+3,y+3,original:getPixel(111+x,146+y)) end end
door:newCel(baseline,1,sourcePatch,Point(0,0))
local widths={21,17,11,5,2}; local drops={0,1,2,3,3}
local poses={}
for f=1,5 do
  if f>1 then door:newEmptyFrame() end
  door.frames[f].duration=0.080
  local img=Image(27,27,ColorMode.RGB); img:clear()
  local w=widths[f]
  for x=0,w-1 do
    local sx=math.floor(x*20/math.max(1,w-1)+0.5)
    local dy=math.floor(x*drops[f]/math.max(1,w-1)+0.5)
    for y=0,18 do img:drawPixel(3+x,3+y+dy,original:getPixel(111+sx,146+y)) end
  end
  -- The exposed narrow edge is picked from the source's warm timber frame.
  if f>1 then for y=5,18 do img:drawPixel(3+w-1,3+y+drops[f],original:getPixel(110,146+y)) end end
  door:newCel(leaf,f,img,Point(0,0)); poses[f]=Image(img)
end
local tag=door:newTag(1,5); tag.name='opening'; tag.aniDir=AniDir.FORWARD
local slice=door:newSlice(Rectangle(0,0,27,27)); slice.name='hinge_contact'; slice.pivot=Point(3,22)
door:saveAs(base..'farmhouse_door_open_v4.aseprite'); door:close()

local threshold=Sprite(27,27,ColorMode.RGB); threshold.layers[1].name='Dark aperture behind opening leaf'
local interior=Image(27,27,ColorMode.RGB); interior:clear()
for y=0,18 do for x=0,20 do
  -- Native shadow colors from beneath the source lintel; no invented palette.
  local c=original:getPixel(116+(x%7),146+(y%2))
  if y>=17 then c=original:getPixel(118+(x%6),164) end
  interior:drawPixel(3+x,3+y,c)
end end
threshold:newCel(threshold.layers[1],1,interior,Point(0,0))
local ts=threshold:newSlice(Rectangle(0,0,27,27)); ts.name='hinge_contact'; ts.pivot=Point(3,22)
threshold:saveAs(base..'farmhouse_door_threshold_v4.aseprite')
threshold:saveCopyAs(assets..'props/farmhouse_door_threshold_v4.png'); threshold:close()

local playback=Sprite(212,205,ColorMode.RGB)
playback.layers[1].name='Approved house unchanged'
local shadow=playback:newLayer(); shadow.name='Aperture cover'
local animated=playback:newLayer(); animated.name='Door opening and closing'
local order={1,2,3,4,5,5,4,3,2,1}
for f,index in ipairs(order) do
  if f>1 then playback:newEmptyFrame() end
  playback.frames[f].duration=(f==1 or f==6 or f==10) and 0.5 or 0.08
  playback:newCel(playback.layers[1],f,Image(original),Point(0,0))
  playback:newCel(shadow,f,Image(interior),Point(108,143))
  playback:newCel(animated,f,Image(poses[index]),Point(108,143))
end
playback:saveAs(base..'farmhouse-door-context.aseprite')
playback:saveCopyAs(base..'farmhouse-door-context.gif'); playback:close(); house:close()

local waterSource=assert(app.open(assets..'tiles/ground_water_keyart_v4.png'))
local waterOriginal=Image(waterSource.cels[1].image)
local water=Sprite(64,64,ColorMode.RGB); local waves=water.layers[1]; waves.name='Coherent one-pixel surface waves'
local waterBaseline=water:newLayer(); waterBaseline.name='Approved frame zero - preserved hidden'; waterBaseline.isVisible=false
water:newCel(waterBaseline,1,Image(waterOriginal),Point(0,0))
local function nearest(value) return math.floor(value+0.5) end
for f=1,6 do
  if f>1 then water:newEmptyFrame() end
  water.frames[f].duration=0.2
  local img=Image(64,64,ColorMode.RGB)
  local phase=(f-1)*math.pi/3
  for y=0,63 do
    -- A long, periodic ripple transports existing pixels horizontally. The
    -- relative phase makes frame zero byte-identical at the pixel level.
    local rowPhase=y*math.pi/16
    local offset=nearest(1.15*math.sin(rowPhase+phase))-nearest(1.15*math.sin(rowPhase))
    for x=0,63 do img:drawPixel(x,y,waterOriginal:getPixel((x-offset+64)%64,y)) end
  end
  water:newCel(waves,f,img,Point(0,0))
end
local wt=water:newTag(1,6); wt.name='surface_loop'; wt.aniDir=AniDir.FORWARD
water:saveAs(base..'ground_water_loop_v4.aseprite'); water:saveCopyAs(base..'ground_water_loop_v4.gif')
water:close(); waterSource:close()

-- Reopen the actual deliverables, validating native timing and layer state.
for _,v in ipairs({{'farmhouse_door_open_v4.aseprite',5,0.08,27},{'ground_water_loop_v4.aseprite',6,0.2,64}}) do
  local s=assert(app.open(base..v[1])); assert(#s.frames==v[2] and s.width==v[4] and s.height==v[4])
  assert(#s.tags==1 and not s.layers[2].isVisible)
  for _,frame in ipairs(s.frames) do assert(math.abs(frame.duration-v[3])<0.00001) end
  s:close()
end
print('PASS: reopened frame counts, square canvases, durations, tags, hidden preserved baselines')
