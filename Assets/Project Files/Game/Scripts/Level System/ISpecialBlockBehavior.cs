#pragma warning disable 0649

namespace Watermelon
{
    public interface ISpecialBlockBehavior
    {
        public void ApplyToFigureVisual(LevelFigureVisualsElement levelFigureVisualsElement);
        public void ApplyToLevelElement(LevelElementBehavior levelElementBehavior);
        public void OnLevelElementCollected(LevelElementBehavior levelElementBehavior);
    }
}