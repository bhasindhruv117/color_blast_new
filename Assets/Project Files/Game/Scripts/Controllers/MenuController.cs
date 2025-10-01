using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon.Map;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] UIController uiController;

        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static MapController mapController;
        private static SkinController skinController;
        private static SkinStoreController skinStoreController;

        private void Awake()
        {
            GameData gameData = GameData.Data;
            if (gameData == null)
                Debug.LogError("GameData is null. Please add the Game Settings component to the Project Init Settings and link Game Data scriptable object.");

            // Cache components
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out mapController);
            gameObject.CacheComponent(out skinController);
            gameObject.CacheComponent(out skinStoreController);

            // Initialize UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            // Initialize other controllers
            particlesController.Init();
            floatingTextController.Init();

            skinController.Init();
            skinStoreController.Init(skinController);

            mapController.Init(gameData.MapData);

            // Initialize currency cloud and pages
            uiController.InitPages();
        }

        private void Start()
        {
            // Display default page
            UIController.ShowPage<UIMainMenu>();
        }

        public static void OnAdventureLevelClicked(int levelID)
        {
            ActiveSession session = new ActiveSession(GameMode.Adventure, levelID);

            if (LivesSystem.Lives > 0 || LivesSystem.InfiniteMode)
            {
                LivesSystem.LockLife();

                LoadGame(session);
            }
            else
            {
                UIAddLivesPanel.Show((lifeRecieved) =>
                {
                    if(lifeRecieved)
                    {
                        LivesSystem.LockLife();

                        LoadGame(session);
                    }
                });
            }
        }

        public static void OnAdventurePageOpened()
        {
            mapController.Show();
        }

        public static void OnAdventurePageClosed()
        {
            mapController.Hide();
        }

        public static void OnClassicModeSelected()
        {
            LoadGame(new ActiveSession(GameMode.Classic, 0));
        }

        public static void LoadGame(ActiveSession session)
        {
            ActiveSession.SetSession(session);

            Overlay.Show(0.3f, () =>
            {
                Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            }, true);
        }

        public static void Unload(SimpleCallback onUnloaded)
        {
            // Do menu unload

            onUnloaded?.Invoke();
        }
    }
}