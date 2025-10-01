#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class LevelDockElementBehavior : MonoBehaviour, IClickableObject
    {
        private static readonly Color DISABLE_COLOR = new Color(1, 1, 1, 0.4f);

        [SerializeField] LevelFigureVisuals levelFigureBehavior;
        [SerializeField] SimpleBounce spawnBounce;

        public bool IsActive { get; private set; }

        public LevelFigureVisuals FigureVisuals => levelFigureBehavior;
        public LevelFigure Figure => levelFigureBehavior.LevelFigure;

        private LevelDockBehavior levelDockBehavior;

        public void Init(LevelDockBehavior levelDockBehavior)
        {
            this.levelDockBehavior = levelDockBehavior;

            levelFigureBehavior.Init(transform);

            spawnBounce.Init(transform);
        }

        public void Disable()
        {
            levelFigureBehavior.SetColor(DISABLE_COLOR);
        }

        public void SetFigure(LevelFigure levelFigure, ElementSpriteType elementSpriteType)
        {
            levelFigureBehavior.Spawn(levelFigure, elementSpriteType, 10);
            levelFigureBehavior.GameObject.SetActive(true);

            IsActive = true;

            PlaySpawnAnimation();
        }

        private void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;

            spawnBounce.Bounce();
        }

        public void OnObjectClicked()
        {
            if (LevelController.IsAnimationPlaying) return;
            if (!LevelController.IsGameActive) return;
            if (UIController.IsPopupOpened) return;
            if (!IsActive) return;

            IsActive = false;

            levelFigureBehavior.GameObject.SetActive(false);

            FloatingFigureBehavior.Select(levelFigureBehavior.LevelFigure, levelFigureBehavior.ElementSpriteType, OnFloatingFigureDropped);
        }

        private void OnFloatingFigureDropped(bool placedOnMap)
        {
            if(!placedOnMap)
            {
                IsActive = true;

                levelFigureBehavior.GameObject.SetActive(true);
            }
            else
            {
                levelDockBehavior.OnElementPlaced();
            }
        }
    }
}