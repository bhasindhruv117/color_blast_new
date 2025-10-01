#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class LevelAreaBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] SpriteRenderer outlineSpriteRenderer;

        [Space]
        [SerializeField] Vector2 borderOffset;
        [SerializeField] Vector2 maxAreaSize;

        public Vector2 ElementSize { get; private set; }
        public Vector2 AreaSize { get; private set; }

        public Vector2 MaxAreaSize => maxAreaSize;

        public void RecalculateSize(Vector2Int size)
        {
            float aspectRatio = (float)size.x / size.y;
            Vector2 areaSize = maxAreaSize;

            if (maxAreaSize.x / maxAreaSize.y > aspectRatio)
            {
                // MaxAreaSize is wider than the desired aspect ratio, so limit by height
                areaSize.x = maxAreaSize.y * aspectRatio;
            }
            else
            {
                // MaxAreaSize is taller than the desired aspect ratio, so limit by width
                areaSize.y = maxAreaSize.x / aspectRatio;
            }

            backgroundSpriteRenderer.size = new Vector2(areaSize.x - 0.1f, areaSize.y - 0.1f);
            outlineSpriteRenderer.size = new Vector2(areaSize.x, areaSize.y);

            AreaSize = new Vector2(areaSize.x - borderOffset.x * 2, areaSize.y - borderOffset.y * 2);
            ElementSize = new Vector2(AreaSize.x / size.x, AreaSize.y / size.y);
        }

        public void ApplyScreenSize(ScreenSize screenSize)
        {
            maxAreaSize = screenSize.AreaSize;

            transform.position = screenSize.AreaPosition;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 position = transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(position, maxAreaSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(position, new Vector2(maxAreaSize.x - borderOffset.x * 2, maxAreaSize.y - borderOffset.y * 2));

            if (Application.isPlaying) return;

            ResizeSprites();
        }

        public void ResizeSprites()
        {
            if (backgroundSpriteRenderer != null)
                backgroundSpriteRenderer.size = new Vector2(maxAreaSize.x - 0.1f, maxAreaSize.y - 0.1f);

            if (outlineSpriteRenderer != null)
                outlineSpriteRenderer.size = maxAreaSize;
        }
    }
}