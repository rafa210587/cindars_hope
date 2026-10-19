namespace CindarsHope.Farm
{
    /// <summary>
    /// fable_37 — ponto ÚNICO nomeado do farm para o pico de Lua Verde (crops +N estágios). O
    /// WorldEventService DIRIGE este hook; o farm não conhece o serviço. Localiza o registro de canteiros
    /// via FarmPlotRegistry.Active (auto-registrado, sem busca global de cena em gameplay) e avança o
    /// estágio de cada canteiro plantado pelo método nomeado FarmPlot.TryAdvanceStageFromLunarPeak.
    /// Neutro (no-op) quando não há registro ativo ou nenhum canteiro plantado.
    /// </summary>
    public static class WorldEventFarmHook
    {
        /// <summary>Avança <paramref name="stages"/> estágios em todos os canteiros plantados do registro
        /// ativo. Retorna quantos avanços de estágio foram concedidos (para feedback/teste).</summary>
        public static int AdvanceAllCropsStages(int stages)
        {
            if (stages <= 0)
            {
                return 0;
            }

            var registry = FarmPlotRegistry.Active;
            if (registry == null || registry.Plots == null)
            {
                return 0;
            }

            var advanced = 0;
            foreach (var plot in registry.Plots)
            {
                if (plot == null)
                {
                    continue;
                }

                for (var step = 0; step < stages; step++)
                {
                    if (!plot.TryAdvanceStageFromLunarPeak())
                    {
                        break; // canteiro já colhível ou não-plantado: para de avançar este canteiro
                    }

                    advanced++;
                }
            }

            return advanced;
        }
    }
}
