// Offline geometry evidence; this does not execute Unity physics or transitions.
const fs = require('fs');
const assert = require('assert/strict');
const path = require('path');
const add = (a,b) => a.map((x,i)=>x+b[i]);
const sub = (a,b) => a.map((x,i)=>x-b[i]);
const mul = (a,k) => a.map(x=>x*k);
const mag = a => Math.hypot(...a);
function ribbon(points,width,factors=points.map(()=>1)) {
  const samples=[], widths=[];
  for(let span=0;span<points.length-1;span++) {
    const a=points[Math.max(0,span-1)],b=points[span],c=points[span+1],d=points[Math.min(points.length-1,span+2)];
    const count=Math.max(2,Math.ceil(mag(sub(b,c))/.5));
    for(let step=0;step<count;step++) {
      const t=step/count;
      samples.push(b.map((v,i)=>.5*(2*v+(-a[i]+c[i])*t+(2*a[i]-5*v+4*c[i]-d[i])*t*t+(-a[i]+3*v-3*c[i]+d[i])*t*t*t)));
      widths.push(width*(factors[span]+(factors[span+1]-factors[span])*t*t*(3-2*t)));
    }
  }
  samples.push(points.at(-1));widths.push(width*factors.at(-1));
  const left=[],right=[];
  samples.forEach((sample,i)=>{
    const d=sub(samples[Math.min(i+1,samples.length-1)],samples[Math.max(0,i-1)]);
    const normal=mul([-d[1],d[0]],widths[i]/(2*mag(d)));
    left.push(add(sample,normal));right.push(sub(sample,normal));
  });
  return {polygon:left.concat(right.reverse()),samples};
}
function clip(poly,rect) {
  let result=poly;
  for(const [axis,bound,sign] of [[0,rect[0],1],[0,rect[2],-1],[1,rect[1],1],[1,rect[3],-1]]) {
    const next=[];
    result.forEach((b,i)=>{
      const a=result[(i+result.length-1)%result.length];
      const ain=sign*(a[axis]-bound)>=0, bin=sign*(b[axis]-bound)>=0;
      if(ain!==bin) { const t=(bound-a[axis])/(b[axis]-a[axis]); next.push(add(a,mul(sub(b,a),t))); }
      if(bin)next.push(b);
    });
    result=next;
  }
  return result;
}
const area=poly=>Math.abs(poly.reduce((sum,a,i)=>{const b=poly[(i+1)%poly.length];return sum+a[0]*b[1]-b[0]*a[1]},0))*.5;
const player={width:.640625,height:1.125};
const inflated=rect=>[rect[0]-player.width/2,rect[1]-player.height,rect[2]+player.width/2,rect[3]];
const rect=(x,y,w,h)=>[x-w/2,y-h/2,x+w/2,y+h/2];
const spur=ribbon([[-12.8,15.3],[-9.2,16.1],[-4.2,15.5],[-.2,13.8],[1.5,10.6]],.9);
const well={};
for(const [name,box] of Object.entries({north:rect(-4.2,14.3,2.7,.28),east:rect(-1.8,13.1,.28,1.05)})) {
  const intersection=clip(spur.polygon,inflated(box));
  const overlapArea=area(intersection);
  assert(overlapArea<1e-10, name+' fence intersects full road ribbon/player sweep');
  well[name]={bounds:box,roadPlayerSweepOverlapArea:overlapArea};
}
const pixel=6.75/419;
const world=([x,y])=>[-22.75+(x-265.5)*pixel,20.1+y*pixel];
const sourceRect=(a,b)=>world(a).concat(world(b));
const west=sourceRect([12,43],[200,205]),east=sourceRect([337,43],[515,205]);
const openingWidth=east[0]-west[2];
const openingFeet=[west[2]+player.width/2,east[0]-player.width/2];
assert(openingWidth>player.width);
const overlaps=(feet,box)=>feet[0]>box[0]-player.width/2 && feet[0]<box[2]+player.width/2 && feet[1]>box[1]-player.height && feet[1]<box[3];
for(let y=18;y<22.2;y+=.025) {
  assert(!overlaps([-22.75,y],west));assert(!overlaps([-22.75,y],east));
}
assert(overlaps([-25,21],west));assert(overlaps([-20,21],east));
const caveTrigger=[west[2],18.35,east[0],world([337,115])[1]];
// Every side-base rear reaches the permanent cliff, so a player cannot slip behind a jamb.
const localBoundary=[[-29,18.5],[-26,22.4],[-22.75,23.5],[-18,22.4],[-8,18]];
for(const box of [west,east]) for(let x=box[0];x<=box[2];x+=.01) {
  const i=localBoundary.findIndex((a,i)=>i<localBoundary.length-1 && x>=a[0] && x<=localBoundary[i+1][0]);
  assert(i>=0);
  const a=localBoundary[i],b=localBoundary[i+1],boundaryY=a[1]+(b[1]-a[1])*(x-a[0])/(b[0]-a[0]);
  assert(box[3]>=boundaryY,'Gap behind a cave side base');
}
const closestDistance=(p,r)=>Math.hypot(Math.max(r[0]-p[0],0,p[0]-r[2]),Math.max(r[1]-p[1],0,p[1]-r[3]));
assert(closestDistance([-22.5,18.5],caveTrigger)<=.45);
const townTrigger=rect(34.5,3,1.8,1.8),townBarrier=rect(36.175,3,.35,3);
const townContact=[townBarrier[0]-player.width/2,3];
const townDistance=closestDistance(townContact,townTrigger);
assert(townDistance<=.45);
const primaryStart=ribbon([[-22.75,20.1],[-20,18.2],[-16,17.1],[-12.8,15.3],[-11.5,11.7],[-8.4,10.2]],1.3,[.95,1.1,.85,1,1.05,.9]);
const centerlineOverlaps=primaryStart.samples.filter(p=>overlaps(p,west)||overlaps(p,east));
const caveRoadShoulderOverlap=area(clip(primaryStart.polygon,inflated(west)))+area(clip(primaryStart.polygon,inflated(east)));
assert.equal(centerlineOverlaps.length,0,'Existing cave trail centerline is blocked');
const report={kind:'Static math only, exact authored ribbon polygon vs feet-collider Minkowski rectangles',player,well,cave:{pixel,west,east,openingWidth,openingFeet,trigger:caveTrigger,centerlineOverlaps,caveRoadShoulderOverlap,thresholdSourceBottomLeft:[265.5,43],thresholdWorld:world([265.5,43])},town:{barrier:townBarrier,trigger:townTrigger,contact:townContact,selectionDistance:townDistance,nativePostWidth:6/18.285714,nativeRailThickness:3/18.285714}};
fs.writeFileSync(path.join(__dirname,'entrance-geometry-report.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify(report,null,2));
