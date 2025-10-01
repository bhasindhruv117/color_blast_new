#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CollectableData
    {
        [SerializeField] string id;
        public string ID => id;

        [Space]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] AudioClip pickAudioClip;
        public AudioClip PickAudioClip => pickAudioClip;

        public Sprite CombinedSprite { get; private set; }

        public void GenerateCombinedSprite(Sprite backgroundSprite)
        {
            CombinedSprite = SpriteUtils.CombineSprites(backgroundSprite, icon);
        }
    }
}
