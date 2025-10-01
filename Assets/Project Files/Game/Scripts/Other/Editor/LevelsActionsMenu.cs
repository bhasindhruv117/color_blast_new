using UnityEngine;
using Watermelon;
using UnityEditor;
using UnityEditor.Overlays;

namespace Watermelon
{
    public static class LevelsActionsMenu
    {
        [MenuItem("Actions/Levels/Unlock All", priority = 22)]
        private static void UnlockAll()
        {
            var gameData = EditorUtils.GetAsset<GameData>();
            int maxLevel = gameData.LevelDatabase.Levels.Length;

            if (Application.isPlaying)
            {
                LevelSave save = SaveController.GetSaveObject<LevelSave>("level");
                save.MaxReachedLevelIndex = maxLevel;
            }
            else
            {
                GlobalSave globalSave = SaveController.GetGlobalSave();
                LevelSave levelSave = globalSave.GetSaveObject<LevelSave>("level");
                levelSave.MaxReachedLevelIndex = maxLevel;

                SaveController.SaveCustom(globalSave);
            }
        }

        [MenuItem("Actions/Levels/Reset Progress", priority = 23)]
        private static void ResetProgress()
        {
            if (Application.isPlaying)
            {
                LevelSave save = SaveController.GetSaveObject<LevelSave>("level");
                save.MaxReachedLevelIndex = 0;
            }
            else
            {
                GlobalSave globalSave = SaveController.GetGlobalSave();
                LevelSave levelSave = globalSave.GetSaveObject<LevelSave>("level");
                levelSave.MaxReachedLevelIndex = 0;

                SaveController.SaveCustom(globalSave);
            }
        }
    }
}