#pragma warning disable 0649

using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [System.Serializable]
    public class AdventurePreloadLevelData : PreloadLevelData
    {
        [Space]
        [FormerlySerializedAs("useCollectable")]
        [SerializeField] bool hasCollectable = false;
        public bool HasCollectable => hasCollectable;

        [LevelDataPicker(LevelDataType.CollectableObject)]
        [FormerlySerializedAs("resourceName")]
        [SerializeField] string collectableName;
        public string CollectableName => collectableName;
    }
}