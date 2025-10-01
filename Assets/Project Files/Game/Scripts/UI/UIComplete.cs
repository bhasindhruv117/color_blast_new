using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;
        [BoxGroup("References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanelUI;

        [Space]
        [BoxGroup("Content", "Content")]
        [SerializeField] TextMeshProUGUI rewardAmountText;
        [BoxGroup("Content")]
        [SerializeField] Image rewardCurrencyIconImage;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button nextLevelButton;

        private UIScaleAnimation coinsPanelScalable;

        private int currentReward;

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);

            coinsPanelScalable = new UIScaleAnimation(coinsPanelUI);
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            CurrencyAmount completeReward = LevelController.LevelRepresentation.LevelData.CompleteReward;

            coinsPanelUI.Init(completeReward.CurrencyType);
            rewardCurrencyIconImage.sprite = completeReward.Currency.Icon;
            rewardAmountText.text = completeReward.FormattedPrice;

            coinsPanelScalable.Hide(immediately: true);

            backgroundFade.Show(duration: 0.3f);

            coinsPanelScalable.Show();

            currentReward = completeReward.Amount; // update reward here

            Tween.DoFloat(0, currentReward, 0.6f, (value) =>
            {
                rewardAmountText.text = value.ToString("F0");
            }).OnComplete(() =>
            {
                FloatingCloud.SpawnCurrency(completeReward.CurrencyType.ToString(), rewardAmountText.rectTransform, coinsPanelUI.Image.rectTransform, 10, "", () =>
                {
                    CurrencyController.Add(completeReward.CurrencyType, currentReward);
                });
            });

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.25f);
            coinsPanelScalable.Hide();

            UIController.OnPageClosed(this);
        }
        #endregion

        #region Buttons
        private void OnNextLevelButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    LivesSystem.LockLife();

                    ActiveSession currentSession = ActiveSession.Current;

                    ActiveSession.SetSession(new ActiveSession(currentSession.GameMode, currentSession.LevelIndex + 1));

                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }
        #endregion
    }
}
