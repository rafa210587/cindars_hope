namespace Pilot {
    public partial class Thing {
        public void Work(int value) { }
        public void Work(string value) { }
    }
    public static class Extensions {
        public static void Extend(this Thing value) { }
    }
    public class Outer { public class Inner { } }
}
