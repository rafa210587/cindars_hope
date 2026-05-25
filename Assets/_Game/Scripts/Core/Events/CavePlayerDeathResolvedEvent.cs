namespace CindarsHope.Core.Events
{
    public struct CavePlayerDeathResolvedEvent
    {
        public string CorpseId { get; set; }
        public string CaveRunId { get; set; }
        public int CaveLevel { get; set; }
    }
}
