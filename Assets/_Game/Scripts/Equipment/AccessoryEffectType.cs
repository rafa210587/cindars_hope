namespace CindarsHope.Equipment
{
    /// <summary>
    /// fable_23 — efeitos TIPADOS de acessórios/relíquias (filosofia canônica §15: acessório nunca
    /// dá dano direto; dá a "vida ao redor do dano" — economia, sustain, resistência, conforto).
    ///
    /// Cada efeito é consultado por UM ponto único nomeado (o hook), nunca por <c>if</c> espalhado
    /// (regra de não duplicação da spec). O <see cref="AccessoryEffectRouter"/> agrega os efeitos dos
    /// 3 slots tipo-acessório aplicando NÃO-STACK (o maior valor por tipo vale).
    ///
    /// Magnitudes canônicas vêm do ITEM_CATALOG §16-17 (mapeadas em <see cref="AccessoryCatalog"/>).
    /// </summary>
    public enum AccessoryEffectType
    {
        None = 0,

        // ─── Acessórios §16 (efeitos condicionais — consultados por hooks pontuais) ───────────────
        /// <summary>+% ouro em vendas (Anel de Finan +0.05). Hook: ponto único de venda da economia.</summary>
        GoldGainPercent = 1,
        /// <summary>+% durabilidade de ferramentas (Anel de Thoren +0.15). Hook: durabilidade.</summary>
        ToolDurabilityPercent = 2,
        /// <summary>+chance de 1 roll extra de loot raro (Anel de Alihana +0.10). Hook: loot resolver.</summary>
        ExtraLootRollChance = 3,
        /// <summary>-% custo de stamina de dash/dodge (Anel de Swiftcurrent -0.10, valor positivo). Hook: ações.</summary>
        DashDodgeStaminaReductionPercent = 4,
        /// <summary>-% fadiga noturna (Amuleto de Nyx -0.30, valor positivo). Hook: fadiga F16.</summary>
        NightFatigueReductionPercent = 5,
        /// <summary>+% efeito de comida (Charm de Thandra +0.15). Hook: FoodConsumer.</summary>
        FoodEffectPercent = 6,
        /// <summary>+% produto de animais (Charm de Thandra +0.10). Hook: produção animal F12.</summary>
        AnimalProductPercent = 7,
        /// <summary>-% knockback recebido (Charm de Stoneheart -0.50, valor positivo). Hook: knockback.</summary>
        KnockbackResistPercent = 8,
        /// <summary>+% cura recebida (Amuleto de Anya +0.25). Hook: cura.</summary>
        HealingReceivedPercent = 9,

        // ─── Acessórios §16 (efeitos de stat — entram pelos inputs de DerivedStats; dormentes aqui) ──
        /// <summary>+resistência Fire/Heat (Anel de Emberward). Stat via DerivedStats (dormente nesta spec).</summary>
        FireHeatResistFlat = 20,
        /// <summary>+% Block Stability (Amuleto de Kanthor +0.10). Stat/combate (dormente nesta spec).</summary>
        BlockStabilityPercent = 21,
        /// <summary>-% postura recebida (Amuleto de Kanthor -0.20, valor positivo). Combate (dormente).</summary>
        PostureTakenReductionPercent = 22,
        /// <summary>+% dano mágico (Amuleto de Senya +0.10). Combate mágico (dormente nesta spec).</summary>
        MagicDamagePercent = 23,
        /// <summary>+% custo de MP (Amuleto de Senya +0.10, trade-off). Magia (dormente nesta spec).</summary>
        MpCostIncreasePercent = 24,
        /// <summary>-% move speed (Charm de Stoneheart -0.05, valor positivo). Movimento (dormente).</summary>
        MoveSpeedReductionPercent = 25,
        /// <summary>Imunidade a Root + Chill -50% duração (Anel de Rootguard). Flag de status (dormente).</summary>
        RootImmuneChillResist = 26,

        // ─── Relíquias §17 (hooks dormentes até a dependência existir — F27/F17/F37) ──────────────
        /// <summary>Relíquia de Kanthor "Julgamento": perfect block cura 2% HP máx (hook F27, dormente).</summary>
        RelicPerfectBlockHealPercent = 40,
        /// <summary>Relíquia de Kaand "Fúria": crítico estende janela de vulnerabilidade +0.5s (hook F05/F24, dormente).</summary>
        RelicCritWindowExtendSeconds = 41,
        /// <summary>Relíquia de Anya "Esperança": Água Viva +1/dia (hook F17, dormente).</summary>
        RelicLivingWaterPerDay = 42,
        /// <summary>Relíquia de Alihana "Véu": 1 sonho/mês revela segredo do calendário (hook F37, dormente).</summary>
        RelicMonthlyCalendarReveal = 43
    }

    /// <summary>
    /// fable_23 — deus maior associado a uma relíquia divina (§17). Apenas 1 relíquia equipada e,
    /// por extensão canônica (F23/F68), o bônus de relíquia de um deus não soma com outra fonte do
    /// MESMO deus — o maior vale. <see cref="None"/> = item não-relíquia.
    /// </summary>
    public enum RelicGod
    {
        None = 0,
        Kanthor = 1,
        Kaand = 2,
        Anya = 3,
        Alihana = 4
    }
}
