using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Localization;

[System.Serializable]
public class Expression
{
    public enum ExpressionType
    {
        Idle,
        Happy,
        Upset
    }

    #region Fields
    [SerializeField] private ExpressionType _typeOfExpression;
    [SerializeField] private Sprite _sprite;
    #endregion

    #region Properties
    public ExpressionType TypeOfExpression => _typeOfExpression;
    public Sprite Sprite => _sprite;
    #endregion
}

[CreateAssetMenu(fileName = "C_Character", menuName = "Scriptable Objects/Character")]
public class CharacterScriptableObject : ScriptableObject
{
    public enum CharacterNameToUse
    {
        DisplayName,
        FullName,
        UnknownName
    }

    #region Fields
    [SerializeField] private bool _isNameLocalized = false;
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized", true)] [SerializeField] private string _fullName = "fullName";
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized")] [SerializeField] private LocalizedString _localizedFullName;
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized", true)] [SerializeField] private string _displayName = "displayName";
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized")] [SerializeField] private LocalizedString _localizedDisplayName;
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized", true)] [SerializeField] private string _unknownName = "unknownName";
    [HorizontalLine(1)]
    [ShowIf("_isNameLocalized")] [SerializeField] private LocalizedString _localizedUnknownName;
    [HorizontalLine(1)]
    [SerializeField] private Expression[] _expressions;
    #endregion

    #region Properties
    public bool IsNameLocalized => _isNameLocalized;
    #endregion

    #region Methods
    public Sprite GetExpression(Expression.ExpressionType pExpressionType)
    {
        foreach (Expression expression in _expressions)
            if (expression.TypeOfExpression == pExpressionType)
                return expression.Sprite;

        return null;
    }

    public LocalizedString GetLocalizedName(CharacterNameToUse pNameToUse)
    {
        return pNameToUse switch
        {
            CharacterNameToUse.DisplayName => _localizedDisplayName,
            CharacterNameToUse.FullName => _localizedFullName,
            CharacterNameToUse.UnknownName => _localizedUnknownName,
            _ => null
        };
    }

    public string GetName(CharacterNameToUse pNameToUse)
    {
        return pNameToUse switch
        {
            CharacterNameToUse.DisplayName => _displayName,
            CharacterNameToUse.FullName => _fullName,
            CharacterNameToUse.UnknownName => _unknownName,
            _ => ""
        };
    }
    #endregion
}
