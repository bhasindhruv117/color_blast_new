#pragma warning disable 0649

using System;
using UnityEngine;

namespace Watermelon
{
    public class LevelDockBehavior : MonoBehaviour
    {
        [SerializeField] GameObject elementPrefab;

        [Space]
        [SerializeField] Vector2 zoneSize;
        [SerializeField] float spacing;
        [SerializeField] int elementsAmount;

        private LevelDockElementBehavior[] elements;

        public int ElementsAmount => elementsAmount;

        public void Init()
        {
            elements = new LevelDockElementBehavior[elementsAmount];
            float elementWidth = zoneSize.x / elementsAmount - spacing;
            float startX = -zoneSize.x / 2 + elementWidth / 2;

            for (int i = 0; i < elementsAmount; i++)
            {
                GameObject elementObject = Instantiate(elementPrefab, transform);
                elementObject.transform.localPosition = new Vector3(startX + i * (elementWidth + spacing), 0, 0);

                LevelDockElementBehavior elementBehavior = elementObject.GetComponent<LevelDockElementBehavior>();
                elementBehavior.Init(this);

                elements[i] = elementBehavior;
            }
        }

        public void Disable()
        {
            foreach (LevelDockElementBehavior element in elements)
            {
                element.Disable();
            }
        }

        public void CheckElementsState()
        {
            bool noMoreMoves = true;

            foreach (LevelDockElementBehavior element in elements)
            {
                if (element.IsActive)
                {
                    bool state = LevelController.CanPlaceElement(element.Figure);

                    if (state)
                    {
                        noMoreMoves = false;

                        break;
                    }
                }
            }

            if (noMoreMoves)
                LevelController.OnNoAvailableMoves();
        }

        public void SpawnFigures(LevelFigure[] figures)
        {
            for(int i = 0; i < figures.Length; i++)
            {
                LevelFigure figure = figures[i];

                if (Collectables.IsActive)
                {
                    int possibleSpecialElements = Collectables.GetCollectablesBlocksAmount(figure.ActivePoints);
                    for(int p = 0; p < possibleSpecialElements; p++)
                    {
                        CollectedData collectedData = Collectables.GetRequiredCollectable();
                        if (collectedData != null)
                        {
                            PointData point = figure.GetRegularPoint();
                            if(point != null)
                            {
                                point.SetSpecialBehavior(new CollectableSpecialBlockBehavior(collectedData.CollectableData));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetFigure(figures[i], LevelController.GetRandomElementSprite());
            }
        }

        public void OnElementPlaced()
        {
            bool resetFigures = true;
            foreach(LevelDockElementBehavior element in elements)
            {
                if (element.IsActive)
                {
                    resetFigures = false;

                    break;
                }
            }

            if(resetFigures)
            {
                LevelFigure[] newFigures = LevelController.GetRandomLevelFigure(elements.Length);
                SpawnFigures(newFigures);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, zoneSize);
        }
    }
}
