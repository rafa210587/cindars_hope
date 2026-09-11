local sprite=assert(app.open(app.params.source))
local x=tonumber(app.params.x); local y=tonumber(app.params.y)
local w=tonumber(app.params.w); local h=tonumber(app.params.h)
sprite:crop(x,y,w,h)
sprite.layers[1].name='Approved keyart crop'
sprite:saveAs(app.params.output)
sprite:saveCopyAs(app.params.preview)
sprite:close()
