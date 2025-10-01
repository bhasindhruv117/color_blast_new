using UnityEngine;

namespace Watermelon
{
    public class UICollectablesRequirementsPanel : MonoBehaviour, IWinCondition
    {
        [SerializeField] UICollectableRequirementElement prefab;
        [SerializeField] Transform containerTransform;

        [Space]
        [SerializeField] GameObject floatingCollectablePrefab;

        private UICollectableRequirementElement[] collectablesRequirementElements;

        public void Init(RequirementCollectableData[] requirementCollectables)
        {
            collectablesRequirementElements = new UICollectableRequirementElement[requirementCollectables.Length];
            for (int i = 0; i < requirementCollectables.Length; i++)
            {
                UICollectableRequirementElement collectableRequirementElement = Instantiate(prefab, containerTransform);

                collectableRequirementElement.Init(requirementCollectables[i]);
                collectableRequirementElement.HideAmount();

                collectablesRequirementElements[i] = collectableRequirementElement;
            }
        }

        public bool IsWinConditionMet()
        {
            return Collectables.IsAllCollected();
        }

        public void SpawnFloatingCollectable(CollectableData collectableData, Vector3 worldPosition, FloatingCollectableBehavior.FloatingCollectableCallback targetReachedCallback)
        {
            UICollectableRequirementElement targetElement = FindCollectableRequirementElement(collectableData.ID);
            if (targetElement != null)
            {
                GameObject floatingCollectable = Instantiate(floatingCollectablePrefab, worldPosition, Quaternion.identity);
                floatingCollectable.transform.SetParent(transform);
                floatingCollectable.transform.localScale = Vector3.one;

                FloatingCollectableBehavior floatingCollectableBehavior = floatingCollectable.GetComponent<FloatingCollectableBehavior>();
                floatingCollectableBehavior.Init(collectableData, targetElement.transform.position, targetElement, targetReachedCallback);
            }
        }

        private UICollectableRequirementElement FindCollectableRequirementElement(string collectableName)
        {
            foreach (UICollectableRequirementElement element in collectablesRequirementElements)
            {
                if (element.CollectableName == collectableName)
                {
                    return element;
                }
            }

            return null;
        }
    }
}
