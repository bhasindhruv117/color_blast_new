#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Level Database", fileName = "Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] AdventureModeLevelData[] levels;
        public LevelData[] Levels => levels;

        [SerializeField, LevelEditorSetting] InfiniteModeLevelData infiniteLevelData;
        public LevelData InfiniteLevelData => infiniteLevelData;

        [Space]
        [SerializeField] LevelAnimation defaultSpawnAnimation;
        public LevelAnimation DefaultSpawnAnimation => defaultSpawnAnimation;
        [SerializeField] LevelAnimation failAnimation;
        public LevelAnimation FailAnimation => failAnimation;

        [Space]
        [SerializeField, LevelEditorSetting] LevelFigure[] figures;
        public LevelFigure[] Figures => figures;

        [Space]
        [SerializeField, LevelEditorSetting] CollectableData[] collectables;
        public CollectableData[] Collectables => collectables;

        public int AmountOfLevels => levels.Length;

        /// <summary>
        /// Is called when LevelController is initialized
        /// </summary>
        public void Init()
        {

        }

        /// <summary>
        /// Is called when LevelController component is destroyed (each time when Game scene is unloaded)
        /// </summary>
        public void Unload()
        {

        }

        public LevelData GetRandomLevel()
        {
            int randomLevelIndex;

            int attempts = 0;

            do
            {
                randomLevelIndex = Random.Range(0, levels.Length);

                attempts++;
                if (attempts > 100)
                {
                    return levels[randomLevelIndex];
                }
            }
            while (!levels[randomLevelIndex].UseInRandomizer);

            return levels[randomLevelIndex];
        }

        public LevelData GetLevel(int i)
        {
            if (i < AmountOfLevels && i >= 0)
                return levels[i];

            return GetRandomLevel();
        }
    }
}
