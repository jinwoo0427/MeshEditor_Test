using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XDPaint.Demo.UI
{
    public class UIDoubleClick : MonoBehaviour, IPointerDownHandler
    {
        [Serializable]
        public class OnDoubleClickEvent : UnityEvent<Vector3>
        {
        }
        
        public OnDoubleClickEvent OnDoubleClick = new OnDoubleClickEvent();
        [SerializeField] private float timeBetweenTaps = 0.5f;
        
        private float firstTapTime;
        private bool doubleTapInitialized;
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (Time.time - firstTapTime >= timeBetweenTaps)
            {
                doubleTapInitialized = false;
            }
            else if (doubleTapInitialized)
            {
                OnDoubleClick.Invoke(transform.position);
                doubleTapInitialized = false;
            }

            if (!doubleTapInitialized)
            {
                doubleTapInitialized = true;
                firstTapTime = Time.time;
            }
        }
    }
}