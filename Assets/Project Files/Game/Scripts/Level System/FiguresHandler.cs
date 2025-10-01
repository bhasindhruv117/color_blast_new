#pragma warning disable 0649

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class FiguresHandler
    {
        private LevelFigure[] figures;
        private LevelRepresentation levelRepresentation;

        private DifficultyPreset difficultyPreset;
        private DockSpawnSettings[] difficultySpawnSettings;

        private List<LevelFigure> availableFigures;
        private int nextFigureScore;

        public FiguresHandler(LevelFigure[] figures)
        {
            this.figures = figures;

            nextFigureScore = -1;
            difficultySpawnSettings = null;
            availableFigures = new List<LevelFigure>();
        }

        public bool PerfectPlacementChance()
        {
            if (difficultySpawnSettings == null)
                return false;

            int score = Score.Value;
            foreach (var setting in difficultySpawnSettings)
            {
                if (score >= setting.TargetScore)
                {
                    return setting.PerfectPlacementChance >= Random.Range(0.0f, 1.0f);
                }
            }

            return false;
        }

        private void RecalculateAvailableFigures()
        {
            int score = Score.Value;
            if (score >= nextFigureScore)
            {
                availableFigures.Clear();

                bool scoreFound = false;
                for (int i = 0; i < figures.Length; i++)
                {
                    if (score >= figures[i].MinimumScore)
                    {
                        availableFigures.Add(figures[i]);
                    }
                    else
                    {
                        nextFigureScore = figures[i].MinimumScore;

                        scoreFound = true;

                        break;
                    }
                }

                if (!scoreFound)
                    nextFigureScore = int.MaxValue;
            }

            availableFigures.Shuffle();
        }

        public void OnLevelLoaded(int levelIndex, LevelRepresentation levelRepresentation)
        {
            this.levelRepresentation = levelRepresentation;
            this.difficultyPreset = levelRepresentation.LevelData.DifficultyPreset;

            if (difficultyPreset != null)
                difficultySpawnSettings = difficultyPreset.DockSpawnSettings.OrderBy(x => x.TargetScore).ToArray();

            figures = figures.Where(x => levelIndex >= x.MinimumLevel && x.Size.x <= levelRepresentation.LevelData.Size.x && x.Size.y <= levelRepresentation.LevelData.Size.y).OrderBy(x => x.MinimumScore).ToArray();

            RecalculateAvailableFigures();
        }

        public LevelFigure[] GetPerfectFigures(int count)
        {
            RecalculateAvailableFigures();

            bool[,] baseMatrix = levelRepresentation.GetSimplifiedLevelMatrix();

            LevelFigure[] levelFigures = new LevelFigure[count];
            for (int i = 0; i < count; i++)
            {
                PerfectFigure perfectFigure = GetPerfectFigures(ref baseMatrix, levelFigures);
                if (perfectFigure != null)
                {
                    levelFigures[i] = perfectFigure.Figure;
                    PlaceFigure(ref baseMatrix, perfectFigure.Figure, perfectFigure.Position, true);
                }
                else
                {
                    levelFigures[i] = availableFigures.GetRandomItem();
                }
            }

            levelFigures.Shuffle();

            for (int i = 0; i < levelFigures.Length; i++)
            {
                levelFigures[i] = levelFigures[i].Clone();
            }

            return levelFigures;
        }

        public LevelFigure[] GetRandomFigures(int count)
        {
            RecalculateAvailableFigures();

            bool[,] baseMatrix = levelRepresentation.GetSimplifiedLevelMatrix();

            LevelFigure[] levelFigures = new LevelFigure[count];
            for (int i = 0; i < count; i++)
            {
                if (PerfectPlacementChance())
                {
                    PerfectFigure perfectFigure = GetPerfectFigures(ref baseMatrix, levelFigures);
                    if (perfectFigure != null)
                    {
                        levelFigures[i] = perfectFigure.Figure;
                        PlaceFigure(ref baseMatrix, perfectFigure.Figure, perfectFigure.Position, true);
                    }
                    else
                    {
                        levelFigures[i] = availableFigures.GetRandomItem();
                    }
                }
                else
                {
                    levelFigures[i] = availableFigures.GetRandomItem();
                }
            }

            levelFigures.Shuffle();
            
            for(int i = 0; i < levelFigures.Length; i++)
            {
                levelFigures[i] = levelFigures[i].Clone();
            }

            return levelFigures;
        }

        public void PlaceFigure(ref bool[,] matrix, LevelFigure levelFigure, Vector2Int position, bool state)
        {
            for (int y = 0; y < levelFigure.Size.y; y++)
            {
                for (int x = 0; x < levelFigure.Size.x; x++)
                {
                    int realX = position.x + x;
                    int realY = position.y + y;

                    if (realX >= 0 && realX < matrix.GetLength(0) && realY >= 0 && realY < matrix.GetLength(1))
                    {
                        int index = y * levelFigure.Size.x + x;
                        if (levelFigure.Points[index].IsActive)
                        {
                            matrix[realX, realY] = state;
                        }
                    }
                }
            }
        }

        private bool IsIgnoredFigure(LevelFigure figure, LevelFigure[] ignoredFigures)
        {
            if (ignoredFigures == null)
                return false;

            for (int i = 0; i < ignoredFigures.Length; i++)
            {
                if (figure == ignoredFigures[i])
                    return true;
            }

            return false;
        }

        private PerfectFigure GetPerfectFigures(ref bool[,] baseMatrix, LevelFigure[] ignoredFigures)
        {
            List<PerfectFigure> figureWeights = new List<PerfectFigure>();

            bool figureAdded = false;
            foreach(var figure in availableFigures)
            {
                int maxY = baseMatrix.GetLength(1) - figure.Size.y;
                int maxX = baseMatrix.GetLength(0) - figure.Size.x;

                for (int y = 0; y <= maxY; y++)
                {
                    for (int x = 0; x <= maxX; x++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        int weight = EvaluateFigurePlacement(figure, baseMatrix, position);
                        if (weight > 1)
                        {
                            figureWeights.Add(new PerfectFigure(figure, weight, position));

                            figureAdded = true;
                        }
                    }
                }
            }

            if (!figureAdded)
                return null;

            figureWeights.Shuffle();

            // Find the figure with the highest weight
            PerfectFigure bestFigure = figureWeights[0];
            foreach (var figure in figureWeights)
            {
                if (!IsIgnoredFigure(figure.Figure, ignoredFigures) && figure.Weight > bestFigure.Weight)
                {
                    bestFigure = figure;
                }
            }

            return bestFigure;
        }

        public int EvaluateFigurePlacement(LevelFigure figure, bool[,] matrix, Vector2Int position)
        {
            int weight = figure.DefaultWeight;

            // Check if the figure can be placed and calculate weight
            for (int y = 0; y < figure.Size.y; y++)
            {
                for (int x = 0; x < figure.Size.x; x++)
                {
                    int realX = position.x + x;
                    int realY = position.y + y;

                    if (realX >= matrix.GetLength(0) || realY >= matrix.GetLength(1))
                    {
                        return -1; // Out of bounds
                    }

                    int index = y * figure.Size.x + x;
                    if (figure.Points[index].IsActive && matrix[realX, realY])
                    {
                        return -1; // Overlapping
                    }
                }
            }

            // Place the figure temporarily to evaluate its impact
            PlaceFigure(ref matrix, figure, position, true);

            // Add extra weight if figure simple can be placed
            weight += 3;

            weight += GetFigureWeight(ref matrix, figure, position);

            // Remove the figure from the matrix
            PlaceFigure(ref matrix, figure, position, false);

            return weight;
        }

        private int GetFigureWeight(ref bool[,] matrix, LevelFigure figure, Vector2Int position)
        {
            int weight = 0;

            for (int y = 0; y < figure.Size.y; y++)
            {
                for (int x = 0; x < figure.Size.x; x++)
                {
                    int index = y * figure.Size.x + x;
                    if (figure.Points[index].IsActive)
                    {
                        int realX = position.x + x;
                        int realY = position.y + y;

                        if (IsFullLine(matrix, realX, realY, true) || IsFullLine(matrix, realX, realY, false))
                            weight += 10;
                    }
                }
            }

            return weight;
        }

        private bool IsFullLine(bool[,] matrix, int x, int y, bool horizontal)
        {
            if (horizontal)
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    if (!matrix[i, y])
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    if (!matrix[x, i])
                        return false;
                }
            }

            return true;
        }

        private class PerfectFigure
        {
            public LevelFigure Figure;
            public int Weight;
            public Vector2Int Position;

            public PerfectFigure(LevelFigure figure, int weight, Vector2Int position)
            {
                Figure = figure;
                Weight = weight;
                Position = position;
            }
        }
    }
}