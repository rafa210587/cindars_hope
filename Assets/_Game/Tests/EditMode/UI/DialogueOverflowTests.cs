using System.Reflection;
using CindarsHope.UI.Dialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace CindarsHope.Tests.EditMode.UI
{
    public class DialogueOverflowTests
    {
        [Test] public void LongSpeech_UsesMaskedScrollableContentWithoutMovingChoicesArea()
        {
            var host = new GameObject("DialogueTest", typeof(RectTransform));
            try
            {
                var textObject = new GameObject("Speech", typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(host.transform, false);
                var text = textObject.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 18;
                text.rectTransform.sizeDelta = new Vector2(220, 60);
                text.rectTransform.anchoredPosition = new Vector2(0, 108);
                var controller = host.AddComponent<DialogueModal>();
                typeof(DialogueModal).GetField("_dialogueText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, text);
                controller.Show(new string('A', 500));
                var scroll = text.GetComponentInParent<ScrollRect>();
                Assert.That(scroll, Is.Not.Null);
                Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
                Assert.That(scroll.viewport.anchoredPosition.y, Is.EqualTo(108));
                Assert.That(scroll.viewport.rect.height, Is.EqualTo(60));
                Assert.That(scroll.content.rect.height, Is.GreaterThanOrEqualTo(60));
                controller.Show("Texto curto");
                Assert.That(host.GetComponentsInChildren<ScrollRect>().Length, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
