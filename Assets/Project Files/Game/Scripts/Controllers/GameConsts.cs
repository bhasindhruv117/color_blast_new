namespace Watermelon
{
    public static class GameConsts
    {
        public static readonly string SCENE_GAME = "Game";
        public static readonly string SCENE_MENU = "Menu";

        public static readonly ActiveSession DEFAULT_SESSION_MODE = new ActiveSession(GameMode.Classic, 0);
    }
}