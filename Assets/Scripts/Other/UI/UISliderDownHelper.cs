using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GetampedPaint.Demo.UI
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