#pragma warning disable 0649

using System;
using UnityEngine;

namespace Watermelon
{
    public class FloatingFigureBehavior : MonoBehaviour
    {
        private static FloatingFigureBehavior floatingFigure;

        [SerializeField] Vector3 offset;
        [SerializeField] float snapDisableDistance = 1.0f;

        [Space]
        [SerializeField] SimpleBounce bounceAnimation;

        private bool isSelected;
        private SimpleBoolCallback placementCallback;

        private bool isSnapped;
        private Vector3 snapPosition;
        private Vector2Int snapGridPosition = new Vector2Int(-1, -1);
        private Vector2Int tempSnapGridPosition = new Vector2Int(-1, -1);

        private Vector3 raycastFigureOffset;

        private LevelFigureVisuals levelFigureBehavior;

        public void Init()
        {
            floatingFigure = this;

            levelFigureBehavior = new LevelFigureVisuals();
            levelFigureBehavior.Init(transform);

            bounceAnimation.Init(levelFigureBehavior.Transform);

            isSelected = false;
        }

        public void Disable()
        {
            if (isSelected)
                Unselect(false);

            if (isSnapped)
                ResetSnap();
        }

        private void Update()
        {
            if (!LevelController.IsGameActive)
            {
                if (isSelected)
                    Unselect(false);

                if (isSnapped)
                    ResetSnap();

                return;
            }

            if (!isSelected) return;

            if (InputController.ClickAction.WasReleasedThisFrame())
            {
                LevelController.OnElementDrop();

                if (isSnapped)
                {
                    bool elementPlaced = LevelController.PlaceElement(levelFigureBehavior.SkinSprite, levelFigureBehavior.ElementSpriteType, levelFigureBehavior.LevelFigure, snapGridPosition);

                    Unselect(elementPlaced);

                    if (elementPlaced)
                        LevelController.OnElementPlaced(levelFigureBehavior.LevelFigure);

                    ResetSnap();
                }
                else
                {
                    Unselect(false);
                    ResetSnap();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!LevelController.IsGameActive)
            {
                if(isSelected) 
                    Unselect(false);

                if (isSnapped)
                    ResetSnap();

                return;
            }

            if (!isSelected) return;

            Vector3 mousePosition = InputController.MousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));

            // Fix object to the mouse position with offset
            transform.position = worldPosition + offset;

            Vector3 raycastPosition = levelFigureBehavior.Transform.position + raycastFigureOffset;

            Debug.DrawRay(raycastPosition, raycastPosition.SetZ(10));

            LevelController.LevelRepresentation.WorldToGridPosition(raycastPosition, ref tempSnapGridPosition);

            if (isSnapped)
            {
                float snapDistance = Vector3.Distance(raycastPosition, snapPosition);
                if (snapDistance > snapDisableDistance)
                {
                    ResetSnap();

                    return;
                }
            }

            if (tempSnapGridPosition != snapGridPosition)
            {
                if ((tempSnapGridPosition.x != -1 || tempSnapGridPosition.y != -1) && LevelController.CanPlaceElement(levelFigureBehavior.LevelFigure, tempSnapGridPosition))
                {
                    if (isSnapped)
                        LevelController.DisablePreview();

                    isSnapped = true;
                    snapGridPosition = tempSnapGridPosition;
                    snapPosition = raycastPosition;

                    SpriteData spriteData = LevelController.GetElementSpriteData(levelFigureBehavior.ElementSpriteType);

                    LevelController.EnablePreview(spriteData, levelFigureBehavior.LevelFigure, snapGridPosition);
                }
            }
        }

        private void ResetSnap()
        {
            LevelController.DisablePreview();

            snapGridPosition.x = -1;
            snapGridPosition.y = -1;

            tempSnapGridPosition.x = -1;
            tempSnapGridPosition.y = -1;

            isSnapped = false;
        }

        public void SetSize(Vector2 elementSize)
        {
            levelFigureBehavior.OverrideSize(elementSize.x);
        }

        public static void Unselect(bool placed)
        {
            if (!floatingFigure.isSelected) return;

            floatingFigure.levelFigureBehavior.GameObject.SetActive(false);
            floatingFigure.isSelected = false;

            floatingFigure.placementCallback?.Invoke(placed);
            floatingFigure.placementCallback = null;
        }

        public static void Select(LevelFigure levelFigure, ElementSpriteType elementSpriteType, SimpleBoolCallback placementCallback)
        {
            if (floatingFigure.isSelected) return;

            floatingFigure.placementCallback = placementCallback;

            LevelFigureVisuals figureBehavior = floatingFigure.levelFigureBehavior;
            figureBehavior.Spawn(levelFigure, elementSpriteType, 10);
            figureBehavior.Transform.localPosition = Vector3.zero;
            figureBehavior.GameObject.SetActive(true);
            figureBehavior.Transform.DOLocalMoveY(figureBehavior.FigureBounds.size.y / 2, 0.08f);

            floatingFigure.bounceAnimation.Bounce();

            // Calculate raycast offset
            float halfElementSize = figureBehavior.ElementSize / 2;
            float halfFigureWidth = figureBehavior.FigureBounds.size.x / 2;
            float halfFigureHeight = figureBehavior.FigureBounds.size.y / 2;

            floatingFigure.raycastFigureOffset = new Vector3(-halfFigureWidth + halfElementSize, -halfFigureHeight + halfElementSize, 0);

            floatingFigure.isSelected = true; 
            
            // Calculate mouse position
            Vector3 mousePosition = InputController.MousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));

            // Fix object to the mouse position with offset
            floatingFigure.transform.position = worldPosition + floatingFigure.offset;

            LevelController.OnElementPicked();
        }

        private void OnDrawGizmosSelected()
        {
            if (levelFigureBehavior == null || levelFigureBehavior.GameObject == null || !levelFigureBehavior.GameObject.activeSelf) return;

            Gizmos.color = Color.red;

            Bounds bounds = levelFigureBehavior.FigureBounds;
            Gizmos.DrawWireCube(bounds.center + levelFigureBehavior.Transform.position, bounds.size);
        }
    }
}