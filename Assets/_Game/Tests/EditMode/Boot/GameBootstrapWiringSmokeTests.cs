using System.Linq;
using System.Reflection;
using CindarsHope.Core.Bootstrap;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Boot
{
    /// <summary>
    /// spec_codex_07_boot_smoke_editmode: static/reflection smoke over <see cref="GameBootstrap"/>.
    ///
    /// This is NOT a Play Mode test and does not prove that scene wiring is actually populated
    /// (that requires an editor/batchmode scene validator, out of scope here). It proves, without
    /// instantiating a MonoBehaviour or opening a scene, that the field<->property contract that
    /// runtime consumers rely on (e.g. GameBootstrap.Instance.PlayerManager) is structurally intact:
    /// every private [SerializeField] manager/database field has a corresponding public property of
    /// the same type. See .claude/skills/boot-integration-smoke/SKILL.md for the procedure this
    /// follows and what is explicitly deferred to Play Mode / editor validators.
    /// </summary>
    public class GameBootstrapWiringSmokeTests
    {
        // Known field<->property name mismatches that are intentional (documented in the spec's
        // Phase 0 audit / "Riscos técnicos" section). Do not fail the test for these; the resolver
        // below checks them explicitly by field name.
        private static readonly string[] KnownMismatchFieldNames =
        {
            "_progressionManager", // property is PlayerProgressionManager, not ProgressionManager
        };

        private static FieldInfo[] GetSerializedManagerFields()
        {
            return typeof(GameBootstrap)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null)
                .Where(f => !IsExcludedFromWiringContract(f))
                .ToArray();
        }

        // GameBootstrap has two [SerializeField] fields that are NOT exposed via a matching public
        // property by design, per Phase 0 audit of the spec:
        //   _playerData (PlayerDataSO) — consumed internally at Awake time, never read back by
        //       other systems via GameBootstrap; there is no public PlayerData property.
        // Everything else follows the 1:1 field->property convention and is checked below.
        private static bool IsExcludedFromWiringContract(FieldInfo field)
        {
            return field.Name == "_playerData";
        }

        [Test]
        public void EveryPrivateManagerField_HasCorrespondingPublicProperty()
        {
            var fields = GetSerializedManagerFields();
            Assert.IsNotEmpty(fields, "Expected GameBootstrap to declare [SerializeField] manager/database fields.");

            var properties = typeof(GameBootstrap)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            int checkedCount = 0;
            var missing = new System.Collections.Generic.List<string>();

            foreach (var field in fields)
            {
                checkedCount++;
                string expectedPropertyName = FieldNameToExpectedPropertyName(field.Name);

                bool hasMatch = properties.Any(p =>
                    p.PropertyType == field.FieldType &&
                    (p.Name == expectedPropertyName || p.Name.Equals(expectedPropertyName, System.StringComparison.OrdinalIgnoreCase)));

                if (!hasMatch)
                {
                    missing.Add($"{field.Name} ({field.FieldType.Name}) -> expected property '{expectedPropertyName}'");
                }
            }

            Assert.IsEmpty(missing,
                $"GameBootstrap has [SerializeField] manager fields without a matching public property. " +
                $"Checked {checkedCount} fields. Missing: {string.Join("; ", missing)}");

            TestContext.WriteLine($"GameBootstrapWiringSmokeTests: verified {checkedCount} SerializeField->property pairs via reflection.");
        }

        [Test]
        public void KnownMismatches_AreExplicitlyDocumented_AndStillResolve()
        {
            // _progressionManager -> PlayerProgressionManager is a deliberate naming mismatch already
            // present in GameBootstrap.cs. Confirm it still resolves so a future rename that breaks
            // the contract silently is caught, without asserting the naive 1:1 name convention on it.
            var field = typeof(GameBootstrap).GetField("_progressionManager", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Expected GameBootstrap to still declare _progressionManager.");

            var property = typeof(GameBootstrap).GetProperty("PlayerProgressionManager", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, "Expected GameBootstrap.PlayerProgressionManager property to exist for _progressionManager.");
            Assert.AreEqual(field.FieldType, property.PropertyType, "_progressionManager field type must match PlayerProgressionManager property type.");

            CollectionAssert.Contains(KnownMismatchFieldNames, "_progressionManager");
        }

        [Test]
        public void Instance_IsNull_OutsidePlayMode()
        {
            // GameBootstrap.Instance is populated in Awake() on a real scene MonoBehaviour, which only
            // runs in Play Mode (or Editor/batchmode scene load). In pure EditMode, no GameBootstrap
            // has been instantiated, so Instance must be null. This documents the limitation instead
            // of attempting to work around it (e.g. by instantiating a GameObject and calling Awake
            // via reflection), which the spec explicitly rules out as out of scope.
            Assert.IsNull(GameBootstrap.Instance,
                "GameBootstrap.Instance is expected to be null in EditMode: Awake() only runs in Play Mode / scene load, " +
                "which this smoke test intentionally does not simulate.");
        }

        private static string FieldNameToExpectedPropertyName(string fieldName)
        {
            // Convention used throughout GameBootstrap.cs: "_camelCase" -> "PascalCase".
            string trimmed = fieldName.TrimStart('_');
            if (trimmed.Length == 0)
            {
                return trimmed;
            }

            return char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1);
        }
    }
}
