#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class CollectableSpecialBlockBehavior : ISpecialBlockBehavior
    {
        private CollectableData collectableData;
        public CollectableData CollectableData => collectableData;

        public CollectableSpecialBlockBehavior(CollectableData collectableData)
        {
            this.collectableData = collectableData;
        }

        public void ApplyToFigureVisual(LevelFigureVisualsElement levelFigureVisualsElement)
        {
            levelFigureVisualsElement.SpriteRenderer.sprite = collectableData.CombinedSprite;
        }

        public void ApplyToLevelElement(LevelElementBehavior levelElementBehavior)
        {
            levelElementBehavior.SetSprite(collectableData.CombinedSprite);
        }

        public void OnLevelElementCollected(LevelElementBehavior levelElementBehavior)
        {
            if (!Collectables.IsActive) return;

            Collectables.Add(collectableData.ID, 1);

            Collectables.OnCollectableBlockPicked();

            LevelController.CheckWinCondition();

            UIGame gameUI = UIController.GetPage<UIGame>();
            gameUI.TargetCollectablesPanel.SpawnFloatingCollectable(collectableData, levelElementBehavior.transform.position, (element) =>
            {
                element.OnFloatingCollectableReached();
            });
        }
    }
}