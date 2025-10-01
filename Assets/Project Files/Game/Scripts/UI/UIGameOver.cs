using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;
        [BoxGroup("References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Content", "Content")]
        [SerializeField] UIScaleAnimation levelFailed;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button replayButton;

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            replayButton.onClick.AddListener(OnReplayButtonClicked);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            backgroundFade.Show(0.3f);

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(immediately: true);

            UIController.OnPageClosed(this);
        }

        #endregion

        #region Buttons 

        public void OnReplayButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (LivesSystem.InfiniteMode || LivesSystem.Lives > 0)
            {
                Overlay.Show(0.3f, () =>
                {
                    LivesSystem.LockLife();

                    GameController.Unload(() =>
                    {
                        SceneManager.LoadScene(GameConsts.SCENE_GAME);
                    });
                });
            }
            else
            {
                UIAddLivesPanel.Show((lifeRecieved) =>
                {
                    if (lifeRecieved)
                    {
                        Overlay.Show(0.3f, () =>
                        {
                            LivesSystem.LockLife();

                            GameController.Unload(() =>
                            {
                                SceneManager.LoadScene(GameConsts.SCENE_GAME);
                            });
                        });
                    }
                    else
                    {
                        Overlay.Show(0.3f, () =>
                        {
                            GameController.LoadMenu();
                        });
                    }
                });
            }
        }
        #endregion
    }
}