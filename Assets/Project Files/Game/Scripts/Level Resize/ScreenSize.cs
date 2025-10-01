using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ScreenSize
    {
        [SerializeField] string note;

        [Space]
        [SerializeField] float width;
        [SerializeField] float height;
        [SerializeField] float aspectRatio;

        [Space]
        [SerializeField] Vector3 dockPosition;
        [SerializeField] Vector2 areaSize;
        [SerializeField] Vector3 areaPosition;

        public string Note => note;

        public float Width => width;
        public float Height => height;
        public float AspectRatio => aspectRatio;

        public Vector3 DockPosition => dockPosition;
        public Vector2 AreaSize => areaSize;
        public Vector3 AreaPosition => areaPosition;

        public ScreenSize(float width, float height, float aspectRatio)
        {
            this.width = width;
            this.height = height;
            this.aspectRatio = aspectRatio;
        }
    }
}