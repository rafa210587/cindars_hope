using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// fable_87 — Aplica recolor (palette swap) no SpriteRenderer do player via MaterialPropertyBlock,
    /// SEM instanciar material (preserva batching) e SEM trabalho em Update (aplica so na mudanca).
    /// As cores-FONTE canonicas (medidas do walk) ficam aqui, em um unico lugar. O material do
    /// SpriteRenderer deve usar o shader "CindarsHope/PlayerRecolor".
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerRecolorController : MonoBehaviour
    {
        /// <summary>Dados de um slot de recolor resolvidos a partir do ColorSet (parte deterministica, testavel).</summary>
        public readonly struct RecolorSlot
        {
            public readonly bool Enabled;
            public readonly Color Src;
            public readonly Color Dst;
            public RecolorSlot(bool enabled, Color src, Color dst) { Enabled = enabled; Src = src; Dst = dst; }
        }

        // Cores-FONTE canonicas do fazendeiro (medidas no walk): cabelo, camisa/creme, macacao, metal.
        public static readonly Color SrcHair  = HexToColor(0x99, 0x4d, 0x12);
        public static readonly Color SrcShirt = HexToColor(0xe9, 0xbb, 0x6d);
        public static readonly Color SrcPants = HexToColor(0x64, 0x3a, 0x11);
        public static readonly Color SrcMetal = HexToColor(0x7a, 0x73, 0x6b);

        public const float DefaultTolerance = 0.16f;

        [SerializeField] private PlayerColorSetSO _colorSet;
        [SerializeField, Range(0f, 0.5f)] private float _tolerance = DefaultTolerance;

        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _mpb;

        private static readonly int[] SrcId = { Shader.PropertyToID("_Src0"), Shader.PropertyToID("_Src1"), Shader.PropertyToID("_Src2"), Shader.PropertyToID("_Src3") };
        private static readonly int[] DstId = { Shader.PropertyToID("_Dst0"), Shader.PropertyToID("_Dst1"), Shader.PropertyToID("_Dst2"), Shader.PropertyToID("_Dst3") };
        private static readonly int[] TolId = { Shader.PropertyToID("_Tol0"), Shader.PropertyToID("_Tol1"), Shader.PropertyToID("_Tol2"), Shader.PropertyToID("_Tol3") };
        private static readonly int[] EnaId = { Shader.PropertyToID("_Ena0"), Shader.PropertyToID("_Ena1"), Shader.PropertyToID("_Ena2"), Shader.PropertyToID("_Ena3") };

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
            Apply();
        }

        /// <summary>Troca a paleta e reaplica. Idempotente; chamavel quando a cor mudar.</summary>
        public void SetColorSet(PlayerColorSetSO set)
        {
            _colorSet = set;
            Apply();
        }

        /// <summary>Constroi o MaterialPropertyBlock a partir do ColorSet atual e aplica no renderer.</summary>
        public void Apply()
        {
            if (_renderer == null) return;
            _mpb ??= new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_mpb);

            RecolorSlot[] slots = BuildSlots(_colorSet);
            for (int i = 0; i < slots.Length; i++)
            {
                _mpb.SetColor(SrcId[i], slots[i].Src);
                _mpb.SetColor(DstId[i], slots[i].Dst);
                _mpb.SetFloat(TolId[i], _tolerance);
                _mpb.SetFloat(EnaId[i], slots[i].Enabled ? 1f : 0f);
            }
            _renderer.SetPropertyBlock(_mpb);
        }

        /// <summary>
        /// Parte deterministica e testavel: mapeia um ColorSet para os 4 slots (ena/src/dst).
        /// Ordem fixa: 0=cabelo, 1=camisa, 2=calca, 3=metal. Set nulo => todos desabilitados.
        /// </summary>
        public static RecolorSlot[] BuildSlots(PlayerColorSetSO set)
        {
            if (set == null)
            {
                return new[]
                {
                    new RecolorSlot(false, SrcHair,  Color.white),
                    new RecolorSlot(false, SrcShirt, Color.white),
                    new RecolorSlot(false, SrcPants, Color.white),
                    new RecolorSlot(false, SrcMetal, Color.white),
                };
            }
            return new[]
            {
                new RecolorSlot(set.RecolorHair,  SrcHair,  set.HairColor),
                new RecolorSlot(set.RecolorShirt, SrcShirt, set.ShirtColor),
                new RecolorSlot(set.RecolorPants, SrcPants, set.PantsColor),
                new RecolorSlot(set.RecolorMetal, SrcMetal, set.MetalColor),
            };
        }

        private static Color HexToColor(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);
    }
}
