using GetampedPaint.Utils;

namespace GetampedPaint.Controllers
{
    public class ControllersContainer : Singleton<ControllersContainer>
    {
        private new void Awake()
        {
            base.Awake();
            if (InputController.Instance == null)
            {
                gameObject.AddComponent<InputController>();
            }
            if (RaycastController.Instance == null)
            {
                gameObject.AddComponent<RaycastController>();
            }
            if (PaintController.Instance == null)
            {
                gameObject.AddComponent<PaintController>();
            }
        }
    }
}