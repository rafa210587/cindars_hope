using Alias = Pilot.Thing;
namespace Pilot {
    public partial class Thing { }
    public class Caller {
        public Outer.Inner Nested { get; set; }
        public Thing Current { get; set; }
        public Thing Echo(Thing value) { return value; }
        public void Call() {
            var one = new Alias();
            one.Work(1);
            one.Work("x");
            one.Extend();
#if FEATURE_ON
            one.Work(2);
#else
            one.Work("off");
#endif
        }
    }
}
