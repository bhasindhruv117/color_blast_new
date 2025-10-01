using UnityEngine;

namespace Watermelon
{
    public class LevelRepresentation
    {
        public LevelData LevelData { get; private set; }

        public LevelElementBehavior[,] LevelMatrix { get; private set; }

        public Transform LevelTransform { get; private set; }

        public LevelSkinData LevelSkinData { get; private set; }

        private Vector2 areaSize;
        private Vector2 elementSize;
        private Vector2Int gridSize;
        private Transform areaTransform;
        private Vector2 startPosition;

        public Vector2 AreaSize => areaSize;
        public Vector2 ElementSize => elementSize;

        public LevelRepresentation(LevelData level)
        {
            LevelData = level;
            LevelMatrix = new LevelElementBehavior[LevelData.Size.x, LevelData.Size.y];

            GameObject levelObject = new GameObject("[LEVEL]");
            LevelTransform = levelObject.transform;

            LevelSkinData = (LevelSkinData)SkinController.Instance.GetSelectedSkin<LevelSkinDatabase>();
        }

        public void SpawnLevelElements(GameObject prefab)
        {
            Vector2Int levelSize = LevelData.Size;
            for (int y = 0; y < levelSize.y; y++)
            {
                for (int x = 0; x < levelSize.x; x++)
                {
                    GameObject elementObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, LevelTransform);

                    LevelElementBehavior elementBehavior = elementObject.GetComponent<LevelElementBehavior>();
                    elementBehavior.Init(new Vector2Int(x, y), this);

                    LevelMatrix[x, y] = elementBehavior;
                }
            }
        }

        public void PlacePreloadElements(AdventurePreloadLevelData[] preloadLevelData)
        {
            foreach (AdventurePreloadLevelData levelData in preloadLevelData)
            {
                Vector2Int elementPosition = levelData.Position;

                if (elementPosition.x < 0 || elementPosition.x >= LevelMatrix.GetLength(0) || elementPosition.y < 0 || elementPosition.y >= LevelMatrix.GetLength(1)) continue;

                LevelElementBehavior levelElement = LevelMatrix[elementPosition.x, elementPosition.y];

                Sprite elementSprite;
                if (levelData.UseRandomSprite)
                {
                    elementSprite = LevelController.GetElementSprite(LevelController.GetRandomElementSprite());
                }
                else
                {
                    elementSprite = LevelController.GetElementSprite(levelData.SpriteType);
                }

                ISpecialBlockBehavior specialBlockBehavior = null;
                if (!string.IsNullOrEmpty(levelData.CollectableName))
                {
                    specialBlockBehavior = new CollectableSpecialBlockBehavior(LevelController.GetCollectableObjectData(levelData.CollectableName));
                }

                levelElement.PlaceElement(elementSprite, specialBlockBehavior);
            }
        }

        public void PlaceOnLevelArea(LevelAreaBehavior areaBehavior)
        {
            areaTransform = areaBehavior.transform;
            elementSize = areaBehavior.ElementSize;
            areaSize = areaBehavior.AreaSize;
            gridSize = LevelData.Size;

            startPosition = new Vector2(
                areaTransform.position.x - (gridSize.x * elementSize.x) / 2 + elementSize.x / 2,
                areaTransform.position.y - (gridSize.y * elementSize.y) / 2 + elementSize.y / 2
            );

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2 position = new Vector2(
                        startPosition.x + x * elementSize.x,
                        startPosition.y + y * elementSize.y
                    );

                    LevelElementBehavior elementBehavior = LevelMatrix[x, y];
                    elementBehavior.transform.position = position;
                    elementBehavior.SetSize(elementSize);
                }
            }
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            // Calculate the grid position
            int x = Mathf.FloorToInt((worldPosition.x - startPosition.x) / elementSize.x);
            int y = Mathf.FloorToInt((worldPosition.y - startPosition.y) / elementSize.y);

            // Ensure the position is within the bounds of the grid
            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            {
                return new Vector2Int(-1, -1); // Return an invalid position if out of bounds
            }

            return new Vector2Int(x, y);
        }

        public void WorldToGridPosition(Vector3 worldPosition, ref Vector2Int gridPosition)
        {
            // Calculate the grid position
            int x = Mathf.FloorToInt((worldPosition.x - startPosition.x + elementSize.x * 0.5f) / elementSize.x);
            int y = Mathf.FloorToInt((worldPosition.y - startPosition.y + elementSize.y * 0.5f) / elementSize.y);

            // Ensure the position is within the bounds of the grid
            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            {
                gridPosition.x = -1;
                gridPosition.y = -1;

                return;
            }

            gridPosition.x = x;
            gridPosition.y = y;
        }

        public Vector3 GetLineCenterPosition(int linePosition, bool isHorizontal)
        {
            if (isHorizontal)
            {
                float centerX = startPosition.x + (gridSize.x * elementSize.x) / 2 - elementSize.x / 2;
                float centerY = startPosition.y + linePosition * elementSize.y;
                return new Vector3(centerX, centerY, 0);
            }
            else
            {
                float centerX = startPosition.x + linePosition * elementSize.x;
                float centerY = startPosition.y + (gridSize.y * elementSize.y) / 2 - elementSize.y / 2;
                return new Vector3(centerX, centerY, 0);
            }
        }

        public bool[,] GetSimplifiedLevelMatrix()
        {
            int rows = LevelMatrix.GetLength(0);
            int cols = LevelMatrix.GetLength(1);

            bool[,] boolMatrix = new bool[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    boolMatrix[i, j] = LevelMatrix[i, j].IsOccupied;
                }
            }

            return boolMatrix;
        }

        public void Clear()
        {

        }
    }
}