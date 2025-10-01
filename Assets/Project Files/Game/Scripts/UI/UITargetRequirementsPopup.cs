using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITargetRequirementsPopup : MonoBehaviour, IPopupWindow
    {
        private const string COLLECTABLES_TITLE = "Target Collection";
        private const string SCORE_TITLE = "Target Score";

        [SerializeField] Image backgroundImage;
        [SerializeField] TextMeshProUGUI titleText;

        [Space]
        [FormerlySerializedAs("resourcesPrefab")]
        [SerializeField] UICollectableRequirementElement collectablePrefab;
        [FormerlySerializedAs("resourcesContainerTransform")]
        [SerializeField] Transform collectablesContainerTransform;

        [Space]
        [SerializeField] Transform scoreContainerTransform;
        [SerializeField] TextMeshProUGUI scoreText;

        public bool IsOpened => gameObject.activeSelf;

        private CanvasGroup canvasGroup;
        private UIGame gameUI;

        private UICollectableRequirementElement[] collectablesRequirementElements;

        public void Init()
        {
            canvasGroup = gameObject.GetOrSetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            gameObject.SetActive(false);

            gameUI = UIController.GetPage<UIGame>();
        }

        public void Show(AdventureModeLevelData levelData)
        {
            gameObject.SetActive(true);

            canvasGroup.DOFade(1.0f, 0.3f);

            UIController.OnPopupWindowOpened(this);

            collectablesContainerTransform.gameObject.SetActive(false);
            scoreContainerTransform.gameObject.SetActive(false);

            if(levelData.RequirementType == LevelRequirementType.Collectables)
            {
                collectablesContainerTransform.gameObject.SetActive(true);

                titleText.text = COLLECTABLES_TITLE;

                RequirementCollectableData[] requirementCollectables = levelData.RequirementCollectables;
                collectablesRequirementElements = new UICollectableRequirementElement[requirementCollectables.Length];
                for (int i = 0; i < requirementCollectables.Length; i++)
                {
                    UICollectableRequirementElement requirementElement = Instantiate(collectablePrefab, collectablesContainerTransform);

                    requirementElement.Init(requirementCollectables[i]);

                    collectablesRequirementElements[i] = requirementElement;
                }
            }
            else if(levelData.RequirementType == LevelRequirementType.Score)
            {
                scoreContainerTransform.gameObject.SetActive(true);

                titleText.text = SCORE_TITLE;

                scoreText.text = levelData.RequirementScore.ToString();

                gameUI.TargetScorePanel.TargetScoreText.DisableTargetScore();
            }

            Tween.DelayedCall(1.0f, () =>
            {
                if (levelData.RequirementType == LevelRequirementType.Collectables)
                {
                    for (int i = 0; i < collectablesRequirementElements.Length; i++)
                    {
                        UICollectableRequirementElement requirementElement = collectablesRequirementElements[i];

                        gameUI.TargetCollectablesPanel.SpawnFloatingCollectable(requirementElement.CollectableData, requirementElement.transform.position, (element) =>
                        {
                            element.ShowAmount();
                        });
                    }
                }
                else if(levelData.RequirementType == LevelRequirementType.Score)
                {
                    gameUI.TargetScorePanel.SpawnFloatingScore(scoreContainerTransform.transform.position, () =>
                    {
                        gameUI.TargetScorePanel.TargetScoreText.EnableTargetScore();
                        gameUI.TargetScorePanel.TargetScoreText.PlayTargetScoreBounceAnimation();
                    });
                }

                gameObject.SetActive(false);

                UIController.OnPopupWindowClosed(this);
            });
        }
    }
}
