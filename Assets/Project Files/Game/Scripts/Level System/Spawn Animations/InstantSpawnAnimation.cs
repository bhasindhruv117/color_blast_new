using System.Collections;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Instant Spawn Animation", menuName = "Data/Level/Instant Spawn Animation")]
    public class InstantSpawnAnimation : LevelAnimation
    {
        public override IEnumerator PlayLoseAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted)
        {
            yield return null;

            onAnimationCompleted?.Invoke();
        }

        public override IEnumerator PlayStartAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted)
        {
            yield return null;

            onAnimationCompleted?.Invoke();
        }

        public override void Clear()
        {

        }
    }
}