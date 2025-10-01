using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("References")]
        [SerializeField] RevivePopup revivePopup;
        [BoxGroup("References")]
        [SerializeField] UITargetRequirementsPopup targetRequirementsPopup;
        [BoxGroup("References")]
        [SerializeField] CanvasGroup noSpaceCanvasGroup;

        [BoxGroup("Modes", "Modes")]
        [SerializeField] UIClassicModePanel classicPanel;

        [Space]
        [BoxGroup("Modes")]
        [SerializeField] GameObject targetScorePanelObject;
        [BoxGroup("Modes")]
        [SerializeField] UIScoreRequirementsPanel targetScorePanel;

        [Space]
        [BoxGroup("Modes")]
        [FormerlySerializedAs("targetResourcesPanelObject")]
        [SerializeField] GameObject targetCollectablesPanelObject;
        [BoxGroup("Modes")]
        [FormerlySerializedAs("targetResourcesPanel")]
        [SerializeField] UICollectablesRequirementsPanel targetCollectablesPanel;

        public UICollectablesRequirementsPanel TargetCollectablesPanel => targetCollectablesPanel;
        public UIScoreRequirementsPanel TargetScorePanel => targetScorePanel;
        public RevivePopup RevivePopup => revivePopup;
        public UITargetRequirementsPopup TargetRequirementsPopup => targetRequirementsPopup;

        private TweenCase noSpaceTweenCase;

        public override void Init()
        {
            targetRequirementsPopup.Init();
            revivePopup.Init();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            noSpaceCanvasGroup.alpha = 0;
            noSpaceCanvasGroup.gameObject.SetActive(false);

            UIController.OnPageOpened(this);
        }

        public void ActivateClassicModeUI()
        {
            targetScorePanelObject.SetActive(false);
            targetCollectablesPanelObject.SetActive(false);

            classicPanel.gameObject.SetActive(true);
        }

        public void ActivateAdventureScoreModeUI(int targetScore)
        {
            classicPanel.gameObject.SetActive(false);
            targetCollectablesPanelObject.SetActive(false);

            targetScorePanelObject.SetActive(true);

            targetScorePanel.Init(targetScore);

            LevelController.SetWinCondition(targetScorePanel);
        }

        public void ActivateAdventureCollectableModeUI(RequirementCollectableData[] requirementCollectables)
        {
            classicPanel.gameObject.SetActive(false);
            targetScorePanelObject.SetActive(false);

            targetCollectablesPanelObject.SetActive(true);

            targetCollectablesPanel.Init(requirementCollectables);

            LevelController.SetWinCondition(targetCollectablesPanel);
        }

        public void ShowNoSpaceLabel()
        {
            noSpaceTweenCase.KillActive();

            noSpaceCanvasGroup.gameObject.SetActive(true);
            noSpaceTweenCase = noSpaceCanvasGroup.DOFade(1.0f, 0.2f);
        }

        public void HideNoSpaceLabel()
        {
            noSpaceTweenCase.KillActive();
            noSpaceCanvasGroup.gameObject.SetActive(false);
            noSpaceCanvasGroup.alpha = 0;
        }
    }
}
