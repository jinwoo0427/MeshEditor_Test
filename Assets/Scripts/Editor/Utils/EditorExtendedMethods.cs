using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XDPaint.Editor.Utils
{
    public static class EditorExtendedMethods
    {
        public static T GetInstance<T>(this SerializedProperty property) where T : class 
        {
            T obj = null;
            try
            {
                obj = PropertyDrawerUtility.GetActualObjectForSerializedProperty<T>(property);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            return obj;
        }
        
        public static int GetFieldsCount(this SerializedProperty property)
        {
            var fieldCounts = new Dictionary<string, int>();
            if (fieldCounts.TryGetValue(property.type, out var count)) 
                return count;
            
            var children = property.Copy().GetEnumerator();
            while (children.MoveNext())
            {
                count++;
            }
            
            fieldCounts[property.type] = count;
            return count;
        }
    }
}