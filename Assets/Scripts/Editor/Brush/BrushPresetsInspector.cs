using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XDPaint.Core.Materials;
using XDPaint.Tools;

namespace XDPaint.Editor
{
    [CustomEditor(typeof(BrushPresets))]
    public class BrushPresetsInspector : UnityEditor.Editor
    {
        private SerializedProperty presetsProperty;

        private void OnEnable()
        {
            presetsProperty = serializedObject.FindProperty("Presets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(presetsProperty);
            var presets = BrushPresets.Instance.Presets;
            var duplicates = presets
                .Select((b, i) => new { Name = b.Name, Index = i })
                .GroupBy(g => g.Name)
                .Where(g => g.Count() > 1 || g.Key == BrushDrawerHelper.CustomPresetName)
                .ToDictionary(x => x.Key, y => y.Select(b => b.Index));

            if (duplicates.Count > 0)
            {
                EditorGUILayout.HelpBox("Please, enter unique name for brush " + duplicates.Keys.First().ToString(), MessageType.Warning);
                if (GUILayout.Button("Fix Names"))
                {
                    Undo.RecordObject(target, "Brush Rename");
                    UpdateDuplicateNames(duplicates);
                    
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateDuplicateNames(Dictionary<string, IEnumerable<int>> items)
        {
            const string postfix = "_";
            foreach (var item in items)
            {
                var startFrom = item.Key == BrushDrawerHelper.CustomPresetName ? 0 : 1;
                for (var i = startFrom; i < item.Value.Count(); i++)
                {
                    var index = item.Value.ElementAt(i);
                    var brush = BrushPresets.Instance.Presets[index];
                    var duplicatesCount = 0;
                    if (brush.Name.Contains(postfix))
                    {
                        var postfixNumbers = brush.Name.Split(postfix.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (postfixNumbers.Length > 1)
                        {
                            var numbers = postfixNumbers.Last();
                            int num;
                            var isNumeric = int.TryParse(numbers, out num);
                            if (isNumeric)
                            {
                                duplicatesCount = num;
                            }
                        }

                        do
                        {
                            duplicatesCount++;
                            var prefixCount = postfixNumbers[0].Length;
                            brush.Name = brush.Name.Remove(prefixCount, brush.Name.Length - prefixCount);
                            brush.Name += postfix + duplicatesCount;
                        } 
                        while (CheckForDuplicateName(brush));
                    }
                    else
                    {
                        var brushName = brush.Name;
                        do
                        {
                            duplicatesCount++;
                            brush.Name = brushName + postfix + duplicatesCount;
                        } 
                        while (CheckForDuplicateName(brush));
                        BrushPresets.Instance.Presets[index] = brush;
                    }
                }
            }
        }

        private bool CheckForDuplicateName(Brush brush)
        {
            foreach (var preset in BrushPresets.Instance.Presets)
            {
                if (preset != brush && preset.Name == brush.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}