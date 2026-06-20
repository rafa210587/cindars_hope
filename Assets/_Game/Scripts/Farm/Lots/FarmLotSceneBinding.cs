using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — ponte de cena dos lotes de expansão. O gerador (CreateMvpFarmScene) cria os
    /// 3 lotes cercados e PREENCHE estas referências serializadas (cerca, raiz de conteúdo,
    /// placa) por lote. O <see cref="FarmLotService"/> lê este binding e liga/desliga GameObjects
    /// SEM nenhum GameObject.Find/FindObjectOfType — apenas referências diretas.
    ///
    /// Estado visual aplicado:
    ///  - Locked: cerca ATIVA, placa ATIVA, conteúdo interno INATIVO.
    ///  - Owned:  cerca INATIVA, placa INATIVA, conteúdo interno ATIVO.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FarmLotSceneBinding : MonoBehaviour
    {
        [System.Serializable]
        public sealed class LotBinding
        {
            [Tooltip("ID estável do lote (lot_north / lot_east / lot_west).")]
            public string LotId;

            [Tooltip("Raiz da cerca + placa (visível enquanto Locked; some ao destravar).")]
            public GameObject FenceRoot;

            [Tooltip("Placa interactable informativa (subconjunto da cerca; some ao destravar).")]
            public GameObject SignObject;

            [Tooltip("Raiz do conteúdo interno gerado DESATIVADO (plots/pasto/árvores; ativa ao destravar).")]
            public GameObject ContentRoot;
        }

        [SerializeField] private List<LotBinding> _lots = new List<LotBinding>();

        public IReadOnlyList<LotBinding> Lots => _lots;

        public LotBinding GetBinding(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return null;
            }

            for (int i = 0; i < _lots.Count; i++)
            {
                if (_lots[i] != null && _lots[i].LotId == lotId)
                {
                    return _lots[i];
                }
            }

            return null;
        }

        /// <summary>Aplica o estado visual de um lote. Locked = cerca/placa on, conteúdo off; Owned = inverso.</summary>
        public void ApplyVisualState(string lotId, FarmLotState state)
        {
            var binding = GetBinding(lotId);
            if (binding == null)
            {
                return;
            }

            var owned = state == FarmLotState.Owned;

            if (binding.FenceRoot != null)
            {
                binding.FenceRoot.SetActive(!owned);
            }

            if (binding.SignObject != null)
            {
                binding.SignObject.SetActive(!owned);
            }

            if (binding.ContentRoot != null)
            {
                binding.ContentRoot.SetActive(owned);
            }
        }

#if UNITY_EDITOR
        /// <summary>Usado pelo gerador para registrar um lote com suas referências serializadas.</summary>
        public void EditorAddLot(string lotId, GameObject fenceRoot, GameObject signObject, GameObject contentRoot)
        {
            _lots.Add(new LotBinding
            {
                LotId = lotId,
                FenceRoot = fenceRoot,
                SignObject = signObject,
                ContentRoot = contentRoot
            });
        }
#endif
    }
}
