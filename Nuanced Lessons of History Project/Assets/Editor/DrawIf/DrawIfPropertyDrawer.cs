using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
    // Reference to the attribute on the property.
    DrawIfAttribute drawIf;

    // Field that is being compared.
    SerializedProperty comparedField;

    public override float GetPropertyHeight(SerializedProperty pProperty, GUIContent pLabel)
    {
        {
            if (!ShowMe(pProperty) && drawIf.DisablingType == DisablingType.DontDraw)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                if (pProperty.propertyType == SerializedPropertyType.Generic)
                {
                    int numChildren = 0;
                    float totalHeight = 0.0f;

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

                return EditorGUI.GetPropertyHeight(pProperty, pLabel);
            }
        }
    }

    /// <summary>
    /// Errors default to showing the property.
    /// </summary>
    private bool ShowMe(SerializedProperty pProperty)
    {
        drawIf = attribute as DrawIfAttribute;
        // Replace propertyname to the value from the parameter
        string path = pProperty.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(pProperty.propertyPath, drawIf.ComparedPropertyName) : drawIf.ComparedPropertyName;

        comparedField = pProperty.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("Cannot find property with name: " + path);
            return true;
        }

        // get the value & compare based on types
        switch (comparedField.type)
        { // Possible extend cases to support your own type
            case "bool":
                return comparedField.boolValue.Equals(drawIf.ComparedValue) != drawIf.Reversed;
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)drawIf.ComparedValue) != drawIf.Reversed;
            case "int":
                return comparedField.intValue.Equals((int)drawIf.ComparedValue) != drawIf.Reversed;
            default:
                Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                return true;
        }
    }

    public override void OnGUI(Rect pPosition, SerializedProperty pRoperty, GUIContent pLabel)
    {
        // If the condition is met, simply draw the field.
        if (ShowMe(pRoperty))
        {
            // A Generic type means a custom class...
            if (pRoperty.propertyType == SerializedPropertyType.Generic)
            {
                IEnumerator children = pRoperty.GetEnumerator();

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
                EditorGUI.PropertyField(pPosition, pRoperty, pLabel);
            }

        } //...check if the disabling type is read only. If it is, draw it disabled
        else if (drawIf.DisablingType == DisablingType.ReadOnly)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(pPosition, pRoperty, pLabel);
            GUI.enabled = true;
        }
    }
}
