using UnityEngine;

namespace Watermelon
{
    public class UIScoreRequirementsPanel : MonoBehaviour, IWinCondition
    {
        [SerializeField] UITargetScoreText targetScoreText;

        [Space]
        [SerializeField] ParticleSystem comboParticleSystem;
        [SerializeField] int comboAmount = 2;

        [Space]
        [SerializeField] GameObject floatingScorePrefab;

        public UITargetScoreText TargetScoreText => targetScoreText;

        private int targetScore;
        private int storedScore;

        public void Init(int targetScore)
        {
            this.targetScore = targetScore;

            targetScoreText.SetTargetScore(targetScore);
        }

        private void OnEnable()
        {
            Score.ValueChanged += OnScoreChanged;
            ComboManager.AmountChanged += OnAmountChanged;
        }

        private void OnDisable()
        {
            Score.ValueChanged -= OnScoreChanged;
            ComboManager.AmountChanged -= OnAmountChanged;
        }

        public void SpawnFloatingScore(Vector3 worldPosition, SimpleCallback targetReachedCallback)
        {
            GameObject floatingScore = Instantiate(floatingScorePrefab, worldPosition, Quaternion.identity);
            floatingScore.transform.SetParent(transform);
            floatingScore.transform.localScale = Vector3.one;

            FloatingScoreBehavior floatingScoreBehavior = floatingScore.GetComponent<FloatingScoreBehavior>();
            floatingScoreBehavior.Init(targetScoreText.TargetScoreRectTransform.position, targetReachedCallback);
        }

        private void OnScoreChanged(int difference)
        {
            storedScore = Score.Value;

            targetScoreText.UpdateScore(storedScore);
        }

        private void OnAmountChanged(int value)
        {
            if (value >= comboAmount)
            {
                comboParticleSystem.Play();
            }
            else
            {
                comboParticleSystem.Stop();
            }
        }

        public bool IsWinConditionMet()
        {
            return storedScore >= targetScore;
        }
    }
}
