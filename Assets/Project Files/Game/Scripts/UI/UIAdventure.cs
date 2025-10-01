using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;
using Watermelon.Map;

namespace Watermelon
{
    public class UIAdventure : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("References")]
        [SerializeField] Button backButton;
        [BoxGroup("References")]
        [SerializeField] Button playButton;
        [BoxGroup("References")]
        [SerializeField] TMP_Text playButtonText;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanel;
        [BoxGroup("Top Panel")]
        [SerializeField] LivesIndicator livesIndicator;

        private UIScaleAnimation backButtonScaleAnimation;
        private UIScaleAnimation playButtonScaleAnimation;
        private UIScaleAnimation coinsScaleAnimation;
        private UIScaleAnimation livesScaleAnimation;

        public override void Init()
        {
            backButtonScaleAnimation = new UIScaleAnimation(backButton);
            playButtonScaleAnimation = new UIScaleAnimation(playButton);
            coinsScaleAnimation = new UIScaleAnimation(coinsPanel);
            livesScaleAnimation = new UIScaleAnimation(livesIndicator);

            coinsPanel.Init();

            backButton.onClick.AddListener(OnBackButtonClicked);
            playButton.onClick.AddListener(OnPlayButtonClicked);

            coinsPanel.AddButton.onClick.AddListener(OnAddCoinsButtonClicked);

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            backButtonScaleAnimation.Show();
            playButtonScaleAnimation.Show();
            coinsScaleAnimation.Show();
            livesScaleAnimation.Show();

            playButtonText.text = $"LEVEL {MapController.MaxLevelReached + 1}";

            MenuController.OnAdventurePageOpened();

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            MenuController.OnAdventurePageClosed();

            UIController.OnPageClosed(this);
        }

        #endregion

        #region Buttons
        public void OnPlayButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            MenuController.OnAdventureLevelClicked(MapController.MaxLevelReached);
        }

        private void OnAddCoinsButtonClicked()
        {
            UIController.ShowPage<UIStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnBackButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIAdventure>();

            UIController.ShowPage<UIMainMenu>();
        }

        #endregion
    }
}
