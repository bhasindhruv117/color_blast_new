#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Difficulty Preset", fileName = "Difficulty Preset")]
    public class DifficultyPreset : ScriptableObject
    {
        [SerializeField] DockSpawnSettings[] dockSpawnSettings;
        public DockSpawnSettings[] DockSpawnSettings => dockSpawnSettings;
    }
}