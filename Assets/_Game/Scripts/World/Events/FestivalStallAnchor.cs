using UnityEngine;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — marcador de cena do ponto onde as barracas de festival nascem na praça. Criado pelo
    /// gerador CreateMvpTownScene (ADITIVO). Em OnEnable injeta seu Transform no WorldEventService via
    /// SetPlazaAnchor (sem busca global de cena em gameplay — é o anchor que se registra no serviço,
    /// mesmo idioma do registro de pontos de forrageio/âncoras de cena). Ausente ⇒ barracas nascem na
    /// origem (fallback do serviço).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FestivalStallAnchor : MonoBehaviour
    {
        private void OnEnable()
        {
            var service = WorldEventService.Instance;
            if (service != null)
            {
                service.SetPlazaAnchor(transform);
            }
        }
    }
}
