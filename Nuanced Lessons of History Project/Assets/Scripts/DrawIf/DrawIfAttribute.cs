using UnityEngine;
using System;

/// <summary>
/// Types of comperisons.
/// </summary>
public enum ComparisonType
{
    Equals = 1,
    NotEqual = 2,
    GreaterThan = 3,
    SmallerThan = 4,
    SmallerOrEqual = 5,
    GreaterOrEqual = 6
}


/// <summary>
/// The ways you can disable a field when using the DrawIf attribute and the condition isn't met.
/// </summary>
public enum DisablingType
{
    ReadOnly = 2,
    DontDraw = 3
}

/// <summary>
/// Draws the field/property ONLY if the copared property compared by the comparison type with the value of comparedValue returns true.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{
    public string ComparedPropertyName { get; private set; }
    public object ComparedValue { get; private set; }
    public ComparisonType Comparison { get; private set; }
    public DisablingType DisablingType { get; private set; }

    /// <summary>
    /// Only draws the field only if a condition is met. Supports enum and bools.
    /// </summary>
    /// <param name="pComparedPropertyName">The name of the property that is being compared (case sensitive).</param>
    /// <param name="pComparedValue">The value the property is being compared to.</param>
    /// <param name="pDisablingType">The type of disabling that should happen if the condition is NOT met. Defaulted to DisablingType.DontDraw.</param>
    public DrawIfAttribute(string pComparedPropertyName, object pComparedValue, ComparisonType pComparisonType = ComparisonType.Equals, DisablingType pDisablingType = DisablingType.DontDraw)
    {
        ComparedPropertyName = pComparedPropertyName;
        ComparedValue = pComparedValue;
        Comparison = pComparisonType;
        DisablingType = pDisablingType;
    }
}
