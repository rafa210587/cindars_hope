namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_46 — estágios canônicos de romance por NPC, por cima da amizade (F26).
    /// None → Interesse → Namoro → Compromisso. A confissão leva None→Interesse; a progressão
    /// posterior é por marcos simples (N interações de parceiro + 1 presente por estágio).
    ///
    /// "Parceiro" = stage &gt;= Namoro (usado pelo limite de 2 simultâneos, pelas falas IsPartner
    /// e pelo bônus de +50% de presente). Interesse ainda NÃO conta como parceiro.
    /// </summary>
    public enum RomanceStage
    {
        None = 0,
        Interesse = 1,
        Namoro = 2,
        Compromisso = 3
    }
}
