using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class RevivePopup : MonoBehaviour, IPopupWindow
    {
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] Image backgroundImage;
        [SerializeField] Image fillImage;
        [SerializeField] Button reviveButton;

        public bool IsOpened => gameObject.activeSelf;

        private SimpleBoolCallback closeCallback;

        private TweenCase timerTweenCase;

        private StringBuilder stringBuilder = new StringBuilder();

        public void Init()
        {
            reviveButton.onClick.AddListener(OnReviveButtonClicked);
            backgroundImage.AddEvent(EventTriggerType.PointerClick, (data) => OnCloseButtonClicked());
        }

        private void OnDestroy()
        {
            timerTweenCase.KillActive();
        }

        public void Show(int duration, SimpleBoolCallback closeCallback)
        {
            this.closeCallback = closeCallback;

            gameObject.SetActive(true);

            fillImage.fillAmount = 1.0f;
            UpdateTimerText(duration);

            timerTweenCase = Tween.DoFloat(duration, 0, duration, (value) =>
            {
                fillImage.fillAmount = 1.0f - timerTweenCase.State;

                UpdateTimerText(value);
            }).OnComplete(() =>
            {
                OnCloseButtonClicked();
            });

            UIController.OnPopupWindowOpened(this);
        }

        public void Hide()
        {
            timerTweenCase.KillActive();

            gameObject.SetActive(false);

            UIController.OnPopupWindowClosed(this);
        }

        private void UpdateTimerText(float value)
        {
            int seconds = Mathf.CeilToInt(value);

            stringBuilder.Clear();
            stringBuilder.Append(seconds);

            string oldText = timerText.text;

            timerText.text = stringBuilder.ToString();

            if(oldText != timerText.text)
            {
                timerText.DOPushScale(1.2f,1f,0.1f,0.2f, Ease.Type.QuadIn, Ease.Type.QuadOut);
            }
        }

        private void OnCloseButtonClicked()
        {
            Hide();

            closeCallback?.Invoke(false);
        }

        private void OnReviveButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            AdsManager.Instance.OnRewardedAdWatchSuccessFull += OnRewardedAdWatchSuccessFull;
            AdsManager.Instance.OnRewardedAdWatchFailed += OnRewardedAdWatchFailed;
            AdsManager.Instance.ShowRewardedAd();
        }

        private void OnRewardedAdWatchFailed()
        {
            Hide();

            closeCallback?.Invoke(false);
            AdsManager.Instance.OnRewardedAdWatchSuccessFull -= OnRewardedAdWatchSuccessFull;
            AdsManager.Instance.OnRewardedAdWatchFailed -= OnRewardedAdWatchFailed;
        }

        private void OnRewardedAdWatchSuccessFull()
        {
            Hide();

            closeCallback?.Invoke(true);
            AdsManager.Instance.OnRewardedAdWatchSuccessFull -= OnRewardedAdWatchSuccessFull;
            AdsManager.Instance.OnRewardedAdWatchFailed -= OnRewardedAdWatchFailed;
        }
    }
}
