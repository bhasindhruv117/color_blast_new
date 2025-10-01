using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITargetScoreText : MonoBehaviour
    {
        [SerializeField] RectTransform scrollBackgroundRectTransform;

        [Space]
        [SerializeField] TextMeshProUGUI currentScoreText;
        [SerializeField] RectTransform currentScoreRectTransform;
        [SerializeField] Image fillImage;

        [Space]
        [SerializeField] TextMeshProUGUI targetScoreText;
        [SerializeField] RectTransform targetScoreRectTransform;
        [SerializeField] SimpleBounce targetScoreBounceAnimation;

        public RectTransform TargetScoreRectTransform => targetScoreRectTransform;

        private int targetScore;
        private int storedScore;

        private bool targetScoreReached;

        private void Awake()
        {
            targetScoreBounceAnimation.Init(targetScoreRectTransform);
        }

        public void SetTargetScore(int targetScore)
        {
            this.targetScore = targetScore;

            targetScoreReached = false;

            targetScoreText.text = targetScore.ToString();

            UpdateScore(Score.Value);
        }

        public void UpdateScore(int value)
        {
            storedScore = value;
            currentScoreText.text = storedScore.ToString();

            // Calculate fill amount
            float fillAmount = Mathf.Clamp01((float)storedScore / targetScore);
            fillImage.fillAmount = fillAmount;

            // Reposition currentScoreRectTransform
            float newXPosition = scrollBackgroundRectTransform.rect.width * fillAmount;
            currentScoreRectTransform.anchoredPosition = new Vector2(newXPosition, currentScoreRectTransform.anchoredPosition.y);

            if (!targetScoreReached && fillAmount >= 1.0f)
            {
                targetScoreReached = true;

                OnTargetScoreReached();
            }
        }

        private void OnTargetScoreReached()
        {
            AudioController.PlaySound(AudioController.AudioClips.requirementMet);
        }

        public void DisableTargetScore()
        {
            targetScoreRectTransform.gameObject.SetActive(false);
        }

        public void EnableTargetScore()
        {
            targetScoreRectTransform.gameObject.SetActive(true);
        }

        public void PlayTargetScoreBounceAnimation()
        {
            targetScoreBounceAnimation.Bounce();
        }
    }
}
