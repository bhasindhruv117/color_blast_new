using UnityEngine;
using Watermelon;

namespace Watermelon
{
    public class CollectedData
    {
        public readonly string CollectableName;
        public readonly int RequiredAmount;
        public readonly CollectableData CollectableData;

        public int Amount;

        public CollectedData(RequirementCollectableData requirementCollectableData)
        {
            CollectableName = requirementCollectableData.CollectableName;
            RequiredAmount = requirementCollectableData.Amount;
            CollectableData = LevelController.GetCollectableObjectData(CollectableName);

            Amount = RequiredAmount;
        }
    }
}
