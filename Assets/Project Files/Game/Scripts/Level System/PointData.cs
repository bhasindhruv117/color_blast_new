using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class PointData
    {
        [SerializeField] bool isActive;
        public bool IsActive => isActive;

        [System.NonSerialized] ISpecialBlockBehavior specialBlockBehavior;
        public ISpecialBlockBehavior SpecialBlockBehavior => specialBlockBehavior;

        public PointData(bool isActive)
        {
            this.isActive = isActive;
        }

        public void SetSpecialBehavior(ISpecialBlockBehavior specialBlockBehavior)
        {
            this.specialBlockBehavior = specialBlockBehavior;
        }
    }
}
