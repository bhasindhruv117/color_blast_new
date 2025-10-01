#pragma warning disable 0649

using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Adventure Level Data", fileName = "Adventure Level Data")]
    public class AdventureModeLevelData : LevelData
    {
        public const float DEFAULT_COLLECTABLES_BLOCK_PERCENTAGE = 0.23f;

        [SerializeField, LevelEditorSetting] bool useInRandomizer;
        public bool UseInRandomizer => useInRandomizer;

        [SerializeField, LevelEditorSetting] LevelRequirementType requirementType;
        public LevelRequirementType RequirementType => requirementType;

        [SerializeField, ShowIf("IsScoreLevelType"), LevelEditorSetting] int requirementScore;
        public int RequirementScore => requirementScore;

        [FormerlySerializedAs("requirementResources")]
        [SerializeField, HideIf("IsScoreLevelType"), LevelEditorSetting] RequirementCollectableData[] requirementCollectables;
        public RequirementCollectableData[] RequirementCollectables => requirementCollectables;

        [Slider(0.0f, 1.0f)]
        [FormerlySerializedAs("resourceBlocksPercent")]
        [SerializeField, HideIf("IsScoreLevelType"), LevelEditorSetting] float collectableBlocksPercent = DEFAULT_COLLECTABLES_BLOCK_PERCENTAGE;
        public float CollectableBlocksPercent => collectableBlocksPercent;

        [Slider(0.0f, 1.0f)]
        [FormerlySerializedAs("resourceSpawnChance")]
        [SerializeField, HideIf("IsScoreLevelType"), LevelEditorSetting] float collectableSpawnChance = 0.6f;
        public float CollectableSpawnChance => collectableSpawnChance;

        [Space]
        [SerializeField, LevelEditorSetting] AdventurePreloadLevelData[] preloadLevelData;
        public AdventurePreloadLevelData[] PreloadLevelData => preloadLevelData;

        protected bool IsScoreLevelType => requirementType == LevelRequirementType.Score;
    }
}