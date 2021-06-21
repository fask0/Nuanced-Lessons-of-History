using UnityEngine;

public interface IExpandableUIElement
{
    public void Expand(Vector2 pTargetSize, float pTransitionDuration);
    public void Expand(float pTransitionDuration);
    public void Collapse(Vector2 pTargetSize, float pTransitionDuration);
    public void Collapse(float pTransitionDuration);
}
