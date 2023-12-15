using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Editor.Utils;

namespace XDPaint.Editor
{
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerDrawer : PropertyDrawer
    {
        private float MarginBetweenFields => EditorGUIUtility.standardVerticalSpacing;
        private float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private float TotalHeight => MarginBetweenFields * 4f + SingleLineHeight * 3f;

        private LayersController layersController;
        private Layer layer;
        private Rect mainRect, rect;
        private SerializedProperty name, texture, maskTexture, index, enabled, maskEnabled, blendingMode, opacity;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return TotalHeight;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            mainRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            mainRect.height = TotalHeight;
            mainRect.y += MarginBetweenFields;

            rect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            rect.height = SingleLineHeight;
            rect.y += MarginBetweenFields;
            
            name = property.FindPropertyRelative("name");
            enabled = property.FindPropertyRelative("enabled");
            maskEnabled = property.FindPropertyRelative("maskEnabled");
            blendingMode = property.FindPropertyRelative("blendingMode");
            opacity = property.FindPropertyRelative("opacity");
            layer = property.GetInstance<Layer>();
            layersController = property.GetInstance<LayersController>();

            var isLayerSelected = false;
            if (layersController != null)
            {
                isLayerSelected = layersController.ActiveLayer == layer;
            }
            Color color;
            if (isLayerSelected)
            {
                color = EditorGUIUtility.isProSkin ? new Color32(108, 108, 108, 255) : new Color32(224, 224, 224, 255);
            }
            else
            {
                color = EditorGUIUtility.isProSkin ? new Color32(88, 88, 88, 255) : new Color32(194, 194, 194, 255);
            }
            EditorGUI.DrawRect(new Rect(position.position, new Vector2(EditorGUIUtility.currentViewWidth, TotalHeight)), color);

            const float marginX = 16f;

            #region Drag
            
            const float dragWidth = 30f;
            var allDragRect = new Rect(mainRect)
            {
                width = dragWidth,
                height = mainRect.height - MarginBetweenFields * 2f,
                x = mainRect.x - 10f
            };
            EditorGUI.DrawRect(allDragRect, new Color32(100, 100, 100, 255));
            
            for (var i = 0; i < 3; i++)
            {
                var dragRect = new Rect(mainRect)
                {
                    width = 10,
                    height = 2f,
                    y = allDragRect.height / 2f + mainRect.y + 5 * (i - 1) - 1f
                };
                EditorGUI.DrawRect(dragRect, LayerDrawerHelper.GrayColor);
            }
            
            #endregion

            #region Enable
            
            var enableRect = new Rect(mainRect)
            {
                x = mainRect.x + marginX + 4f,
                width = 34
            };
            EditorGUI.BeginDisabledGroup(!layer.CanBeDisabled);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(enableRect, enabled, GUIContent.none);
            rect.y += MarginBetweenFields + SingleLineHeight;
            if (EditorGUI.EndChangeCheck())
            {
                layer.Enabled = enabled.boolValue;
            }
            EditorGUI.EndDisabledGroup();

            #endregion

            #region Vertical Line
            
            var verticalLineRect = new Rect(mainRect)
            {
                x = enableRect.x + enableRect.width + 10f,
                y = mainRect.y - 2,
                width = 2f,
            };
            EditorGUI.DrawRect(verticalLineRect, LayerDrawerHelper.Gray2Color);

            #endregion

            #region Textures

            var textureRect = new Rect(verticalLineRect);
            if (layer != null && layer.RenderTexture != null)
            {
                var width = Mathf.Clamp(layer.RenderTexture.width, 1f, mainRect.height - 2);
                var height = Mathf.Clamp(layer.RenderTexture.height, 1f, mainRect.height - 2);
                textureRect.x = verticalLineRect.x + verticalLineRect.width + marginX;
                if (height < mainRect.height - 2)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        textureRect.y += (mainRect.height - 2f - height) / 2f;
                    }
                }
                textureRect.width = width;
                textureRect.height = height;
                EditorGUI.DrawTextureTransparent(textureRect, layer.RenderTexture, ScaleMode.ScaleToFit);
            }
            
            var maskTextureRect = new Rect(textureRect);
            if (layer != null && layer.MaskRenderTexture != null)
            {
                var width = Mathf.Clamp(layer.MaskRenderTexture.width, 1f, mainRect.height - 2);
                var height = Mathf.Clamp(layer.MaskRenderTexture.height, 1f, mainRect.height - 2);
                maskTextureRect.x = verticalLineRect.x + verticalLineRect.width + marginX * 2f + textureRect.width;
                maskTextureRect.width = width;
                maskTextureRect.height = height;
                EditorGUI.DrawTextureTransparent(maskTextureRect, layer.MaskRenderTexture, ScaleMode.ScaleToFit);
                
                var maskTextureEnableRect = new Rect(maskTextureRect)
                {
                    width = 25
                };
                maskTextureEnableRect.x -= width / 2f - maskTextureEnableRect.width * 0.5f + 2f;
                maskTextureEnableRect.y += maskTextureEnableRect.width * 0.5f + 4f;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(maskTextureEnableRect, maskEnabled, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    layer.MaskEnabled = maskEnabled.boolValue;
                }
            }

            #endregion

            #region Layer Name
            
            var layerNameLabelRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width,
                y = mainRect.y,
                width = 70,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.LabelField(layerNameLabelRect, LayerDrawerHelper.NameLabel);

            var layerNameRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width + layerNameLabelRect.width,
                y = layerNameLabelRect.y,
                width = EditorGUIUtility.currentViewWidth - maskTextureRect.x - maskTextureRect.width - layerNameLabelRect.width - marginX,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(layerNameRect, name, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                layer.Name = name.stringValue;
            }

            #endregion

            #region Blending Mode
            
            var blendingModeLabelRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width,
                y = mainRect.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                width = 70,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.LabelField(blendingModeLabelRect, LayerDrawerHelper.BlendingModeLabel);
            
            var blendingModeRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width + blendingModeLabelRect.width,
                y = blendingModeLabelRect.y,
                width = EditorGUIUtility.currentViewWidth - maskTextureRect.x - maskTextureRect.width - blendingModeLabelRect.width - marginX,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.BeginChangeCheck();

            //var modes = Enum.GetNames(typeof(BlendingMode)).Select(x => string.Concat(x.Select(t => Char.IsUpper(t) ? " " + t : t.ToString())).TrimStart(' ')).ToArray();
            //blendingMode.enumValueIndex = EditorGUI.Popup(blendingModeRect, blendingMode.enumValueIndex, modes);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    var mode = (BlendingMode)Enum.Parse(typeof(BlendingMode), modes[blendingMode.enumValueIndex].Replace(" ", ""));
            //    layer.BlendingMode = mode;
            //}

            #endregion

            #region Opacity
            
            var opacityLabelRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width,
                y = mainRect.y + EditorGUIUtility.standardVerticalSpacing * 2f + EditorGUIUtility.singleLineHeight * 2f,
                width = 70,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.LabelField(opacityLabelRect, LayerDrawerHelper.OpacityLabel);
            
            var opacityRect = new Rect(mainRect)
            {
                x = maskTextureRect.x + maskTextureRect.width + opacityLabelRect.width,
                y = opacityLabelRect.y,
                width = EditorGUIUtility.currentViewWidth - maskTextureRect.x - maskTextureRect.width - opacityLabelRect.width - marginX,
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.BeginChangeCheck();
            var guiColor = GUI.color;
            GUI.backgroundColor = color;
            EditorGUI.Slider(opacityRect, opacity, 0f, 1f, GUIContent.none);
            GUI.color = guiColor;
            if (EditorGUI.EndChangeCheck())
            {
                layer.Opacity = opacity.floatValue;
            }

            #endregion
            
            EditorGUI.EndProperty();
        }
    }
}