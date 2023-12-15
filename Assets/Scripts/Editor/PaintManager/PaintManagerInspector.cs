using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Editor.Utils;
using XDPaint.Tools;
using XDPaint.Tools.Layers;

namespace XDPaint.Editor
{
    [CustomEditor(typeof(PaintManager))]
    public class PaintManagerInspector : UnityEditor.Editor
    {
        private SerializedProperty objectForPaintingProperty;
        private SerializedProperty paintMaterialProperty;
        private SerializedProperty shaderTextureNameProperty;
        private SerializedProperty paintModeProperty;
        private SerializedProperty filterModeProperty;
        private SerializedProperty defaultTextureWidth;
        private SerializedProperty defaultTextureHeight;
        private SerializedProperty defaultTextureColor;
        private SerializedProperty subMeshIndexProperty;
        private SerializedProperty uvChannelProperty;
        private SerializedProperty copySourceTextureToLayerProperty;
        private SerializedProperty useSourceTextureAsBackgroundProperty;
        private SerializedProperty paintToolProperty;
        private SerializedProperty brushProperty;
        private SerializedProperty toolsManagerProperty;
        private SerializedProperty layersControllerProperty;
        private SerializedProperty layersContainerProperty;
        private PaintManager paintManager;
        private Component component;

        private int shaderTextureNameSelectedId;
        private int subMeshSelectedValue;
        private int[] subMeshValues;
        private string[] subMeshStrings;
        private int uvChannelSelectedValue;
        private int[] uvChannels;
        private string[] uvChannelsStrings;
        private bool isMeshObject;
        private bool objectForPaintChanged;
        private int selectedPresetIndex;
        private bool shouldCheckTexture = true;
        private bool sortPresetsByName = true;
        private bool hasTexture;
        private PaintTool tool = PaintTool.Brush;
        private EnumDrawer<PaintTool> paintTool;
        private EnumDrawer<PaintTool> paintToolDrawer
        {
            get
            {
                if (paintTool == null)
                {
                    paintTool = new EnumDrawer<PaintTool>();
                    paintTool.Init();
                }
                return paintTool;
            }
        }

        private EnumDrawer<PaintMode> paintMode;
        private EnumDrawer<PaintMode> paintModeDrawer
        {
            get
            {
                if (paintMode == null)
                {
                    paintMode = new EnumDrawer<PaintMode>();
                    paintMode.Init();
                }
                return paintMode;
            }
        }
        
        private EnumDrawer<FilterMode> filterMode;
        private EnumDrawer<FilterMode> filterModeDrawer
        {
            get
            {
                if (filterMode == null)
                {
                    filterMode = new EnumDrawer<FilterMode>();
                    filterMode.Init();
                }
                return filterMode;
            }
        }

        private bool showWarning;
        private bool allowSavePresetsInRuntime = false;
        private string savedName;
        private bool rename;
        private bool showDialogName;

        #region Menu Items
        
        [MenuItem("GameObject/2D\u22153D Paint", false, 32)]
        static void AddPaintManagerObject()
        {
            var gameObject = new GameObject("2D/3D Paint");
            gameObject.AddComponent<PaintManager>();
            Selection.activeObject = gameObject.gameObject;
        }

        [MenuItem("Component/2D\u22153D Paint")]
        static void AddPaintManagerComponent()
        {
            if (Selection.activeGameObject != null && !Selection.activeGameObject.TryGetComponent<PaintManager>(out _))
            {
                Selection.activeGameObject.AddComponent<PaintManager>();
            }
        }

        #endregion

        void OnEnable()
        {
            paintManager = (PaintManager)target;
            objectForPaintingProperty = serializedObject.FindProperty("objectForPainting");
            paintMaterialProperty = serializedObject.FindProperty("material.SourceMaterial");
            shaderTextureNameProperty = serializedObject.FindProperty("material.shaderTextureName");
            paintModeProperty = serializedObject.FindProperty("paintModeType");
            filterModeProperty = serializedObject.FindProperty("filterMode");
            defaultTextureWidth = serializedObject.FindProperty("material.defaultTextureWidth");
            defaultTextureHeight = serializedObject.FindProperty("material.defaultTextureHeight");
            defaultTextureColor = serializedObject.FindProperty("material.defaultTextureColor");
            subMeshIndexProperty = serializedObject.FindProperty("subMesh");
            uvChannelProperty = serializedObject.FindProperty("uvChannel");
            copySourceTextureToLayerProperty = serializedObject.FindProperty("copySourceTextureToLayer");
            useSourceTextureAsBackgroundProperty = serializedObject.FindProperty("useSourceTextureAsBackground");
            paintToolProperty = serializedObject.FindProperty("paintTool");
            brushProperty = serializedObject.FindProperty("brush");
            toolsManagerProperty = serializedObject.FindProperty("toolsManager");
            layersControllerProperty = serializedObject.FindProperty("layersController");
            layersContainerProperty = serializedObject.FindProperty("layersContainer");
            UpdateTexturesList();
            UpdateMeshData();
        }

        private void UpdateTexturesList()
        {
            var material = paintMaterialProperty.objectReferenceValue as Material;
            if (material != null)
            {
                var shaderTextureNames = PaintManagerHelper.GetTexturesListFromShader(material);
                shaderTextureNameSelectedId = Array.IndexOf(shaderTextureNames, shaderTextureNameProperty.stringValue);
            }
            if (paintManager.Material.SourceMaterial == null && material != null)
            {
                paintManager.Material.SourceMaterial = material;
            }
        }

        private void UpdateMeshData()
        {
            component = PaintManagerHelper.GetSupportedComponent(objectForPaintingProperty.objectReferenceValue as GameObject);
            isMeshObject = PaintManagerHelper.IsMeshObject(component);
            if (isMeshObject)
            {
                paintManager = (PaintManager)target;
                subMeshSelectedValue = paintManager.SubMesh;
                subMeshValues = PaintManagerHelper.GetSubMeshes(component);
                subMeshStrings = subMeshValues.Select(x => x.ToString()).ToArray();
                uvChannelSelectedValue = paintManager.UVChannel;
                uvChannels = PaintManagerHelper.GetUVChannels(component);
                uvChannelsStrings = uvChannels.Select(x => x.ToString()).ToArray();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(objectForPaintingProperty, new GUIContent("Object For Painting", PaintManagerHelper.ObjectForPaintingTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                objectForPaintChanged = true;
                shouldCheckTexture = true;
            }
            
            if (objectForPaintingProperty.objectReferenceValue != null)
            {
                paintManager = (PaintManager)target;
                component = PaintManagerHelper.GetSupportedComponent(objectForPaintingProperty.objectReferenceValue as GameObject);
                isMeshObject = PaintManagerHelper.IsMeshObject(component);
                if (isMeshObject && objectForPaintChanged)
                {
                    UpdateModelSettings();
                    objectForPaintChanged = false;
                    UpdateMeshData();
                    MarkAsDirty();
                }
                DrawMaterialTextureBlock();
                if (paintMaterialProperty.objectReferenceValue != null)
                {
                    DrawCheckboxesBlock();
                    DrawToolsBlock();
                    if (Settings.Instance != null)
                    {
                        DrawPresetsBlock();
                    }

                    DrawLayersBlock();
                    DrawButtonsBlock();
                }
            }
            DrawAutoFillButton();
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawAutoFillButton()
        {
            var disabled = objectForPaintingProperty.objectReferenceValue == null || paintMaterialProperty.objectReferenceValue == null;
            EditorGUI.BeginDisabledGroup(!disabled);
            if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(disabled)))
            {
                if (GUILayout.Button(new GUIContent("Auto Fill", PaintManagerHelper.AutoFillButtonTooltip), GUILayout.ExpandWidth(true)))
                {
                    var objectForPaintingFillResult = FindObjectForPainting();
                    var findMaterialResult = FindMaterial();
                    if (!objectForPaintingFillResult && !findMaterialResult)
                    {
                        Debug.LogWarning("Can't find ObjectForPainting and Material.");
                    }
                    else if (!objectForPaintingFillResult)
                    {
                        Debug.LogWarning("Can't find ObjectForPainting.");
                    }
                    else if (!findMaterialResult)
                    {
                        Debug.LogWarning("Can't find Material.");
                    }
                    else
                    {
                        MarkAsDirty();
                    }
                    UpdateModelSettings();
                    UpdateMeshData();
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUI.EndDisabledGroup();
        }

        private bool FindObjectForPainting()
        {
            if (objectForPaintingProperty.objectReferenceValue == null)
            {
                var supportedComponent = PaintManagerHelper.GetSupportedComponent(paintManager.gameObject);
                if (supportedComponent != null)
                {
                    objectForPaintingProperty.objectReferenceValue = supportedComponent.gameObject;
                    return true;
                }
                if (paintManager.gameObject.transform.childCount > 0)
                {
                    var compatibleComponents = new List<Component>();
                    var allComponents = paintManager.gameObject.transform.GetComponentsInChildren<Component>();
                    foreach (var currentComponent in allComponents)
                    {
                        var childComponent = PaintManagerHelper.GetSupportedComponent(currentComponent.gameObject);
                        if (childComponent != null)
                        {
                            compatibleComponents.Add(childComponent);
                            break;
                        }
                    }
                    if (compatibleComponents.Count > 0)
                    {
                        objectForPaintingProperty.objectReferenceValue = compatibleComponents[0].gameObject;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        private bool FindMaterial()
        {
            var result = false;
            component = PaintManagerHelper.GetSupportedComponent(objectForPaintingProperty.objectReferenceValue as GameObject);
            if (component != null)
            {
                var renderer = component as Renderer;
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    paintMaterialProperty.objectReferenceValue = renderer.sharedMaterial;
                    result = true;
                }
                var maskableGraphic = component as RawImage;
                if (maskableGraphic != null && maskableGraphic.material != null)
                {
                    paintMaterialProperty.objectReferenceValue = maskableGraphic.material;
                    result = true;
                }
            }
            UpdateTexturesList();
            return result;
        }

        private void DrawMaterialTextureBlock()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(paintMaterialProperty, new GUIContent("Material", PaintManagerHelper.MaterialTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                UpdateTexturesList();
                shouldCheckTexture = true;
            }
            if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(paintMaterialProperty.objectReferenceValue != null)))
            {
                var shaderTextureNames = PaintManagerHelper.GetTexturesListFromShader(paintMaterialProperty.objectReferenceValue as Material);
                var shaderTextureNamesContent = new GUIContent[shaderTextureNames.Length];
                for (var i = 0; i < shaderTextureNamesContent.Length; i++)
                {
                    shaderTextureNamesContent[i] = new GUIContent(shaderTextureNames[i]);
                }

                var shaderTextureName = paintManager.Material.ShaderTextureName;
                if (shaderTextureNames.Contains(shaderTextureName))
                {
                    for (var i = 0; i < shaderTextureNames.Length; i++)
                    {
                        if (shaderTextureNames[i] == shaderTextureName)
                        {
                            shaderTextureNameSelectedId = i;
                            break;
                        }
                    }
                }
                
                shaderTextureNameSelectedId = Mathf.Clamp(shaderTextureNameSelectedId, 0, int.MaxValue);
                EditorGUI.BeginChangeCheck();
                shaderTextureNameSelectedId = EditorGUILayout.Popup(new GUIContent("Shader Texture Name", PaintManagerHelper.ShaderTextureNameTooltip), shaderTextureNameSelectedId, shaderTextureNamesContent);
                if (EditorGUI.EndChangeCheck())
                {
                    shouldCheckTexture = true;
                }

                if (shaderTextureNames.Length > 0 && shaderTextureNames.Length > shaderTextureNameSelectedId && shaderTextureNames[shaderTextureNameSelectedId] != shaderTextureName)
                {
                    shaderTextureNameProperty.stringValue = shaderTextureNames[shaderTextureNameSelectedId];
                    paintManager.Material.ShaderTextureName = shaderTextureNameProperty.stringValue;
                    MarkAsDirty();
                }
                
                if ((!hasTexture || shouldCheckTexture) && !Application.isPlaying)
                {
                    if (shouldCheckTexture)
                    {
                        serializedObject.ApplyModifiedProperties();
                        hasTexture = PaintManagerHelper.HasTexture(paintManager);
                        shouldCheckTexture = false;
                    }
                    if (!hasTexture)
                    {
                        EditorGUILayout.HelpBox("Object does not have source texture, new texture will be created. Please specify the texture size and color", MessageType.Warning);
                        EditorGUILayout.PropertyField(defaultTextureWidth, new GUIContent("Texture Width", PaintManagerHelper.TextureSizeTip));
                        EditorGUILayout.PropertyField(defaultTextureHeight, new GUIContent("Texture Height", PaintManagerHelper.TextureSizeTip));
                        EditorGUILayout.PropertyField(defaultTextureColor, new GUIContent("Texture Color", PaintManagerHelper.TextureColorTip));
                        
                        var filter = FilterMode.Point;
                        var filterModeChanged = filterModeDrawer.Draw(filterModeProperty, "Filter Mode", PaintManagerHelper.FilteringModeTooltip, ref filter);
                        if (filterModeChanged)
                        {
                            paintManager.FilterMode = filter;
                            MarkAsDirty();
                        }
                        
                        EditorHelper.DrawHorizontalLine();
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawPresetsBlock()
        {
            if (PaintController.Instance == null)
                return;
            
            EditorHelper.DrawHorizontalLine();
            //getting all presets
            var additionalPresetsCount = 2;
            var options = new string[BrushPresets.Instance.Presets.Count + additionalPresetsCount];
            options[0] = BrushDrawerHelper.CustomPresetName;
            options[1] = BrushDrawerHelper.DefaultPresetName;
            var unnamedPresetsCount = 0;
            for (var i = additionalPresetsCount; i < options.Length; i++)
            {
                var preset = BrushPresets.Instance.Presets[i - additionalPresetsCount];
                if (preset != null)
                {
                    if (string.IsNullOrEmpty(preset.Name))
                    {
                        unnamedPresetsCount++;
                    }
                    options[i] = string.IsNullOrEmpty(preset.Name) ? "Unnamed preset " + unnamedPresetsCount : " [" + (i - 1) + "] " + preset.Name;
                }
            }
            
            //update selected index
            for (var i = 0; i < BrushPresets.Instance.Presets.Count; i++)
            {
                if (paintManager.Brush != null && string.IsNullOrEmpty(paintManager.Brush.Name))
                {
                    paintManager.Brush.Name = Guid.NewGuid().ToString("N");
                }
                if (paintManager.Brush != null &&
                    !string.IsNullOrEmpty(paintManager.Brush.Name) && 
                    paintManager.Brush.Name != BrushDrawerHelper.CustomPresetName && 
                    BrushPresets.Instance.Presets[i].Name == paintManager.Brush.Name)
                {
                    selectedPresetIndex = i + additionalPresetsCount;
                    break;
                }
            }
            
            //preset popup
            EditorGUI.BeginDisabledGroup((Application.isPlaying && PaintController.Instance.UseSharedSettings) || showDialogName);
            EditorGUI.BeginChangeCheck();
            selectedPresetIndex = EditorGUILayout.Popup("Brush", selectedPresetIndex, options);
            var presetChanged = EditorGUI.EndChangeCheck();
            EditorGUI.EndDisabledGroup();

            if (selectedPresetIndex == 0 && paintManager.Initialized)
            {
                paintManager.Brush.Name = BrushDrawerHelper.CustomPresetName;
            }

            if (presetChanged)
            {
                Undo.RecordObjects(targets, "Brush Preset Update");
                foreach (var script in targets)
                {
                    var targetPaintManager = script as PaintManager;
                    if (targetPaintManager != null)
                    {
                        if (selectedPresetIndex == 0)
                        {
                            if (Application.isPlaying)
                            {
                                PaintController.Instance.UseSharedSettings = false;
                                targetPaintManager.InitBrush();
                            }
                        }
                        else if (selectedPresetIndex == 1)
                        {
                            if (Application.isPlaying)
                            {
                                targetPaintManager.Brush = PaintController.Instance.Brush;
                            }
                            else
                            {
                                targetPaintManager.Brush = new Brush((Brush)targetPaintManager.Brush)
                                {
                                    Name = savedName
                                };
                            }
                        }
                        else
                        {
                            if (Application.isPlaying)
                            {
                                targetPaintManager.InitBrush();
                                ((Brush)targetPaintManager.Brush).SetValues(BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount]);
                            }
                            {
                                targetPaintManager.Brush = BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount].Clone();
                            }
                        }
                        
                        EditorHelper.MarkComponentAsDirty(targetPaintManager);
                        serializedObject.Update();
                    }
                }
            }
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying && PaintController.Instance.UseSharedSettings);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(brushProperty, new GUIContent("Brush", PaintManagerHelper.BrushTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                brushProperty.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndDisabledGroup();

            if (!showDialogName)
            {
                //save and remove buttons
                var enableButtons = !Application.isPlaying || Application.isPlaying && allowSavePresetsInRuntime;
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!enableButtons);
                if (GUILayout.Button("Save As", GUILayout.ExpandWidth(true)))
                {
                    showDialogName = true;
                    showWarning = true;
                    if (selectedPresetIndex == 0 || selectedPresetIndex == 1)
                    {
                        savedName = "Brush " + (BrushPresets.Instance.Presets.Count + 1);
                    }
                    else
                    {
                        savedName = BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount].Name;
                    }
                }
                EditorGUI.BeginDisabledGroup(selectedPresetIndex == 0 || selectedPresetIndex == 1);
                if (GUILayout.Button("Rename", GUILayout.ExpandWidth(true)))
                {
                    rename = true;
                    showDialogName = true;
                    showWarning = false;
                    savedName = BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount].Name;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(selectedPresetIndex == 0 || selectedPresetIndex == 1 || !enableButtons);
                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
                {
                    var result = EditorUtility.DisplayDialog("Remove selected brush?", 
                        "Are you sure that you want to remove " + BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount].Name + " brush?", "Remove", "Cancel");
                    if (result)
                    {
                        BrushPresets.Instance.Presets.RemoveAt(selectedPresetIndex - additionalPresetsCount);
                        selectedPresetIndex = 0;
                        foreach (var script in targets)
                        {
                            var targetPaintManager = script as PaintManager;
                            if (targetPaintManager != null)
                            {
                                targetPaintManager.Brush.Name = string.Empty;
                                EditorHelper.MarkAsDirty(targetPaintManager);
                                serializedObject.Update();
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                //enter name for a new preset
                savedName = GUILayout.TextArea(savedName, GUILayout.ExpandWidth(true));
                var savedNameTrimmed = savedName.Trim();
                var hasSavedPresetWithSameName = false;
                foreach (var preset in BrushPresets.Instance.Presets)
                {
                    if (preset.Name == savedNameTrimmed)
                    {
                        hasSavedPresetWithSameName = true;
                        break;
                    }
                }

                var hasPresetWithSameName = showWarning = 
                    savedNameTrimmed == BrushDrawerHelper.DefaultPresetName || hasSavedPresetWithSameName || string.IsNullOrEmpty(savedNameTrimmed);
                if (showWarning)
                {
                    EditorGUILayout.HelpBox("Please, enter unique name for brush", MessageType.Warning);
                }
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(hasPresetWithSameName);
                if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
                {
                    if (!hasPresetWithSameName)
                    {
                        var selectedBrush = (Brush)(selectedPresetIndex == 0 ? paintManager.Brush : BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount]);
                        var preset = new Brush(selectedBrush)
                        {
                            Name = savedNameTrimmed
                        };
                        if (rename)
                        {
                            BrushPresets.Instance.Presets[selectedPresetIndex - additionalPresetsCount] = preset;
                        }
                        else
                        {
                            BrushPresets.Instance.Presets.Insert(selectedPresetIndex - additionalPresetsCount + 1, preset);
                            if (sortPresetsByName)
                            {
                                BrushPresets.Instance.Presets.Sort((brush1, brush2) => string.Compare(brush1.Name, brush2.Name, StringComparison.Ordinal));
                                selectedPresetIndex = BrushPresets.Instance.Presets.IndexOf(preset) + additionalPresetsCount;
                            }
                            else
                            {
                                selectedPresetIndex++;
                            }
                        }
                        foreach (var script in targets)
                        {
                            var targetPaintManager = script as PaintManager;
                            if (targetPaintManager != null)
                            {
                                targetPaintManager.Brush = preset.Clone();
                                EditorHelper.MarkAsDirty(targetPaintManager);
                                serializedObject.Update();
                            }
                        }
                        showDialogName = false;
                        showWarning = false;
                        rename = false;
                    }
                }
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
                {
                    showDialogName = false;
                    showWarning = false;
                    rename = false;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawLayersBlock()
        {
            EditorHelper.DrawHorizontalLine();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(copySourceTextureToLayerProperty, new GUIContent("Copy Source Texture To Layer", PaintManagerHelper.CopySourceTextureToPaintTextureTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                paintManager.CopySourceTextureToLayer = copySourceTextureToLayerProperty.boolValue;
                MarkAsDirty();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useSourceTextureAsBackgroundProperty, new GUIContent("Use Source Texture as Background Layer", PaintManagerHelper.UseSourceTextureAsBackgroundTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                paintManager.UseSourceTextureAsBackground = useSourceTextureAsBackgroundProperty.boolValue;
                MarkAsDirty();
            }

            EditorGUILayout.PropertyField(layersContainerProperty, new GUIContent("Layers Container"));
            if (Application.isPlaying && paintManager.Initialized)
            {
                EditorGUILayout.PropertyField(layersControllerProperty);

                GUILayout.Space(5f);
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                var width = EditorGUIUtility.currentViewWidth / 2f;
                if (GUILayout.Button(new GUIContent("Add Layer", PaintManagerHelper.AddLayerTooltip), GUILayout.MaxWidth(width)))
                {
                    paintManager.LayersController.AddNewLayer();
                }
                EditorGUI.BeginDisabledGroup(!paintManager.LayersController.CanRemoveLayer);
                if (GUILayout.Button(new GUIContent("Remove Layer", PaintManagerHelper.RemoveLayer), GUILayout.MaxWidth(width)))
                {
                    paintManager.LayersController.RemoveActiveLayer();
                }
                EditorGUI.BeginDisabledGroup(!Application.isPlaying || paintManager.LayersController.ActiveLayer.MaskRenderTexture == null);
                if (GUILayout.Button(new GUIContent("Remove Layer Mask", PaintManagerHelper.RemoveLayerMask), GUILayout.MaxWidth(width)))
                {
                    paintManager.LayersController.RemoveActiveLayerMask();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                width = EditorGUIUtility.currentViewWidth / 3f;
                EditorGUI.BeginDisabledGroup(!paintManager.LayersController.CanMergeLayers);
                if (GUILayout.Button(new GUIContent("Merge Layers", PaintManagerHelper.MergeLayers), GUILayout.MaxWidth(width)))
                {
                    paintManager.LayersController.MergeLayers();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!paintManager.LayersController.CanMergeAllLayers);
                if (GUILayout.Button(new GUIContent("Merge All Layers", PaintManagerHelper.MergeAllLayers), GUILayout.MaxWidth(width)))
                {
                    paintManager.LayersController.MergeAllLayers();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(paintManager.LayersController.Layers.Count < 2);
                if (GUILayout.Button(new GUIContent("Set Next Active", PaintManagerHelper.SetNextActiveLayer), GUILayout.MaxWidth(width)))
                {
                    var nextIndex = (paintManager.LayersController.ActiveLayerIndex + 1) % paintManager.LayersController.Layers.Count;
                    paintManager.LayersController.SetActiveLayer(nextIndex);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                
                EditorHelper.DrawHorizontalLine();
            }
        }

        private void UpdateModelSettings()
        {
            if (component is SkinnedMeshRenderer renderer)
            {
                var sharedMesh = renderer.sharedMesh;
                if (sharedMesh != null && !sharedMesh.isReadable)
                {
                    PaintManagerHelper.SetModelReadable(sharedMesh);
                }
            }
                
            if (component is MeshFilter filter)
            {
                var sharedMesh = filter.sharedMesh;
                if (sharedMesh != null && !sharedMesh.isReadable)
                {
                    PaintManagerHelper.SetModelReadable(sharedMesh);
                }
            }
        }

        private void DrawCheckboxesBlock()
        {
            if (isMeshObject)
            {
                if (subMeshValues != null && subMeshValues.Length > 1)
                {
                    EditorGUI.BeginChangeCheck();
                    subMeshSelectedValue = EditorGUILayout.IntPopup("SubMesh", subMeshSelectedValue, subMeshStrings, subMeshValues);
                    if (EditorGUI.EndChangeCheck())
                    {
                        paintManager.SubMesh = subMeshSelectedValue;
                        serializedObject.ApplyModifiedProperties();
                        MarkAsDirty();
                    }
                }

                if (uvChannels != null && uvChannels.Length > 1)
                {
                    EditorGUI.BeginChangeCheck();
                    uvChannelSelectedValue = EditorGUILayout.IntPopup("UV Channel", uvChannelSelectedValue, uvChannelsStrings, uvChannels);
                    if (EditorGUI.EndChangeCheck())
                    {
                        paintManager.UVChannel = uvChannelSelectedValue;
                        serializedObject.ApplyModifiedProperties();
                        MarkAsDirty();
                    }
                }
            }
        }

        private void DrawToolsBlock()
        {
            // EditorHelper.DrawHorizontalLine();
            
            if (Application.isPlaying && PaintController.Instance == null && PaintController.Instance.UseSharedSettings)
            {
                EditorGUILayout.HelpBox("Shared Settings are enabled. Use PaintController to change parameters.", MessageType.Info);
            }
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying && PaintController.Instance == null && PaintController.Instance.UseSharedSettings);
            var mode = PaintMode.Default;
            var paintModeChanged = paintModeDrawer.Draw(paintModeProperty, "Paint Mode", PaintManagerHelper.PaintingModeTooltip, ref mode);
            if (paintModeChanged)
            {
                paintManager.SetPaintMode(mode);
                MarkAsDirty();
            }
            
            Undo.RecordObject(paintManager, "Paint Tool");
            var paintToolChanged = paintToolDrawer.Draw(paintToolProperty, "Paint Tool", PaintManagerHelper.PaintingToolTooltip, ref tool);
            if (paintToolChanged)
            {
                paintManager.Tool = tool;
                MarkAsDirty();
            }
            if (Application.isPlaying && paintManager.Initialized)
            {
                tool = paintManager.Tool;
            }
            else
            {
                tool = (PaintTool)paintToolDrawer.ModeId;
            }
            EditorGUILayout.PropertyField(toolsManagerProperty, new GUIContent("Tools Manager"));
            EditorGUI.EndDisabledGroup();
        }

        private void DrawButtonsBlock()
        {
            if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(isMeshObject)))
            {

            }
            EditorGUILayout.EndFadeGroup();
                        
            var disablePlaying = Application.isPlaying;
            EditorGUI.BeginDisabledGroup(!disablePlaying);
            if (GUILayout.Button(new GUIContent("Initialize", PaintManagerHelper.InitializeTip), GUILayout.ExpandWidth(true)))
            {
                paintManager.Init();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(disablePlaying);
            if (GUILayout.Button(new GUIContent("Clone Material", PaintManagerHelper.CloneMaterialTooltip), GUILayout.ExpandWidth(true)))
            {
                var clonedMaterial = PaintManagerHelper.CloneMaterial(paintMaterialProperty.objectReferenceValue as Material);
                if (clonedMaterial != null)
                {
                    paintMaterialProperty.objectReferenceValue = clonedMaterial;
                }
            }
            if (GUILayout.Button(new GUIContent("Clone Texture", PaintManagerHelper.CloneTextureTooltip), GUILayout.ExpandWidth(true)))
            {
                PaintManagerHelper.CloneTexture(paintMaterialProperty.objectReferenceValue as Material, shaderTextureNameProperty.stringValue);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
                
            GUILayout.BeginHorizontal();
            var disableUndo = !(paintManager != null && paintManager.Initialized && paintManager.StatesController != null && paintManager.StatesController.CanUndo());
            EditorGUI.BeginDisabledGroup(disableUndo);
            if (GUILayout.Button(new GUIContent("Undo", PaintManagerHelper.UndoTooltip), GUILayout.ExpandWidth(true)))
            {
                paintManager.StatesController.Undo();
            }
            EditorGUI.EndDisabledGroup();
            var disableRedo = !(paintManager != null && paintManager.Initialized && paintManager.StatesController != null && paintManager.StatesController.CanRedo());
            EditorGUI.BeginDisabledGroup(disableRedo);
            if (GUILayout.Button(new GUIContent("Redo", PaintManagerHelper.RedoTooltip), GUILayout.ExpandWidth(true)))
            {
                paintManager.StatesController.Redo();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button(new GUIContent("Save Texture", PaintManagerHelper.SaveToFileTooltip), GUILayout.ExpandWidth(true)))
            {
                PaintManagerHelper.SaveResultTextureToFile(paintManager);
            }

            if (GUILayout.Button(new GUIContent("Save Layers", PaintManagerHelper.SaveToFileTooltip), GUILayout.ExpandWidth(true)))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Layers Data to LayersContainer", "LayersContainer", "asset", "LayersContainer asset saving");
                if (!string.IsNullOrEmpty(path))
                {
                    var asset = CreateInstance<LayersContainer>();
                    asset.ActiveLayerIndex = paintManager.LayersController.ActiveLayerIndex;
                    asset.LayersData = paintManager.GetLayersData();
                    for (var i = 0; i < asset.LayersData.Length; i++)
                    {
                        var layerData = asset.LayersData[i];
                        var directoryInfo = new FileInfo(path).Directory;
                        var directory = directoryInfo.FullName;
                        var fileName = Path.GetFileNameWithoutExtension(path);
                        if (layerData.Texture != null)
                        {
                            var texturePath = Path.Combine(directory, $"{fileName}_Layer_{i}_{layerData.Name}.png");
                            var textureData = layerData.Texture.EncodeToPNG();
                            File.WriteAllBytes(texturePath, textureData);
                            AssetDatabase.Refresh();
                                                    
                            texturePath = texturePath.Replace(Application.dataPath, "Assets");
                            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                            
                            var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                            if (textureImporter != null)
                            {
                                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                                EditorUtility.SetDirty(textureImporter);
                                textureImporter.SaveAndReimport();
                            }
                            
                            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                            layerData.Texture = texture;
                            if (layerData.SourceTexture == null)
                            {
                                layerData.SourceTexture = texture;
                            }
                        }

                        if (layerData.Mask != null)
                        {
                            var maskPath = Path.Combine(directory, $"{fileName}_Layer_{i}_{layerData.Name}_Mask.png");
                            var maskData = layerData.Mask.EncodeToPNG();
                            File.WriteAllBytes(maskPath, maskData);
                            maskPath = maskPath.Replace(Application.dataPath, "Assets");
                            AssetDatabase.ImportAsset(maskPath, ImportAssetOptions.ForceUpdate);
                            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
                            layerData.Mask = mask;
                        }
                    }
                    
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    serializedObject.Update();
                    layersContainerProperty.objectReferenceValue = asset;
                    MarkAsDirty();
                }
            }
            
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void MarkAsDirty()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(paintManager);
                EditorSceneManager.MarkSceneDirty(paintManager.gameObject.scene);
            }
        }
    }
}