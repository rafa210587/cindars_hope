// Analytical design model, not a Unity simulation. Run: node docs/validation/skills_balance_v1/balance-model.mjs
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import assert from 'node:assert/strict';
import { fileURLToPath } from 'node:url';
const outputDir = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(outputDir, '../../..');
const sources = [
 'Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs',
 'Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs',
 'Assets/_Game/Scripts/Skills/SkillTierRules.cs',
 'Assets/_Game/Scripts/Player/ManaManager.cs',
 'Assets/_Game/Scripts/Player/DerivedFollowupFormulas.cs',
 'Assets/_Game/Scripts/Economy/Pricing/PricingProfile.cs',
 'docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md',
 'docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md',
 'Assets/_Game/Scripts/Core/GameTimeManager.cs',
 'Assets/_Game/Scripts/Player/Progression/ProgressionCurve.cs',
 'Assets/_Game/Scripts/Skills/SkillPurchaseService.cs'
];
const input = Object.fromEntries(sources.map(p => [p, fs.readFileSync(path.join(root,p),'utf8')]));
const catalog = input[sources[0]];
assert(catalog.includes('mods: Mod(SkillModifierType.ManaRegenFlat, 1f)'));
assert(input[sources[3]].includes('DefaultManaRegenPerSecond = 2f'));
assert(input[sources[4]].includes('CraftTimeReductionCap = 0.75f'));
assert(input[sources[8]].includes('TickIntervalSeconds = 1f'));
for(const expected of ['CraftTimeReductionPercent, 0.1f','CraftTimeReductionPercent, 0.05f','CraftTimeReductionPercent, 0.15f']) assert(catalog.includes(expected));
const tiers = Object.fromEntries([...catalog.matchAll(/\{ "([^"]+)", ([1-5]) \}/g)].map(m=>[m[1],+m[2]]));
const calls = [...catalog.matchAll(/Node\("([^"]+)",\s*"([^"]+)"/g)];
const nodes = calls.map((m,i)=>{
 const body = catalog.slice(m.index,calls[i+1]?.index ?? catalog.length);
 return {id:m[1],tree:m[2],tier:tiers[m[1]]??1,prereq:body.match(/prereq: "([^"]+)"/)?.[1]??null,
  active: /unlockAction: "/.test(body),capstone:/isCapstone: true/.test(body)};
});
assert.equal(nodes.length,66);
assert.equal(nodes.filter(n=>n.active).length,31);
const thresholds = [0,0,5,11,18,26];
assert(input[sources[2]].includes('{ 0, 0, 5, 11, 18, 26 }'));
const cap = spent => spent<5?2:spent<11?3:spent<18?4:5;
for (const [effect, damage, cost, cd] of [
 ['combat.melee.offhand_cut',8,10,2.5],['combat.melee.whirl_cut',10,22,6],
 ['combat.ranged.line_piercer',12,18,7],['combat.ranged.multishot_fan',8,24,8],
 ['combat.magic.fire_spark',12,10,3],['magic.chama_breve',8,8,2.5]
]) {
 const row=input[sources[1]].split('\n').find(l=>l.includes(`"${effect}"`));
 assert(row, effect);
 assert.equal(+row.match(/baseDamage: (\d+)/)[1],damage);
 assert.equal(+row.match(/(?:staminaCost|resourceCost): (\d+)/)[1],cost);
 assert.equal(+row.match(/cooldownSeconds: ([\d.]+)f/)[1],cd);
}
// Construct a legal witness, not an optimizer. Validates current tier/prerequisite/rank rules.
function witness(tree, targetId, targetRank=1) {
 const local=nodes.filter(n=>n.tree===tree), rank={}, sequence=[];
 const byId=Object.fromEntries(local.map(n=>[n.id,n]));
 const ancestry=[];
 function visit(id){const n=byId[id];assert(n);if(n.prereq)visit(n.prereq);if(!ancestry.includes(id))ancestry.push(id);}
 visit(targetId);
 while((rank[targetId]??0)<targetRank && sequence.length<80){
  const spent=sequence.length;
  const legal=local.filter(n=>{
   const r=rank[n.id]??0;
   return r>0 ? r<cap(spent) : spent>=thresholds[n.tier] && (!n.prereq||rank[n.prereq]>0);
  });
  if(!legal.length)throw Error(`No legal purchase: ${tree}`);
  const pick=legal.find(n=>ancestry.includes(n.id)&&!rank[n.id])
   ??legal.find(n=>n.id===targetId&&(rank[n.id]??0)<targetRank)
   ??legal.find(n=>!n.capstone&&rank[n.id]>0)
   ??legal.find(n=>!n.capstone)??legal[0];
  rank[pick.id]=(rank[pick.id]??0)+1;
  sequence.push({point:spent+1,node:pick.id,rank:rank[pick.id]});
 }
 assert.equal(rank[targetId],targetRank);
 return {tree,target:targetId,targetRank,points:sequence.length,rank,sequence};
}
const capstones=nodes.filter(n=>n.capstone).map(n=>witness(n.tree,n.id));
const pricing=input[sources[5]];
const buy=+pricing.match(/PriceChannel.ShopSellToPlayer, ([\d.]+)f/)[1];
const sell=+pricing.match(/PriceChannel.SellPoint, ([\d.]+)f/)[1];
const round=x=>Math.round(x*1e6)/1e6;
const results={
 model:'Analytical bounds and structural witnesses; no enemy AI, damage mitigation, animation, Unity or human playtest',
 sourceHashes:Object.fromEntries(sources.map(p=>[p,crypto.createHash('sha256').update(input[p]).digest('hex')])),
 parameters:{tierThresholds:thresholds,pointCeiling:55,baseBuyMultiplier:buy,baseSellMultiplier:sell,
  manaBasePerTick:2,tickIntervalSeconds:1,markBonuses:[.05,.08,.12],markDurations:[8,10,12]},
 capstoneWitnesses:capstones,
 dualCapstone:{minimumArithmetic:2*(26+1),witnessPair:capstones.slice(0,2).reduce((a,x)=>a+x.points,0),budget:55,
  bothRankThree:2*(26+3)},
 // Rank deltas from numeric addendum; throughput ignores animation and misses, not total combat DPS.
 offensiveEfficiency:[
  ['offhand',8,2,2.5,10],['whirl',10,2,6,22],['line_piercer',12,3,7,18],
  ['multishot_per_arrow',8,2,8,24],['fire_spark',12,2,3,10],['chama_breve',8,2,2.5,8]
 ].map(([id,base,delta,cd,cost])=>({id,cost,cooldown:cd,
  R1:{damage:base,damagePerResource:round(base/cost),damagePerCooldownSecond:round(base/cd)},
  R3:{damage:base+2*delta,damagePerResource:round((base+2*delta)/cost),damagePerCooldownSecond:round((base+2*delta)/cd)}})),
 manaSustain:[0,1,2,3,5].map(rank=>({quickChannelRank:rank,regenPerSecond:2+rank,
  fireSparkSpendPerSecond:round(10/3),netPerSecond:round(2+rank-10/3),
  intendedPercentRegenAtRank:round(2*(1+.05*rank))})),
 markBreakEven:[0,1,2].map(i=>{const b=[.05,.08,.12][i],duration=[8,10,12][i];
  return {rank:i+1,bonus:b,duration,maxLostAttackTimeSeconds:round(b*duration/(1+b)),
   assumption:'Fixed target exposure horizon equal to listed duration, including cast; no ally contribution. Full duration after cast gives b*T instead.'};}),
 recoveryBounds:{
  threeDistinctHpSkillsPerMinute:round(60*(30/45+40/90+15/60)),
  uniqueLastBreathHpPerMinute:round(60*40/90),fourCopiesLastBreathHpPerMinute:round(4*60*40/90),
  safeCampMpPerMinute:15,instinctStaminaPerMinute:100,
  caveat:'Long-run capacity, not exact first-minute casts; excludes overheal and interruption. Three HP actions use three slots.'},
 aoeBounds:{pierceFourFlat:48,pierceFourWithTwentyPercentDecay:round(12*(1+.8+.8**2+.8**3)),
  fanOneArrow:8,fanThreeSameTarget:24,caveat:'Alternatives of collision/target policy, not measured hit probability'},
 craftThroughput:[0,1,2,3,5].map(rank=>({rankEach:rank,
  rawReduction:round((.10+.05+.15)*rank),effectiveReduction:Math.min(.75,(.10+.05+.15)*rank),
  throughput:round(1/(1-Math.min(.75,(.10+.05+.15)*rank)))})),
 craftReachableWitness:witness('crafting','crafting_capstone_master_artisan',3),
 arbitrage:{symmetricBonusBreakEven:round((buy-sell)/(buy+sell)),
  scenarios:[0,.10,.15,.20,.25].map(b=>({bonus:b,buy:round(buy*(1-b)),sell:round(sell*(1+b)),
   grossProfit:round(sell*(1+b)-buy*(1-b))})),
  caveat:'Common item normalized BV=1 without rounding, no reputation/events. Not proof all channels safe.'},
 lunarSeeds:[30,60,100,200].map(maxMp=>({maxMp,threshold:.35*maxMp,
  castsAtTenMp:Math.ceil(.35*maxMp/10),
  caveat:'Threshold sensitivity only; 6s sliding window and legal cast timing must also hold'})),
 rankMilestones:[1,5,10,25,50,75,100].map(level=>({level,baseSkillPoints:Math.floor(level/2),
  nextXpAnalytical:Math.round(60*level**1.5),note:level===100?'cap; next XP not a playable next level':'double precision approximation'}))
};
// Conservation/sanity checks on the analytical model, not claims about Unity tests.
assert.equal(results.dualCapstone.witnessPair,54);
assert.equal(results.dualCapstone.bothRankThree,58);
assert(results.arbitrage.scenarios.find(s=>s.bonus===.25).grossProfit>0);
assert(results.manaSustain.find(s=>s.quickChannelRank===2).netPerSecond>0);
assert(results.craftThroughput.every(x=>x.throughput>0&&x.throughput<=4));
assert.equal(results.craftThroughput.at(-1).throughput,4);
fs.writeFileSync(path.join(outputDir,'balance-results.json'),JSON.stringify(results,null,2)+'\n');
console.log(JSON.stringify({status:'ANALYTICAL_MODEL_CHECKS_PASS',nodes:nodes.length,
 capstonePoints:capstones.map(x=>[x.tree,x.points]),dualCapstone:results.dualCapstone,
 manaSustain:results.manaSustain,markBreakEven:results.markBreakEven,
 arbitrage:results.arbitrage,craft:results.craftThroughput,recovery:results.recoveryBounds},null,2));
