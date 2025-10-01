#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class RequirementCollectableData
    {
        [LevelDataPicker(LevelDataType.CollectableObject)]
        [SerializeField] string collectableName;
        public string CollectableName => collectableName;

        [SerializeField] int amount;
        public int Amount => amount;

        public RequirementCollectableData(string collectableName, int amount)
        {
            this.collectableName = collectableName;
            this.amount = amount;
        }
    }
}