using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace CindarsHope.Core
{
    /// <summary>
    /// Barramento global de eventos do jogo.
    ///
    /// Regras do projeto:
    /// - Sistemas não devem chamar outros sistemas diretamente.
    /// - Comunicação entre sistemas deve passar por eventos pequenos e tipados.
    /// - Eventos não devem carregar GameObject, Transform, MonoBehaviour ou ScriptableObject.
    /// - Quem assina deve cancelar a assinatura em OnDisable/OnDestroy.
    ///
    /// Uso padrão:
    ///     private void OnEnable() => GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
    ///     private void OnDisable() => GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
    ///     private void OnDayStarted(DayStartedEvent evt) { ... }
    ///
    /// Também é possível guardar o IDisposable retornado por Subscribe para dispose manual.
    /// </summary>
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, HandlerBucket> HandlersByType =
            new Dictionary<Type, HandlerBucket>();
        private static readonly ProfilerMarker PublishMarker = new ProfilerMarker("CindarsHope.EventBus.Publish");

        /// <summary>
        /// Retorna true quando há pelo menos um listener registrado para o tipo de evento.
        /// Útil para diagnósticos e testes.
        /// </summary>
        public static bool HasSubscribers<TEvent>()
        {
            var eventType = typeof(TEvent);
            return HandlersByType.TryGetValue(eventType, out var bucket) && bucket.Count > 0;
        }

        /// <summary>
        /// Retorna a quantidade de listeners registrados para o tipo de evento.
        /// Útil para smoke tests e para detectar vazamento de subscriptions.
        /// </summary>
        public static int CountSubscribers<TEvent>()
        {
            var eventType = typeof(TEvent);
            return HandlersByType.TryGetValue(eventType, out var bucket) ? bucket.Count : 0;
        }

        /// <summary>
        /// Assina um evento tipado.
        /// Retorna um IDisposable opcional para cancelar a assinatura sem precisar guardar o handler.
        /// Mesmo assim, em MonoBehaviour, o padrão recomendado é Subscribe em OnEnable e Unsubscribe em OnDisable.
        /// </summary>
        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            if (!HandlersByType.TryGetValue(eventType, out var bucket))
            {
                bucket = new HandlerBucket();
                HandlersByType[eventType] = bucket;
            }

            // Evita subscription duplicada acidental no mesmo ciclo de vida.
            bucket.Add(handler);

            return new EventSubscription<TEvent>(handler);
        }

        /// <summary>
        /// Assina um evento uma única vez. Após o primeiro Publish, o handler é removido automaticamente.
        /// Útil para fluxos pontuais, como esperar confirmação de save/load.
        /// </summary>
        public static IDisposable SubscribeOnce<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Action<TEvent> wrapper = null;
            wrapper = evt =>
            {
                Unsubscribe(wrapper);
                handler(evt);
            };

            return Subscribe(wrapper);
        }

        /// <summary>
        /// Cancela assinatura de um evento tipado.
        /// Chamar em OnDisable/OnDestroy para evitar callbacks em objetos destruídos.
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
            if (!HandlersByType.TryGetValue(eventType, out var bucket))
            {
                return;
            }

            bucket.Remove(handler);
            if (bucket.Count == 0)
            {
                HandlersByType.Remove(eventType);
            }
        }

        /// <summary>
        /// Publica um evento para todos os listeners daquele tipo.
        /// Usa snapshot para permitir subscribe/unsubscribe durante o dispatch sem quebrar a iteração.
        /// Uma exceção em um listener é logada, mas não impede os demais listeners de receberem o evento.
        /// </summary>
        public static void Publish<TEvent>(TEvent evt)
        {
            var eventType = typeof(TEvent);
            if (!HandlersByType.TryGetValue(eventType, out var bucket) || bucket.Count == 0)
            {
                return;
            }

            using (PublishMarker.Auto())
            {
                Delegate[] snapshot = bucket.GetSnapshot();
                foreach (Delegate rawHandler in snapshot)
                {
                    if (!(rawHandler is Action<TEvent> handler))
                    {
                        continue;
                    }

                    try
                    {
                        handler(evt);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
        }

        /// <summary>
        /// Remove todos os listeners de um tipo de evento.
        /// Uso esperado: testes, reset de sessão ou ferramentas de debug.
        /// Evitar usar em gameplay normal.
        /// </summary>
        public static void Clear<TEvent>()
        {
            HandlersByType.Remove(typeof(TEvent));
        }

        /// <summary>
        /// Remove todos os listeners de todos os eventos.
        /// Uso esperado: testes, reset de sessão ou ferramentas de debug.
        /// Evitar usar em gameplay normal.
        /// </summary>
        public static void ClearAll()
        {
            HandlersByType.Clear();
        }

        private sealed class EventSubscription<TEvent> : IDisposable
        {
            private Action<TEvent> _handler;
            private bool _disposed;

            public EventSubscription(Action<TEvent> handler)
            {
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                Unsubscribe(_handler);
                _handler = null;
                _disposed = true;
            }
        }

        private sealed class HandlerBucket
        {
            private readonly List<Delegate> _handlers = new List<Delegate>();
            private Delegate[] _snapshot = Array.Empty<Delegate>();
            private bool _snapshotDirty;

            public int Count => _handlers.Count;

            public void Add(Delegate handler)
            {
                if (_handlers.Contains(handler))
                {
                    return;
                }

                _handlers.Add(handler);
                _snapshotDirty = true;
            }

            public void Remove(Delegate handler)
            {
                if (_handlers.Remove(handler))
                {
                    _snapshotDirty = true;
                }
            }

            public Delegate[] GetSnapshot()
            {
                if (_snapshotDirty)
                {
                    _snapshot = _handlers.ToArray();
                    _snapshotDirty = false;
                }

                return _snapshot;
            }
        }
    }
}
