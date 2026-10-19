using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Validates the visual-only landmark composition after FarmScene regeneration.</summary>
    public static class ValidateFarmSceneLandmarks
    {
        [MenuItem("CindarsHope/Validar Marcos FarmScene")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateConfiguration(out var validLandmarks);
            Debug.Log("ValidateFarmSceneLandmarks: Landmarks valid: " + validLandmarks + "/7");
            Debug.Log("ValidateFarmSceneLandmarks: " + errors + " error(s)");
        }

        public static int ValidateConfiguration(out int validLandmarks)
        {
            var errors = 0;
            validLandmarks = 0;
            foreach (FarmLandmarkId landmark in System.Enum.GetValues(typeof(FarmLandmarkId)))
            {
                var landmarkErrors = ValidateLandmark(landmark);
                errors += landmarkErrors;
                if (landmarkErrors == 0) validLandmarks++;
            }

            return errors;
        }

        private static int ValidateLandmark(FarmLandmarkId landmark)
        {
            var errors = 0;
            var requiredObjects = FarmLandmarkCompositionContract.RequiredSceneObjects(landmark);
            for (var i = 0; i < requiredObjects.Count; i++)
            {
                if (FindTransform(requiredObjects[i]) != null) continue;
                errors++;
                Debug.LogError("ValidateFarmSceneLandmarks: " + landmark + " missing required object " + requiredObjects[i]);
            }

            var approaches = CountWalkableApproaches(landmark);
            var requiredApproaches = FarmLandmarkCompositionContract.RequiredApproachCount(landmark);
            if (approaches < requiredApproaches)
            {
                errors++;
                Debug.LogError("ValidateFarmSceneLandmarks: " + landmark + " approaches=" + approaches + "/" + requiredApproaches);
            }

            if (landmark == FarmLandmarkId.CentralField)
            {
                Debug.Log("ValidateFarmSceneLandmarks: CentralField: " + (errors == 0 ? "PASS" : "FAIL") + " (rows>=4, approaches>=" + requiredApproaches + ")");
            }

            return errors;
        }

        private static int CountWalkableApproaches(FarmLandmarkId landmark)
        {
            var points = landmark == FarmLandmarkId.CentralField
                ? new[] { new Vector2(-2f, 8f), new Vector2(-2f, -8f), new Vector2(-13f, 0f), new Vector2(9f, 0f) }
                : new[] { GetBoundsApproach(FarmLandmarkCompositionContract.GetBounds(landmark)) };

            var walkable = 0;
            for (var i = 0; i < points.Length; i++)
            {
                if (!FarmSceneNavigationRaster.IsBlocked(points[i])) walkable++;
            }
            return walkable;
        }

        private static Vector2 GetBoundsApproach(Bounds bounds)
        {
            return new Vector2(bounds.center.x, bounds.min.y - 1f);
        }

        private static Transform FindTransform(string name)
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == name) return transforms[i];
            }
            return null;
        }
    }
}
