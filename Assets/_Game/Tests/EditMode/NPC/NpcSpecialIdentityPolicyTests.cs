using CindarsHope.NPC;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcSpecialIdentityPolicy: identidade por NpcId canonico (case-insensitive) com
    /// fallback por substring do DisplayName, e null-safety.
    /// </summary>
    [TestFixture]
    public sealed class NpcSpecialIdentityPolicyTests
    {
        private static NpcDataSO CreateNpc(string npcId, string displayName)
        {
            var data = ScriptableObject.CreateInstance<NpcDataSO>();
            data.NpcId = npcId;
            data.DisplayName = displayName;
            return data;
        }

        [Test]
        public void IsThalindra_ReturnsFalse_ForNullNpcData()
        {
            Assert.That(NpcSpecialIdentityPolicy.IsThalindra(null), Is.False);
        }

        [Test]
        public void IsBrumdar_ReturnsFalse_ForNullNpcData()
        {
            Assert.That(NpcSpecialIdentityPolicy.IsBrumdar(null), Is.False);
        }

        [Test]
        public void IsThalindra_ReturnsTrue_ForCanonicalIdCaseInsensitive()
        {
            var data = CreateNpc("NPC_THALINDRA", "Pesquisadora Anonima");

            Assert.That(NpcSpecialIdentityPolicy.IsThalindra(data), Is.True);
        }

        [Test]
        public void IsThalindra_ReturnsTrue_ForDisplayNameSubstringFallback_WhenIdDiffers()
        {
            var data = CreateNpc("npc_placeholder", "Dra. Thalindra, a Sabia");

            Assert.That(NpcSpecialIdentityPolicy.IsThalindra(data), Is.True);
        }

        [Test]
        public void IsThalindra_ReturnsFalse_WhenNeitherIdNorDisplayNameMatch()
        {
            var data = CreateNpc("npc_brumdar", "Brumdar");

            Assert.That(NpcSpecialIdentityPolicy.IsThalindra(data), Is.False);
        }

        [Test]
        public void IsBrumdar_ReturnsTrue_ForCanonicalIdCaseInsensitive()
        {
            var data = CreateNpc("NPC_BRUMDAR", "O Ferreiro");

            Assert.That(NpcSpecialIdentityPolicy.IsBrumdar(data), Is.True);
        }

        [Test]
        public void IsBrumdar_ReturnsTrue_ForDisplayNameSubstringFallback_WhenIdDiffers()
        {
            var data = CreateNpc("npc_placeholder", "Mestre Brumdar");

            Assert.That(NpcSpecialIdentityPolicy.IsBrumdar(data), Is.True);
        }

        [Test]
        public void IsBrumdar_ReturnsFalse_WhenNeitherIdNorDisplayNameMatch()
        {
            var data = CreateNpc("npc_thalindra", "Thalindra");

            Assert.That(NpcSpecialIdentityPolicy.IsBrumdar(data), Is.False);
        }
    }
}
