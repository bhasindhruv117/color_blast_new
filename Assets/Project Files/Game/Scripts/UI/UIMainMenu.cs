using System;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;

        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("References")]
        [SerializeField] RectTransform tapToPlayRect;
        [BoxGroup("References")]
        [SerializeField] Image backgroundImage;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button classicButton;
        [BoxGroup("Buttons")]
        [SerializeField] Button adventureButton;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanel;

        [BoxGroup("Side Buttons", "Side Buttons")]
        [SerializeField] UIMainMenuButton iapStoreButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton noAdsButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton skinsButton;
                
        private UIScaleAnimation coinsLabelScalable;

        private void OnEnable()
        {
            // AdsManager.ForcedAdDisabled += ForceAdPurchased;
        }

        private void OnDisable()
        {
            // AdsManager.ForcedAdDisabled -= ForceAdPurchased;
        }

        public override void Init()
        {
            coinsLabelScalable = new UIScaleAnimation(coinsPanel);
            coinsPanel.Init();

            iapStoreButton.Init(STORE_AD_RIGHT_OFFSET_X);
            noAdsButton.Init(STORE_AD_RIGHT_OFFSET_X);
            skinsButton.Init(STORE_AD_RIGHT_OFFSET_X);

            classicButton.onClick.AddListener(OnClassicButtonClicked);
            adventureButton.onClick.AddListener(OnAdventureButtonClicked);

            iapStoreButton.Button.onClick.AddListener(OnIAPStoreButtonClicked);
            noAdsButton.Button.onClick.AddListener(NoAdButton);
            skinsButton.Button.onClick.AddListener(OnSkinsStoreButtonClicked);
            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            HideAdButton(true);
            iapStoreButton.Hide(true);
            skinsButton.Hide(true);

            coinsLabelScalable.Show();

            ShowAdButton();
            iapStoreButton.Show();
            skinsButton.Show();

            backgroundImage.color = backgroundImage.color.SetAlpha(1.0f);

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backgroundImage.DOFade(0.0f, 0.3f);

            UIController.OnPageClosed(this);
        }

        #endregion

        #region Ad Button Label

        private void ShowAdButton(bool immediately = false)
        {
            if (true)
            {
                noAdsButton.Show(immediately);
            }
            else
            {
                noAdsButton.Hide(immediately: true);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            if(true)
            {
                noAdsButton.Hide(immediately);
            }
        }

        private void ForceAdPurchased()
        {
            noAdsButton.Hide(true);
        }

        #endregion

        #region Buttons

        private void OnAdventureButtonClicked()
        {
            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIAdventure>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnClassicButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            MenuController.OnClassicModeSelected();
        }

        public void OnIAPStoreButtonClicked()
        {
            UIController.ShowPage<UIStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void OnSkinsStoreButtonClicked()
        {
            UIController.ShowPage<UISkinStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void NoAdButton()
        {
            UIController.ShowPage<UINoAdsPopUp>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void AddCoinsButton()
        {
            UIController.ShowPage<UIStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        #endregion
    }
}
