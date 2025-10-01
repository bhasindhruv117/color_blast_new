using TMPro;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UIScoreText : MonoBehaviour
    {
        [SerializeField] JuicyBounce scaleAnimation;

        private TextMeshProUGUI text;

        private int storedScore;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();

            scaleAnimation.Init(transform);
        }

        private void OnEnable()
        {
            Score.ValueChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            Score.ValueChanged -= OnScoreChanged;
        }

        private void Start()
        {
            storedScore = Score.Value;
            text.text = storedScore.ToString();
        }

        private void OnScoreChanged(int difference)
        {
            int newScore = Score.Value;
            if(newScore > storedScore)
            {
                scaleAnimation.Bounce();

                storedScore = newScore;
                text.text = newScore.ToString();
            }
        }
    }
}
