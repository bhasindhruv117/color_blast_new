#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Level Data", fileName = "Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] protected Vector2Int size = new Vector2Int(8, 8);
        public Vector2Int Size => size;

        // dock settings
        [SerializeField, LevelEditorSetting] protected bool useRandomColorsForDockFigures = true;
        public bool UseRandomColorsForDockFigures => useRandomColorsForDockFigures;

        [SerializeField, LevelEditorSetting] protected ElementSpriteType[] dockFiguresColors;
        public ElementSpriteType[] DockFiguresSprites => dockFiguresColors;

        [SerializeField, LevelEditorSetting] protected DifficultyPreset difficultyPreset;
        public DifficultyPreset DifficultyPreset => difficultyPreset;

        [SerializeField, LevelEditorSetting] protected CurrencyAmount completeReward;
        public CurrencyAmount CompleteReward => completeReward;

        [SerializeField] protected LevelAnimation customSpawnAnimation;
        public LevelAnimation CustomSpawnAnimation => customSpawnAnimation;
    }
}