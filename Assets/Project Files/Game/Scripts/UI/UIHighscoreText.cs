using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class UIHighscoreText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;

        private int storedHighscore;

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
            storedHighscore = Score.HighScore;
            text.text = storedHighscore.ToString();
        }

        private void OnScoreChanged(int difference)
        {
            int newScore = Score.Value;
            if (newScore > storedHighscore)
            {
                storedHighscore = newScore;
                text.text = newScore.ToString();
            }
        }
    }
}
