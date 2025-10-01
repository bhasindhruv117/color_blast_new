#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class DockSpawnSettings
    {
        [SerializeField] int targetScore;
        public int TargetScore => targetScore;

        [Slider(0.0f, 1.0f)]
        [SerializeField] float perfectPlacementChance;
        public float PerfectPlacementChance => perfectPlacementChance;
    }
}