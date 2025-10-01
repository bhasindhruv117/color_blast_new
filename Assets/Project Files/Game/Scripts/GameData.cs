using UnityEngine;
using Watermelon.Map;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Data/Game Data")]
    public class GameData : ScriptableObject
    {
        [SerializeField] MapData mapData;
        public MapData MapData => mapData;

        [SerializeField] LevelDatabase levelDatabase;
        public LevelDatabase LevelDatabase => levelDatabase;

        [SerializeField] ScoreData scoreData;
        public ScoreData ScoreData => scoreData;

        public static GameData Data { get; private set; }

        public void Init()
        {
            Data = this;
        }
    }
}
