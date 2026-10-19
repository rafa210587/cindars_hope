namespace CindarsHope.UI.Magic
{
    public class SpellDetailViewModel
    {
        public string SpellId { get; set; }
        public string SpellName { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public int CooldownSeconds { get; set; }
        public string[] Effects { get; set; }
        public string[] ScalingStats { get; set; }
        public bool IsLocked { get; set; }

        public string LockReason { get; set; }
        public bool CanCast => !IsLocked;
    }
}
