#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class PreloadLevelData
    {
        [SerializeField] protected Vector2Int position;
        public Vector2Int Position => position;

        [SerializeField] protected bool useRandomSprite;
        public bool UseRandomSprite => useRandomSprite;

        [SerializeField] protected ElementSpriteType spriteType;
        public ElementSpriteType SpriteType => spriteType;
    }
}