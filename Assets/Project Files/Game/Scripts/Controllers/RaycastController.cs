using UnityEngine;

namespace Watermelon
{
    public class RaycastController : MonoBehaviour
    {
        private static readonly int PARTICLE_CLICK_HASH = "Click".GetHashCode();

        [SerializeField] bool clickParticle;

        private UIGame gameUI;

        public void Init()
        {
            gameUI = UIController.GetPage<UIGame>();
        }

        private void Update()
        {
            if (InputController.ClickAction.WasPressedThisFrame() && !UIController.IsPopupOpened && gameUI.IsPageDisplayed)
            {
                Ray ray = Camera.main.ScreenPointToRay(InputController.MousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    IClickableObject clickableObject = hit.transform.GetComponent<IClickableObject>();
                    if (clickableObject != null)
                    {
                        clickableObject.OnObjectClicked();
                    }
                }

                if (clickParticle)
                    ParticlesController.PlayParticle(PARTICLE_CLICK_HASH).SetPosition(ray.origin);
            }
        }
    }
}
