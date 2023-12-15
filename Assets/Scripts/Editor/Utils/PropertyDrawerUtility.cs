using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace XDPaint.Editor.Utils
{
    public class PropertyDrawerUtility
    {
        public static T GetActualObjectForSerializedProperty<T>(SerializedProperty property) where T : class
        {
            object obj = property.serializedObject.targetObject;
            var propertyNames = property.propertyPath.Split('.');
 
            //clear property path from 'Array' and 'data[i]'
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();
 
            //get last object of the property path
            foreach (var path in propertyNames)
            {
                obj = obj?.GetType().GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj);
                if (string.Equals(path, typeof(T).Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }

            if (obj == null)
            {
                return null;
            }
            T actualObject;
            if (obj.GetType().IsArray && ((T[])obj).Length > 0)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                actualObject = ((T[])obj).Length > index ? ((T[])obj)[index] : ((T[])obj)[((T[])obj).Length - 1];
            }
            else if (obj.GetType() == typeof(List<T>) && ((List<T>)obj).Count > 0)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                actualObject = ((List<T>)obj).Count > index ? ((List<T>) obj)[index] : ((List<T>) obj)[((List<T>)obj).Count - 1];
            }
            else
            {
                actualObject = obj as T;
            }
            return actualObject;
        }
    }
}