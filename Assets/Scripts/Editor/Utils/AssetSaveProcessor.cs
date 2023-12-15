using System.Collections.Generic;
using UnityEditor;
using XDPaint.Tools;

namespace XDPaint.Editor.Utils
{
    public class AssetSaveProcessor : UnityEditor.AssetModificationProcessor
    {
        public static bool SavePresets;
        
        static string[] OnWillSaveAssets(string[] paths)
        {
            if (SavePresets)
            {
                SavePresets = false;
                var pathsList = new List<string>();
                if (Settings.Instance != null)
                {
                    var path = AssetDatabase.GetAssetPath(Settings.Instance);
                    if (!string.IsNullOrEmpty(path))
                    {
                        pathsList.Add(path);
                    }
                }
                if (BrushPresets.Instance != null)
                {
                    var path = AssetDatabase.GetAssetPath(BrushPresets.Instance);
                    if (!string.IsNullOrEmpty(path))
                    {
                        pathsList.Add(path);
                    }
                }
                if (pathsList.Count > 0)
                {
                    return pathsList.ToArray();
                }
            }
            return paths;
        }
    }
}