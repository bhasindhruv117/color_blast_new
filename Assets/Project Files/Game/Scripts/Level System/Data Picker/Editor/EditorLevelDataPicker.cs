using UnityEditor;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public static class EditorLevelDataPicker
    {
        public static LevelDatabase LevelDatabase { get; private set; }

        public static bool IsDatabaseExists => LevelDatabase != null;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            GameData gameData = EditorUtils.GetAsset<GameData>();
            if(gameData != null)
            {
                LevelDatabase = gameData.LevelDatabase;
            }
            else
            {
                EditorApplication.delayCall += Init;
            }
        }

        public static string[] GetItems(LevelDataType levelDataType)
        {
            if (LevelDatabase == null)
            {
                Debug.LogError("LevelDatabase is null");
                return null;
            }

            if(levelDataType == LevelDataType.CollectableObject)
            {
                return LevelDatabase.Collectables.Select(x => x.ID).ToArray();    
            }

            Debug.LogError("Unknown LevelDataType");
            return null;
        }
    }
}
