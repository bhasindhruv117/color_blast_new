#pragma warning disable 0649

namespace Watermelon
{
    public class ActiveSession
    {
        public readonly GameMode GameMode;

        public readonly int LevelIndex;

        private static ActiveSession currentSession = GameConsts.DEFAULT_SESSION_MODE;
        public static ActiveSession Current
        {
            get
            {
                if(currentSession == null)
                    currentSession = GameConsts.DEFAULT_SESSION_MODE;

                return currentSession;
            }
        }

        public ActiveSession()
        {
            GameMode = GameMode.Classic;

            LevelIndex = 0;
        }

        public ActiveSession(GameMode gameMode, int levelIndex)
        {
            GameMode = gameMode;

            LevelIndex = levelIndex;
        }

        public static void SetSession(ActiveSession session)
        {
            currentSession = session;
        }
    }
}