using UnityEngine;

namespace Watermelon
{
    public class FloatingScoreBehavior : MonoBehaviour
    {
        [SerializeField] AnimationCurve movementCurve;
        [SerializeField] float duration = 0.5f;

        public void Init(Vector3 targetPosition, SimpleCallback targetReachedCallback)
        {
            transform.DOMove(targetPosition, duration).SetCurveEasing(movementCurve).OnComplete(() =>
            {
                targetReachedCallback?.Invoke();

                Destroy(gameObject);
            });
        }
    }
}
