using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
    #region Fields
    private DrawIfAttribute _drawIf;
    private SerializedProperty _comparedField;
    #endregion

    #region Methods
    public override float GetPropertyHeight(SerializedProperty pProperty, GUIContent pLabel)
    {
        if (!ShowMe(pProperty) && _drawIf.DisablingType == DisablingType.DontDraw) return -EditorGUIUtility.standardVerticalSpacing;

        if (pProperty.propertyType == SerializedPropertyType.Generic)
        {
            int numChildren = 0;
            float totalHeight = 0;
            IEnumerator children = pProperty.GetEnumerator();
            while (children.MoveNext())
            {
                SerializedProperty child = children.Current as SerializedProperty;
                GUIContent childLabel = new GUIContent(child.displayName);
                totalHeight += EditorGUI.GetPropertyHeight(child, childLabel) + EditorGUIUtility.standardVerticalSpacing;
                numChildren++;
            }
            // Remove extra space at end, (we only want spaces between items)
            totalHeight -= EditorGUIUtility.standardVerticalSpacing;

            return totalHeight;
        }
        else
        {
            return EditorGUI.GetPropertyHeight(pProperty, pLabel);
        }
    }

    /// <summary>
    /// Errors default to showing the property.
    /// </summary>
    private bool ShowMe(SerializedProperty pProperty)
    {
        _drawIf = attribute as DrawIfAttribute;
        // Replace propertyname to the value from the parameter
        string path = pProperty.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(pProperty.propertyPath, _drawIf.ComparedPropertyName) : _drawIf.ComparedPropertyName;

        _comparedField = pProperty.serializedObject.FindProperty(path);

        if (_comparedField == null)
        {
            Debug.LogError("Cannot find property with name: " + path);
            return true;
        }

        if (_comparedField.isArray)
        {
            int relativeIntVal = _comparedField.arraySize.CompareTo((int)_drawIf.ComparedValue);
            return _drawIf.Comparison switch
            {
                ComparisonType.Equals => relativeIntVal == 0,
                ComparisonType.NotEqual => relativeIntVal != 0,
                ComparisonType.GreaterThan => relativeIntVal == 1,
                ComparisonType.SmallerThan => relativeIntVal == -1,
                ComparisonType.SmallerOrEqual => relativeIntVal == -1 || relativeIntVal == 0,
                ComparisonType.GreaterOrEqual => relativeIntVal == 1 || relativeIntVal == 0,
                _ => true,
            };
        }
        else
        {
            switch (_comparedField.type)
            {
                case "bool":
                    return _drawIf.Comparison switch
                    {
                        ComparisonType.Equals => _comparedField.boolValue.Equals(_drawIf.ComparedValue),
                        ComparisonType.NotEqual => !_comparedField.boolValue.Equals(_drawIf.ComparedValue),
                        _ => true,
                    };
                case "Enum":
                    return _drawIf.Comparison switch
                    {
                        ComparisonType.Equals => _comparedField.enumValueIndex.Equals((int)_drawIf.ComparedValue),
                        ComparisonType.NotEqual => !_comparedField.enumValueIndex.Equals((int)_drawIf.ComparedValue),
                        _ => true,
                    };
                case "int":
                    int relativeIntVal = _comparedField.intValue.CompareTo((int)_drawIf.ComparedValue);
                    return _drawIf.Comparison switch
                    {
                        ComparisonType.Equals => relativeIntVal == 0,
                        ComparisonType.NotEqual => relativeIntVal != 0,
                        ComparisonType.GreaterThan => relativeIntVal == 1,
                        ComparisonType.SmallerThan => relativeIntVal == -1,
                        ComparisonType.SmallerOrEqual => relativeIntVal == -1 || relativeIntVal == 0,
                        ComparisonType.GreaterOrEqual => relativeIntVal == 1 || relativeIntVal == 0,
                        _ => true,
                    };
                case "float":
                    float relativeFloatVal = _comparedField.floatValue.CompareTo((float)_drawIf.ComparedValue);
                    return _drawIf.Comparison switch
                    {
                        ComparisonType.Equals => relativeFloatVal == 0,
                        ComparisonType.NotEqual => relativeFloatVal != 0,
                        ComparisonType.GreaterThan => relativeFloatVal == 1,
                        ComparisonType.SmallerThan => relativeFloatVal == -1,
                        ComparisonType.SmallerOrEqual => relativeFloatVal == -1 || relativeFloatVal == 0,
                        ComparisonType.GreaterOrEqual => relativeFloatVal == 1 || relativeFloatVal == 0,
                        _ => true,
                    };
                default:
                    Debug.LogError("Error: " + _comparedField.type + " is not supported of " + path);
                    return true;
            }
        }
    }

    public override void OnGUI(Rect pPosition, SerializedProperty pProperty, GUIContent pLabel)
    {
        // If the condition is met, simply draw the field.
        if (ShowMe(pProperty))
        {
            // A Generic type means a custom class...
            if (pProperty.propertyType == SerializedPropertyType.Generic)
            {
                IEnumerator children = pProperty.GetEnumerator();
                Rect offsetPosition = pPosition;
                while (children.MoveNext())
                {
                    SerializedProperty child = children.Current as SerializedProperty;
                    GUIContent childLabel = new GUIContent(child.displayName);

                    float childHeight = EditorGUI.GetPropertyHeight(child, childLabel);
                    offsetPosition.height = childHeight;
                    EditorGUI.PropertyField(offsetPosition, child, childLabel);
                    offsetPosition.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            else
            {
                EditorGUI.PropertyField(pPosition, pProperty, pLabel);
            }

        } //...check if the disabling type is read only. If it is, draw it disabled
        else if (_drawIf.DisablingType == DisablingType.ReadOnly)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(pPosition, pProperty, pLabel);
            GUI.enabled = true;
        }
    }
    #endregion
}
