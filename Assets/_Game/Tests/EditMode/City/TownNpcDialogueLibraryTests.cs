using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.NPC;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// Structural integrity of the expanded town dialogue content: 23 NPCs, 13 nodes each,
    /// unique node ids, all choice targets resolvable, and a reachable goodbye.
    /// </summary>
    [TestFixture]
    public class TownNpcDialogueLibraryTests
    {
        [Test]
        public void Library_CoversAll23CanonicalNpcs()
        {
            Assert.AreEqual(23, TownNpcDialogueLibrary.AllContent.Count);
        }

        [Test]
        public void EveryNpc_HasCompleteContentBlock()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                Assert.IsNotEmpty(content.NpcId);
                Assert.IsNotEmpty(content.DialogueSetId);
                Assert.GreaterOrEqual(content.Greetings.Length, 2, content.NpcId);
                Assert.IsNotEmpty(content.Role1, content.NpcId);
                Assert.IsNotEmpty(content.Role2, content.NpcId);
                Assert.IsNotEmpty(content.Town1, content.NpcId);
                Assert.IsNotEmpty(content.Town2, content.NpcId);
                Assert.IsNotEmpty(content.Service1, content.NpcId);
                Assert.IsNotEmpty(content.Service2, content.NpcId);
                Assert.GreaterOrEqual(content.AdviceLines.Length, 2, content.NpcId);
                Assert.IsNotEmpty(content.Rumor1, content.NpcId);
                Assert.IsNotEmpty(content.Rumor2, content.NpcId);
                Assert.GreaterOrEqual(content.Goodbyes.Length, 2, content.NpcId);
            }
        }

        [Test]
        public void BuiltTrees_HaveExpectedNodeCount_AndUniqueIds()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var nodes = TownNpcDialogueLibrary.BuildNodes(content);
                Assert.AreEqual(TownNpcDialogueLibrary.NodesPerNpc, nodes.Count, content.NpcId);

                var ids = new HashSet<string>();
                foreach (var node in nodes)
                {
                    Assert.IsNotEmpty(node.NodeId, content.NpcId);
                    Assert.IsTrue(ids.Add(node.NodeId), $"Duplicated node id '{node.NodeId}' for {content.NpcId}.");
                }
            }
        }

        [Test]
        public void AllChoiceTargets_ResolveToExistingNodes()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var nodes = TownNpcDialogueLibrary.BuildNodes(content);
                var ids = new HashSet<string>();
                foreach (var node in nodes)
                {
                    ids.Add(node.NodeId);
                }

                foreach (var node in nodes)
                {
                    foreach (var choice in node.Choices)
                    {
                        if (string.IsNullOrEmpty(choice.NextNodeId))
                        {
                            continue; // terminal choice (close/open shop)
                        }

                        Assert.IsTrue(ids.Contains(choice.NextNodeId),
                            $"{content.NpcId}: node '{node.NodeId}' points to missing node '{choice.NextNodeId}'.");
                    }
                }
            }
        }

        [Test]
        public void GreetingNode_AlwaysOffersGoodbye()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var nodes = TownNpcDialogueLibrary.BuildNodes(content);
                var greeting = nodes.Find(n => n.NodeId == "node_greeting");
                Assert.IsNotNull(greeting, content.NpcId);

                bool hasGoodbyePath = greeting.Choices.Exists(c => c.NextNodeId == "node_goodbye");
                Assert.IsTrue(hasGoodbyePath, $"{content.NpcId}: greeting has no goodbye path.");
            }
        }

        [Test]
        public void ShopNpcs_ExposeOpenShopChoice()
        {
            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var nodes = TownNpcDialogueLibrary.BuildNodes(content);
                var service = nodes.Find(n => n.NodeId == "node_service");
                Assert.IsNotNull(service, content.NpcId);

                bool hasOpenShop = service.Choices.Exists(c => c.ActionType == DialogueActionType.OpenShop);
                Assert.AreEqual(content.HasShop, hasOpenShop,
                    $"{content.NpcId}: OpenShop choice mismatch (HasShop={content.HasShop}).");
            }
        }

        [Test]
        public void Registry_DerivesFromLibrary()
        {
            Assert.AreEqual(TownNpcDialogueLibrary.AllContent.Count, NpcDialogueSetRegistry.TotalNpcsWithDialogue);
            Assert.AreEqual(TownNpcDialogueLibrary.AllContent.Count, NpcDialogueSetRegistry.NpcsWithAtLeast10Nodes);

            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                Assert.IsTrue(NpcDialogueSetRegistry.TryGet(content.NpcId, out var entry), content.NpcId);
                Assert.AreEqual(TownNpcDialogueLibrary.NodesPerNpc, entry.NodeCount, content.NpcId);
            }
        }
    }
}
