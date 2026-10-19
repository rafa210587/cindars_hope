#if UNITY_EDITOR

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Contract for architecture validators.
    /// </summary>
    public interface IProjectValidator
    {
        /// <summary>
        /// Unique identifier for this validator (e.g., "projectile_validator", "combat_db_validator").
        /// </summary>
        string ValidatorId { get; }

        /// <summary>
        /// Human-readable display name (e.g., "Projectile System Validator").
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Run the validation and return a report with issues.
        /// </summary>
        ValidationReport Run();
    }
}

#endif
