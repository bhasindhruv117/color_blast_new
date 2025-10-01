#pragma warning disable 0618

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ScoreFloatingTextBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text textRef;

        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] AnimationCurve scaleAnimationCurve;

        private Vector3 defaultScale;

        private TweenCase scaleTween;
        private TweenCase moveTween;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public void Activate(string text, Color color)
        {
            textRef.text = text;
            textRef.color = color;

            transform.localScale = Vector3.zero;

            scaleTween = transform.DOScale(defaultScale, scaleTime).SetCurveEasing(scaleAnimationCurve).OnComplete(OnTweenCompleted);
            moveTween = transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(OnTweenCompleted);
        }

        private void OnTweenCompleted()
        {
            if ((moveTween != null && moveTween.IsCompleted) && (scaleTween != null && scaleTween.IsCompleted))
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            scaleTween.KillActive();
            moveTween.KillActive();
        }
    }
}