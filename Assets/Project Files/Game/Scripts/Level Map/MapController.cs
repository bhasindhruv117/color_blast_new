using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Watermelon.Map
{
    [HelpURL("https://www.notion.so/wmelongames/Level-Map-6401a1ee9c054ab6b072b711ce9fdfe8")]
    public class MapController : MonoBehaviour
    {
        private static MapController instance;

        public List<MapChunkBehavior> loadedChunks;

        public MapChunkBehavior LowestLoadedChunk => loadedChunks[0];
        public MapChunkBehavior HighestLoadedChunk => loadedChunks[^1];

        public float MapVisibleRectWidth { get; private set; }
        public float MapVisibleRectHeight { get; private set; }

        public static int MaxLevelReached => levelSave.MaxReachedLevelIndex;
        public static int AmountOfLevels { get; private set; }

        private static LevelSave levelSave;

        private bool isMouseDown = false;

        private float mousePressPosY;
        private float mouseReleasePosY;

        private float currentLowestChunkPosY;
        private float mousePrevFramePosY;
        private float mouseMoveDeltaY;

        private TweenCase rubberCase;

        private bool isPopupOpened;

        private Vector2 mousePosition;

        private Ease.IEasingFunction easingFunction;

        public MapData Data { get; private set; }

        public void Init(MapData data)
        {
            Data = data;

            levelSave = SaveController.GetSaveObject<LevelSave>("level");

            GameData gameData = GameData.Data;
            AmountOfLevels = gameData.LevelDatabase.AmountOfLevels;

            instance = this;
            loadedChunks = new List<MapChunkBehavior>();

            easingFunction = Ease.GetFunction(Ease.Type.SineOut);

            // The height of the orthographic camera in default units
            MapVisibleRectHeight = Camera.main.orthographicSize * 2;

            if (!data.AdjustForWideScreenes || Camera.main.aspect < 9f / 16f)
            {
                // Real width of rge orthographic camera
                MapVisibleRectWidth = MapVisibleRectHeight * Camera.main.aspect;
            }
            else
            {
                // Constraind width for correct scaling on wide screenes
                MapVisibleRectWidth = MapVisibleRectHeight * 9f / 16f;
            }

            enabled = false;

            UIController.PopupOpened += OnPopupStateChanged;
            UIController.PopupClosed += OnPopupStateChanged;
        }

        private void OnDestroy()
        {
            rubberCase?.KillActive();

            UIController.PopupOpened -= OnPopupStateChanged;
            UIController.PopupClosed -= OnPopupStateChanged;
        }

        private void OnPopupStateChanged(IPopupWindow popupWindow, bool state)
        {
            isPopupOpened = state;
            isMouseDown = false;
        }

        public void Show()
        {
            enabled = true;
            isMouseDown = false;

            int lastReachedChunkId = GetLastReachedChunkId(out int totalLevelsCount);

            var lastReachedChunk = Instantiate(Data.Chunks[lastReachedChunkId % Data.Chunks.Length]).GetComponent<MapChunkBehavior>();

            lastReachedChunk.SetMap(this);
            lastReachedChunk.Init(lastReachedChunkId, totalLevelsCount - lastReachedChunk.LevelsCount);
            loadedChunks.Add(lastReachedChunk);

            // The initial Y position of the lastReachedChunk is 0. we scroll down to the position of the last reached level, and then scrolling up to the desired position
            var lastReachedLevelPos = -lastReachedChunk.CurrentLevelPosition + Data.CurrentLevelVerticalOffset;
            if (lastReachedChunkId == 0 && lastReachedLevelPos > Data.FirstChunkMaxLevelVerticalOffset)
            {
                lastReachedLevelPos = Data.FirstChunkMaxLevelVerticalOffset;
            }

            // We just reseting lastReachedChunks position, populaing parameters to let ScrollMap method do all the work of moving the map to the position we calculated above 
            lastReachedChunk.SetPosition(0);

            currentLowestChunkPosY = 0;
            mouseMoveDeltaY = lastReachedLevelPos;
            ScrollMap();

            // Populaing the map to fill the whole screen
            CheckBottomChunks();
            CheckTopChunks();

            if (!Data.InfiniteMap)
            {
                for (int i = 0; i < loadedChunks.Count; i++)
                {
                    var chunk = loadedChunks[i];

                    if (chunk.HasDisabledLevels)
                    {
                        if (chunk.FirstDisabledLevelPostion <= Data.LastLevelVerticalOffset)
                        {
                            ScrollMap();

                            TopRubber(chunk);

                            break;
                        }
                    }
                }
            }
        }

        public void Hide()
        {
            enabled = false;
            for (int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                loadedChunks[i].gameObject.SetActive(false);

                Destroy(loadedChunks[i].gameObject);

                loadedChunks.RemoveAt(i);
            }
        }

        #region Movement

        public static void EnableScroll()
        {
            instance.enabled = true;
            instance.isMouseDown = false;
        }

        public static void DisableScroll()
        {
            instance.enabled = false;
            instance.isMouseDown = false;
        }

        /// <param name="totalLevelsCount">The amount of levels from all previous chunks up to and including last reached chunk</param>
        private int GetLastReachedChunkId(out int totalLevelsCount)
        {
            int lastReachedChunkId = -1;

            totalLevelsCount = 0;
            while (totalLevelsCount <= MaxLevelReached)
            {
                lastReachedChunkId++;

                var chunk = Data.Chunks[lastReachedChunkId % Data.Chunks.Length].GetComponent<MapChunkBehavior>();
                totalLevelsCount += chunk.LevelsCount;
            }

            return lastReachedChunkId;
        }

        private void Update()
        {
            mousePosition = InputController.MousePosition;

            if (isPopupOpened) return;

            if (InputController.ClickAction.WasPressedThisFrame())
            {
                // mouse press y position mapped on 0-1 scale. 0 is the bottom of the screen, 1 is the top)
                mousePressPosY = mousePosition.y / Camera.main.pixelHeight;
                mousePrevFramePosY = mousePressPosY;
                currentLowestChunkPosY = LowestLoadedChunk.Position;

                isMouseDown = true;

                rubberCase.KillActive();
            }
            else if (InputController.ClickAction.WasReleasedThisFrame())
            {
                if (!isMouseDown) return;

                isMouseDown = false;

                if (LowestLoadedChunk.ChunkId == 0 && LowestLoadedChunk.Position > Data.FirstChunkMaxLevelVerticalOffset)
                {
                    // Scrolled to much down, need to return back up a little bit
                    BottomRubber();
                }
                else
                {
                    if (!Data.InfiniteMap)
                    {
                        for (int i = 0; i < loadedChunks.Count; i++)
                        {
                            var chunk = loadedChunks[i];

                            if (chunk.HasDisabledLevels)
                            {
                                if (chunk.FirstDisabledLevelPostion <= Data.LastLevelVerticalOffset)
                                {
                                    TopRubber(chunk);
                                }

                                return;
                            }
                        }
                    }

                    mouseReleasePosY = mousePosition.y / Camera.main.pixelHeight;
                    var dif = mouseReleasePosY - mousePrevFramePosY;

                    // There was a swipe movement, need to scroll a little bit more for a little bit of time to feel natural
                    if (Mathf.Abs(dif) > 0.001f)
                    {
                        ContinuousScroll(dif);
                    }
                }
            }
            else if (isMouseDown)
            {
                var mousePosY = mousePosition.y / Camera.main.pixelHeight;
                mousePrevFramePosY = mousePosY;

                mouseMoveDeltaY = mousePosY - mousePressPosY;

                ScrollMap();
            }
        }

        private void ContinuousScroll(float scrollFrameDistance)
        {
            float scrollDuration = Mathf.Clamp(Mathf.Abs(scrollFrameDistance / 0.1f), 0, 1);

            rubberCase = Tween.DoFloat(scrollFrameDistance, 0, scrollDuration, (value) => {
                mouseMoveDeltaY += value;

                var cachedPos = currentLowestChunkPosY;

                ScrollMap();

                if (Mathf.Approximately(cachedPos, currentLowestChunkPosY))
                {
                    rubberCase.KillActive();
                    rubberCase.InvokeCompleteEvent();
                }
            }).SetEasing(Ease.Type.SineOut).OnComplete(() => {
                if (LowestLoadedChunk.ChunkId == 0 && LowestLoadedChunk.Position > Data.FirstChunkMaxLevelVerticalOffset)
                {
                    BottomRubber();
                }
                else if (!Data.InfiniteMap)
                {
                    for (int i = 0; i < loadedChunks.Count; i++)
                    {
                        var chunk = loadedChunks[i];

                        if (chunk.HasDisabledLevels)
                        {
                            if (chunk.FirstDisabledLevelPostion <= Data.LastLevelVerticalOffset)
                            {
                                TopRubber(chunk);
                            }

                            break;
                        }
                    }
                }
            });
        }

        private void BottomRubber()
        {
            rubberCase = Tween.DoFloat(LowestLoadedChunk.Position, Data.FirstChunkMaxLevelVerticalOffset, 0.3f, (value) => {
                SetChunksPosition(value);
            }).SetEasing(Ease.Type.QuadOut);
        }

        private void TopRubber(MapChunkBehavior chunk)
        {
            var refferencePos = currentLowestChunkPosY + Data.LastLevelVerticalOffset - chunk.FirstDisabledLevelPostion;

            rubberCase = Tween.DoFloat(chunk.FirstDisabledLevelPostion, Data.LastLevelVerticalOffset, 0.3f, (value) => {

                var pos = refferencePos - Data.LastLevelVerticalOffset + value;

                SetChunksPosition(pos);
            }).SetEasing(Ease.Type.QuadOut);
        }

        private void ScrollMap()
        {
            var pos = currentLowestChunkPosY + mouseMoveDeltaY;

            if (pos > Data.FirstChunkMaxLevelVerticalOffset && LowestLoadedChunk.ChunkId == 0)
            {
                // There are some math that kinda works

                // The overshoot distance from the end of the map
                var rubberDistance = pos - Data.FirstChunkMaxLevelVerticalOffset;
                // Adding Easing for rubber effect
                var interpolatedRubberDistance = easingFunction.Interpolate(rubberDistance);
                // smoothing position depending on mouseDelta. If the mouse is not moving, we're just sticking to the actual position
                var smoothedPos = Mathf.Lerp(pos, Data.FirstChunkMaxLevelVerticalOffset + interpolatedRubberDistance, mouseMoveDeltaY);
                // Clamping position in order not to overshoot too far
                pos = Mathf.Clamp(smoothedPos, Data.FirstChunkMaxLevelVerticalOffset, Data.FirstChunkMaxLevelVerticalOffset + 0.1f);
            }

            SetChunksPosition(pos);

            if (!Data.InfiniteMap)
            {
                for (int i = 0; i < loadedChunks.Count; i++)
                {
                    var chunk = loadedChunks[i];

                    if (chunk.HasDisabledLevels)
                    {
                        if (chunk.FirstDisabledLevelPostion <= Data.LastLevelVerticalOffset)
                        {
                            var rubberDistance = Data.LastLevelVerticalOffset - chunk.FirstDisabledLevelPostion;

                            var interpolatedRubberDistance = Ease.Interpolate(rubberDistance, Ease.Type.SineOut);
                            // smoothing position depending on mouseDelta. If the mouse is not moving, we're just sticking to the actual position
                            var smoothedPos = Mathf.Lerp(chunk.FirstDisabledLevelPostion, Data.LastLevelVerticalOffset + interpolatedRubberDistance, Mathf.Abs(mouseMoveDeltaY));
                            // Clamping position in order not to overshoot too far

                            var clampedPos = Mathf.Clamp(smoothedPos, Data.LastLevelVerticalOffset - 0.1f, Data.LastLevelVerticalOffset);

                            var refferencePos = pos + Data.LastLevelVerticalOffset - chunk.FirstDisabledLevelPostion;
                            pos = refferencePos - Data.LastLevelVerticalOffset + clampedPos;

                            SetChunksPosition(pos);
                        }

                        break;
                    }
                }
            }

            CheckTopChunks();
            CheckBottomChunks();
        }

        private void SetChunksPosition(float pos)
        {
            for (int i = 0; i < loadedChunks.Count; i++)
            {
                var chunk = loadedChunks[i];

                chunk.SetPosition(pos);
                pos += chunk.AdjustedHeight;
            }
        }

        private void CheckBottomChunks()
        {
            // Checking for the chunks that are bellow the camera and not visible to the player anymore
            while (LowestLoadedChunk.Position + LowestLoadedChunk.AdjustedHeight < -0.05f)
            {
                Destroy(LowestLoadedChunk.gameObject);
                loadedChunks.RemoveAt(0);
            }


            while (LowestLoadedChunk.Position >= 0 && LowestLoadedChunk.ChunkId != 0)
            {
                var newLowestChunk = Instantiate(Data.Chunks[(LowestLoadedChunk.ChunkId - 1) % Data.Chunks.Length]).GetComponent<MapChunkBehavior>();
                newLowestChunk.SetMap(this);
                newLowestChunk.Init(LowestLoadedChunk.ChunkId - 1, LowestLoadedChunk.StartLevelCount - newLowestChunk.LevelsCount);
                newLowestChunk.SetPosition(LowestLoadedChunk.Position - newLowestChunk.AdjustedHeight);

                loadedChunks.Insert(0, newLowestChunk);
            }

            // Reseting movement parameters in order to preserve scroll smoothness
            mousePressPosY = mousePosition.y / Camera.main.pixelHeight;
            currentLowestChunkPosY = LowestLoadedChunk.Position;

            mouseMoveDeltaY = 0;
        }

        private void CheckTopChunks()
        {
            // Checking for the chunks that are above the camera and not visible to the player anymore
            while (HighestLoadedChunk.Position > 1.05f)
            {
                Destroy(HighestLoadedChunk.gameObject);
                loadedChunks.RemoveAt(loadedChunks.Count - 1);
            }

            // Checking if there is the need to spawn a new chunk at the top of the screen
            while (HighestLoadedChunk.Position + HighestLoadedChunk.AdjustedHeight <= 1)
            {
                var newHighestChunk = Instantiate(Data.Chunks[(HighestLoadedChunk.ChunkId + 1) % Data.Chunks.Length]).GetComponent<MapChunkBehavior>();

                newHighestChunk.SetMap(this);
                newHighestChunk.Init(HighestLoadedChunk.ChunkId + 1, HighestLoadedChunk.StartLevelCount + HighestLoadedChunk.LevelsCount);
                newHighestChunk.SetPosition(HighestLoadedChunk.Position + HighestLoadedChunk.AdjustedHeight);

                loadedChunks.Add(newHighestChunk);
            }
        }
    }

    #endregion
}