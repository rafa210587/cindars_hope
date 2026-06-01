#if UNITY_EDITOR

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Represents a single validation issue found during architecture validation.
    /// </summary>
    public class ValidationIssue
    {
        public string Area { get; set; }
        public string Code { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; }
        public string AssetPath { get; set; }
        public string ObjectName { get; set; }
        public string SuggestedFix { get; set; }

        public ValidationIssue()
        {
        }

        public ValidationIssue(string area, string code, ValidationSeverity severity, string message,
            string assetPath = "", string objectName = "", string suggestedFix = "")
        {
            Area = area;
            Code = code;
            Severity = severity;
            Message = message;
            AssetPath = assetPath;
            ObjectName = objectName;
            SuggestedFix = suggestedFix;
        }

        public override string ToString()
        {
            return $"[{Severity}] {Area}.{Code}: {Message}" +
                   (string.IsNullOrEmpty(AssetPath) ? "" : $" (Asset: {AssetPath})") +
                   (string.IsNullOrEmpty(ObjectName) ? "" : $" (Object: {ObjectName})") +
                   (string.IsNullOrEmpty(SuggestedFix) ? "" : $" [Fix: {SuggestedFix}]");
        }
    }
}

#endif
