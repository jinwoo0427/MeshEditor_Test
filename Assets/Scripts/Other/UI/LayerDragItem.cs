using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Core.Layers;

namespace XDPaint.Demo.UI
{
    public class LayerDragItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform dragTransform;

        public Action OnDragStarted;
        public Action<ILayer, int> OnDragEnded;
        
        private GameObject mainContent;
        private Vector3 currentPosition;
        private Vector3 startPosition;
        private int totalChild;
        private int layerOrder;
        private int defaultSiblingIndex;
        private int startSiblingIndex;
        private bool isDragStarted;

        private void Awake()
        {
            defaultSiblingIndex = dragTransform.GetSiblingIndex();
        }

        public void RestoreSiblingIndex()
        {
            dragTransform.SetSiblingIndex(defaultSiblingIndex);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragStarted = false;
            currentPosition = dragTransform.position;
            mainContent = dragTransform.parent.gameObject;
            totalChild = mainContent.transform.childCount;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragStarted)
            {
                isDragStarted = true;
                OnDragStarted?.Invoke();
                startPosition = dragTransform.position;
                startSiblingIndex = dragTransform.GetSiblingIndex();
                dragTransform.SetSiblingIndex(totalChild - 1);
            }
            dragTransform.position = new Vector3(dragTransform.position.x, eventData.position.y, dragTransform.position.z);
            for (var i = 0; i < totalChild; i++)
            {
                if (i == dragTransform.GetSiblingIndex()) 
                    continue;
                var otherTransform = mainContent.transform.GetChild(i);
                if (!otherTransform.gameObject.activeSelf)
                    continue;
                if (otherTransform.TryGetComponent<LayoutElement>(out var otherLayout) && otherLayout.ignoreLayout)
                    continue;
                var distance = (int)Vector3.Distance(dragTransform.position, otherTransform.position);
                if (distance <= 50f)
                {
                    var otherTransformOldPosition = otherTransform.position;
                    otherTransform.position = new Vector3(otherTransform.position.x, currentPosition.y, otherTransform.position.z);
                    dragTransform.position = new Vector3(dragTransform.position.x, otherTransformOldPosition.y, dragTransform.position.z);
                    layerOrder = otherTransform.GetSiblingIndex();
                    currentPosition = dragTransform.position;
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragTransform.position = currentPosition;
            if (!isDragStarted)
                return;

            int siblingIndex;
            if (Vector3.Distance(dragTransform.position, startPosition) < dragTransform.sizeDelta.y / 2f)
            {
                siblingIndex = startSiblingIndex;
            }
            else if (startSiblingIndex == layerOrder)
            {
                siblingIndex = startSiblingIndex + 1;
            }
            else if (startSiblingIndex > layerOrder)
            {
                siblingIndex = layerOrder;
            }
            else
            {
                siblingIndex = layerOrder + 1;
            }
            dragTransform.SetSiblingIndex(siblingIndex);
            if (dragTransform.TryGetComponent<LayerUIItem>(out var layerUIItem))
            {
                OnDragEnded?.Invoke(layerUIItem.Layer, siblingIndex);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragStarted)
                return;
            if (eventData.pointerPress.transform.parent.TryGetComponent<LayerUIItem>(out var layerUIItem))
            {
                layerUIItem.SetActive();
            }
        }
    }
}