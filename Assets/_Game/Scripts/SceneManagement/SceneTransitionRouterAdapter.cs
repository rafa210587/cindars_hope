using CindarsHope.World.Scenes;

namespace CindarsHope.SceneManagement
{
    /// <summary>
    /// arch: facade estatico que dispara uma transicao de cena chamando o SceneTransitionRouter
    /// estatico (CindarsHope.World.Scenes) por tras de uma assinatura de strings primitivos. Existe
    /// para que chamadores fora de World (ex.: Player/Death/AnyaFountainRespawnFlow) disparem a
    /// transicao sem nomear CindarsHope.World.Scenes — quebrando o par mutuo Player|World e
    /// preservando o comportamento exato do router. Vive em SceneManagement, que ja depende de World
    /// em outros arquivos (ScenePortal, SceneSpawnInstaller), entao nao introduz direcao nova.
    /// Sem estado, sem instancia e sem auto-bootstrap de inicializacao (nao registra nada no boot).
    /// </summary>
    public static class SceneTransitionRouterAdapter
    {
        /// <summary>
        /// Executa a transicao. Retorna true em sucesso; em falha, retorna false e preenche
        /// <paramref name="reason"/> com o motivo (ex.: transicao ja em andamento).
        /// </summary>
        public static bool Execute(
            string sourceSceneId,
            string destinationSceneId,
            string targetSpawnAnchorId,
            string initiatingGateId,
            out string reason)
        {
            var request = new SceneTransitionRequest(
                sourceSceneId,
                destinationSceneId,
                targetSpawnAnchorId,
                initiatingGateId);

            var result = SceneTransitionRouter.Execute(request);
            reason = result.Reason;
            return result.Succeeded;
        }
    }
}
