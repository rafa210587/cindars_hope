using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// SPEC_06: Result object for projectile spawning.
    /// Indicates success/failure and contains the spawned projectile or error info.
    /// </summary>
    public class ProjectileSpawnResult
    {
        public bool Success { get; set; }
        public GameObject Projectile { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }

        public ProjectileSpawnResult()
        {
            Success = false;
            Projectile = null;
            ErrorCode = null;
            Message = null;
        }

        public static ProjectileSpawnResult CreateSuccess(GameObject projectile)
        {
            return new ProjectileSpawnResult
            {
                Success = true,
                Projectile = projectile,
                ErrorCode = null,
                Message = null
            };
        }

        public static ProjectileSpawnResult CreateError(string errorCode, string message)
        {
            return new ProjectileSpawnResult
            {
                Success = false,
                Projectile = null,
                ErrorCode = errorCode,
                Message = message
            };
        }
    }
}
