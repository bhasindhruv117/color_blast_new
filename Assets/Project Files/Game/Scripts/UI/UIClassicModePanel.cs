using UnityEngine;

namespace Watermelon
{
    public class UIClassicModePanel : MonoBehaviour
    {
        [SerializeField] ParticleSystem comboParticleSystem;
        [SerializeField] int activateComboAmount = 2;

        private void OnEnable()
        {
            ComboManager.AmountChanged += OnAmountChanged;
        }

        private void OnDisable()
        {
            ComboManager.AmountChanged -= OnAmountChanged;
        }

        private void OnAmountChanged(int value)
        {
            if (value >= activateComboAmount)
            {
                comboParticleSystem.Play();
            }
            else
            {
                comboParticleSystem.Stop();
            }
        }
    }
}
