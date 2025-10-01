using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class UILevelQuitPopUp : UIPage, IPopupWindow
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Button closeSmallButton;
        [SerializeField] Button confirmButton;

        private SimpleBoolCallback pageClosed;

        public bool IsOpened => canvas.enabled;

        public override void Init()
        {
            backgroundImage.AddEvent(EventTriggerType.PointerClick, (data) => ExitPopCloseButton());
            closeSmallButton.onClick.AddListener(ExitPopCloseButton);
            confirmButton.onClick.AddListener(ExitPopUpConfirmExitButton);
        }

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
            UIController.OnPopupWindowOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
            UIController.OnPopupWindowClosed(this);
        }

        public void ExitPopCloseButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UILevelQuitPopUp>();

            pageClosed?.Invoke(false);
            pageClosed = null;
        }

        public void ExitPopUpConfirmExitButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UILevelQuitPopUp>();

            pageClosed?.Invoke(true);
            pageClosed = null;
        }

        public static void Show(SimpleBoolCallback onPageClosed = null)
        {
            if(!LivesSystem.IsLocked || LivesSystem.InfiniteMode)
            {
                onPageClosed?.Invoke(true);

                return;
            }

            UIController.ShowPage<UILevelQuitPopUp>();

            UILevelQuitPopUp quitPopUp = UIController.GetPage<UILevelQuitPopUp>();
            quitPopUp.pageClosed += onPageClosed;
        }
    }
}
