#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelSkinData : AbstractSkinData
    {
        [SkinPreview]
        [SerializeField] Sprite previewSprite;

        [Space]
        [SerializeField] SpriteData[] sprites;

        [Space]
        [SerializeField] Sprite specialSprite;
        public Sprite SpecialSprite => specialSprite;

        public Sprite GetElementSprite(ElementSpriteType type)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].Type == type)
                    return sprites[i].Sprite;
            }

            return specialSprite;
        }

        public SpriteData GetSpriteData(ElementSpriteType type)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].Type == type)
                    return sprites[i];
            }

            Debug.LogError($"Sprite with type: {type} not found.");
            return null;
        }

        
    }
}