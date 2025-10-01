#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelFigure
    {
        [LevelEditorSetting]
        [SerializeField] Vector2Int size = new Vector2Int(3, 3);
        public Vector2Int Size => size;

        [SerializeField, LevelEditorSetting] PointData[] points;
        public PointData[] Points => points;

        [SerializeField, LevelEditorSetting] int activePoints = -1;
        public int ActivePoints => activePoints;

        [SerializeField] int minimumScore = 0;
        public int MinimumScore => minimumScore;

        [SerializeField] int minimumLevel = 0;
        public int MinimumLevel => minimumLevel;

        [SerializeField] int defaultWeight = 1;
        public int DefaultWeight => defaultWeight;

        public LevelFigure Clone()
        {
            LevelFigure levelFigure = new LevelFigure();
            levelFigure.size = size;
            levelFigure.points = new PointData[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                levelFigure.points[i] = new PointData(points[i].IsActive);
            }
            levelFigure.activePoints = activePoints;
            levelFigure.minimumScore = minimumScore;
            levelFigure.minimumLevel = minimumLevel;
            levelFigure.defaultWeight = defaultWeight;

            return levelFigure;
        }

        public PointData GetRegularPoint()
        {
            if (points.Length == 0)
                return null;

            int startIndex = Random.Range(0, points.Length + 1);
            for (int i = 0; i < points.Length; i++)
            {
                int index = (startIndex + i) % points.Length;
                if (points[index].IsActive && points[index].SpecialBlockBehavior == null)
                    return points[index];
            }

            return null;
        }

        
    }
}