using UnityEngine;

namespace Watermelon
{
    public class LevelSizeController : MonoBehaviour
    {
        [SerializeField] ScreenSize[] screenSizes;
        public ScreenSize[] ScreenSizes => screenSizes;

        public int GetRecommendedScreenSizeIndex()
        {
            if (screenSizes.IsNullOrEmpty()) return -1;

            Camera camera = Camera.main;

            float currentWidth = camera.pixelWidth;
            float currentHeight = camera.pixelHeight;
            float currentDiagonal = UIUtils.GetDeviceDiagonalSizeInInches(camera);
            float currentAspectRatio = UIUtils.GetAspectRatio(camera);

            int bestMatchIndex = -1;
            int highestWeight = int.MinValue;

            for(int i = 0; i < screenSizes.Length; i++)
            {
                ScreenSize screenSize = screenSizes[i];

                int weight = 0;

                // Compare portrait or landscape mode
                if ((currentWidth > currentHeight && screenSize.Width > screenSize.Height) || (currentWidth < currentHeight && screenSize.Width < screenSize.Height))
                {
                    weight += 3;
                }

                // Compare width and height differences
                float widthDifference = Mathf.Abs(screenSize.Width - currentWidth);
                float heightDifference = Mathf.Abs(screenSize.Height - currentHeight);
                if (widthDifference < 100 && heightDifference < 100) // Example threshold
                {
                    weight += 1;
                }

                // Compare aspect ratios
                float aspectRatioDifference = Mathf.Abs(screenSize.AspectRatio - currentAspectRatio);
                if (aspectRatioDifference < 0.06f)
                {
                    weight += 3;
                }
                else if (aspectRatioDifference < 0.12f)
                {
                    weight += 2;
                }
                else if (aspectRatioDifference < 0.18f)
                {
                    weight += 1;
                }

                if (weight > highestWeight)
                {
                    highestWeight = weight;
                    bestMatchIndex = i;
                }
            }

            return bestMatchIndex;
        }

        public ScreenSize GetRecommendedScreenSize()
        {
            int recommendedIndex = GetRecommendedScreenSizeIndex();
            if(recommendedIndex == -1)
            {
                Debug.LogWarning("No recommended screen size found.");

                return null;
            }

            return screenSizes[recommendedIndex];
        }

        
    }
}