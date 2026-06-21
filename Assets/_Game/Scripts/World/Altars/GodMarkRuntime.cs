using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — ponto de acesso de runtime ao <see cref="GodMarkService"/> (singleton leve, sem
    /// MonoBehaviour e sem busca global de cena). Mesmo idioma de <see cref="AccessoryEffectRouter.Active"/>:
    /// um acessor estático que os hooks/interactables consomem; null em teste puro ⇒ tratado como neutro.
    ///
    /// O dono de runtime (um bootstrap de cena/bootstrap do jogo, fora do escopo de código desta spec)
    /// instala a instância e fornece o <see cref="IGodMarkWorldContext"/>. A assinatura ao
    /// <see cref="DayStartedEvent"/> (expiração diária / aplicação de pendências) é registrada por
    /// <see cref="Activate"/> e removida por <see cref="Deactivate"/> — nenhuma comunicação direta
    /// entre MonoBehaviours (rule unity-architecture §2).
    /// </summary>
    public static class GodMarkRuntime
    {
        private static IDisposable _daySubscription;

        /// <summary>Service ativo (null fora de uma sessão de jogo / em teste puro).</summary>
        public static GodMarkService Service { get; private set; }

        /// <summary>Contexto do mundo ativo (null ⇒ oração não pode validar condições).</summary>
        public static IGodMarkWorldContext Context { get; private set; }

        /// <summary>Instala service + contexto e assina a virada de dia. Idempotente.</summary>
        public static void Activate(GodMarkService service, IGodMarkWorldContext context)
        {
            Service = service;
            Context = context;
            Deactivate(disposeServiceState: false);
            _daySubscription = GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        /// <summary>Remove a assinatura. <paramref name="disposeServiceState"/> também limpa o service/contexto.</summary>
        public static void Deactivate(bool disposeServiceState = true)
        {
            _daySubscription?.Dispose();
            _daySubscription = null;
            if (disposeServiceState)
            {
                Service = null;
                Context = null;
            }
        }

        private static void OnDayStarted(DayStartedEvent evt)
        {
            Service?.OnDayStarted(evt.DayNumber);
        }

        /// <summary>
        /// Tenta orar numa Marca pelo ponto único de runtime, publicando <see cref="GodMarkPrayedEvent"/>.
        /// Retorna o resultado (ou <see cref="GodMarkPrayResult.UnknownMark"/> se não há service/contexto).
        /// </summary>
        public static GodMarkPrayResult Pray(string markId)
        {
            if (Service == null || Context == null)
            {
                return GodMarkPrayResult.UnknownMark;
            }

            var result = Service.TryPray(markId, Context);

            GodMarkCatalog.TryGetById(markId, out var def);
            var god = def != null ? (int)def.God : 0;
            var effect = (def != null && result == GodMarkPrayResult.Granted) ? (int)def.Effect : 0;
            var magnitude = (def != null && result == GodMarkPrayResult.Granted) ? def.Magnitude : 0f;

            GameEventBus.Publish(new GodMarkPrayedEvent(markId, god, (int)result, effect, magnitude));
            return result;
        }
    }
}
