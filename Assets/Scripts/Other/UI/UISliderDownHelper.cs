using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XDPaint.Demo.UI
{
    public class UISliderDownHelper : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Slider slider;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            slider.OnDrag(eventData);
        }
    }
}