using System.Linq;
using UnityEditor;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools;

namespace XDPaint.Editor
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty defaultBrushProperty;
        private SerializedProperty defaultCircleBrushProperty;
        private SerializedProperty defaultPatternTextureProperty;
        private SerializedProperty pressureEnabledProperty;
        private SerializedProperty checkCanvasRaycastsProperty;
        private SerializedProperty raycastsMethodProperty;
        private SerializedProperty brushDuplicatePartWidthProperty;
        private SerializedProperty pixelPerUnitProperty;
        private SerializedProperty containerGameObjectNameProperty;
        
        void OnEnable()
        {
            defaultBrushProperty = serializedObject.FindProperty("DefaultBrush");
            defaultCircleBrushProperty = serializedObject.FindProperty("DefaultCircleBrush");
            defaultPatternTextureProperty = serializedObject.FindProperty("DefaultPatternTexture");
            pressureEnabledProperty = serializedObject.FindProperty("PressureEnabled");
            checkCanvasRaycastsProperty = serializedObject.FindProperty("CheckCanvasRaycasts");
            raycastsMethodProperty = serializedObject.FindProperty("RaycastsMethod");
            brushDuplicatePartWidthProperty = serializedObject.FindProperty("BrushDuplicatePartWidth");
            pixelPerUnitProperty = serializedObject.FindProperty("PixelPerUnit");
            containerGameObjectNameProperty = serializedObject.FindProperty("ContainerGameObjectName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(defaultBrushProperty, new GUIContent("Default Brush"));
            EditorGUILayout.PropertyField(defaultCircleBrushProperty, new GUIContent("Default Circle Brush"));
            EditorGUILayout.PropertyField(defaultPatternTextureProperty, new GUIContent("Default Pattern Texture"));
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                var group = EditorUserBuildSettings.selectedBuildTargetGroup;
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var allDefines = defines.Split(';').ToList();

                for (var i = allDefines.Count - 1; i >= 0; i--)
                {
                    if (Constants.Defines.VREnabled.Contains(allDefines[i]))
                    {
                        allDefines.RemoveAt(i);
                    }
                }
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
            }
            EditorGUILayout.PropertyField(pressureEnabledProperty, new GUIContent("Pressure Enabled"));
            EditorGUILayout.PropertyField(checkCanvasRaycastsProperty, new GUIContent("Check Canvas Raycasts"));
            EditorGUILayout.PropertyField(raycastsMethodProperty, new GUIContent("Raycasts Method"));
#if !BURST
            if ((RaycastSystemType)raycastsMethodProperty.enumValueIndex == RaycastSystemType.JobSystem)
            {
                EditorGUILayout.HelpBox("It is recommended to use the Burst package to increase performance. " +
                                        "Please, install the Burst package from Package Manager.", MessageType.Warning);
                if (GUILayout.Button("Open Package Manager", GUILayout.ExpandWidth(true)))
                {
                    UnityEditor.PackageManager.UI.Window.Open("com.unity.burst");
                }
            }
#endif
            EditorGUILayout.PropertyField(brushDuplicatePartWidthProperty, new GUIContent("Brush Duplicate Part Width"));
            EditorGUILayout.PropertyField(pixelPerUnitProperty, new GUIContent("Pixel per Unit"));
            EditorGUILayout.PropertyField(containerGameObjectNameProperty, new GUIContent("Container GameObject Name"));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Version: 3.1.0 (853e715b)");
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}