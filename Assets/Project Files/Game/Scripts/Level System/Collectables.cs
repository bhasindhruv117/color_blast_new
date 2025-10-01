#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public static class Collectables
    {
        public static bool IsActive { get; private set; }

        private static Dictionary<string, CollectedData> dataLink;
        private static CollectedData[] collectedDatas;

        private static float fillPercent;
        private static float spawnChance;

        public static int PickedCollectableIndex { get; private set; }

        public static CollectablesCallback ValueChanged;

        public static void Init()
        {
            IsActive = false;

            fillPercent = 0f;
            spawnChance = 0f;

            PickedCollectableIndex = 0;
        }

        public static int GetCollectablesBlocksAmount(int activePoints)
        {
            if (Random.value <= spawnChance)
            {
                return Mathf.RoundToInt(fillPercent * activePoints);
            }

            return 0;
        }

        public static void SetBlocksPercent(float fillPercent, float spawnChance)
        {
            Collectables.fillPercent = fillPercent;
            Collectables.spawnChance = spawnChance;
        }

        public static void SetTargetCollectables(RequirementCollectableData[] requirementCollectables)
        {
            IsActive = true;

            dataLink = new Dictionary<string, CollectedData>();
            collectedDatas = new CollectedData[requirementCollectables.Length];
            for (int i = 0; i < requirementCollectables.Length; i++)
            {
                CollectedData collectedData = new CollectedData(requirementCollectables[i]);

                collectedDatas[i] = collectedData;

                if (!dataLink.ContainsKey(collectedData.CollectableName))
                    dataLink.Add(collectedData.CollectableName, collectedData);
            }
        }

        public static CollectedData GetRequiredCollectable()
        {
            if (collectedDatas.IsNullOrEmpty()) return null;

            int startIndex = Random.Range(0, collectedDatas.Length);
            for (int i = 0; i < collectedDatas.Length; i++)
            {
                int index = (startIndex + i) % collectedDatas.Length;
                if (collectedDatas[index].Amount > 0)
                {
                    return collectedDatas[index];
                }
            }

            return null;
        }

        public static void Add(string name, int amount)
        {
            if (dataLink.TryGetValue(name, out CollectedData collectedData))
            {
                collectedData.Amount -= amount;

                ValueChanged?.Invoke(collectedData);
            }
        }

        public static bool IsAllCollected()
        {
            foreach (var collectedData in collectedDatas)
            {
                if (collectedData.Amount > 0)
                    return false;
            }

            return true;
        }

        public static void OnCollectableBlockPicked()
        {
            PickedCollectableIndex++;
        }

        public static void OnCollectingFinished()
        {
            PickedCollectableIndex = 0;
        }

        private static void UnloadStatic()
        {
            dataLink = null;
            collectedDatas = null;

            ValueChanged = null;
        }

        public delegate void CollectablesCallback(CollectedData collectedData);
    }
}