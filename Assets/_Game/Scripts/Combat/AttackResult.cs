namespace CindarsHope.Combat
{
    public class AttackResult
    {
        public bool Success { get; private set; }
        public string ErrorCode { get; private set; }
        public string Message { get; private set; }

        private AttackResult() { }

        public static AttackResult CreateSuccess()
        {
            return new AttackResult { Success = true };
        }

        public static AttackResult CreateError(string errorCode, string message = null)
        {
            return new AttackResult { Success = false, ErrorCode = errorCode, Message = message };
        }
    }
}
