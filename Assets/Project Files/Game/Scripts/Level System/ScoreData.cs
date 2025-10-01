#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Score Data", menuName = "Data/Score Data")]
    public class ScoreData : ScriptableObject
    {
        [SerializeField] int scorePerBlock = 1;
        public int ScorePerBlock => scorePerBlock;

        [SerializeField] int scorePerLine = 100;
        public int ScorePerLine => scorePerLine;

        [SerializeField] int scorePerMapClear = 500;
        public int ScorePerMapClear => scorePerMapClear;

        [SerializeField] int scoreRequiredForReward = 100;
        public int ScoreRequiredForReward => scoreRequiredForReward;
    }
}