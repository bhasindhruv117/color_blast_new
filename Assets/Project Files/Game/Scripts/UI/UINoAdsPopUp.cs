using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class UINoAdsPopUp : UIPage, IPopupWindow
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] UIScaleAnimation panelScalable; 
        [SerializeField] Button smallCloseButton;
        [SerializeField] IAPButton removeAdsButton;

        public bool IsOpened => canvas.enabled;

        private UIFadeAnimation backFade;

        private void OnEnable()
        {
            IAPManager.PurchaseCompleted += OnPurchaseCompleted;
        }

        private void OnDisable()
        {
            IAPManager.PurchaseCompleted -= OnPurchaseCompleted;
        }

        private void OnPurchaseModuleInitted()
        {
            if (removeAdsButton != null)
                removeAdsButton.Init(ProductKeyType.NoAds);
        }

        private void OnPurchaseCompleted(ProductKeyType productKeyType)
        {
            if(productKeyType == ProductKeyType.NoAds)
            {
                AdsManager.DisableForcedAd();

                gameObject.SetActive(false);
            }
        }

        public override void Init()
        {
            backFade = new UIFadeAnimation(gameObject);

            backgroundImage.AddEvent(EventTriggerType.PointerClick, OnBackgroundClicked);

            smallCloseButton.onClick.AddListener(OnCloseButtonClicked);

            IAPManager.SubscribeOnPurchaseModuleInitted(OnPurchaseModuleInitted);

            backFade.Hide(immediately: true);
            panelScalable.Hide(immediately: true);
        }

        public override void PlayShowAnimation()
        {
            backFade.Show(0.2f, onCompleted: () =>
            {
                panelScalable.Show(immediately: false, duration: 0.3f);
            });

            UIController.OnPageOpened(this);
            UIController.OnPopupWindowOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backFade.Hide(0.2f);
            panelScalable.Hide(immediately: false, duration: 0.4f, onCompleted: () =>
            {
                UIController.OnPageClosed(this);
                UIController.OnPopupWindowClosed(this);
            });
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UINoAdsPopUp>();
        }

        private void OnBackgroundClicked(PointerEventData data)
        {
            UIController.HidePage<UINoAdsPopUp>();
        }
    }
}