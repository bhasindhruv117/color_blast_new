#pragma warning disable 0649

using System;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(ParticleSystem))]
    public class LineBurstParticle : MonoBehaviour
    {
        [SerializeField] ParticleSystem leftParticle;
        [SerializeField] ParticleSystem rightParticle;
        [SerializeField] SpriteRenderer lineRenderer;
        [SerializeField] SpriteRenderer outlineRenderer;

        [Space]
        [SerializeField] Vector3 horizontalRotation = new Vector3(0, 90, -90);
        [SerializeField] Vector3 verticalRotation = new Vector3(-90, 0, 0);

        private ParticleSystem burstParticle;

        public void Awake()
        {
            burstParticle = GetComponent<ParticleSystem>();
        }

        public void Activate(Color color, Vector3 lineCenterPosition, bool isHorizontal)
        {
            transform.eulerAngles = isHorizontal ? horizontalRotation : verticalRotation;

            LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;

            float lineSizeHalf;
            float elementSize;

            if (isHorizontal)
            {
                lineSizeHalf = levelRepresentation.AreaSize.x / 2;
                elementSize = levelRepresentation.ElementSize.y;
            }
            else
            {
                lineSizeHalf = levelRepresentation.AreaSize.y / 2;
                elementSize = levelRepresentation.ElementSize.x;
            }

            ParticleSystem.MainModule mainLeft = leftParticle.main;
            mainLeft.startColor = color;

            ParticleSystem.ShapeModule shapeLeft = leftParticle.shape;
            shapeLeft.position = new Vector3(0, 0, 1);
            shapeLeft.scale = new Vector3(elementSize * 0.8f, lineSizeHalf, 0.5f);

            ParticleSystem.MainModule mainRight = rightParticle.main;
            mainRight.startColor = color;

            ParticleSystem.ShapeModule shapeRight = rightParticle.shape;
            shapeRight.position = new Vector3(0, 0, 1);
            shapeRight.scale = new Vector3(elementSize * 0.8f, lineSizeHalf, 0.5f);

            leftParticle.Stop();
            rightParticle.Stop();

            lineRenderer.color = color;
            lineRenderer.size = new Vector2(elementSize * 0.5f, lineSizeHalf * 1.5f);

            lineRenderer.transform.localScale = new Vector3(1f, 0f, 1f);
            lineRenderer.DOScaleY(1f, 0.3f).SetEasing(Ease.Type.QuintOut);
            lineRenderer.DOFade(0f, 0.4f).SetEasing(Ease.Type.QuintIn);

            outlineRenderer.color = color.SetAlpha(0f);
            outlineRenderer.size = new Vector2(elementSize, lineSizeHalf * 2f);

            Tween.DelayedCall(0.1f, () =>
            {
                outlineRenderer.color = color.SetAlpha(1f);
                outlineRenderer.DOFade(0f, 0.5f).SetEasing(Ease.Type.QuintIn);

                leftParticle.time = 0;
                leftParticle.Play();

                rightParticle.time = 0;
                rightParticle.Play();
            });
        }
    }
}