using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIScoreGameOver : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;
        [BoxGroup("References")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("References")]
        [SerializeField] TextMeshProUGUI levelFailedText;

        [BoxGroup("Content", "Content")]
        [SerializeField] TextMeshProUGUI scoreText;
        [BoxGroup("Content")]
        [SerializeField] TextMeshProUGUI highscoreText;

        [BoxGroup("Reward", "Reward")]
        [SerializeField] GameObject rewardObject;
        [BoxGroup("Reward")]
        [SerializeField] TextMeshProUGUI rewardAmountText;
        [BoxGroup("Reward")]
        [SerializeField] Image rewardCurrencyIconImage;
        [BoxGroup("Reward")]
        [SerializeField] CurrencyUIPanelSimple currencyPanelUI;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button playButton;

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        public override void PlayShowAnimation()
        {
            levelFailedText.text = GetLevelFailedText();

            scoreText.text = Score.Value.ToString();
            highscoreText.text = Score.HighScore.ToString();

            backgroundFade.Show(0.3f);

            CurrencyAmount completeReward = LevelController.LevelRepresentation.LevelData.CompleteReward;
            currencyPanelUI.Init(completeReward.CurrencyType);

            int requiredScore = Score.Data.ScoreRequiredForReward;
            int rewardAmount = Score.Value / requiredScore;

            if (rewardAmount > 0)
            {
                int reward = completeReward.Amount * rewardAmount;

                rewardObject.SetActive(true);
                rewardCurrencyIconImage.sprite = completeReward.Currency.Icon;
                rewardAmountText.text = CurrencyHelper.Format(reward);

                Tween.DoFloat(0, reward, 0.6f, (value) =>
                {
                    rewardAmountText.text = value.ToString("F0");
                }).OnComplete(() =>
                {
                    FloatingCloud.SpawnCurrency(completeReward.CurrencyType.ToString(), rewardAmountText.rectTransform, currencyPanelUI.Image.rectTransform, 10, "", () =>
                    {
                        CurrencyController.Add(completeReward.CurrencyType, reward);
                    });
                });
            }
            else
            {
                rewardObject.SetActive(false);
            }

            UIController.OnPageOpened(this);

            Score.OnHighscorePopupDisaplyed();
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(immediately: true);

            UIController.OnPageClosed(this);
        }

        private string GetLevelFailedText()
        {
            float scorePersentage = Score.Value / Score.HighScore;

            if (scorePersentage < 0.2f)
            {
                return "OOPS!";
            }
            else if (scorePersentage < 0.7f)
            {
                return "NICE TRY!";
            }
            else
            {
                return "IMPRESIVE!";
            }
        }


        #region Buttons 

        public void OnPlayButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });
        }
        #endregion
    }
}