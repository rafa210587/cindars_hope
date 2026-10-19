# WAVE_INTEGRATION_15 — Human Unity Quest Wiring Instructions

Date: 2026-06-10

---

## Step 1: Verify QuestRuntimeBootstrap Auto-Creates

1. Open Unity Editor
2. Press Play in any scene that has GameBootstrap
3. Check Console for: "[QuestRuntimeBootstrap] Quest runtime initialized"
4. If not found: check that QuestRuntimeBootstrap.cs is compiled (it uses RuntimeInitializeOnLoadMethod)

Note: QuestRuntimeBootstrap.cs was manually added to Assembly-CSharp.csproj.
Unity will auto-regenerate the .csproj; if the entry is lost, the file will be re-included
automatically by Unity's asset import process (it scans all .cs files).

---

## Step 2: Add QuestGiverInteractable to npc_thalindra

1. Open TownScene (Assets/_Game/Scenes/TownScene.unity)
2. Find the GameObject for npc_thalindra in the Hierarchy
3. Add Component → search "QuestGiverInteractable"
4. In Inspector:
   - NPC Id: `npc_thalindra`
   - Offered Quest Ids: size 1 → element 0 = `quest_first_supplies_for_cindar`
   - Offer Prompt: `Interagir com` (or leave default)
   - Turn In Prompt: `Entregar quest para` (or leave default)

---

## Step 3: (Optional) Add QuestBoard_FirstQuest_01

1. In TownScene Hierarchy: right-click → Create Empty
2. Name it `QuestBoard_FirstQuest_01`
3. Position it near the center of town
4. Add Component → `QuestBoardInteractable`
5. In Inspector:
   - Board Id: `board_first_quest_01`
   - Posted Quest Ids: size 1 → element 0 = `quest_first_supplies_for_cindar`

---

## Step 4: Test Quest Offer Flow

1. Press Play in TownScene
2. Move player near npc_thalindra
3. Interaction prompt should appear: "Interagir com npc_thalindra"
4. Press E (or the interact key bound to IInteractable in your PlayerController)
5. IMGUI panel appears with quest details
6. Press "Aceitar" button
7. Console: "[QuestService] Quest accepted: quest_first_supplies_for_cindar"

---

## Step 5: Test Quest Log

1. Press J during Play Mode
2. Quest Log IMGUI panel opens
3. Shows active quest with objectives
4. Press J again or Escape to close

---

## Step 6: (Future) Canvas Panel Wiring

When Canvas-based UI is available:

1. Create Canvas Panel for Quest Offer:
   - Add Canvas → Panel
   - Add QuestOfferPanelController component
   - Wire: Title text, Description text, Objectives text, Rewards text, Accept button, Decline button via SerializeField

2. Create Canvas Panel for Quest Log:
   - Add Canvas → Panel
   - Add QuestLogPanelController + QuestLogRuntimeBinder components
   - Wire: Quest list container, active/completed scroll areas

The IMGUI fallback will continue working even after Canvas panels are added.

---

## Notes

- QuestRuntimeBootstrap.QuestService and QuestRuntimeBootstrap.QuestRegistry are static accessors
  available to any component without Find/FindObjectOfType calls
- QuestProgressEventBridge is wired automatically in QuestRuntimeBootstrap.Initialize
- QuestOfferPanelController and QuestLogPanelController are auto-created by QuestRuntimeBootstrap
  as DontDestroyOnLoad GameObjects if not already present in the scene
