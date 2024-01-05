using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorPickerUtil.Demo
{
    public class FloatingWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform target;

        Camera eventCam;
        Vector2 beginPos, dragPos;

        public Vector2 GetMousePosition()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, Input.mousePosition, eventCam, out mousePos);
            return mousePos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventCam = eventData.pressEventCamera;
            beginPos = GetMousePosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragPos = GetMousePosition();
            target.anchoredPosition += dragPos - beginPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }
    }
}