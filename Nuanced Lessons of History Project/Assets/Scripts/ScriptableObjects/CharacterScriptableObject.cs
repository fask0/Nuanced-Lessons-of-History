using UnityEngine;

public enum ExpressionType
{
    Idle,
    Happy,
    Upset
}

[System.Serializable]
public class Expression
{
    #region Fields
    [SerializeField] private ExpressionType _typeOfExpression;
    [SerializeField] private Sprite _sprite;
    #endregion

    #region Properties
    public ExpressionType TypeOfExpression => _typeOfExpression;
    public Sprite Sprite => _sprite;
    #endregion
}

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character")]
public class CharacterScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private string _name = "newCharacter";
    [SerializeField] private Expression[] _expressions;
    #endregion

    #region Properties
    public string Name => _name;
    #endregion

    #region Methods
    public Sprite GetExpression(ExpressionType pExpressionType)
    {
        foreach (Expression expression in _expressions)
            if (expression.TypeOfExpression == pExpressionType)
                return expression.Sprite;

        Debug.LogError("Failed to find " + _name + "'s " + pExpressionType.ToString() + " expression.");
        return null;
    }
    #endregion
}
