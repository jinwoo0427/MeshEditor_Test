using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace GetampedPaint.Tools.Raycast
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class CanvasGraphicRaycaster : MonoBehaviour
    {
        private GraphicRaycaster raycaster;
        private EventSystem eventSystem;
        private PointerEventData pointerEventData;
        private List<RaycastResult> results;

        private const int ResultsCapacity = 64;

        void Start()
        {
            raycaster = GetComponent<GraphicRaycaster>();
            eventSystem = GetComponent<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>();
            }
            results = new List<RaycastResult>(ResultsCapacity);
        }

        public List<RaycastResult> GetRaycasts(Vector2 position)
        {
            if (raycaster == null)
                return null;
            
            pointerEventData = new PointerEventData(eventSystem) {position = position};
            results.Clear();
            results.Capacity = ResultsCapacity;
            raycaster.Raycast(pointerEventData, results);
            return results;
        }
    }
}