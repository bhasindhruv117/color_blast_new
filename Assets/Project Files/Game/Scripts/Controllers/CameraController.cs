using UnityEngine;

namespace Watermelon
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController cameraController;

        [BoxGroup("Combo", "Combo")]
        [SerializeField] float comboShakeMagnitude;
        [BoxGroup("Combo")]
        [SerializeField] float comboShakeDuration;

        private static TweenCase shakeTweenCase;
        private static TweenCase uiShakeTweenCase;

        private void Awake()
        {
            cameraController = this;
        }

        private void OnDestroy()
        {
            shakeTweenCase.KillActive();
            uiShakeTweenCase.KillActive();
        }

        public static void ShakeCombo()
        {
            if (cameraController == null) return;

            shakeTweenCase.CompleteActive();
            uiShakeTweenCase.CompleteActive();

            shakeTweenCase = cameraController.transform.DOShake(cameraController.comboShakeDuration, cameraController.comboShakeMagnitude);

            UIGame gameUI = UIController.GetPage<UIGame>();
            if(gameUI != null)
            {
                RectTransform rectTransform = (RectTransform)gameUI.transform;
                uiShakeTweenCase = rectTransform.DOAnchoredPositionShake(cameraController.comboShakeDuration, cameraController.comboShakeMagnitude);
            }
        }

        public static void Shake(float magnitude, float duration)
        {
            if (cameraController == null) return;

            shakeTweenCase.CompleteActive();
            uiShakeTweenCase.CompleteActive();

            shakeTweenCase = cameraController.transform.DOShake(magnitude, duration);

            UIGame gameUI = UIController.GetPage<UIGame>();
            if(gameUI != null)
            {
                RectTransform rectTransform = (RectTransform)gameUI.transform;
                uiShakeTweenCase = rectTransform.DOAnchoredPositionShake(magnitude, duration);
            }
        }
    }
}