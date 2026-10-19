using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// fable_87 — Paleta de recolor do player (palette swap por shader). Define a cor-ALVO de cada
    /// regiao (cabelo, camisa/creme, calca/macacao, metal) e se ela deve ser recolorida. As cores-FONTE
    /// canonicas vivem no <see cref="PlayerRecolorController"/> (um unico lugar). So tipos simples
    /// (Color/bool) — sem refs de cena, seguro para qualquer uso.
    /// </summary>
    [CreateAssetMenu(menuName = "CindarsHope/Player/Color Set", fileName = "ColorSet_New")]
    public sealed class PlayerColorSetSO : ScriptableObject
    {
        [Header("Cabelo")]
        public bool RecolorHair;
        public Color HairColor = Color.white;

        [Header("Camisa / creme")]
        public bool RecolorShirt;
        public Color ShirtColor = Color.white;

        [Header("Calca / macacao")]
        public bool RecolorPants;
        public Color PantsColor = Color.white;

        [Header("Metal")]
        public bool RecolorMetal;
        public Color MetalColor = Color.white;
    }
}
