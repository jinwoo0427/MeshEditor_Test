using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XDPaint.Core.Layers;
using XDPaint.Editor.Utils;

namespace XDPaint.Editor
{
    [CustomPropertyDrawer(typeof(LayersController))]
    public class LayersControllerDrawer : PropertyDrawer
    {
        private float MarginBetweenFields => EditorGUIUtility.standardVerticalSpacing;
        private float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private float SingleLineHeightWithMargin => SingleLineHeight + MarginBetweenFields;

        private LayersController layersController;
        private List<Rect> rects = new List<Rect>();
        private Rect rect;
        private Rect foldoutRect;
        private float textureHeight;
        private SerializedProperty layers;
        private EditorInput input;
        private Rect[] layersDragRects, layersDragRectsLayout;
        private int? selectedArrayIndex;
        private Vector2 clickPosition;
        private bool isDragStarted;
        private int? moveToIndex;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            layersController = property.GetInstance<LayersController>();
            if (property.isExpanded)
            {
                var singleElementHeight = MarginBetweenFields * 4f + SingleLineHeight * 3f;
                return SingleLineHeight + singleElementHeight * layersController.Layers.Count +
                       MarginBetweenFields * (layersController.Layers.Count + 1);
            }
            return SingleLineHeightWithMargin;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }
        
        private void OnMouseDown(int controlId)
        {
            rects.Clear();
            for (var i = 0; i < layersDragRects.Length; i++)
            {
                var dragRect = layersDragRects[i];
                var dragRectCopy = new Rect(dragRect.x, dragRect.y, dragRect.width, dragRect.height);
                var mouse = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
                if (dragRectCopy.Contains(mouse))
                {
                    rects.Add(dragRectCopy);
                    clickPosition = mouse;
                    selectedArrayIndex = i;
                    layersController.SetActiveLayer(selectedArrayIndex.Value);
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    break;
                }
            }
        }
        
        private void OnMouseDrag(int controlId)
        {
            if (GUIUtility.hotControl == controlId && selectedArrayIndex.HasValue)
            {
                var distance = Vector2.Distance(clickPosition, Event.current.mousePosition);
                if (distance > 10f)
                {
                    isDragStarted = true;
                }
                Event.current.Use();
            }
        }

        private void OnMouseUp(int controlId)
        {
            if (GUIUtility.hotControl == controlId)
            {
                GUIUtility.hotControl = 0;

                if (!isDragStarted && selectedArrayIndex != null)
                {
                    layersController.SetActiveLayer(selectedArrayIndex.Value);
                }
                if (moveToIndex.HasValue)
                {
                    var selectedLayer = layersController.ActiveLayer as Layer;
                    layersController.SetLayerOrder(selectedLayer, moveToIndex.Value);
                }
                moveToIndex = null;
                selectedArrayIndex = null;
                isDragStarted = false;
                Event.current.Use();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (input == null)
            {
                input = new EditorInput
                {
                    OnMouseDown = OnMouseDown,
                    OnMouseDrag = OnMouseDrag,
                    OnMouseUp = OnMouseUp,
                };
            }

            EditorGUI.BeginProperty(position, label, property);
            
            rect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            rect.height = SingleLineHeight;
            rect.y += MarginBetweenFields;
            
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            layers = property.FindPropertyRelative("layersList");
            foldoutRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, SingleLineHeightWithMargin);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "Layers");
            if (property.isExpanded)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Layer Parameters");
                layersController = property.GetInstance<LayersController>();
                rect.y += SingleLineHeightWithMargin;
                input.Update();

                Action onDrag = null;
                Rect moveToRect = default;
                var firstY = rect.y;

                if (Event.current.type == EventType.Repaint)
                {
                    layersDragRects = new Rect[layers.arraySize];
                }
                layersDragRectsLayout = new Rect[layers.arraySize];

                var oneElementHeight = 0f;
                for (var i = layers.arraySize - 1; i >= 0; i--)
                {
                    var arrayElement = layers.GetArrayElementAtIndex(i);
                    var elementLabel = new GUIContent(property.displayName, "");
                    var elementHeight = EditorGUI.GetPropertyHeight(arrayElement, elementLabel, true) + MarginBetweenFields;
                    oneElementHeight = elementHeight;
                    if (Event.current.type == EventType.Repaint)
                    {
                        layersDragRects[i] = new Rect(rect)
                        {
                            width = 30,
                            height = elementHeight - MarginBetweenFields * 2f,
                            x = rect.x + 5f,
                            y = rect.y + MarginBetweenFields
                        };
                    }
                    layersDragRectsLayout[i] = new Rect(rect)
                    {
                        width = 30,
                        height = elementHeight - MarginBetweenFields * 2f,
                        x = rect.x + 5f,
                        y = rect.y + MarginBetweenFields
                    };
                    EditorGUIUtility.AddCursorRect(layersDragRectsLayout[i], MouseCursor.Pan);
                    rect.y += elementHeight;
                }
                rect.y -= oneElementHeight * layers.arraySize;

                for (var i = layers.arraySize - 1; i >= 0; i--)
                {
                    var arrayElement = layers.GetArrayElementAtIndex(i);
                    var elementLabel = new GUIContent(property.displayName);
                    var elementHeight = EditorGUI.GetPropertyHeight(arrayElement, elementLabel, true) + MarginBetweenFields;
                    var selectActive = layersController.ActiveLayerIndex == i;
                    if (i == selectedArrayIndex || selectActive)
                    {
                        const float selectionOffsetX = 0f;
                        var dragRect = new Rect(rect)
                        {
                            x = rect.x - selectionOffsetX
                        };
                        var rectForDrag = dragRect;
                        onDrag = () =>
                        {
                            if (isDragStarted)
                            {
                                rectForDrag.position += Vector2.up * (Event.current.mousePosition.y - clickPosition.y);
                                for (var j = 0; j < layersDragRectsLayout.Length; j++)
                                {
                                    if (j == selectedArrayIndex)
                                        continue;

                                    var distance = layersDragRectsLayout.Select(x => Vector2.Distance(x.position, rectForDrag.position)).ToList();
                                    var dict = distance.Select((k, v) => new { k, v })
                                        .ToDictionary(x => x.v, x => x.k)
                                        .OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                                    
                                    var index = 0;
                                    moveToIndex = null;

                                    if (!selectedArrayIndex.HasValue || !(Mathf.Abs(rectForDrag.position.y - layersDragRectsLayout[selectedArrayIndex.Value].position.y) < elementHeight))
                                    {
                                        foreach (var key in dict.Keys)
                                        {
                                            if (selectedArrayIndex != null && selectedArrayIndex.Value == key)
                                                continue;

                                            if (selectedArrayIndex != null && selectedArrayIndex.Value > key)
                                            {
                                                if (layersDragRectsLayout[key].position.y < rectForDrag.position.y)
                                                {
                                                    index = layers.arraySize - key;
                                                    moveToIndex = key;
                                                    break;
                                                }
                                            }
                                            else if (layersDragRectsLayout[key].position.y > rectForDrag.position.y)
                                            {
                                                index = layers.arraySize - key - 1;
                                                moveToIndex = key;
                                                break;
                                            }
                                        }
                                    }
                                    
                                    moveToRect = new Rect
                                    {
                                        width = rect.width,
                                        height = 5f,
                                        x = rect.x,
                                        y = firstY + elementHeight * index
                                    };
                                }
                            }

                            #region Selection

                            var layer = PropertyDrawerUtility.GetActualObjectForSerializedProperty<Layer>(arrayElement);
                            if (layer != null)
                            {
                                const float selectionBorderSize = 4f;
                                var selectedRect = new Rect(rectForDrag)
                                {
                                    width = rectForDrag.width + selectionBorderSize + selectionOffsetX,
                                    height = elementHeight + selectionBorderSize,
                                    x = rectForDrag.x - selectionBorderSize / 2f,
                                    y = rectForDrag.y - selectionBorderSize / 2f - 1
                                };
                                EditorGUI.DrawRect(selectedRect, LayerDrawerHelper.SelectRectColor);
                            }

                            #endregion
                            
                            EditorGUI.PropertyField(rectForDrag, arrayElement, elementLabel);

                            #region Array Position
                            
                            if (moveToIndex.HasValue)
                            {
                                EditorGUI.DrawRect(moveToRect, LayerDrawerHelper.MoveRectColor);
                            }

                            #endregion
                            
                        };
                    }
                    else
                    {
                        EditorGUI.PropertyField(rect, arrayElement, elementLabel);
                    }
                    rect.y += elementHeight;
                }
                onDrag?.Invoke();
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}