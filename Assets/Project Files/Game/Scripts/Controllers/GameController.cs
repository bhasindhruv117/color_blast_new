using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] UIController uiController;

        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static LevelController levelController;
        private static SkinController skinController;
        private static ComboManager comboManager;
        private static RaycastController raycastController;
        private static LevelSizeController levelSizeController;

        private void Awake()
        {
            GameData gameData = GameData.Data;
            if (gameData == null)
                Debug.LogError("GameData is null. Please add the Game Settings component to the Project Init Settings and link Game Data scriptable object.");

            // Cache components
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out levelController);
            gameObject.CacheComponent(out skinController);
            gameObject.CacheComponent(out comboManager);
            gameObject.CacheComponent(out raycastController);
            gameObject.CacheComponent(out levelSizeController);

            // Get recommended screen size data
            ScreenSize recommendedScreenSize = levelSizeController.GetRecommendedScreenSize();

            // Initialize static components
            Score.Init(gameData.ScoreData);
            Collectables.Init();

            // Initialize UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            // Initialize other controllers
            raycastController.Init();

            particlesController.Init();
            floatingTextController.Init();
            comboManager.Init();
            skinController.Init();

            levelController.ApplyScreenSize(recommendedScreenSize);
            levelController.Init(gameData.LevelDatabase);

            // Initialize currency cloud and pages
            uiController.InitPages();
        }

        private void Start()
        {
            ActiveSession currentSession = ActiveSession.Current;
            if(currentSession.GameMode == GameMode.Adventure)
            {
                levelController.LoadLevelWithID(currentSession.LevelIndex);
            }
            else if(currentSession.GameMode == GameMode.Classic)
            {
                levelController.LoadClassicMode();
            }
            else
            {
                Debug.LogError("Unknown game mode.");
            }

            // Display default page
            UIController.ShowPage<UIGame>();
        }

        public static void LoadMenu()
        {
            Unload(() =>
            {
                SceneManager.LoadScene(GameConsts.SCENE_MENU);
            });
        }

        public static void Unload(SimpleCallback onUnloaded)
        {
            // Do game unload
            levelController.UnloadLevel();

            onUnloaded?.Invoke();
        }
    }
}