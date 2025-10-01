using UnityEngine;

namespace Watermelon
{
    public class LevelFigureVisualsElement
    {
        public Vector2Int Position { get; private set; }
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }

        public SpriteRenderer SpecialSpriteRenderer { get; private set; }

        public LevelFigureVisualsElement(Transform parentTransform, Vector2Int position)
        {
            Position = position;

            GameObject = new GameObject("[Square]");

            Transform = GameObject.transform;
            Transform.SetParent(parentTransform);

            SpriteRenderer = GameObject.AddComponent<SpriteRenderer>();
        }

        public void SetSpecialSprite(Sprite sprite)
        {
            if(SpecialSpriteRenderer == null)
            {
                GameObject specialGameObject = new GameObject("[Special Sprite]");

                Transform specialTransform = specialGameObject.transform;
                specialTransform.SetParent(Transform);
                specialTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                SpecialSpriteRenderer = specialGameObject.AddComponent<SpriteRenderer>();
                SpecialSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            }

            SpecialSpriteRenderer.sprite = sprite;
            SpecialSpriteRenderer.size = SpriteRenderer.size;
            SpecialSpriteRenderer.sortingOrder = SpriteRenderer.sortingOrder + 1;
        }

        public void Destroy()
        {
            if (GameObject != null)
            {
                GameObject.Destroy(GameObject);

                GameObject = null;
            }
        }
    }
}

