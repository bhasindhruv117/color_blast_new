#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class LevelElementBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer backgroundSprite;
        [SerializeField] SpriteRenderer spriteRenderer;

        public Vector2Int Position { get; private set; }
        public Vector2 Size { get; private set; }

        public bool IsOccupied { get; private set; }
        public bool IsCollected { get; private set; }

        public bool IsPreviewActive { get; private set; }
        public bool IsLinePreview { get; private set; }

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private LevelRepresentation levelRepresentation;

        private ISpecialBlockBehavior specialBlockBehavior;

        private Sprite defaultSprite;

        public void Init(Vector2Int position, LevelRepresentation levelRepresentation)
        {
            this.levelRepresentation = levelRepresentation;

            Position = position;
            IsOccupied = false;
            IsCollected = false;

            name = $"Element [{position.x}, {position.y}]";

            spriteRenderer.enabled = false;
        }

        public void SetSize(Vector2 size)
        {
            Size = size;

            backgroundSprite.size = size * 0.95f;
            spriteRenderer.size = size;
        }

        public void PlaceElement(Sprite sprite, ISpecialBlockBehavior specialBlockBehavior)
        {
            if (IsOccupied) return;

            IsOccupied = true;

            this.specialBlockBehavior = specialBlockBehavior;

            defaultSprite = sprite;

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = true;

            specialBlockBehavior?.ApplyToLevelElement(this);
        }

        public void SetSprite(Sprite sprite)
        {
            defaultSprite = sprite;

            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = true;
        }

        public void SetSprite(ElementSpriteType elementSprite)
        {
            defaultSprite = levelRepresentation.LevelSkinData.GetElementSprite(elementSprite);

            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = true;
        }

        public void EnablePreview(Sprite sprite, Color color, bool linePreview)
        {
            IsPreviewActive = true;
            IsLinePreview = linePreview;

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.enabled = true;
        }

        public void DisablePreview()
        {
            if (!IsPreviewActive) return;

            IsPreviewActive = false;
            IsLinePreview = false;

            if (IsOccupied)
            {
                spriteRenderer.sprite = defaultSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.enabled = false;
            }
        }

        public void MarkAsCollect()
        {
            if (IsCollected) return;

            IsCollected = true;
        }

        public void Collect()
        {
            if (!IsCollected) return;

            specialBlockBehavior?.OnLevelElementCollected(this);

            IsCollected = false;

            Reset();
        }

        public void Reset()
        {
            IsCollected = false;
            IsOccupied = false;

            IsPreviewActive = false;
            IsLinePreview = false;

            spriteRenderer.enabled = false;
        }
    }
}