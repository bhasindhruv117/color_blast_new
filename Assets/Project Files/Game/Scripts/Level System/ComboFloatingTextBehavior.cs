#pragma warning disable 0618

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ComboFloatingTextBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text textRef;
        [SerializeField] ParticleSystem splashParticleSystem;

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

        private ParticleCase particleCase;
        private bool isParticleDisabled;

        private bool isActive;

        public event SimpleCallback Completed;

        private void Awake()
        {
            defaultScale = transform.localScale;

            if (splashParticleSystem != null)
            {
                ParticleSystem.MainModule mainParticle = splashParticleSystem.main;
                if (mainParticle.loop)
                    Debug.LogError("Splash particle system should not loop.", gameObject);

                isParticleDisabled = false;
            }
            else
            {
                // Automatically disable particle if it's not set
                isParticleDisabled = true;
            }
        }

        public void Activate(string text, Color color)
        {
            isActive = true;

            textRef.text = text;
            textRef.color = color;

            RepositionText();

            transform.localScale = Vector3.zero;

            scaleTween = transform.DOScale(defaultScale, scaleTime).SetCurveEasing(scaleAnimationCurve).OnComplete(OnTweenCompleted);
            moveTween = transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(OnTweenCompleted);

            if (splashParticleSystem != null)
            {
                particleCase = splashParticleSystem.PlayCase();
                particleCase.Disabled += OnParticleDisabled;
                particleCase.ApplyToParticles((particle) =>
                {
                    ParticleSystem.MainModule mainParticle = particle.main;
                    mainParticle.startColor = color;
                });
            }
        }

        private void RepositionText()
        {
            // Calculate the width of the text
            textRef.ForceMeshUpdate();
            Bounds textBounds = textRef.textBounds;
            float textWidth = textBounds.size.x;

            // Convert text width from local space to world space
            float textWidthWorld = textRef.rectTransform.TransformVector(new Vector3(textWidth, 0, 0)).x;

            // Get the screen width in world units
            float screenWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect * 0.96f;

            // Calculate the new position to ensure the text fits within the screen
            Vector3 newPosition = transform.position;
            if (newPosition.x + textWidthWorld / 2 > screenWidth / 2)
            {
                newPosition.x = screenWidth / 2 - textWidthWorld / 2;
            }
            else if (newPosition.x - textWidthWorld / 2 < -screenWidth / 2)
            {
                newPosition.x = -screenWidth / 2 + textWidthWorld / 2;
            }

            transform.position = newPosition;
        }

        private void OnParticleDisabled()
        {
            if (!isActive) return;

            isParticleDisabled = true;

            if ((moveTween != null && moveTween.IsCompleted) && (scaleTween != null && scaleTween.IsCompleted))
            {
                isActive = false;

                Completed?.Invoke();

                Destroy(gameObject);
            }
        }

        private void OnTweenCompleted()
        {
            if (!isActive) return;

            if (isParticleDisabled && (moveTween != null && moveTween.IsCompleted) && (scaleTween != null && scaleTween.IsCompleted))
            {
                isActive = false;

                Completed?.Invoke();

                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            isActive = false;

            if (particleCase != null)
                particleCase.ForceDisable();

            scaleTween.KillActive();
            moveTween.KillActive();
        }
    }
}