using System.Collections.Generic;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — lógica determinística do grimório, em C# puro (sem MonoBehaviour, sem Unity refs),
    /// para permitir EditMode tests sem cena. <see cref="PlayerSpellbook"/> embrulha esta classe e
    /// publica os eventos. Regras: aprendizado idempotente; CanCast = conhecida OU concedida por
    /// equipamento; tomo aprende ao atingir N usos; save só com IDs string estáveis.
    /// </summary>
    public class SpellbookState
    {
        private readonly HashSet<string> _known = new HashSet<string>();
        private readonly HashSet<string> _equipmentGranted = new HashSet<string>();
        private readonly Dictionary<string, int> _tomeUses = new Dictionary<string, int>();

        /// <summary>Magias aprendidas permanentemente (persistidas).</summary>
        public IReadOnlyCollection<string> KnownSpellIds => _known;

        /// <summary>Magias concedidas por item equipado (transitório, NÃO persistido).</summary>
        public IReadOnlyCollection<string> GrantedByEquipment => _equipmentGranted;

        /// <summary>
        /// Adiciona uma magia ao conhecimento permanente. Retorna false se já conhecida (idempotente)
        /// ou se o id for nulo/vazio. Limpa o contador de tomo correspondente (estudo concluído).
        /// </summary>
        public bool TryLearn(string spellId, SpellSourceType source)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return false;
            }

            if (!_known.Add(spellId))
            {
                return false;
            }

            // Estudo concluído por qualquer fonte: contador de tomo deixa de ser relevante.
            _tomeUses.Remove(spellId);
            return true;
        }

        /// <summary>True se a magia foi aprendida permanentemente (ignora equipamento).</summary>
        public bool IsKnown(string spellId)
        {
            return !string.IsNullOrWhiteSpace(spellId) && _known.Contains(spellId);
        }

        /// <summary>Castabilidade por estado de conhecimento: conhecida OU concedida por equipamento.</summary>
        public bool CanCast(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return false;
            }

            return _known.Contains(spellId) || _equipmentGranted.Contains(spellId);
        }

        public void AddEquipmentGrant(string spellId)
        {
            if (!string.IsNullOrWhiteSpace(spellId))
            {
                _equipmentGranted.Add(spellId);
            }
        }

        public void RemoveEquipmentGrant(string spellId)
        {
            if (!string.IsNullOrWhiteSpace(spellId))
            {
                _equipmentGranted.Remove(spellId);
            }
        }

        public void ClearEquipmentGrants()
        {
            _equipmentGranted.Clear();
        }

        /// <summary>
        /// Incrementa o estudo de um tomo. Aprende (e retorna true) ao atingir usesRequired.
        /// firstTimeKnown indica se ESTE uso foi o que adicionou o conhecimento (para publicar evento
        /// uma única vez). Se já conhecida, retorna true sem incrementar e firstTimeKnown=false.
        /// </summary>
        public bool RegisterTomeUse(string spellId, int usesRequired, out bool firstTimeKnown)
        {
            firstTimeKnown = false;

            if (string.IsNullOrWhiteSpace(spellId))
            {
                return false;
            }

            if (_known.Contains(spellId))
            {
                // Já aprendida: tomo extra é no-op (idempotente).
                return true;
            }

            int required = usesRequired < 1 ? 1 : usesRequired;

            _tomeUses.TryGetValue(spellId, out int current);
            current += 1;

            if (current >= required)
            {
                _tomeUses.Remove(spellId);
                _known.Add(spellId);
                firstTimeKnown = true;
                return true;
            }

            _tomeUses[spellId] = current;
            return false;
        }

        public int GetTomeUses(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return 0;
            }

            return _tomeUses.TryGetValue(spellId, out int uses) ? uses : 0;
        }

        // ---------------------------------------------------------------- save

        public SpellbookSaveData CaptureSaveData()
        {
            var data = new SpellbookSaveData();
            foreach (var spellId in _known)
            {
                data.KnownSpellIds.Add(spellId);
            }

            foreach (var pair in _tomeUses)
            {
                data.TomeProgress.Add(new TomeProgressEntry { SpellId = pair.Key, Uses = pair.Value });
            }

            return data;
        }

        /// <summary>
        /// Restaura o estado persistente. Substitui o conteúdo atual (idempotente em reload).
        /// Entradas nulas/vazias são ignoradas; equipment grants NÃO são tocados (transitórios).
        /// Null = grimório vazio (saves legados sem a seção).
        /// </summary>
        public void RestoreFromSaveData(SpellbookSaveData saveData)
        {
            _known.Clear();
            _tomeUses.Clear();

            if (saveData == null)
            {
                return;
            }

            if (saveData.KnownSpellIds != null)
            {
                foreach (var spellId in saveData.KnownSpellIds)
                {
                    if (!string.IsNullOrWhiteSpace(spellId))
                    {
                        _known.Add(spellId);
                    }
                }
            }

            if (saveData.TomeProgress != null)
            {
                foreach (var entry in saveData.TomeProgress)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.SpellId))
                    {
                        continue;
                    }

                    // Não persistir progresso de algo já aprendido; clamp em >= 1.
                    if (_known.Contains(entry.SpellId))
                    {
                        continue;
                    }

                    if (entry.Uses > 0)
                    {
                        _tomeUses[entry.SpellId] = entry.Uses;
                    }
                }
            }
        }
    }
}
