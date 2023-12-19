using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XDPaint.Demo.UI
{
    public class UIPointerHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<PointerEventData> OnDown;
        public event Action<PointerEventData> OnUp;

        [SerializeField] private UnityEvent onDownEvent;
        [SerializeField] private UnityEvent onUpEvent;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            onDownEvent?.Invoke();
            OnDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onUpEvent?.Invoke();   
            OnUp?.Invoke(eventData);
        }
    }
}