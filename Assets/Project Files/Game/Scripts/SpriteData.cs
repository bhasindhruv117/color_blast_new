using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SpriteData
    {
        [SerializeField] ElementSpriteType type;
        public ElementSpriteType Type => type;

        [SerializeField] Sprite sprite;
        public Sprite Sprite => sprite;

        [Space]
        [SerializeField] Color previewParticleColor = Color.white;
        public Color PreviewParticleColor => previewParticleColor;

        [SerializeField] Color burstParticleColor = Color.white;
        public Color BurstParticleColor => burstParticleColor;
    }
}