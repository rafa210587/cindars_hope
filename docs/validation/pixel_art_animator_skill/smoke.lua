local s=assert(app.open(app.params.source))
s:newFrame(1)
s.frames[1].duration=0.120
s.frames[2].duration=0.180
local t=s:newTag(1,2); t.name='timing-study'; t.aniDir=AniDir.FORWARD
s:saveAs(app.params.output); s:close()
local r=assert(app.open(app.params.output))
assert(#r.frames==2 and #r.tags==1)
assert(math.abs(r.frames[1].duration-0.120)<0.001)
assert(math.abs(r.frames[2].duration-0.180)<0.001)
assert(r.tags[1].fromFrame.frameNumber==1 and r.tags[1].toFrame.frameNumber==2)
r:close()
print('ANIMATION_METADATA_ROUNDTRIP_PASS')
