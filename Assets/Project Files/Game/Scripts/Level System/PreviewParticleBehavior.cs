#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PreviewParticleBehavior : MonoBehaviour
    {
        [SerializeField] int rateOverTimePerElement = 4;
        private ParticleSystem previewParticle;

        public void Init()
        {
            previewParticle = GetComponent<ParticleSystem>();

            gameObject.SetActive(false);
        }

        public void Enable(Mesh mesh, Color color, int elementsCount)
        {
            gameObject.SetActive(true);

            ParticleSystem.EmissionModule emission = previewParticle.emission;
            emission.rateOverTime = rateOverTimePerElement * elementsCount;

            ParticleSystem.ShapeModule shape = previewParticle.shape;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.mesh = mesh;

            ParticleSystem.MainModule main = previewParticle.main;
            main.startColor = color;

            previewParticle.Play();
        }

        public void Disable()
        {
            previewParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            gameObject.SetActive(false);
        }
    }
}