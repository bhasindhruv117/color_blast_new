using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip figurePlace;
        [BoxGroup("Gameplay")]
        public AudioClip figurePick;
        [BoxGroup("Gameplay")]
        public AudioClip lineCollected;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip levelComplete;
        [BoxGroup("Gameplay")]
        public AudioClip levelFail;
        [BoxGroup("Gameplay")]
        public AudioClip levelRevive;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip requirementMet;

        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------