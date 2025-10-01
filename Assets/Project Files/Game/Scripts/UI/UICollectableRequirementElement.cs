using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UICollectableRequirementElement : MonoBehaviour
    {
        [SerializeField] Image previewImage;
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] Image checkmarkImage;

        [Space]
        [SerializeField] JuicyBounce bounceAnimation;

        private RequirementCollectableData requirementCollectableData;

        public CollectableData CollectableData { get; private set; }
        public string CollectableName => CollectableData.ID;

        public int StoredAmount { get; private set; }

        public void Init(RequirementCollectableData requirementCollectableData)
        {
            this.requirementCollectableData = requirementCollectableData;

            CollectableData = LevelController.GetCollectableObjectData(requirementCollectableData.CollectableName);

            StoredAmount = requirementCollectableData.Amount;

            previewImage.sprite = CollectableData.Icon;

            bounceAnimation.Init(transform);

            UpdateAmount();
        }

        public void UpdateAmount()
        {
            if(StoredAmount <= 0 && checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(true);
                amountText.gameObject.SetActive(false);

                AudioController.PlaySound(AudioController.AudioClips.requirementMet);
            }
            else
            {
                amountText.text = StoredAmount.ToString();
            }
        }

        public void HideAmount()
        {
            amountText.gameObject.SetActive(false);
        }

        public void ShowAmount()
        {
            amountText.gameObject.SetActive(true);

            bounceAnimation.Bounce();
        }

        public void OnFloatingCollectableReached()
        {
            StoredAmount = Mathf.Max(StoredAmount - 1, 0);

            UpdateAmount();

            bounceAnimation.Bounce();
        }
    }
}
