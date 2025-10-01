#pragma warning disable 0649

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController levelController;

        [SerializeField] LevelAreaBehavior areaBehavior;
        [SerializeField] LevelDockBehavior dockBehavior;
        [SerializeField] FloatingFigureBehavior floatingFigureBehavior;
        [SerializeField] GameObject levelElementPrefab;
        [SerializeField] PreviewParticleBehavior previewParticle;

        private static LevelDatabase levelDatabase;

        public static LevelRepresentation LevelRepresentation { get; private set; }
        public static bool IsLevelLoaded { get; private set; } = false;

        public static bool IsGameActive { get; private set; } = false;
        public static bool IsAnimationPlaying { get; private set; } = false;

        private static bool noMoreMoves;

        public static IWinCondition WinCondition { get; private set; }

        private static LevelSkinData levelSkinData;

        private static LevelSave levelSave;

        private static FiguresHandler figuresHandler;

        private static ElementSpriteType[] allowedSpriteTypes;

        private LevelAnimation currentAnimation;
        private Coroutine animationCoroutine;
        private TweenCase delayTweenCase;
        private Mesh previewMesh;

        private static TweenCase levelFailTweenCase;

        public void Init(LevelDatabase levelDatabase)
        {
            levelController = this;

            if (levelDatabase == null)
                Debug.LogError("Level database is not set. Please check the Game Data scriptable object.");

            LevelController.levelDatabase = levelDatabase;

            levelDatabase.Init();

            figuresHandler = new FiguresHandler(levelDatabase.Figures);

            levelSave = SaveController.GetSaveObject<LevelSave>("level");

            levelSkinData = (LevelSkinData)SkinController.Instance.GetSelectedSkin<LevelSkinDatabase>();
            PrepareCollectablesSprites();

            previewParticle.Init();
            dockBehavior.Init();
            floatingFigureBehavior.Init();

            WinCondition = null;
            levelFailTweenCase = null;

            IsAnimationPlaying = false;
            IsLevelLoaded = false;
            IsGameActive = true;

            noMoreMoves = false;

            SkinController.SkinSelected += OnSkinSelected;
        }

        private void OnSkinSelected(ISkinData skinData)
        {
            PrepareCollectablesSprites();
        }

        private void PrepareCollectablesSprites()
        {
            foreach (CollectableData collectableData in levelDatabase.Collectables)
            {
                collectableData.GenerateCombinedSprite(levelSkinData.SpecialSprite);
            }
        }

        private void OnDestroy()
        {
            if (levelDatabase != null)
                levelDatabase.Unload();

            if (currentAnimation != null)
                currentAnimation.Clear();

            SkinController.SkinSelected -= OnSkinSelected;

            delayTweenCase?.KillActive();
        }

        public void ApplyScreenSize(ScreenSize screenSize)
        {
            if (screenSize == null) return;

            dockBehavior.transform.position = screenSize.DockPosition;
        }

        public void LoadLevelWithID(int level)
        {
            LevelData levelData = levelDatabase.GetLevel(level);
            if (levelData == null)
            {
                Debug.LogError($"Can't load level with {level} index. Please check the level database.");
            }

            LoadLevel(levelData);
        }

        public void LoadClassicMode()
        {
            LevelData levelData = levelDatabase.InfiniteLevelData;
            if (levelData == null)
            {
                Debug.LogError($"Can't load classic level data. Please check the level database.");
            }

            LoadLevel(levelData);
        }

        public void LoadLevel(LevelData levelData)
        {
            if (IsLevelLoaded)
                UnloadLevel();

            UIGame gamePage = UIController.GetPage<UIGame>();

            if (levelData.UseRandomColorsForDockFigures)
            {
                allowedSpriteTypes = EnumUtils.GetEnumArray<ElementSpriteType>();
            }
            else
            {
                allowedSpriteTypes = levelData.DockFiguresSprites;
            }

            LevelRepresentation = new LevelRepresentation(levelData);
            LevelRepresentation.SpawnLevelElements(levelElementPrefab);

            areaBehavior.RecalculateSize(levelData.Size);

            floatingFigureBehavior.SetSize(areaBehavior.ElementSize);

            LevelRepresentation.PlaceOnLevelArea(areaBehavior);

            // Prepare game mode
            if (levelData is InfiniteModeLevelData)
            {
                // There is no win condition in classic mode
                WinCondition = null;

                gamePage.ActivateClassicModeUI();

                LevelAnimation levelSpawnAnimation = levelDatabase.DefaultSpawnAnimation;
                if (levelData.CustomSpawnAnimation != null)
                {
                    levelSpawnAnimation = levelData.CustomSpawnAnimation;
                }

                if (levelSpawnAnimation != null)
                {
                    IsAnimationPlaying = true;

                    currentAnimation = levelSpawnAnimation;
                    animationCoroutine = StartCoroutine(levelSpawnAnimation.PlayStartAnimation(LevelRepresentation, () =>
                    {
                        IsAnimationPlaying = false;
                    }));
                }
            }
            else if (levelData is AdventureModeLevelData)
            {
                AdventureModeLevelData adventureModeLevelData = (AdventureModeLevelData)levelData;
                if (adventureModeLevelData.RequirementType == LevelRequirementType.Score)
                {
                    // Activate score visuals
                    Score.SetState(true);

                    gamePage.ActivateAdventureScoreModeUI(adventureModeLevelData.RequirementScore);
                    gamePage.TargetRequirementsPopup.Show(adventureModeLevelData);
                }
                else if (adventureModeLevelData.RequirementType == LevelRequirementType.Collectables)
                {
                    // Disable score visuals
                    Score.SetState(false);

                    Collectables.SetBlocksPercent(adventureModeLevelData.CollectableBlocksPercent, adventureModeLevelData.CollectableSpawnChance);
                    Collectables.SetTargetCollectables(adventureModeLevelData.RequirementCollectables);

                    gamePage.ActivateAdventureCollectableModeUI(adventureModeLevelData.RequirementCollectables);
                    gamePage.TargetRequirementsPopup.Show(adventureModeLevelData);
                }
                else
                {
                    Debug.LogError("Unknown requirement type.");
                }

                // Load predefined blocks
                LevelRepresentation.PlacePreloadElements(adventureModeLevelData.PreloadLevelData);

                SavePresets.CreateSave("Level " + (ActiveSession.Current.LevelIndex + 1).ToString("0000"), "Levels");
            }

            figuresHandler.OnLevelLoaded(ActiveSession.Current.LevelIndex, LevelRepresentation);
            dockBehavior.SpawnFigures(figuresHandler.GetRandomFigures(dockBehavior.ElementsAmount));

            IsLevelLoaded = true;
        }

        public void UnloadLevel()
        {
            if (!IsLevelLoaded) return;

            IsLevelLoaded = false;

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);

                animationCoroutine = null;
            }

            if (currentAnimation != null)
                currentAnimation.Clear();

            delayTweenCase.KillActive();

            // Unload level
        }

        public static void DisablePreview()
        {
            LevelElementBehavior[,] matrix = LevelRepresentation.LevelMatrix;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j].DisablePreview();
                }
            }

            levelController.previewParticle.Disable();
        }

        public static void EnablePreview(SpriteData spriteData, LevelFigure figure, Vector2Int elementPosition)
        {
            if (CanPlaceElement(figure, elementPosition))
            {
                LevelElementBehavior[,] matrix = LevelRepresentation.LevelMatrix;

                for (int y = 0; y < figure.Size.y; y++)
                {
                    for (int x = 0; x < figure.Size.x; x++)
                    {
                        int index = y * figure.Size.x + x;
                        if (figure.Points[index].IsActive)
                        {
                            matrix[elementPosition.x + x, elementPosition.y + y].EnablePreview(spriteData.Sprite, new Color(1f, 1f, 1f, 0.5f), false);
                        }
                    }
                }

                int previewPointsCount = 0;
                bool linePreview = false;

                int width = matrix.GetLength(0);
                int height = matrix.GetLength(1);

                // Check and draw horizontal lines
                for (int y = 0; y < height; y++)
                {
                    bool isFullLine = true;
                    for (int x = 0; x < width; x++)
                    {
                        if (!matrix[x, y].IsOccupied && !matrix[x, y].IsPreviewActive)
                        {
                            isFullLine = false;
                            break;
                        }
                    }

                    if (isFullLine)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (!matrix[x, y].IsLinePreview)
                                matrix[x, y].EnablePreview(spriteData.Sprite, new Color(1f, 1f, 1f, 1.0f), true);
                        }

                        previewPointsCount += width;

                        linePreview = true;
                    }
                }

                // Check and draw vertical lines
                for (int x = 0; x < width; x++)
                {
                    bool isFullLine = true;
                    for (int y = 0; y < height; y++)
                    {
                        if (!matrix[x, y].IsOccupied && !matrix[x, y].IsPreviewActive)
                        {
                            isFullLine = false;
                            break;
                        }
                    }

                    if (isFullLine)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (!matrix[x, y].IsLinePreview)
                                matrix[x, y].EnablePreview(spriteData.Sprite, new Color(1f, 1f, 1f, 1.0f), true);
                        }

                        previewPointsCount += height;

                        linePreview = true;
                    }
                }

                if (linePreview)
                {
                    GenerateFlatMesh(matrix);

                    PreviewParticleBehavior previewParticleBehavior = levelController.previewParticle;
                    if(previewParticleBehavior != null)
                    {
                        previewParticleBehavior.transform.position = levelController.areaBehavior.transform.position + new Vector3(0, 0, -1.2f);
                        previewParticleBehavior.Enable(levelController.previewMesh, spriteData.PreviewParticleColor, previewPointsCount);
                    }
                }
            }
        }

        private static void GenerateFlatMesh(LevelElementBehavior[,] matrix)
        {
            if(levelController.previewMesh != null)
            {
                Destroy(levelController.previewMesh);
            }

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();

            Vector2 elementSize = matrix[0, 0].Size;
            Vector2 totalSize = new Vector2(matrix.GetLength(0) * elementSize.x, matrix.GetLength(1) * elementSize.y);
            Vector3 pivotOffset = new Vector3(totalSize.x / 2, totalSize.y / 2, 0);

            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                for (int x = 0; x < matrix.GetLength(0); x++)
                {
                    if (matrix[x, y].IsLinePreview)
                    {
                        Vector3 bottomLeft = new Vector3(x * elementSize.x, y * elementSize.y, 0) - pivotOffset;
                        Vector3 bottomRight = new Vector3((x + 1) * elementSize.x, y * elementSize.y, 0) - pivotOffset;
                        Vector3 topLeft = new Vector3(x * elementSize.x, (y + 1) * elementSize.y, 0) - pivotOffset;
                        Vector3 topRight = new Vector3((x + 1) * elementSize.x, (y + 1) * elementSize.y, 0) - pivotOffset;

                        int vertexIndex = vertices.Count;

                        vertices.Add(bottomLeft);
                        vertices.Add(bottomRight);
                        vertices.Add(topLeft);
                        vertices.Add(topRight);

                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 3);

                        uv.Add(new Vector2(0, 0));
                        uv.Add(new Vector2(1, 0));
                        uv.Add(new Vector2(0, 1));
                        uv.Add(new Vector2(1, 1));
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            mesh.RecalculateNormals();

            levelController.previewMesh = mesh;
        }

        public static bool CanPlaceElement(LevelFigure figure, Vector2Int snapPosition)
        {
            if (snapPosition.x != -1 && snapPosition.y != -1)
            {
                LevelElementBehavior[,] matrix = LevelRepresentation.LevelMatrix;
                for (int y = 0; y < figure.Size.y; y++)
                {
                    for (int x = 0; x < figure.Size.x; x++)
                    {
                        int realX = snapPosition.x + x;
                        int realY = snapPosition.y + y;

                        if (realX >= matrix.GetLength(0) || realY >= matrix.GetLength(1))
                            return false;

                        int index = y * figure.Size.x + x;
                        if (!figure.Points[index].IsActive)
                            continue;

                        if (matrix[realX, realY].IsOccupied)
                            return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool CanPlaceElement(LevelFigure figure)
        {
            LevelElementBehavior[,] matrix = LevelRepresentation.LevelMatrix;
            for (int y = 0; y <= matrix.GetLength(1) - figure.Size.y; y++)
            {
                for (int x = 0; x <= matrix.GetLength(0) - figure.Size.x; x++)
                {
                    if (CanPlaceElement(figure, new Vector2Int(x, y)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void OnLineCleared(Vector3 lineCenterPosition, Vector3 elementPosition, SpriteData spriteData, bool horizontal)
        {
            ComboManager.IncreaseCombo(elementPosition);

            ParticleCase particleCase = ParticlesController.PlayParticle("Line Blast").SetPosition(lineCenterPosition);

            LineBurstParticle lineBurstParticle = particleCase.ParticleSystem.GetComponent<LineBurstParticle>();
            if(lineBurstParticle != null)
            {
                lineBurstParticle.Activate(spriteData.BurstParticleColor, lineCenterPosition, horizontal);
            }
        }

        public static bool PlaceElement(Sprite elementSprite, ElementSpriteType elementSpriteType, LevelFigure figure, Vector2Int snapPosition)
        {
            if (CanPlaceElement(figure, snapPosition))
            {
                SpriteData elementSpriteData = GetElementSpriteData(elementSpriteType);
                LevelElementBehavior[,] matrix = LevelRepresentation.LevelMatrix;

                for (int y = 0; y < figure.Size.y; y++)
                {
                    for (int x = 0; x < figure.Size.x; x++)
                    {
                        int index = y * figure.Size.x + x;
                        if (!figure.Points[index].IsActive)
                            continue;

                        matrix[snapPosition.x + x, snapPosition.y + y].PlaceElement(elementSprite, figure.Points[index].SpecialBlockBehavior);
                    }
                }

                bool lineCleared = false;
                Vector3 elementPosition = matrix[snapPosition.x, snapPosition.y].transform.position;

                // Check and draw horizontal lines
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    bool isFullLine = true;
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        if (!matrix[x, y].IsOccupied)
                        {
                            isFullLine = false;

                            break;
                        }
                    }

                    if (isFullLine)
                    {
                        for (int x = 0; x < matrix.GetLength(0); x++)
                        {
                            matrix[x, y].MarkAsCollect();
                        }

                        Vector3 lineCenterPosition = LevelRepresentation.GetLineCenterPosition(y, true);
                        OnLineCleared(lineCenterPosition, elementPosition, elementSpriteData, true);

                        lineCleared = true;
                    }
                }

                // Check and draw vertical lines
                for (int x = 0; x < matrix.GetLength(0); x++)
                {
                    bool isFullLine = true;
                    for (int y = 0; y < matrix.GetLength(1); y++)
                    {
                        if (!matrix[x, y].IsOccupied)
                        {
                            isFullLine = false;

                            break;
                        }
                    }

                    if (isFullLine)
                    {
                        for (int y = 0; y < matrix.GetLength(1); y++)
                        {
                            matrix[x, y].MarkAsCollect();
                        }

                        Vector3 lineCenterPosition = LevelRepresentation.GetLineCenterPosition(x, false);
                        OnLineCleared(lineCenterPosition, elementPosition, elementSpriteData, false);

                        lineCleared = true;
                    }
                }

                for (int y = matrix.GetLength(1) - 1; y >= 0; y--)
                {
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        matrix[x, y].Collect();
                    }
                }

                ComboManager.OnElementPlaced();
                Collectables.OnCollectingFinished();

                if(lineCleared)
                    Haptic.Play(Haptic.HAPTIC_LIGHT);

                AudioController.PlaySound(AudioController.AudioClips.figurePlace);

                return true;
            }

            return false;
        }

        public static void OnElementPlaced(LevelFigure levelFigure)
        {
            levelController.dockBehavior.CheckElementsState();

            Score.Add(levelFigure.ActivePoints * Score.Data.ScorePerBlock);
        }

        public static void OnElementPicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.figurePick);

            if (levelFailTweenCase != null)
            {
                levelFailTweenCase.Pause();
            }
        }

        public static void OnElementDrop()
        {
            if (levelFailTweenCase != null)
            {
                levelFailTweenCase.Reset();
                levelFailTweenCase.Resume();
            }
        }

        public static void OnNoAvailableMoves()
        {
            if (noMoreMoves) return;

            noMoreMoves = true;

            levelController.floatingFigureBehavior.Disable();

            AudioController.PlaySound(AudioController.AudioClips.levelFail);

            levelFailTweenCase = Tween.DelayedCall(1f, () =>
            {
                // Show Revive
                UIGame gameUI = UIController.GetPage<UIGame>();
                gameUI.ShowNoSpaceLabel();

                IsGameActive = false;

                gameUI.HideNoSpaceLabel();
                gameUI.RevivePopup.Show(6, OnReviveCompleted);
            });
        }

        private static void OnReviveCompleted(bool completed)
        {
            if (completed)
            {
                IsGameActive = true;
                noMoreMoves = false;

                levelController.dockBehavior.SpawnFigures(figuresHandler.GetPerfectFigures(levelController.dockBehavior.ElementsAmount));
                levelController.dockBehavior.CheckElementsState();

                AudioController.PlaySound(AudioController.AudioClips.levelRevive);
            }
            else
            {
                OnGameFailed();
            }
        }

        public static void OnGameFailed()
        {
            IsGameActive = false;

            AdsManager.ShowInterstitial(null);

            LevelAnimation levelSpawnAnimation = levelDatabase.FailAnimation;
            if (levelSpawnAnimation != null)
            {
                IsAnimationPlaying = true;

                levelController.currentAnimation = levelSpawnAnimation;
                levelController.animationCoroutine = levelController.StartCoroutine(levelSpawnAnimation.PlayLoseAnimation(LevelRepresentation, () =>
                {
                    IsAnimationPlaying = false;

                    ShowCompleteUI();
                }));
            }
            else
            {
                ShowCompleteUI();
            }
        }

        private static void ShowCompleteUI()
        {
            LevelData levelData = LevelRepresentation.LevelData;
            if (levelData is InfiniteModeLevelData)
            {
                if(Score.IsHighscoreReached)
                {
                    AudioController.PlaySound(AudioController.AudioClips.levelComplete);

                    UIController.ShowPage<UIHighscoreGameOver>();
                }
                else
                {
                    AudioController.PlaySound(AudioController.AudioClips.levelComplete);

                    UIController.ShowPage<UIScoreGameOver>();
                }
            }
            else if (levelData is AdventureModeLevelData)
            {
                AudioController.PlaySound(AudioController.AudioClips.levelComplete);

                LivesSystem.UnlockLife(true);

                UIController.ShowPage<UIGameOver>();
            }
        }

        public static void CheckWinCondition()
        {
            if (!IsGameActive) return;

            if (WinCondition != null)
            {
                if (!WinCondition.IsWinConditionMet()) return;

                CompleteGame(0.8f);
            }
        }

        public static void CompleteGame(float delay = 0.8f)
        {
            if (!IsGameActive) return;

            // Disable game
            IsGameActive = false;

            levelController.floatingFigureBehavior.Disable();
            levelController.dockBehavior.Disable();

            // Save progress
            int currentLevelIndex = ActiveSession.Current.LevelIndex + 1;
            if (currentLevelIndex > levelSave.MaxReachedLevelIndex)
            {
                levelSave.MaxReachedLevelIndex = currentLevelIndex;

                SaveController.MarkAsSaveIsRequired();
            }

            levelController.delayTweenCase = Tween.DelayedCall(delay, () =>
            {
                AdsManager.ShowInterstitial(null);

                AudioController.PlaySound(AudioController.AudioClips.levelComplete);

                LivesSystem.UnlockLife(false);

                // Show win screen
                UIController.ShowPage<UIComplete>();
            });
        }

        public static void SetWinCondition(IWinCondition winCondition)
        {
            WinCondition = winCondition;
        }

        public static Sprite GetSpecialBackgroundSprite()
        {
            return levelSkinData.SpecialSprite;
        }

        public static Sprite GetElementSprite(ElementSpriteType elementSpriteType)
        {
            return levelSkinData.GetElementSprite(elementSpriteType);
        }

        public static SpriteData GetElementSpriteData(ElementSpriteType elementSpriteType)
        {
            return levelSkinData.GetSpriteData(elementSpriteType);
        }

        public static CollectableData GetCollectableObjectData(string name)
        {
            return levelDatabase.Collectables.First(x => x.ID == name);
        }

        public static LevelFigure[] GetRandomLevelFigure(int count)
        {
            return figuresHandler.GetRandomFigures(count);
        }

        public static ElementSpriteType GetRandomElementSprite()
        {
            return allowedSpriteTypes.GetRandomItem();
        }
    }
}