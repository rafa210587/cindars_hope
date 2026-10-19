#if UNITY_EDITOR

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Severity level for architecture validation issues.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Informational message, no action required.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning about a potential issue that should be reviewed.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error that prevents the system from functioning correctly.
        /// </summary>
        Error = 2
    }
}

#endif
