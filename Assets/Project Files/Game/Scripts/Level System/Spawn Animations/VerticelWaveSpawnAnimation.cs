using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Vertical Wave Spawn Animation", menuName = "Data/Level/Vertical Wave Spawn Animation")]
    public class VerticelWaveSpawnAnimation : LevelAnimation
    {
        [SerializeField] float elementFadeInDuration = 0.2f;
        [SerializeField] float elementFadeInDelay = 0.06f;

        [Space]
        [SerializeField] float elementFadeOutDuration = 0.16f;
        [SerializeField] float elementFadeOutDelay = 0.06f;

        [Space]
        [SerializeField] float fadeOutDelay = 1.0f;

        private TweenCase tweenCase;

        public override IEnumerator PlayStartAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted)
        {
            LevelElementBehavior[,] levelMatrix = levelRepresentation.LevelMatrix;

            Color alphaColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            Color targetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f); 
            
            int width = levelMatrix.GetLength(0);
            int height = levelMatrix.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    LevelElementBehavior element = levelMatrix[x, y];
                    if (!element.IsOccupied)
                    {
                        element.SetSprite(LevelController.GetRandomElementSprite());
                    }
                }
            }

            tweenCase = new FadeTweenCase(levelRepresentation, elementFadeInDuration, elementFadeInDelay, alphaColor, targetColor);
            tweenCase.StartTween();

            yield return new WaitForSeconds(tweenCase.Duration + fadeOutDelay);

            tweenCase = new FadeTweenCase(levelRepresentation, elementFadeOutDuration, elementFadeOutDelay, targetColor, alphaColor);
            tweenCase.StartTween();

            yield return new WaitForSeconds(tweenCase.Duration);

            onAnimationCompleted?.Invoke();
        }

        public override IEnumerator PlayLoseAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted)
        {
            LevelElementBehavior[,] levelMatrix = levelRepresentation.LevelMatrix;

            Color alphaColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            Color targetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            int width = levelMatrix.GetLength(0);
            int height = levelMatrix.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    LevelElementBehavior element = levelMatrix[x, y];
                    if (!element.IsOccupied)
                    {
                        element.SetSprite(LevelController.GetRandomElementSprite());
                    }
                }
            }

            tweenCase = new FadeTweenCase(levelRepresentation, elementFadeInDuration, elementFadeInDelay, alphaColor, targetColor);
            tweenCase.StartTween();

            yield return new WaitForSeconds(tweenCase.Duration + fadeOutDelay);

            onAnimationCompleted?.Invoke();
        }

        public override void Clear()
        {
            tweenCase.KillActive();
        }

        public class FadeTweenCase : TweenCase
        {
            private List<LevelElementBehavior> elements;

            private float[] startTime;
            private float[] endTime;

            private Color startColor;
            private Color targetColor;

            public FadeTweenCase(LevelRepresentation levelRepresentation, float duration, float elementDelay, Color startColor, Color targetColor)
            {
                this.startColor = startColor;
                this.targetColor = targetColor;

                LevelElementBehavior[,] levelMatrix = levelRepresentation.LevelMatrix;

                elements = new List<LevelElementBehavior>();

                int width = levelMatrix.GetLength(0);
                int height = levelMatrix.GetLength(1);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        LevelElementBehavior element = levelMatrix[x, y];
                        if (!element.IsOccupied)
                        {
                            SpriteRenderer spriteRenderer = element.SpriteRenderer;
                            spriteRenderer.color = startColor;

                            elements.Add(element);
                        }
                    }
                }

                startTime = new float[elements.Count];
                endTime = new float[elements.Count];

                float longestDelay = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    Vector2Int elementPosition = elements[i].Position;

                    startTime[i] = elementPosition.y * elementDelay;
                    endTime[i] = startTime[i] + duration;

                    if (longestDelay <= endTime[i])
                        longestDelay = endTime[i];
                }

                // Set total duration based on animation duration + longest delay
                base.duration = longestDelay;
            }

            public override void DefaultComplete()
            {
                for (int i = 0; i < startTime.Length; i++)
                {
                    elements[i].SpriteRenderer.color = targetColor;
                }
            }

            public override void Invoke(float deltaTime)
            {
                for (int i = 0; i < startTime.Length; i++)
                {
                    float reclampedState = Mathf.InverseLerp(startTime[i], endTime[i], state);

                    elements[i].SpriteRenderer.color = Color.Lerp(startColor, targetColor, Interpolate(reclampedState));
                }
            }

            public override bool Validate()
            {
                return true;
            }
        }
    }
}