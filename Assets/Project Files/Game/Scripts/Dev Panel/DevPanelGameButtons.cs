using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(DevPanel))]
    public class DevPanelGameButtons : MonoBehaviour
    {
        [SerializeField] Button failLevelButton;
        [SerializeField] Button completeLevelButton;
        [SerializeField] Button nextLevelButton;
        [SerializeField] Button prevLevelButton;
        [SerializeField] Button restartLevelButton;
        [SerializeField] Button classicModeButton;
        [SerializeField] Button adventureModeButton;

        private DevPanel devPanel;

        private void Awake()
        {
            devPanel = GetComponent<DevPanel>();

            failLevelButton.onClick.AddListener(() => OnFailLevelButtonClicked());
            completeLevelButton.onClick.AddListener(() => OnCompleteLevelButtonClicked());
            nextLevelButton.onClick.AddListener(() => OnNextLevelButtonClicked());
            prevLevelButton.onClick.AddListener(() => OnPrevLevelButtonClicked());
            restartLevelButton.onClick.AddListener(() => OnRestartLevelButtonClicked());
            classicModeButton.onClick.AddListener(() => OnClassicModeButtonClicked());
            adventureModeButton.onClick.AddListener(() => OnAdventureModeButtonClicked());
        }

        private void OnEnable()
        {
            ActiveSession currentSession = ActiveSession.Current;
            if (currentSession.GameMode == GameMode.Classic)
            {
                nextLevelButton.gameObject.SetActive(false);
                prevLevelButton.gameObject.SetActive(false);

                classicModeButton.gameObject.SetActive(false);
                adventureModeButton.gameObject.SetActive(true);
            }
            else
            {
                nextLevelButton.gameObject.SetActive(true);
                prevLevelButton.gameObject.SetActive(true);

                classicModeButton.gameObject.SetActive(true);
                adventureModeButton.gameObject.SetActive(false);
            }
        }

        private void OnRestartLevelButtonClicked()
        {
            devPanel.DisablePanel();

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }

        private void OnPrevLevelButtonClicked()
        {
            devPanel.DisablePanel();

            ActiveSession currentSession = ActiveSession.Current;
            int levelIndex = Mathf.Clamp(currentSession.LevelIndex - 1, 0, int.MaxValue);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    ActiveSession.SetSession(new ActiveSession(currentSession.GameMode, levelIndex));

                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }

        private void OnNextLevelButtonClicked()
        {
            devPanel.DisablePanel();

            ActiveSession currentSession = ActiveSession.Current;
            int levelIndex = Mathf.Clamp(currentSession.LevelIndex + 1, 0, int.MaxValue);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    ActiveSession.SetSession(new ActiveSession(currentSession.GameMode, levelIndex));

                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }

        private void OnCompleteLevelButtonClicked()
        {
            devPanel.DisablePanel();

            LevelController.CompleteGame(0.1f);
        }

        private void OnFailLevelButtonClicked()
        {
            devPanel.DisablePanel();

            LevelController.OnGameFailed();
        }

        private void OnAdventureModeButtonClicked()
        {
            ActiveSession currentSession = ActiveSession.Current;
            if (currentSession.GameMode == GameMode.Adventure)
                return;

            devPanel.DisablePanel();

            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>("level");
            int levelIndex = Mathf.Clamp(levelSave.MaxReachedLevelIndex, 0, int.MaxValue);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    ActiveSession.SetSession(new ActiveSession(GameMode.Adventure, levelIndex));

                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }

        private void OnClassicModeButtonClicked()
        {
            ActiveSession currentSession = ActiveSession.Current;
            if (currentSession.GameMode == GameMode.Classic)
                return;

            devPanel.DisablePanel();

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    ActiveSession.SetSession(new ActiveSession(GameMode.Classic, 0));

                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }
    }
}
