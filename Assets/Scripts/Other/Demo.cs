using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Demo.UI;
using XDPaint.Tools;
using XDPaint.Tools.Image;
using XDPaint.Tools.Image.Base;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace XDPaint.Demo
{
    public class Demo : MonoBehaviour
    {
        [Serializable]
        public class PaintManagersData
        {
            public PaintManager PaintManager;
            public string Text;
        }
        
        [Serializable]
        public class ButtonPaintItem
        {
            public Image Image;
            public Button Button;
        }
        
        [Serializable]
        public class TogglePaintItem
        {
            public Image Image;
            public Toggle Toggle;
        }

        [Flags]
        public enum PanelType
        {
            None = 0,
            ColorPalette = 1,
            Brushes = 2,
            Bucket = 4,
            Smoothing = 8
        }

        [SerializeField] private PaintManagersData[] paintManagers;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private bool loadPrefs = true;
        
        [Header("Top panel")]
        [SerializeField] private ToolToggle[] toolsToggles; 
        [SerializeField] private Button brushClick;
        [SerializeField] private UIDoubleClick brushToolDoubleClick;
        [SerializeField] private UIDoubleClick eraseToolDoubleClick;
        [SerializeField] private UIDoubleClick bucketToolDoubleClick;
        [SerializeField] private UIDoubleClick eyedropperToolDoubleClick;
        [SerializeField] private RawImage brushPreview;
        [SerializeField] private RectTransform brushPreviewTransform;
        [SerializeField] private EventTrigger topPanel;
        [SerializeField] private EventTrigger colorPanel;
        [SerializeField] private EventTrigger brushesPanel;
        [SerializeField] private EventTrigger bucketPanel;
        [SerializeField] private Slider bucketSlider;
        [SerializeField] private EventTrigger lineSmoothingPanel;
        [SerializeField] private Slider lineSmoothingSlider;
        [SerializeField] private ButtonPaintItem[] colors;
        [SerializeField] private TogglePaintItem[] brushes;
        [SerializeField] private RectTransform toolSettingsPalette;
        [SerializeField] private VerticalLayoutGroup toolSettingsLayoutGroup;

        [Header("Left panel")]
        [SerializeField] private Slider opacitySlider;
        [SerializeField] private Slider brushSizeSlider;
        [SerializeField] private Slider hardnessSlider;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private EventTrigger rightPanel;

        [Header("Right panel")]
        [SerializeField] private LayersUIController layersUI;

        [SerializeField] private EventTrigger allArea;
        [SerializeField] private EventTrigger uiLocker;
        
        private EventTrigger.Entry hoverEnter;
        private EventTrigger.Entry hoverExit;
        private EventTrigger.Entry onDown;
        private PaintManager PaintManager => paintManagers[currentPaintManagerId].PaintManager;
        private Texture selectedBrushTexture;
        private Animator paintManagerAnimator;
        private PaintTool previousTool;
        private Vector3 defaultPalettePosition;
        private int currentPaintManagerId;
        private bool previousCameraMoverState;
        
        private const int TutorialShowCount = 5;

        void Awake()
        {
#if XDP_DEBUG
            Application.runInBackground = false;
#endif
#if !UNITY_WEBGL
            Application.targetFrameRate = Mathf.Clamp(Screen.currentResolution.refreshRate, 30, Screen.currentResolution.refreshRate);
#endif
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            selectedBrushTexture = Settings.Instance.DefaultBrush;
            PreparePaintManagers();

            for (var i = 0; i < paintManagers.Length; i++)
            {
                var manager = paintManagers[i];
                var active = i == 0;
                manager.PaintManager.gameObject.SetActive(active);
            }

            PaintManager.OnInitialized += OnInitialized;
                
        }

        private IEnumerator Start()
        {
            yield return null;

            defaultPalettePosition = toolSettingsPalette.position;
            hoverEnter = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            hoverEnter.callback.AddListener(HoverEnter);
            hoverExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            hoverExit.callback.AddListener(HoverExit);
            
            //top panel
            brushToolDoubleClick.OnDoubleClick.AddListener(OpenBrushPanel);
            eraseToolDoubleClick.OnDoubleClick.AddListener(OpenErasePanel);
            bucketToolDoubleClick.OnDoubleClick.AddListener(OpenBucketPanel);
            eyedropperToolDoubleClick.OnDoubleClick.AddListener(ClosePanels);
            brushClick.onClick.AddListener(() => OpenColorPalette(brushClick.transform.position));
            topPanel.triggers.Add(hoverEnter);
            topPanel.triggers.Add(hoverExit);
            colorPanel.triggers.Add(hoverEnter);
            colorPanel.triggers.Add(hoverExit);
            brushesPanel.triggers.Add(hoverEnter);
            brushesPanel.triggers.Add(hoverExit);
            bucketPanel.triggers.Add(hoverEnter);
            bucketPanel.triggers.Add(hoverExit);
            bucketSlider.onValueChanged.AddListener(OnBucketSlider);
            lineSmoothingPanel.triggers.Add(hoverEnter);
            lineSmoothingPanel.triggers.Add(hoverExit);
            lineSmoothingSlider.onValueChanged.AddListener(OnLineSmoothingSlider);

            brushSizeSlider.value = PaintController.Instance.Brush.Size;
            hardnessSlider.value = PaintController.Instance.Brush.Hardness;
            opacitySlider.value = PaintController.Instance.Brush.Color.a;

            //right panel
            opacitySlider.onValueChanged.AddListener(OnOpacitySlider);
            brushSizeSlider.onValueChanged.AddListener(OnBrushSizeSlider);
            hardnessSlider.onValueChanged.AddListener(OnHardnessSlider);
            undoButton.onClick.AddListener(OnUndo);
            redoButton.onClick.AddListener(OnRedo);
            rightPanel.triggers.Add(hoverEnter);
            rightPanel.triggers.Add(hoverExit);
            
            
            onDown = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};
            onDown.callback.AddListener(ResetPlates);
            allArea.triggers.Add(onDown);
            uiLocker.triggers.Add(onDown);
            uiLocker.transform.SetParent(colorPanel.transform.parent);
            uiLocker.transform.SetSiblingIndex(colorPanel.transform.GetSiblingIndex());

            //colors
            foreach (var colorItem in colors)
            {
                colorItem.Button.onClick.AddListener(delegate { ColorClick(colorItem.Image.color); });
            }
            
            //brushes
            for (var i = 0; i < brushes.Length; i++)
            {
                var brushItem = brushes[i];
                var brushId = i;
                brushItem.Toggle.onValueChanged.AddListener(delegate(bool isOn) { BrushClick(isOn, brushItem, brushId); });
            }
            

            foreach (var toggle in toolsToggles)
            {
                toggle.Toggle.enabled = true;
            }
            
            if (loadPrefs)
            {
                LoadPrefs();
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    OpenToolSettings(Mouse.current.position.ReadValue());
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(1))
            {
                OpenToolSettings(Input.mousePosition);
            }
#endif
        }

        private void OnDestroy()
        {
            hoverEnter?.callback.RemoveListener(HoverEnter);
            hoverExit?.callback.RemoveListener(HoverExit);
            brushToolDoubleClick.OnDoubleClick.RemoveListener(OpenBrushPanel);
            eraseToolDoubleClick.OnDoubleClick.RemoveListener(OpenErasePanel);
            bucketToolDoubleClick.OnDoubleClick.RemoveListener(OpenBucketPanel);
            eyedropperToolDoubleClick.OnDoubleClick.RemoveListener(ClosePanels);
            brushClick.onClick.RemoveListener(() => OpenColorPalette(brushClick.transform.position));
            topPanel.triggers.Remove(hoverEnter);
            topPanel.triggers.Remove(hoverExit);
            colorPanel.triggers.Remove(hoverEnter);
            colorPanel.triggers.Remove(hoverExit);
            brushesPanel.triggers.Remove(hoverEnter);
            brushesPanel.triggers.Remove(hoverExit);
            bucketPanel.triggers.Remove(hoverEnter);
            bucketPanel.triggers.Remove(hoverExit);
            bucketSlider.onValueChanged.RemoveListener(OnBucketSlider);
            lineSmoothingPanel.triggers.Remove(hoverEnter);
            lineSmoothingPanel.triggers.Remove(hoverExit);
            lineSmoothingSlider.onValueChanged.RemoveListener(OnLineSmoothingSlider);
            opacitySlider.onValueChanged.RemoveListener(OnOpacitySlider);
            brushSizeSlider.onValueChanged.RemoveListener(OnBrushSizeSlider);
            hardnessSlider.onValueChanged.RemoveListener(OnHardnessSlider);
            undoButton.onClick.RemoveListener(OnUndo);
            redoButton.onClick.RemoveListener(OnRedo);
            rightPanel.triggers.Remove(hoverEnter);
            rightPanel.triggers.Remove(hoverExit);
            onDown?.callback.RemoveListener(ResetPlates);
            allArea.triggers.Remove(onDown);
            foreach (var colorItem in colors)
            {
                colorItem.Button.onClick.RemoveListener(delegate { ColorClick(colorItem.Image.color); });
            }
            
            for (var i = 0; i < brushes.Length; i++)
            {
                var brushItem = brushes[i];
                var brushId = i;
                brushItem.Toggle.onValueChanged.RemoveListener(delegate(bool isOn) { BrushClick(isOn, brushItem, brushId); });
            }
        }

        public void DisableStates()
        {
            if (PaintManager != null && PaintManager.StatesController != null && PaintManager.Initialized)
            {
                PaintManager.StatesController.Disable();
            }
        }

        public void EnableStates()
        {
            if (PaintManager != null && PaintManager.StatesController != null && PaintManager.Initialized)
            {
                PaintManager.StatesController.Enable();
            }
        }

        private void OnInitialized(PaintManager paintManagerInstance)
        {
            //undo/redo status
            if (paintManagerInstance.StatesController != null)
            {
                paintManagerInstance.StatesController.OnUndoStatusChanged += OnUndoStatusChanged;
                paintManagerInstance.StatesController.OnRedoStatusChanged += OnRedoStatusChanged;
            }
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.OnColorChanged += OnBrushColorChanged;
            }
            else
            {
                PaintManager.Brush.OnColorChanged += OnBrushColorChanged;
            }
            brushPreview.texture = PaintController.Instance.UseSharedSettings 
                ? PaintController.Instance.Brush.RenderTexture 
                : PaintManager.Brush.RenderTexture;

            foreach (var toolToggle in toolsToggles)
            {
                toolToggle.SetPaintManager(paintManagerInstance);
            }
            
            layersUI.OnLayersUpdated -= OnLayersUIUpdated;
            layersUI.OnLayersUpdated += OnLayersUIUpdated;
            layersUI.SetLayersController(paintManagerInstance.LayersController);
            
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            PaintController.Instance.Brush.Preview = false;
#endif
        }

        private void OnLayersUIUpdated()
        {
            foreach (var layerUI in layersUI.LayerUI)
            {
                layerUI.OpacityHelper.OnDown -= OnOpacityHelperDown;
                layerUI.OpacityHelper.OnDown += OnOpacityHelperDown;
                layerUI.OpacityHelper.OnUp -= OnOpacityHelperUp;
                layerUI.OpacityHelper.OnUp += OnOpacityHelperUp;
            }
        }

        private void OnOpacityHelperDown(PointerEventData pointerEventData)
        {
            if (PaintManager != null && PaintManager.StatesController != null && PaintManager.Initialized)
            {
                PaintManager.StatesController.EnableGrouping();
            }
        }
        
        private void OnOpacityHelperUp(PointerEventData pointerEventData)
        {
            if (PaintManager != null && PaintManager.StatesController != null && PaintManager.Initialized)
            {
                PaintManager.StatesController.DisableGrouping();
            }
        }

        private void OnBrushColorChanged(Color color)
        {
            opacitySlider.value = color.a;
        }

        private void LoadPrefs()
        {
            //brush id
            var brushId = PlayerPrefs.GetInt("XDPaintDemoBrushId");
            PaintManager.Brush.SetTexture(brushes[brushId].Image.mainTexture, true, false);
            brushes[brushId].Toggle.isOn = true;
            selectedBrushTexture = brushes[brushId].Image.mainTexture;
            //opacity
            opacitySlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushOpacity", 1f);
            //size
            brushSizeSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushSize", 1f);
            //hardness
            hardnessSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushHardness", 1f);
            //color
            ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("XDPaintDemoBrushColor", "#FFFFFF"), out var color);
            ColorClick(color);
            var brushTool = toolsToggles.First(x => x.Tool == PaintTool.Brush);
            brushTool.Toggle.isOn = true;
        }


        private void PreparePaintManagers()
        {
            for (var i = 0; i < paintManagers.Length; i++)
            {
                paintManagers[i].PaintManager.gameObject.SetActive(i == currentPaintManagerId);
                if (paintManagerAnimator == null)
                {
                    if (paintManagers[i].PaintManager.ObjectForPainting.TryGetComponent<SkinnedMeshRenderer>(out _))
                    {
                        var animator = paintManagers[i].PaintManager.GetComponentInChildren<Animator>(true);
                        if (animator != null)
                        {
                            paintManagerAnimator = animator;
                        }
                    }
                }
            }
        }

        private void OpenToolSettings(Vector3 position)
        {
            var toolType = PaintManager.ToolsManager.CurrentTool.Type;
            var panelType = PanelType.None;
            if (toolType == PaintTool.Brush)
            {
                panelType = PanelType.Brushes | PanelType.Smoothing;
            }

            if (toolType == PaintTool.Erase)
            {
                panelType = PanelType.Brushes | PanelType.Smoothing;
            }

            if (toolType == PaintTool.Bucket)
            {
                panelType = PanelType.Bucket;
            }

            
            UpdatePanels(panelType);
            toolSettingsLayoutGroup.padding.top = 0;
            LayoutRebuilder.ForceRebuildLayoutImmediate(toolSettingsPalette);
            var palettePosition = toolSettingsPalette.position;
            palettePosition = new Vector3(position.x, position.y, palettePosition.z);
            toolSettingsPalette.position = palettePosition;
        }

        private void UpdatePanels(PanelType panelType)
        {
            uiLocker.gameObject.SetActive(panelType != PanelType.None);
            var lineSmoothingEnabled = (panelType & PanelType.Smoothing) != 0;
            lineSmoothingPanel.gameObject.SetActive(lineSmoothingEnabled && PaintManager.PaintObject.CanSmoothLines);
            
            var colorsEnabled = (panelType & PanelType.ColorPalette) != 0;
            colorPanel.gameObject.SetActive(colorsEnabled);

            var brushesEnabled = (panelType & PanelType.Brushes) != 0;
            brushesPanel.gameObject.SetActive(brushesEnabled);
            
            var bucketEnabled = (panelType & PanelType.Bucket) != 0;
            bucketPanel.gameObject.SetActive(bucketEnabled);
            
        }

        private void UpdatePanels(PanelType panelType, Vector3 position)
        {
            uiLocker.gameObject.SetActive(true);
            UpdatePanels(panelType);
            toolSettingsLayoutGroup.padding.top = 70;
            LayoutRebuilder.ForceRebuildLayoutImmediate(toolSettingsPalette);
            var palettePosition = toolSettingsPalette.position;
            palettePosition = new Vector3(position.x, defaultPalettePosition.y, palettePosition.z);
            toolSettingsPalette.position = palettePosition;
        }
        
        private void OpenColorPalette(Vector3 position)
        {
            UpdatePanels(PanelType.ColorPalette, position);
        }

        private void OpenBrushPanel(Vector3 position)
        {
            UpdatePanels(PanelType.Brushes | PanelType.Smoothing, position);
        }
        
        private void OpenErasePanel(Vector3 position)
        {
            UpdatePanels(PanelType.Brushes | PanelType.Smoothing, position);
        }
        
        private void OpenBucketPanel(Vector3 position)
        {
            UpdatePanels(PanelType.Bucket , position);
        }

        private void ClosePanels(Vector3 position)
        {
            UpdatePanels(PanelType.None);
        }


        private void OnOpacitySlider(float value)
        {
            var color = Color.white;
            if (PaintController.Instance.UseSharedSettings)
            {
                color = PaintController.Instance.Brush.Color;
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                color = PaintManager.Brush.Color;
            }
            color.a = value;
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.SetColor(color);
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                PaintManager.Brush.SetColor(color);
            }
            PlayerPrefs.SetFloat("XDPaintDemoBrushOpacity", value);
        }
        
        private void OnBrushSizeSlider(float value)
        {
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.Size = value;
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                PaintManager.Brush.Size = value;
            }
            brushPreviewTransform.localScale = Vector3.one * value;
            PlayerPrefs.SetFloat("XDPaintDemoBrushSize", value);
        }

        private void OnHardnessSlider(float value)
        {
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.Hardness = value;
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                PaintManager.Brush.Hardness = value;
            }
            PlayerPrefs.SetFloat("XDPaintDemoBrushHardness", value);
        }
        
        private void OnBucketSlider(float value)
        {
            if (PaintManager.ToolsManager.CurrentTool is BucketTool bucketTool)
            {
                bucketTool.Settings.Tolerance = value;
            }
        }

        private void OnLineSmoothingSlider(float value)
        {
            var tool = PaintManager.ToolsManager.CurrentTool;
            tool.BaseSettings.Smoothing = (int)value;
        }
        
        private void OnUndo()
        {
            if (PaintManager.StatesController != null && PaintManager.StatesController.CanUndo())
            {
                PaintManager.StatesController.Undo();
                PaintManager.Render();
            }
        }
        
        private void OnRedo()
        {
            if (PaintManager.StatesController != null && PaintManager.StatesController.CanRedo())
            {
                PaintManager.StatesController.Redo();
                PaintManager.Render();
            }
        }

        private void SwitchToNextPaintManager()
        {
            SwitchPaintManager(true);
        }

        private void SwitchToPreviousPaintManager()
        {
            SwitchPaintManager(false);
        }
        
        private void SwitchPaintManager(bool switchToNext)
        {
            PaintManager.gameObject.SetActive(false);
            if (PaintManager.StatesController != null)
            {
                PaintManager.StatesController.OnUndoStatusChanged -= OnUndoStatusChanged;
                PaintManager.StatesController.OnRedoStatusChanged -= OnRedoStatusChanged;
            }
            
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.OnColorChanged -= OnBrushColorChanged;
            }
            else
            {
                PaintManager.Brush.OnColorChanged -= OnBrushColorChanged;
            }
            
            foreach (var layerUI in layersUI.LayerUI)
            {
                layerUI.OpacityHelper.OnDown -= OnOpacityHelperDown;
                layerUI.OpacityHelper.OnUp -= OnOpacityHelperUp;
            }
            layersUI.OnLayersUpdated -= OnLayersUIUpdated;
            PaintManager.DoDispose();
            if (switchToNext)
            {
                currentPaintManagerId = (currentPaintManagerId + 1) % paintManagers.Length;
            }
            else
            {
                currentPaintManagerId--;
                if (currentPaintManagerId < 0)
                {
                    currentPaintManagerId = paintManagers.Length - 1;
                }
            }
            
            toolsToggles.First(x => x.Tool == PaintTool.Brush).Toggle.isOn = true;
            PaintManager.gameObject.SetActive(true);
            PaintManager.OnInitialized -= OnInitialized;
            PaintManager.OnInitialized += OnInitialized;
            PaintManager.Init();
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Tool = PaintTool.Brush;
            }
            else
            {
                PaintManager.Tool = PaintTool.Brush;
            }

            PaintManager.Brush.SetTexture(selectedBrushTexture);
            UpdateButtons();
        }

        private void OnRedoStatusChanged(bool canRedo)
        {
            redoButton.interactable = canRedo;
        }

        private void OnUndoStatusChanged(bool canUndo)
        {
            undoButton.interactable = canUndo;
        }

        private void UpdateButtons()
        {
            var hasSkinnedMeshRenderer = PaintManager.ObjectForPainting.TryGetComponent<SkinnedMeshRenderer>(out _);

            if (paintManagerAnimator != null)
            {
                paintManagerAnimator.enabled = hasSkinnedMeshRenderer;
            }
            
        }
        
        private void HoverEnter(BaseEventData data)
        {
            if (!PaintManager.Initialized)
                return;
            
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.mousePresent)
#endif
            {
                PaintManager.PaintObject.ProcessInput = false;
            }
            PaintManager.PaintObject.FinishPainting();
        }
        
        private void HoverExit(BaseEventData data)
        {
            if (!PaintManager.Initialized)
                return;
            
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.mousePresent)
#endif
            {
                PaintManager.PaintObject.ProcessInput = true;
            }
        }
        
        private void ColorClick(Color color)
        {
            var brushColor = Color.white;
            if (PaintController.Instance.UseSharedSettings)
            {
                brushColor = PaintController.Instance.Brush.Color;
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                brushColor = PaintManager.Brush.Color;
            }
            
            brushColor = new Color(color.r, color.g, color.b, brushColor.a);
            if (PaintController.Instance.UseSharedSettings)
            {
                PaintController.Instance.Brush.SetColor(brushColor);
            }
            else if (PaintManager != null && PaintManager.Initialized)
            {
                PaintManager.Brush.SetColor(brushColor);
            }
            
            var selectedTool = PaintController.Instance.UseSharedSettings ? PaintController.Instance.Tool : PaintManager.Tool;
            if (selectedTool != PaintTool.Brush && selectedTool != PaintTool.Bucket)
            {
                foreach (var toolToggle in toolsToggles)
                {
                    if (toolToggle.Tool == PaintTool.Brush)
                    {
                        toolToggle.Toggle.isOn = true;
                        break;
                    }
                }
            }
            
            var colorString = ColorUtility.ToHtmlStringRGB(brushColor);
            PlayerPrefs.SetString("XDPaintDemoBrushColor", colorString);
        }

        private void BrushClick(bool isOn, TogglePaintItem item, int brushId)
        {
            if (!isOn) 
                return;
            
            Texture texture = null;
            if (item.Image.sprite != null)
            {
                texture = item.Image.sprite.texture;
            }
            PaintManager.Brush.SetTexture(texture, true, false);
            selectedBrushTexture = texture;
            PlayerPrefs.SetInt("XDPaintDemoBrushId", brushId);
        }

        private void ResetPlates(BaseEventData data)
        {
            UpdatePanels(PanelType.None);
            HoverExit(null);
        }
    }
}