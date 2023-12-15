using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Editor.Utils;
using XDPaint.Tools;
using XDPaint.Tools.Image.Base;
using XDPaint.Utils;

namespace XDPaint.Editor
{
	[CustomPropertyDrawer(typeof(ToolsManager))]
	public class ToolsManagerDrawer : PropertyDrawer
	{
		private float MarginBetweenFields => EditorGUIUtility.standardVerticalSpacing;
		private float SingleLineHeight => EditorGUIUtility.singleLineHeight;
		private float SingleLineHeightWithMargin => SingleLineHeight + MarginBetweenFields;

		private ToolsManager toolsManager;
		private SerializedProperty toolProperty;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return Application.isPlaying ? SingleLineHeightWithMargin : 0f;
		}

		public override bool CanCacheInspectorGUI(SerializedProperty property)
		{
			return false;
		}

		public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label)
		{
			if (!Application.isPlaying)
				return;
			
			EditorGUI.BeginProperty(position, label, serializedProperty);
			serializedProperty.isExpanded = EditorGUI.Foldout(position, serializedProperty.isExpanded, "Tool Settings");
			if (serializedProperty.isExpanded)
			{
				Undo.RecordObject(serializedProperty.serializedObject.targetObject, "ToolsManager Parameters");
				toolsManager = PropertyDrawerUtility.GetActualObjectForSerializedProperty<ToolsManager>(serializedProperty);
				var tool = toolsManager.CurrentTool;
				if (tool == null)
					return;

				var settingsFields = toolsManager.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo currentToolField = null;
				foreach (var field in settingsFields)
				{
					var allAttributes = field.GetCustomAttributes(true);
					var hasAttribute = allAttributes.FirstOrDefault(x => x is SerializeField) != null;
					if (hasAttribute && tool == field.GetValue(toolsManager) as IPaintTool)
					{
						currentToolField = field;
						break;
					}
				}

				if (currentToolField != null)
				{
					toolProperty = serializedProperty.FindPropertyRelative(currentToolField.Name);
					var settings = tool.GetType().GetProperty("Settings")?.GetValue(tool);
					if (settings != null)
					{
						var members = settings.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
						foreach (var memberInfo in members)
						{
							DrawProperty(memberInfo, settings);
						}
					}
				}

				EditorGUI.EndProperty();
			}
		}
		
		private void DrawProperty(MemberInfo memberInfo, object obj)
		{
			if (memberInfo is PropertyInfo property)
			{
				var allAttributes = property.GetCustomAttributes(true);
				var hasAttribute = allAttributes.FirstOrDefault(x => x is PaintToolSettingsAttribute) != null;
				if (property.PropertyType == typeof(Texture) && hasAttribute)
				{
					var propPaintToolAttribute = allAttributes.FirstOrDefault(x => x is PaintToolSettingsAttribute);
					if (propPaintToolAttribute != null)
					{
						var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
						foreach (var field in fields)
						{
							var attributes = field.GetCustomAttributes(true);
							var fieldPaintToolAttribute = attributes.FirstOrDefault(x => x is PaintToolSettingsAttribute);
							if (field.FieldType == property.PropertyType && fieldPaintToolAttribute != null && 
								((PaintToolSettingsAttribute)fieldPaintToolAttribute).Group == ((PaintToolSettingsAttribute)propPaintToolAttribute).Group)
							{
								var name = field.Name.ToCamelCaseWithSpace();
								var relativeProperty = toolProperty.FindPropertyRelative("toolSettings");
								var foundProperty = false;
								var fieldsCount = relativeProperty.GetFieldsCount();
								for (var i = 0; i < fieldsCount; i++)
								{
									if (!relativeProperty.NextVisible(true))
										break;

									if (relativeProperty.name == field.Name)
									{
										foundProperty = true;
										break;
									}
								}

								if (foundProperty)
								{
									BeginDisabledGroup(allAttributes, obj);
									EditorGUI.BeginChangeCheck();
									EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), relativeProperty, new GUIContent(name));
									if (EditorGUI.EndChangeCheck())
									{
										field.SetValue(obj, relativeProperty.objectReferenceValue);
										property.SetValue(obj, relativeProperty.objectReferenceValue);
									}
									EndDisabledGroup();
									relativeProperty.Reset();
								}
							}
						}
					}
				}
				
				if (property.PropertyType == typeof(bool) && hasAttribute)
				{
					var value = Convert.ToBoolean(property.GetValue(obj));
					var name = property.Name.ToCamelCaseWithSpace();
					BeginDisabledGroup(allAttributes, obj);
					EditorGUI.BeginChangeCheck();
					value = EditorGUI.Toggle(EditorGUILayout.GetControlRect(), name, value);
					if (EditorGUI.EndChangeCheck())
					{
						property.SetValue(obj, value);
					}
					EndDisabledGroup();
				}

				if (property.PropertyType == typeof(float) && hasAttribute)
				{
					var value = Convert.ToSingle(property.GetValue(obj));
					var name = property.Name.ToCamelCaseWithSpace();
					var rangeMin = 0f;
					var rangeMax = 1f;
					var hasRangeAttribute = false;
					foreach (var objectAttribute in allAttributes)
					{
						if (objectAttribute is PaintToolRangeAttribute range)
						{
							hasRangeAttribute = true;
							rangeMin = range.Min;
							rangeMax = range.Max;
							break;
						}
					}

					BeginDisabledGroup(allAttributes, obj);
					EditorGUI.BeginChangeCheck();
					value = hasRangeAttribute
						? EditorGUI.Slider(EditorGUILayout.GetControlRect(), name, value, rangeMin, rangeMax)
						: EditorGUI.FloatField(EditorGUILayout.GetControlRect(), name, value);
					if (EditorGUI.EndChangeCheck())
					{
						property.SetValue(obj, value);
					}
					EndDisabledGroup();
				}

				if (property.PropertyType == typeof(int) && hasAttribute)
				{
					var value = Convert.ToInt32(property.GetValue(obj));
					var name = property.Name.ToCamelCaseWithSpace();
					var rangeMin = 0;
					var rangeMax = 1;
					var hasRangeAttribute = false;
					foreach (var objectAttribute in allAttributes)
					{
						if (objectAttribute is PaintToolRangeAttribute range)
						{
							hasRangeAttribute = true;
							rangeMin = (int)range.Min;
							rangeMax = (int)range.Max;
							break;
						}
					}
					BeginDisabledGroup(allAttributes, obj);
					EditorGUI.BeginChangeCheck();
					value = hasRangeAttribute
						? EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), name, value, rangeMin, rangeMax)
						: EditorGUI.IntField(EditorGUILayout.GetControlRect(), name, value);
					if (EditorGUI.EndChangeCheck())
					{
						property.SetValue(obj, value);
					}
					EndDisabledGroup();
				}

				if (property.PropertyType == typeof(Vector2) && hasAttribute)
				{
					var value = (Vector2)property.GetValue(obj);
					var name = property.Name.ToCamelCaseWithSpace();
					BeginDisabledGroup(allAttributes, obj);
					EditorGUI.BeginChangeCheck();
					value = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), name, value);
					if (EditorGUI.EndChangeCheck())
					{
						property.SetValue(obj, value);
					}
					EndDisabledGroup();
				}
			}
		}

		private void BeginDisabledGroup(object[] allAttributes, object obj)
		{
			var fieldPaintConditionalAttribute = allAttributes.FirstOrDefault(x => x is PaintToolConditionalAttribute);
			var isDisabled = false;
			if (fieldPaintConditionalAttribute != null)
			{
				var path = ((PaintToolConditionalAttribute)fieldPaintConditionalAttribute).Condition.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (path.Length > 1)
				{
					var nestedObj = obj;
					for (var i = 0; i < path.Length; i++)
					{
						var subPath = path[i];
						var memberInfo = nestedObj.GetType().GetMember(subPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						if (memberInfo.Length > 0)
						{
							if (memberInfo[0] is FieldInfo field)
							{
								if (i == path.Length - 1)
								{
									if (field.FieldType == typeof(bool))
									{
										isDisabled = !Convert.ToBoolean(field.GetValue(nestedObj));
									}
								}
								else
								{
									nestedObj = field.GetValue(obj);
								}
							}
							else if (memberInfo[0] is PropertyInfo property)
							{
								if (i == path.Length - 1)
								{
									if (property.PropertyType == typeof(bool))
									{
										isDisabled = !Convert.ToBoolean(property.GetValue(nestedObj));
									}
								}
								else
								{
									nestedObj = property.GetValue(obj);
								}

							}
						}
					}
				}
				else
				{
					var member = obj.GetType().GetMember(((PaintToolConditionalAttribute)fieldPaintConditionalAttribute).Condition, BindingFlags.Public | BindingFlags.Instance);
					if (member.Length > 0 && member[0] is FieldInfo conditionalField)
					{
						if (conditionalField.FieldType == typeof(bool))
						{
							isDisabled = !Convert.ToBoolean(conditionalField.GetValue(obj));
						}
					}
					if (member.Length > 0 && member[0] is PropertyInfo conditionalProperty)
					{
						if (conditionalProperty.PropertyType == typeof(bool))
						{
							isDisabled = !Convert.ToBoolean(conditionalProperty.GetValue(obj));
						}
					}
				}
			}
			EditorGUI.BeginDisabledGroup(isDisabled);
		}

		private void EndDisabledGroup()
		{
			EditorGUI.EndDisabledGroup();
		}
	}
}