using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public abstract class LevelAnimation : ScriptableObject
    {
        public abstract IEnumerator PlayStartAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted);
        public abstract IEnumerator PlayLoseAnimation(LevelRepresentation levelRepresentation, SimpleCallback onAnimationCompleted);

        public abstract void Clear();
    }
}