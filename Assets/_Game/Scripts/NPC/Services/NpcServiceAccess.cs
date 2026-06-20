using System.Collections.Generic;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — fachada ESTÁTICA de acesso aos serviços únicos de NPC, no padrão de acessor único já
    /// usado por <c>CityServiceAccess</c> (fable_19) / <c>TemperingForgeAccess</c> (fable_22). Mantém o
    /// <see cref="CindarsHope.NPC.NpcShopController"/> DESACOPLADO do executor/host: a montagem das
    /// opções (gates) e a execução são delegadas a um <see cref="NpcServiceExecutor"/> injetado pelo
    /// runtime bridge. Default fail-closed: sem bridge ligado, não há opções e nada executa.
    ///
    /// Pure-friendly: sem busca global de cena aqui. O runtime
    /// (<see cref="NpcServiceRuntime"/>) liga <see cref="ExecutorProvider"/> ao executor real.
    /// </summary>
    public static class NpcServiceAccess
    {
        /// <summary>Provedor do executor ligado pelo runtime bridge. Default => null (fail-closed).</summary>
        public static System.Func<NpcServiceExecutor> ExecutorProvider { get; set; }

        private static NpcServiceExecutor Executor()
        {
            try { return ExecutorProvider != null ? ExecutorProvider() : null; }
            catch { return null; }
        }

        /// <summary>True se este NPC oferece algum serviço único (consulta o catálogo canônico).</summary>
        public static bool HasServices(string npcId) => NpcServiceCatalog.IsServiceProvider(npcId);

        /// <summary>
        /// Modelos das opções de serviço deste NPC para o Conversar (habilitada/desabilitada + motivo —
        /// descoberta > ocultação). Sem executor ligado, retorna lista vazia (fail-closed).
        /// </summary>
        public static List<NpcServiceOptionModel> BuildOptions(string npcId)
        {
            var result = new List<NpcServiceOptionModel>();
            var executor = Executor();
            var defs = NpcServiceCatalog.ForNpc(npcId);
            foreach (var def in defs)
            {
                result.Add(executor != null
                    ? executor.BuildOption(def)
                    : new NpcServiceOptionModel(def.ServiceId, def.NpcId, def.DisplayLabel, false, "Servico indisponivel"));
            }

            return result;
        }

        /// <summary>Executa o serviço pelo executor real (gates/custos/efeito). Sem bridge => Failed.</summary>
        public static NpcServiceResult Execute(string serviceId)
        {
            var executor = Executor();
            if (executor == null)
            {
                return new NpcServiceResult(NpcServiceOutcome.Failed, serviceId, "Servico indisponivel.");
            }

            return executor.TryExecute(serviceId);
        }

        /// <summary>Limpa o provedor (teardown de teste para isolar estado estático).</summary>
        public static void ResetForTests()
        {
            ExecutorProvider = null;
        }
    }
}
