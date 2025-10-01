#pragma warning disable 0649

namespace Watermelon
{
    [StaticUnload]
    public static class Score
    {
        public static bool IsActive { get; private set; }
        public static int Value { get; private set; }

        public static int HighScore => save.HighScore;
        public static bool IsHighscoreReached => save.IsHighScoreReached;

        public static ScoreData Data { get; private set; }

        private static Save save;

        public static SimpleIntCallback ValueChanged;

        public static void Init(ScoreData scoreData)
        {
            Data = scoreData;

            save = SaveController.GetSaveObject<Save>("score");

            Value = 0;

            IsActive = true;
        }

        public static void SetState(bool state)
        {
            IsActive = state;
        }
        
        public static void Add(int difference)
        {
            Value += difference;

            if (Value > save.HighScore)
            {
                save.HighScore = Value;
                save.IsHighScoreReached = true;

                SaveController.MarkAsSaveIsRequired();
            }

            ValueChanged?.Invoke(difference);

            LevelController.CheckWinCondition();
        }

        public static void OnHighscorePopupDisaplyed()
        {
            save.IsHighScoreReached = false;

            SaveController.MarkAsSaveIsRequired();
        }

        private static void UnloadStatic()
        {
            Value = 0;
            ValueChanged = null;

            save = null;
        }

        public class Save : ISaveObject
        {
            public int HighScore;
            public bool IsHighScoreReached;

            public void Flush()
            {

            }
        }
    }
}