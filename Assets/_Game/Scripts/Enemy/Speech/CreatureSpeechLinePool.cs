using System;

namespace CindarsHope.Enemy.Speech
{
    /// <summary>
    /// Fonte pura (sem UnityEngine) das falas ambiente curtas de criaturas da caverna.
    /// Classifica a criatura numa "voz" a partir do enemyId (palavras-chave) com fallback pela
    /// band (bioma 1-7: stone/fungal/ice/fire/ruins/deep/void) e devolve uma fala curta do pool
    /// daquela voz. Determinístico por <see cref="Random"/> (seed do chamador), então o mesmo
    /// inimigo tende a falar de forma consistente. Testável em EditMode.
    ///
    /// Falas são curtas de propósito (estilo balão de HQ) e evocam o tom "pastoral acima / horror
    /// abaixo": quanto mais fundo (void/deep), mais perturbador.
    /// </summary>
    public static class CreatureSpeechLinePool
    {
        public enum Voice
        {
            Undead,
            Vermin,
            Goblinoid,
            Draconic,
            Construct,
            Eldritch,
            Beast,
            Frost,
            Fungal,
            Stone
        }

        // Falas por voz. Curtas (cabem num balão pequeno). Sem acentos perdidos: UTF-8.
        private static readonly string[] Undead =
        {
            "...frio...", "ossos lembram.", "junte-se a nos.", "nao ha descanso.",
            "voce ja esta morto.", "a carne mente."
        };

        private static readonly string[] Vermin =
        {
            "ksssss...", "*click-click*", "carne... quente.", "ssssente o cheiro?",
            "muitos... somos muitos.", "*estala as mandibulas*"
        };

        private static readonly string[] Goblinoid =
        {
            "intruso!", "ouro! ouro!", "pega ele!", "carne fresca!",
            "pra Pedra Negra!", "morre, de cima!", "chefe vai gostar."
        };

        private static readonly string[] Draconic =
        {
            "queime.", "voe ou pereca.", "cinzas... cinzas.", "seu fim arde.",
            "ajoelhe-se, verme.", "o fogo lembra."
        };

        private static readonly string[] Construct =
        {
            "ALVO DETECTADO.", "*rangido metalico*", "acesso negado.", "...processando...",
            "intruso classificado.", "purgar anomalia."
        };

        private static readonly string[] Eldritch =
        {
            "voce nao deveria estar aqui.", "a Fonte mente.", "nos sonhamos voce.",
            "o vazio chama.", "ela nao esta morta.", "olhe para baixo.", "ainda."
        };

        private static readonly string[] Beast =
        {
            "*rosna*", "*guincho*", "grrrr...", "*fareja*", "*uiva ao longe*"
        };

        private static readonly string[] Frost =
        {
            "congele.", "frio eterno.", "*sopro gelido*", "o gelo guarda tudo.", "fique... quieto."
        };

        private static readonly string[] Fungal =
        {
            "espore...", "raizes... fundas.", "*estala umido*", "respire fundo.", "cresca conosco."
        };

        private static readonly string[] Stone =
        {
            "*ecoa na pedra*", "pedra e silencio.", "...quem ousa?", "o fundo te espera.",
            "fundo demais."
        };

        /// <summary>Tenta obter uma fala curta para a criatura. Nunca lança; retorna false se vazio.</summary>
        public static bool TryGetLine(string enemyId, int band, Random rng, out string line)
        {
            line = string.Empty;
            if (rng == null)
            {
                return false;
            }

            var pool = PoolFor(Classify(enemyId, band));
            if (pool == null || pool.Length == 0)
            {
                return false;
            }

            line = pool[rng.Next(pool.Length)];
            return !string.IsNullOrEmpty(line);
        }

        /// <summary>Classificação determinística da voz por palavra-chave do id, com fallback por band.</summary>
        public static Voice Classify(string enemyId, int band)
        {
            string id = (enemyId ?? string.Empty).ToLowerInvariant();

            if (ContainsAny(id, "void", "abyssal", "riftstalker", "reaver", "wyrm", "lich", "shard", "rift"))
                return Voice.Eldritch;
            if (ContainsAny(id, "bone", "skelet", "corrupted", "wraith", "revenant", "glassbone", "knight", "draugr"))
                return Voice.Undead;
            if (ContainsAny(id, "mite", "tick", "crawler", "spider", "gnawer", "leaper", "swarm", "roach", "centiped"))
                return Voice.Vermin;
            if (ContainsAny(id, "goblin", "duergar", "drow", "gnome", "gnomorin", "orc", "acolyte", "cult", "trapper", "scavenger", "adept", "warden", "shieldbreaker", "madcap", "tinker"))
                return Voice.Goblinoid;
            if (ContainsAny(id, "draconic", "wyvern", "drake", "ashspitter", "dragon", "blackstone_wyvern"))
                return Voice.Draconic;
            if (ContainsAny(id, "clockwork", "golem", "furnace", "elemental", "guard", "construct", "automaton", "sentinel"))
                return Voice.Construct;
            if (ContainsAny(id, "frost", "cold", "ice", "wailer", "frostdelver", "glacial", "rime"))
                return Voice.Frost;
            if (ContainsAny(id, "sprout", "blackroot", "spore", "fungal", "fungus", "mold", "bloom"))
                return Voice.Fungal;
            if (ContainsAny(id, "bat", "beast", "stalker", "hound", "lurker", "spitter"))
                return Voice.Beast;

            // Fallback por bioma da band (1 stone, 2 fungal, 3 ice, 4 fire, 5 ruins, 6 deep, 7 void).
            switch (band)
            {
                case 2: return Voice.Fungal;
                case 3: return Voice.Frost;
                case 4: return Voice.Draconic;   // fire band: tom igneo combina com a voz draconica
                case 5: return Voice.Construct;  // ruins band: guardioes/maquinas antigas
                case 6: return Voice.Eldritch;
                case 7: return Voice.Eldritch;
                default: return Voice.Stone;     // band 1 e desconhecido
            }
        }

        private static string[] PoolFor(Voice voice)
        {
            switch (voice)
            {
                case Voice.Undead: return Undead;
                case Voice.Vermin: return Vermin;
                case Voice.Goblinoid: return Goblinoid;
                case Voice.Draconic: return Draconic;
                case Voice.Construct: return Construct;
                case Voice.Eldritch: return Eldritch;
                case Voice.Beast: return Beast;
                case Voice.Frost: return Frost;
                case Voice.Fungal: return Fungal;
                default: return Stone;
            }
        }

        private static bool ContainsAny(string id, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (id.Contains(needles[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
