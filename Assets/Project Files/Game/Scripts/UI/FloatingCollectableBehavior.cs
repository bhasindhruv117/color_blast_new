using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FloatingCollectableBehavior : MonoBehaviour
    {
        [SerializeField] Image collectableImage;

        [Space]
        [SerializeField] AnimationCurve movementCurve;

        private UICollectableRequirementElement targetElement;

        public void Init(CollectableData collectableData, Vector3 targetPosition, UICollectableRequirementElement targetElement, FloatingCollectableCallback targetReachedCallback)
        {
            this.targetElement = targetElement;

            collectableImage.sprite = collectableData.Icon;

            transform.DOMove(targetPosition, 0.5f, delay: Collectables.PickedCollectableIndex * 0.04f).SetCurveEasing(movementCurve).OnComplete(() =>
            {
                targetReachedCallback?.Invoke(targetElement);

                Destroy(gameObject);
            });
        }

        public delegate void FloatingCollectableCallback(UICollectableRequirementElement requirementElement);
    }
}
