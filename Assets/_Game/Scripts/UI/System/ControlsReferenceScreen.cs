using System.Text;
using CindarsHope.Core.Bootstrap;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_62 — adapter MonoBehaviour FINO da tela "Controles" (read-only) da aba Sistema (F56).
    ///
    /// Toda a verdade do conteudo vem de <see cref="ControlsReferenceContent"/> (puro, espelha
    /// input_map.md/F67). Este componente apenas: monta o texto exibivel, abre/fecha via
    /// ModalManager existente (Esc fecha — padrao F14/F56) e nao rouba movimento (enquanto aberto,
    /// o input de gameplay ja e bloqueado pelo modal). Sem estado proprio alem de aberto/fechado;
    /// sem GameObject.Find (resolve o ModalManager via GameBootstrap).
    ///
    /// Entrega parcial documentada: a colocacao visual da tela na hierarquia da aba Sistema exige
    /// trabalho de scene/prefab (fora do escopo desta spec — sem edicao de .unity/.prefab). O
    /// conteudo, a logica de open/close e o texto read-only estao prontos para o bind de UI final
    /// (DEFERRED_TO_FINAL_VALIDATION), conforme o gating de F56 previsto pela spec.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ControlsReferenceScreen : MonoBehaviour
    {
        private ModalManager _modalManager;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            var bootstrap = GameBootstrap.Instance;
            _modalManager = bootstrap != null ? bootstrap.ModalManager : null;
        }

        /// <summary>Abre a tela Controles como modal (input de gameplay bloqueado pelo stack).</summary>
        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            _modalManager?.PushModal(ModalType.SystemConfirm);
        }

        /// <summary>Esc/voltar: fecha a tela sem nenhum efeito destrutivo (read-only).</summary>
        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            _modalManager?.TryPopIfCurrent(ModalType.SystemConfirm);
        }

        /// <summary>
        /// Texto exibivel completo dos controles, montado a partir do conteudo canonico. Read-only;
        /// usado pelo bind de UI final. Determinismo testavel: mesma entrada -> mesmo texto.
        /// </summary>
        public static string BuildDisplayText()
        {
            var sb = new StringBuilder();
            var groups = ControlsReferenceContent.All;
            for (int g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                if (g > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(group.Title);
                var lines = group.Lines;
                for (int i = 0; i < lines.Count; i++)
                {
                    sb.Append("  ").Append(lines[i].Keys).Append("  -  ").AppendLine(lines[i].Action);
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
