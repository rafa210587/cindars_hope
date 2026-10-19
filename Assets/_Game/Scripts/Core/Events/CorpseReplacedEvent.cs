namespace CindarsHope.Core.Events
{
    public struct CorpseReplacedEvent
    {
        public string OldCorpseId { get; set; }
        public string NewCorpseId { get; set; }
    }
}
