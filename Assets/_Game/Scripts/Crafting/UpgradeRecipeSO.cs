using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — receita de UPGRADE de equipamento (alvo por família/banda, foco único §35, nível 1-3).
    /// É a fonte autoral do upgrade na forja: define o foco que UM nível melhora e a banda de material
    /// (que determina o material-âncora consumido e entra no custo derivado Decision 2.11). NÃO duplica
    /// stats por material — só referencia a banda; o custo de ouro/material é derivado por HighTierGearCanon.
    /// Tipos simples e IDs (sem refs Unity nos campos persistidos).
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeRecipe", menuName = "CindarsHope/Crafting/Upgrade Recipe")]
    public class UpgradeRecipeSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;

        public string Id => _id;
        public string DisplayName;
        [TextArea] public string Description;

        [Tooltip("Banda de material do alvo (Mithril/Bromecian/Blackstone/Meteoric).")]
        public MaterialBand TargetBand = MaterialBand.Mithril;

        [Tooltip("Foco ÚNICO que cada nível deste upgrade melhora (dano OU durabilidade OU peso OU stamina OU block).")]
        public UpgradeFocus Focus = UpgradeFocus.Damage;

        [Tooltip("Nível máximo desta linha de upgrade (teto canônico +3).")]
        [Range(1, 3)] public int MaxLevel = HighTierGearCanon.MaxUpgradeLevel;

        public void SetId(string id)
        {
            _id = id;
        }

        private void OnValidate()
        {
            MaxLevel = Mathf.Clamp(MaxLevel, 1, HighTierGearCanon.MaxUpgradeLevel);
            if (Focus == UpgradeFocus.None)
            {
                Focus = UpgradeFocus.Damage;
            }
            if (TargetBand == MaterialBand.None)
            {
                TargetBand = MaterialBand.Mithril;
            }
        }
    }
}
