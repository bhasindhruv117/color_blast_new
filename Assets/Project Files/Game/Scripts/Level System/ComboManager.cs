#pragma warning disable 0649

using System.Linq;
using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public class ComboManager : MonoBehaviour
    {
        private const string COMBO_TAG_TEXT = "{combo}";

        private static ComboManager comboManager;

        [SerializeField] float comboDelay = 0.2f;
        [SerializeField] int stepsResetCounter = 3;
        [SerializeField] bool shakeCamera;
        [SerializeField] bool hapticFeedback;

        [Space]
        [SerializeField] FloatingTextData[] floatingTexts;

        [Space]
        [SerializeField] AudioClip defaultComboSound;
        [SerializeField] string defaultComboText;

        [LineSpacer("Score")]
        [SerializeField] GameObject scoreTextPrefab;

        private static FloatingTextData[] sortedFloatingTexts;

        private static Vector3 lastComboPosition;
        private static int comboCounter = 0;
        private static int linesCollected = 0;
        private static int resetCounter;
        private static TweenCase comboDelayTweenCase;

        public static SimpleIntCallback AmountChanged;

        public void Init()
        {
            comboManager = this;

            sortedFloatingTexts = floatingTexts.OrderBy(x => x.ComboLevel).ToArray();

            linesCollected = 0;
            comboCounter = 0;
            resetCounter = stepsResetCounter + 1;
        }

        public void SpawnScoreText(int score)
        {
            if (score <= 0) return;
            if (!Score.IsActive) return;

            if (scoreTextPrefab != null)
            {
                GameObject scoreTextObject = Instantiate(scoreTextPrefab, lastComboPosition, Quaternion.identity);

                ScoreFloatingTextBehavior scoreTextBehavior = scoreTextObject.GetComponent<ScoreFloatingTextBehavior>();
                scoreTextBehavior.Activate(score.ToString(), Color.white);
            }
        }

        public int ApplyScore(int combo, int linesCollected)
        {
            if (!Score.IsActive) return 0;

            int score = (combo * Score.Data.ScorePerLine) * linesCollected;
            if(score > 0)
            {
                Score.Add(score);
            }

            return score;
        }

        public static void IncreaseCombo(Vector3 position)
        {
            comboCounter++;
            linesCollected++;

            lastComboPosition = position;
            lastComboPosition.z -= 1;

            comboDelayTweenCase.KillActive();
            comboDelayTweenCase = Tween.DelayedCall(comboManager.comboDelay, ApplyCombo);

            resetCounter = comboManager.stepsResetCounter + 1;
        }

        private static void ApplyCombo()
        {
            if (comboCounter > 0 && !sortedFloatingTexts.IsNullOrEmpty())
            {
                int score = comboManager.ApplyScore(comboCounter, linesCollected);

                FloatingTextData selectedTextData = null;
                for (int i = 0; i < sortedFloatingTexts.Length; i++)
                {
                    if (comboCounter >= sortedFloatingTexts[i].ComboLevel)
                    {
                        selectedTextData = sortedFloatingTexts[i];
                    }
                    else
                    {
                        break;
                    }
                }

                if (selectedTextData != null)
                {
                    GameObject floatingTextObject = Instantiate(selectedTextData.FloatingTextPrefab, lastComboPosition, Quaternion.identity);

                    ComboFloatingTextBehavior floatingTextBehavior = floatingTextObject.GetComponent<ComboFloatingTextBehavior>();

                    string text;
                    if (!string.IsNullOrEmpty(selectedTextData.Text))
                    {
                        text = FormatComboText(selectedTextData.Text, comboCounter);
                    }
                    else
                    {
                        text = FormatComboText(comboManager.defaultComboText, comboCounter);
                    }

                    floatingTextBehavior.Activate(text.Replace("\n", "\n"), selectedTextData.Color);

                    if (selectedTextData.AudioClip != null)
                    {
                        AudioController.PlaySound(selectedTextData.AudioClip);
                    }
                    else
                    {
                        if (comboManager.defaultComboSound != null)
                            AudioController.PlaySound(comboManager.defaultComboSound);
                    }

                    floatingTextBehavior.Completed += () =>
                    {
                        comboManager.SpawnScoreText(score);
                    };
                }
                else
                {
                    comboManager.SpawnScoreText(score);

                    if (comboManager.defaultComboSound != null)
                        AudioController.PlaySound(comboManager.defaultComboSound);
                }
            }
            else
            {
                AudioController.PlaySound(AudioController.AudioClips.lineCollected);
            }

            if (comboManager.hapticFeedback)
                Haptic.Play(Haptic.HAPTIC_MEDIUM);

            if (comboManager.shakeCamera)
                CameraController.ShakeCombo();

            linesCollected = 0;

            AmountChanged?.Invoke(comboCounter);
        }

        public static void OnElementPlaced()
        {
            resetCounter--;

            if(resetCounter <= 0)
            {
                ResetCombo();
            }
        }

        private static void ResetCombo()
        {
            comboCounter = 0;

            AmountChanged?.Invoke(comboCounter);
        }

        private static string FormatComboText(string text, int comboCounter)
        {
            return text.Replace(COMBO_TAG_TEXT, comboCounter.ToString());
        }

        private static void UnloadStatic()
        {
            comboCounter = 0;
            linesCollected = 0;
            resetCounter = 0;

            comboDelayTweenCase = null;

            AmountChanged = null;
        }

        [System.Serializable]
        private class FloatingTextData
        {
            [SerializeField] int comboLevel;
            public int ComboLevel => comboLevel;

            [SerializeField] GameObject floatingTextPrefab;
            public GameObject FloatingTextPrefab => floatingTextPrefab;

            [SerializeField] Color color;
            public Color Color => color;

            [SerializeField] AudioClip audioClip;
            public AudioClip AudioClip => audioClip;

            [TextArea]
            [SerializeField] string text;
            public string Text => text;
        }
    }
}