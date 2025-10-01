using UnityEngine;

namespace Watermelon.Map
{
    [CreateAssetMenu(menuName = "Data/Map Data", fileName = "Map Data")]
    public class MapData : ScriptableObject
    {
        [SerializeField] GameObject[] chunks;
        public GameObject[] Chunks => chunks;

        [SerializeField] bool infiniteMap = false;
        public bool InfiniteMap => infiniteMap;

        [SerializeField] bool adjustForWideScreenes = true;
        public bool AdjustForWideScreenes => adjustForWideScreenes;

        [SerializeField] float currentLevelVerticalOffset = 0.4f;
        public float CurrentLevelVerticalOffset => currentLevelVerticalOffset;

        [SerializeField] float firstChunkMaxLevelVerticalOffset = 0.2f;
        public float FirstChunkMaxLevelVerticalOffset => firstChunkMaxLevelVerticalOffset;

        [SerializeField] float lastLevelVerticalOffset = 0.7f;
        public float LastLevelVerticalOffset => lastLevelVerticalOffset;
    }
}