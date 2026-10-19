using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_87 — Cobre o mapeamento deterministico PlayerColorSetSO -> slots de recolor
    /// (PlayerRecolorController.BuildSlots). Logica pura (sem GPU/Play Mode); o shader em si
    /// e validado em Play Mode.
    /// </summary>
    public class PlayerColorSetMappingTests
    {
        [Test]
        public void NullSet_AllSlotsDisabled()
        {
            var slots = PlayerRecolorController.BuildSlots(null);
            Assert.AreEqual(4, slots.Length);
            foreach (var s in slots)
            {
                Assert.IsFalse(s.Enabled);
            }
        }

        [Test]
        public void SlotOrder_IsHairShirtPantsMetal()
        {
            var slots = PlayerRecolorController.BuildSlots(null);
            Assert.AreEqual(PlayerRecolorController.SrcHair,  slots[0].Src);
            Assert.AreEqual(PlayerRecolorController.SrcShirt, slots[1].Src);
            Assert.AreEqual(PlayerRecolorController.SrcPants, slots[2].Src);
            Assert.AreEqual(PlayerRecolorController.SrcMetal, slots[3].Src);
        }

        [Test]
        public void HairEnabled_MapsTargetToSlot0_OthersDisabled()
        {
            var set = ScriptableObject.CreateInstance<PlayerColorSetSO>();
            set.RecolorHair = true;
            set.HairColor = Color.red;

            var slots = PlayerRecolorController.BuildSlots(set);

            Assert.IsTrue(slots[0].Enabled);
            Assert.AreEqual(PlayerRecolorController.SrcHair, slots[0].Src);
            Assert.AreEqual(Color.red, slots[0].Dst);
            Assert.IsFalse(slots[1].Enabled);
            Assert.IsFalse(slots[2].Enabled);
            Assert.IsFalse(slots[3].Enabled);

            Object.DestroyImmediate(set);
        }

        [Test]
        public void ShirtAndPantsEnabled_MapToSlots1And2()
        {
            var set = ScriptableObject.CreateInstance<PlayerColorSetSO>();
            set.RecolorShirt = true;
            set.ShirtColor = Color.green;
            set.RecolorPants = true;
            set.PantsColor = Color.blue;

            var slots = PlayerRecolorController.BuildSlots(set);

            Assert.IsFalse(slots[0].Enabled);
            Assert.IsTrue(slots[1].Enabled);
            Assert.AreEqual(Color.green, slots[1].Dst);
            Assert.IsTrue(slots[2].Enabled);
            Assert.AreEqual(Color.blue, slots[2].Dst);
            Assert.IsFalse(slots[3].Enabled);

            Object.DestroyImmediate(set);
        }

        [Test]
        public void MetalEnabled_MapsToSlot3()
        {
            var set = ScriptableObject.CreateInstance<PlayerColorSetSO>();
            set.RecolorMetal = true;
            set.MetalColor = Color.gray;

            var slots = PlayerRecolorController.BuildSlots(set);

            Assert.IsTrue(slots[3].Enabled);
            Assert.AreEqual(PlayerRecolorController.SrcMetal, slots[3].Src);
            Assert.AreEqual(Color.gray, slots[3].Dst);

            Object.DestroyImmediate(set);
        }
    }
}
