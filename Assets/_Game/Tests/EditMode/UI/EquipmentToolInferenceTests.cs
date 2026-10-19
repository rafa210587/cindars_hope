using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Tools;
using NUnit.Framework;
using UnityEngine;
namespace CindarsHope.Tests.EditMode.UI
{
    public class EquipmentToolInferenceTests
    {
        [Test] public void Pickaxe_IsNotInferredAsAxe()
        {
            var host = new GameObject("ToolInferenceTest");
            try
            {
                var equipment = host.AddComponent<EquipmentManager>();
                equipment.EquipItem(EquipmentSlot.LeftHand, "item_tool_pickaxe_basic");
                Assert.That(equipment.HasTool(ToolType.Pickaxe), Is.True);
                Assert.That(equipment.HasTool(ToolType.Axe), Is.False);
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
