# Farm v18 — execution report

Status: EXECUTING. Spec: .specs/a_implementar/spec_farm_boat_chicken_motion_v18.md.
T1 independent design review:SOUND_WITH_AMENDMENTS; incorporated animal ID vsSpecies, interaction stop, pause and save isolation. Pilot scope boat+chicken; other species preserveidentity, do not claim full animatedherd.
Ownership:rootboat/integration/Unity; animal_motion_implement(runtime/tests,Sol); animal_motion_probe(Editorprobe,Sol); existing art executor(inherited). Skills yielded Assets+Editorlock. No concurrentUnity permitted.
Validation planned: relevantEditMode,generatedassets,60sisolatedPlay and independentvisualreview. NoPASSclaimed beforeevidence.

## First integrated evidence

GenerationUnity6000.5.7f1 exit0. Chicken19spritesMultiple32PPU,Point/None/no-mips; boatclip4poses5s, nativeauthoringPASS. All71tree transform hashes unchanged.
62/62EditMode PASS (editmode.xml/log) on motion, restore and existing animal care/products. SaveDTO/registry/care/dayprocessor hashes identical before/after.
Play1 FAIL immediately afterrestore: report gameplay/capture-metadata.json. Beforefailure:boat10.5003s,2.10006cycles,4sprites,transformdrift0;chicken30sallfourstates,release/restoreoneID,positiveobservedsolidclearance. Failedrestoresample wasnotretained byoldprobe; revise instrumentation tosettlephysics andrecordfailedgeometrybeforeassertion. Do notclaimcompletedPlay until rerun.
Review foundandfixed presentationreference, blockedrestorependingretry and dead/unavailablefrozenpose before62tests. Noeconomymutationadded.

## Classified probe failures and measured correction

PlayB retained the failing sample: restored center exactly at allowed xMax(-11.205), fullBodyInside=True, signed solid distance+.8427, penetrates=False. Rect.Contains excludes max edges; this was an instrumentation false failure, not escaped animal. Corrected to inclusive bounds with0.0001u numeric tolerance; full-body and solid assertions unchanged. Two-fixed-tick settling retained for lifecycle stability.
Independent visual review: original left apron hid chicken behind trough. Moved shared coop pen to offsets(-.4,-2.6)..(2.2,-1.0), an exposed front-right patch; geometry/colliders/sorting unchanged. Fixture obstacle now centered in that pen (test-only, no renderer, never saved). Final generationexit0. PlayC is the proof against corrected setup.
62EditMode results remain applicable: runtime/tests/profiles unchanged since those tests; subsequent edits onlyEditorprobe and authoredpenlocation, covered by currentgeneration/PlayC.

## Final scoped result — PlayC

Generationfinal exit0;62/62EditMode PASS. PlayC exit0/PASS:boat10.5003s(two5scycles),fourposes,0transformdrift;chicken60.0003s,19distinctsprites,3002FixedUpdate ticks,4.494u actualtravel. Canonical release + in-memory providerrestore kept1runtime/stableID; interactionfreezeobserved,carestateunchanged; restoredUnavailable stoppedfor1s.0runtimeErrors andscene/savehashesunchanged. Minimumsignedsolidgap=-.01064u, within declared.011u physics tolerance; do not claim mathematicallyzerooverlap.
Independent visual review: chicken is now visible on open ground; feet/bicada/rest readable, priortroughocclusion resolved. Boatclosedhull maintained. Review examinedPNGs; smoothnessatfullframerate stillneedsnormalgameplay acceptance.

| Criterion | Evidence | Result |
|---|---|---|
| AC1 boatcycles/anchor | generatednativeclip + PlayC4poses/2cycles/drift0 | SCOPED PASS |
| AC2 chickenstates/frames |19spritesobserved,all4states,distance-basedruntime + visualreview | SCOPED PASS |
| AC3 safety/restore |bodybounds+fixtureapproach,unitretry/idempotence,Playrelease/restore | SCOPED PASS; fullherd/fullfarm routes NOT RUN |
| AC4 care/saveidentity |preserved-inputhashes,62tests,interactionstate/providerrestore/savehashes | SCOPED PASS |
| AC5 independent review/delivery |T1review,codefindingsresolved,visualreview,HTMLplayback | delivered; humanacceptancepending |

Changed domains: AnimalMotionState/ProfileSO/SpriteAnimator, FarmAnimalRuntime/AnimalReleaseHandler, FarmAnimalMotionAuthoring +GenerateFarmAnimalAssets, boatnativeauthoring/importerexactexception, scenecreatorwiring/pen, capturehelper/session and scopedtests. NativeAsepriteartin dev/art/aseprite/keyart-v4/motion-v18; originalsintact. No save/catalog/care/production/registry edits. No commit/push.

Remaining: cow/sheep/goat authoredcycles are a follow-on afterpilot, not delivered animations. Fullcapacityanimal/playeraccess, allwater/fence approaches and normalinput/humanvisualacceptance remain unexecuted. Spec retained in a_implementar with scopedstatus, no globalpromotion. Prior62tests reused afterEditor-onlysetup/pen edits; no redundantfullsuite.
LockAssets+EditorreturnedSkills afterPlayCexit0. HTMLindex.html reproducesreal5fpssamples andseparates Asepritepreview. ProbeA/Bfailed evidence retained, superseded by diagnosedC.
