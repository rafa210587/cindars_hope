// arch: quebra do par mutuo Core|Player (2026-07-15) — marcador puro (sem membros) que permite a
// GameBootstrap (Core) resolver a referencia do PlayerProgressionManager via DomainManagerRegistry
// sem nomear CindarsHope.Player.Progression. GameBootstrap nao chama nenhum metodo de dominio nesta
// referencia (so a expoe via property), entao a porta nao precisa de membros; consumidores fora de
// Core que precisem da API completa (AddXp, Level, etc.) resolvem o tipo concreto por cast local
// (ex.: bootstrap.PlayerProgressionManager as PlayerProgressionManager).
namespace CindarsHope.Foundation
{
    public interface IPlayerProgressionRuntime
    {
    }
}
