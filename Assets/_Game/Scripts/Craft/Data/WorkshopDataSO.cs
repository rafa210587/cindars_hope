using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Craft.Data
{
    [CreateAssetMenu(fileName = "WorkshopData", menuName = "CindarsHope/Craft/Workshop")]
    public class WorkshopDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;

        public string Id => _id;
        public string DisplayName;
        [TextArea] public string Description;
        public WorkshopType WorkshopType;
        public int Level = 1;

        private void OnValidate()
        {
            Level = Mathf.Max(1, Level);
        }
    }
}
