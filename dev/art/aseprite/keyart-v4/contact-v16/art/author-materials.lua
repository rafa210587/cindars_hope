local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local output=root..'dev/art/aseprite/keyart-v4/contact-v16/art/'
local pc=app.pixelColor
local function rgba(r,g,b) return pc.rgba(r,g,b,255) end
local function load(path)
 local s=assert(app.open(root..path)); local img=Image(s.width,s.height,ColorMode.RGB); img:drawSprite(s,1); s:close(); return img
end
local function save(img,name)
 local s=Sprite(img.width,img.height,ColorMode.RGB); s:newCel(s.layers[1],1,img,Point(0,0)); s:saveCopyAs(output..name..'.png'); s:close()
end
local function resample(img,w,h)
 local out=Image(w,h,ColorMode.RGB)
 for y=0,h-1 do for x=0,w-1 do out:drawPixel(x,y,img:getPixel(math.min(img.width-1,math.floor((x+.5)*img.width/w)),math.min(img.height-1,math.floor((y+.5)*img.height/h)))) end end
 return out
end
local function inside(x,y,poly)
 local hit=false; local j=#poly
 for i=1,#poly do local a,b=poly[i],poly[j]
  if ((a[2]>y)~=(b[2]>y)) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then hit=not hit end; j=i
 end; return hit
end
local function layered(base,patch,name,baselineName)
 local s=Sprite(base.width,base.height,ColorMode.RGB)
 s.layers[1].name=baselineName; s:newCel(s.layers[1],1,base,Point(0,0))
 local l=s:newLayer(); l.name='Authored quiet ground pockets and grouped accents'; s:newCel(l,1,patch,Point(0,0))
 s:saveAs(output..name..'.aseprite'); s:saveCopyAs(output..name..'.png'); s:close()
 local reopened=assert(app.open(output..name..'.aseprite')); assert(#reopened.layers==2 and reopened.width==base.width)
 local out=Image(base.width,base.height,ColorMode.RGB); out:drawSprite(reopened,1); reopened:close(); return out
end
local road=load('Assets/_Game/Art/Generated/World/tiles/ground_path_aseprite_v1.png')
local grass=load('Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png')
local roadPatch=Image(64,64,ColorMode.RGB); roadPatch:clear()
-- Interlocking irregular quiet sand pockets, kept away from the four-pixel seam ring.
local roadPockets={
 {{5,7},{17,5},{24,9},{23,16},{17,18},{7,15}},
 {{29,6},{43,5},{48,12},{43,19},{32,18},{27,13}},
 {{7,24},{19,21},{28,27},{26,36},{15,38},{6,32}},
 {{36,24},{50,20},{58,27},{55,35},{45,38},{34,33}},
 {{6,44},{17,41},{27,46},{28,55},{19,59},{6,55}},
 {{36,45},{47,41},{59,44},{59,55},{50,59},{35,56}}
}
local sand={rgba(184,135,36),rgba(181,131,34),rgba(186,136,37),rgba(182,132,35),rgba(185,134,36),rgba(180,130,34)}
for i,poly in ipairs(roadPockets) do
 for y=4,59 do for x=4,59 do if inside(x,y,poly) then
  local p=road:getPixel(x,y); local r,g=pc.rgbaR(p),pc.rgbaG(p)
  -- Keep connected green edge flecks and the deepest stone-shadow fragments.
  if not (g>r*.84 and r<138) and r>116 then roadPatch:drawPixel(x,y,sand[i]) end
 end end end
end
-- Six authored compact gravel marks, unequal footprints, no uniform scatter.
for _,a in ipairs({{14,13,3},{39,14,2},{18,31,4},{49,29,2},{21,50,2},{43,52,3}}) do
 for dx=0,a[3]-1 do roadPatch:drawPixel(a[1]+dx,a[2],rgba(151,111,29)) end
 roadPatch:drawPixel(a[1],a[2]+1,rgba(170,125,33))
end
local roadFinal=layered(road,roadPatch,'ground_path_contact_v16','Original shared road - preserved')
-- Chosen quiet areas lie between existing tufts. Nested lobe offsets make irregular shapes,
-- leaving the original dark green tuft bases and isolated multi-pixel shadows intact.
local pockets={
 {28,24,23,13},{88,23,21,13},{150,28,25,14},{219,25,22,14},{279,36,21,13},
 {45,73,24,14},{113,68,23,15},{187,75,27,17},{256,84,25,14},
 {23,124,19,17},{85,122,28,17},{157,121,23,15},{226,130,26,17},{288,137,19,15},
 {44,178,26,17},{113,176,29,16},{184,184,25,18},{257,181,26,16},
 {26,237,22,17},{90,234,26,18},{161,241,26,18},{234,237,28,17},{291,245,17,19},
 {55,284,25,17},{124,282,24,18},{193,285,29,17},{267,289,23,14}}
local function grassPatch(base)
 local patch=Image(base.width,base.height,ColorMode.RGB); patch:clear(); local scale=314/base.width
 for y=0,base.height-1 do for x=0,base.width-1 do
  local u,v=(x+.5)*scale,(y+.5)*scale
  -- Native four-pixel edge remains exact in the selected314 candidate.
  if u>=4 and v>=4 and u<310 and v<310 then
   local p=base:getPixel(x,y); local r,g,b=pc.rgbaR(p),pc.rgbaG(p),pc.rgbaB(p)
   for i,a in ipairs(pockets) do
    local dx,dy=(u-a[1])/a[3],(v-a[2])/a[4]
    local lobe=((u-a[1]-a[3]*.55)/(a[3]*.65))^2+((v-a[2]+a[4]*.3)/(a[4]*.7))^2
    if dx*dx+dy*dy<.83 or lobe<.75 then
     -- Strong green tufts and warm bare-earth islands are left in their existing place.
     if g-r<8 and r<112 and r>53 and b<32 then
      local c
      if r<72 then c=rgba(67,74,12)
      elseif r>94 then c=rgba(91,93,16)
      else c=(i%3==0) and rgba(81,85,13) or rgba(83,86,13) end
      patch:drawPixel(x,y,c)
     end; break
    end
   end
  end
 end end
 return patch
end
local grass314=resample(grass,314,314)
local selected=layered(grass314,grassPatch(grass314),'ground_grass_contact_v16','Original source nearest sample314 - source file preserved')
local full=layered(grass,grassPatch(grass),'ground_grass_contact_v16_fullres_comparison','Original1254 grass - preserved')
-- Offline camera-density board. Nearest sampling only; same world footprint across rows.
local board=Image(960,650,ColorMode.RGB); board:clear(rgba(29,34,30))
local function draw(img,x,y,w,h) board:drawImage(resample(img,w,h),Point(x,y)) end
local function groundTint(img)
 local out=Image(img.width,img.height,ColorMode.RGB)
 for y=0,img.height-1 do for x=0,img.width-1 do local p=img:getPixel(x,y)
  out:drawPixel(x,y,rgba(math.floor(pc.rgbaR(p)*.8+.5),math.floor(pc.rgbaG(p)*.85+.5),math.floor(pc.rgbaB(p)*.78+.5)))
 end end; return out
end
draw(groundTint(grass),12,12,277,277); draw(groundTint(full),324,12,277,277); draw(groundTint(selected),636,12,277,277)
-- Roads repeated4x4 so their2u period can be evaluated rather than enlarged for inspection.
local function repeated(img)
 local out=Image(256,256,ColorMode.RGB)
 for y=0,3 do for x=0,3 do out:drawImage(img,Point(x*64,y*64)) end end; return out
end
draw(repeated(road),12,334,226,226); draw(repeated(roadFinal),324,334,226,226)
-- An unchanged actual gameplay image excerpt is contextual evidence, not a simulated scene.
local shot=load('docs/validation/farm_keyart_v4/gameplay_stage15/homestead.png')
local context=Image(290,290,ColorMode.RGB)
for y=0,289 do for x=0,289 do context:drawPixel(x,y,shot:getPixel(x+165,y+95)) end end
board:drawImage(context,Point(636,334)); save(board,'offline-camera-density-board')
print('PASS authored road64, grass314, fullres comparison1254; native files reopened; offline board exported')
