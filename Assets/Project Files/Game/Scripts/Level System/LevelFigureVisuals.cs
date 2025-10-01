using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelFigureVisuals
    {
        [SerializeField] Vector2 boundsSize;
        [SerializeField] Vector2Int sizeClamp;

        public LevelFigure LevelFigure { get; private set; }
        public ElementSpriteType ElementSpriteType { get; private set; }

        public Sprite SkinSprite { get; private set; }

        private List<LevelFigureVisualsElement> elements;
        
        private float overridenElementSize = -1;

        private Transform transform;
        public Transform Transform => transform;

        private GameObject gameObject;
        public GameObject GameObject => gameObject;

        public Bounds FigureBounds { get; private set; }

        public float ElementSize { get; private set; }

        public void Init(Transform parentTransform)
        {
            gameObject = new GameObject("[Level Figure]");

            transform = gameObject.transform;
            transform.SetParent(parentTransform);
            transform.ResetLocal();
        }

        public void OverrideSize(float elementSize)
        {
            overridenElementSize = elementSize;
        }

        public void Spawn(LevelFigure levelFigure, ElementSpriteType elementSpriteType, int spriteOrder = 0)
        {
            LevelFigure = levelFigure;
            ElementSpriteType = elementSpriteType;

            if (!elements.IsNullOrEmpty())
            {
                // Clear existing objects
                foreach (LevelFigureVisualsElement block in elements)
                {
                    block?.Destroy();
                }
            }

            SkinSprite = LevelController.GetElementSprite(elementSpriteType);

            elements = new List<LevelFigureVisualsElement>();

            // Clamp the size of the levelFigure
            Vector2Int clampedSize = new Vector2Int(
                Mathf.Max(levelFigure.Size.x, sizeClamp.x),
                Mathf.Max(levelFigure.Size.y, sizeClamp.y)
            );

            // Determine the larger side of the clamped size
            int maxSide = Mathf.Max(clampedSize.x, clampedSize.y);

            // Calculate the size of each element based on the larger side
            float elementSize = Mathf.Min(boundsSize.x, boundsSize.y) / maxSide;

            if (overridenElementSize != -1)
                elementSize = overridenElementSize;

            ElementSize = elementSize;

            // Calculate the offset to center the figure within the bounds
            Vector2 offset = new Vector2(
                (boundsSize.x - levelFigure.Size.x * elementSize) / 2,
                (boundsSize.y - levelFigure.Size.y * elementSize) / 2
            );

            float realWidth = levelFigure.Size.x * elementSize;
            float realHeight = levelFigure.Size.y * elementSize;

            // Initialize bounds
            FigureBounds = new Bounds(Vector3.zero, new Vector3(realWidth, realHeight, 0));

            // Spawn squares based on LevelFigure data
            for (int y = 0; y < levelFigure.Size.y; y++)
            {
                for (int x = 0; x < levelFigure.Size.x; x++)
                {
                    int index = y * levelFigure.Size.x + x;
                    if (levelFigure.Points[index].IsActive)
                    {
                        LevelFigureVisualsElement blockData = new LevelFigureVisualsElement(transform, new Vector2Int(x, y));

                        SpriteRenderer spriteRenderer = blockData.SpriteRenderer;
                        spriteRenderer.sprite = SkinSprite;
                        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                        spriteRenderer.size = new Vector2(elementSize, elementSize);
                        spriteRenderer.sortingOrder = spriteOrder;

                        // Adjust the position to center the figure within the bounds
                        float posX = x * elementSize + offset.x - boundsSize.x / 2 + elementSize / 2;
                        float posY = y * elementSize + offset.y - boundsSize.y / 2 + elementSize / 2;

                        blockData.Transform.localPosition = new Vector3(posX, posY, 0);

                        ISpecialBlockBehavior specialBehavior = levelFigure.Points[index].SpecialBlockBehavior;
                        if (specialBehavior != null)
                        {
                            specialBehavior.ApplyToFigureVisual(blockData);
                        }

                        elements.Add(blockData);
                    }
                }
            }
        }

        public void SetColor(Color color)
        {
            foreach (LevelFigureVisualsElement blockData in elements)
            {
                blockData.SpriteRenderer.color = color;
            }
        }
    }
}

