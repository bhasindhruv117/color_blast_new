using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Monetization")]
    public class MonetizationInitModule : InitModule
    {
        public override string ModuleName => "Monetization"; 

        [SerializeField] MonetizationSettings settings;

        public override void CreateComponent()
        {
            Monetization.Init(settings);
            IAPManager.Init(settings);
        }
    }
}