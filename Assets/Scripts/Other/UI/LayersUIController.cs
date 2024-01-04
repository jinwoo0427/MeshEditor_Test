using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Core.Layers;

namespace XDPaint.Demo.UI
{
    public class LayersUIController : MonoBehaviour
    {
        public event Action OnLayersUpdated;
        
        private const int MaxLayersCount = 5;
        private const float MovePlateDuration = 0.3f;
        
        [Header("Buttons")]
        [SerializeField] private Button addLayerButton;
        [SerializeField] private Button removeLayerButton;
        [SerializeField] private Button mergeLayersButton;
        [SerializeField] private Button mergeAllLayersButton;

        [Header("UI")] 
        [SerializeField] private LayerUIItem layerDefaultElement;
        [SerializeField] private Button layersPlateButton;
        [SerializeField] private RectTransform layersTransform;
        [SerializeField] private ContentSizeFitter contentSizeFitter;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        private ILayersController layersController;
        private LayerUIItem[] layersUI;
        private bool isAnimating;
        private bool isContentSizeInitialized;

        public LayerUIItem[] LayerUI => layersUI;
        
        private void Awake()
        {
            // 레이어 생성 및 셋팅
            layersUI = new LayerUIItem[MaxLayersCount];
            for (var i = 0; i < MaxLayersCount; i++)
            {
                var layer = Instantiate(layerDefaultElement, verticalLayoutGroup.transform);
                layer.name = $"Layer_{MaxLayersCount - 1 - i}";
                layer.gameObject.SetActive(true);
                layersUI[MaxLayersCount - 1 - i] = layer;
            }
            foreach (var layerUIItem in layersUI)
            {
                layerUIItem.LayerDragItem.OnDragStarted = () => verticalLayoutGroup.enabled = false;
                layerUIItem.LayerDragItem.OnDragEnded = (layer, order) => StartCoroutine(OnDragEnded(layer, order));
            }
            layersPlateButton.onClick.AddListener(OnLayersPlateButtonClick);
        }

        private IEnumerator OnDragEnded(ILayer layer, int order)
        {
            verticalLayoutGroup.enabled = true;
            yield return null;
            layersController.SetLayerOrder(layer, MaxLayersCount - 1 - order);
            foreach (var layerUI in layersUI)
            {
                layerUI.LayerDragItem.RestoreSiblingIndex();
            }
            UpdateLayersUI();
        }

        private IEnumerator Start()
        {
            yield return null;
            contentSizeFitter.enabled = false;
            isContentSizeInitialized = true;
        }

        private void OnDestroy()
        {
            layersPlateButton.onClick.RemoveListener(OnLayersPlateButtonClick);
        }

        private void UnsubscribeEvents()
        {
            if (layersController != null)
            {
                layersController.OnCanRemoveLayer -= OnCanDeleteLayer;
                layersController.OnLayersCollectionChanged -= OnLayersCollectionChanged;
                layersController.OnActiveLayerSwitched -= OnActiveLayerSwitched;
                layersController.OnLayerChanged -= OnLayerChanged;
            }
        }

        private void OnEnable()
        {
            addLayerButton.onClick.AddListener(OnAddLayer);
            removeLayerButton.onClick.AddListener(OnRemoveLayer);
            mergeLayersButton.onClick.AddListener(OnMergeLayers);
            mergeAllLayersButton.onClick.AddListener(OnMergeAllLayers);
        }

        private void OnDisable()
        {
            addLayerButton.onClick.RemoveListener(OnAddLayer);
            removeLayerButton.onClick.RemoveListener(OnRemoveLayer);
            mergeLayersButton.onClick.RemoveListener(OnMergeLayers);
            mergeAllLayersButton.onClick.RemoveListener(OnMergeAllLayers);
        }

        public void SetLayersController(ILayersController currentLayersController)
        {
            UnsubscribeEvents();
            layersController = currentLayersController;
            layersController.OnCanRemoveLayer += OnCanDeleteLayer;
            layersController.OnLayersCollectionChanged += OnLayersCollectionChanged;
            layersController.OnActiveLayerSwitched += OnActiveLayerSwitched;
            layersController.OnLayerChanged += OnLayerChanged;
            UpdateLayersUI();
        }

        public void SetNextPlateState()
        {
            var isHidden = Math.Abs(layersTransform.anchoredPosition.x - layersTransform.rect.width) < 0.01f;
            var endPosition = new Vector2(isHidden ? 0 : layersTransform.rect.width, layersTransform.anchoredPosition.y);
            layersTransform.anchoredPosition = endPosition;
        }
        
        public void Show()
        {
            var endPosition = new Vector2(0, layersTransform.anchoredPosition.y);
            layersTransform.anchoredPosition = endPosition;
        }
        
        public void Hide()
        {
            var endPosition = new Vector2(layersTransform.rect.width, layersTransform.anchoredPosition.y);
            layersTransform.anchoredPosition = endPosition;
        }

        private void OnLayerChanged(ILayer layer)
        {
            foreach (var layerUI in layersUI)
            {
                if (!layerUI.gameObject.activeInHierarchy)
                    continue;
                
                layerUI.SetEnableInteractableState(layerUI.Layer.CanBeDisabled);
            }
        }

        private void UpdateLayersUI()
        {
            if (layersUI == null)
                return;
            
            if (isContentSizeInitialized)
            {
                contentSizeFitter.enabled = true;
            }
            
            for (var i = 0; i < layersUI.Length; i++)
            {
                var isActive = i < layersController.Layers.Count;
                layersUI[i].gameObject.SetActive(isActive);
                if (isActive)
                {
                    layersUI[i].SetLayer(layersController.Layers[i]);
                    layersUI[i].SetSelection(layer =>
                    {
                        layersController.SetActiveLayer(layer);
                        for (var j = 0; j < layersController.Layers.Count; j++)
                        {
                            var l = layersController.Layers[j];
                            if (l != layer)
                            {
                                layersUI[j].SetInactive();
                            }
                        }
                    });
                    
                    if (layersUI[i].Layer == layersController.ActiveLayer)
                    {
                        layersUI[i].SetActive();
                    }
                    else
                    {
                        layersUI[i].SetInactive();
                    }
                }
            }
            
            OnLayersUpdated?.Invoke();

            addLayerButton.interactable = layersController.Layers.Count < MaxLayersCount;
            removeLayerButton.interactable = layersController.CanRemoveLayer;
            if (isContentSizeInitialized)
            {
                StartCoroutine(DisableContentSizeFitter());
            }
        }

        private IEnumerator DisableContentSizeFitter()
        {
            yield return null;
            contentSizeFitter.SetLayoutVertical();
            yield return null;
            contentSizeFitter.enabled = false;
        }

        private void OnLayersPlateButtonClick()
        {
            if (isAnimating)
                return;
            
            StartCoroutine(AnimatePlateMoving());
        }

        private IEnumerator AnimatePlateMoving()
        {
            isAnimating = true;
            var isHidden = Math.Abs(layersTransform.anchoredPosition.x - layersTransform.rect.width *0.5f) < 0.01f;
            var startPosition = layersTransform.anchoredPosition;
            var endPosition = new Vector2(isHidden ? 0 : layersTransform.rect.width * 0.5f, layersTransform.anchoredPosition.y);
            var t = 0f;
            while (t <= 1f)
            {
                yield return null;
                t += Time.deltaTime / MovePlateDuration;
                var position = Vector2.Lerp(startPosition, endPosition, Mathf.SmoothStep(0f, 1f, t));
                layersTransform.anchoredPosition = position;
            }
            layersTransform.anchoredPosition = endPosition;
            isAnimating = false;
        }
        
        private void OnCanDeleteLayer(bool canDelete)
        {
            removeLayerButton.interactable = canDelete;
        }

        private void OnLayersCollectionChanged(ObservableCollection<ILayer> collection, NotifyCollectionChangedEventArgs args)
        {
            StartCoroutine(WaitOneFrameAction(UpdateLayersUI));
        }

        private IEnumerator WaitOneFrameAction(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private void OnActiveLayerSwitched(ILayer layer)
        {
            if (layersUI == null)
                return;
            
            foreach (var layerUI in layersUI)
            {
                if (!layerUI.gameObject.activeSelf)
                    continue;
                
                if (layerUI.Layer == layer)
                {
                    layerUI.SetActive();
                }
                else
                {
                    layerUI.SetInactive();
                }
            }
        }

        private void OnAddLayer()
        {
            layersController.AddNewLayer();
            UpdateLayersUI();
        }

        private void OnRemoveLayer()
        {
            layersController.RemoveActiveLayer();
            UpdateLayersUI();
        }

        private void OnMergeLayers()
        {
            layersController.MergeLayers();
            UpdateLayersUI();
        }

        private void OnMergeAllLayers()
        {
            layersController.MergeAllLayers();
            UpdateLayersUI();
        }
    }
}