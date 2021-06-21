using UnityEngine;
using UnityEngine.UI;

public class ARPanelSidebar : MonoBehaviour
{
    #region Fields
    [SerializeField] private Vector2 _collapsedSize;
    [SerializeField] private float _transitionDuration;
    private IExpandableUIElement[] _expandables;
    #endregion

    #region Methods
    private void Start()
    {
        _expandables = GetComponentsInChildren<IExpandableUIElement>(true);
        for (int i = 0; i < _expandables.Length; i++)
        {
            ExpandableSidebarElement sidebarElement = _expandables[i] as ExpandableSidebarElement;
            sidebarElement.Init();
            sidebarElement.GetComponent<Button>().onClick.AddListener(() => toggle(sidebarElement));
            sidebarElement.DummyButton.onClick.AddListener(() => toggle(sidebarElement));
        }
    }

    private void toggle(ExpandableSidebarElement pExpandableUIElement)
    {
        for (int i = 0; i < _expandables.Length; i++)
        {
            ExpandableSidebarElement element = _expandables[i] as ExpandableSidebarElement;
            if (pExpandableUIElement == element)
            {
                if (element.State == ExpandableSidebarElement.TransitionState.Expanding ||
                    element.State == ExpandableSidebarElement.TransitionState.Expanded)
                    element.Collapse(_collapsedSize, _transitionDuration);
                else
                    element.Expand(_transitionDuration);
            }
            else if (element != null)
            {
                if (element.State == ExpandableSidebarElement.TransitionState.Expanding ||
                    element.State == ExpandableSidebarElement.TransitionState.Expanded)
                    element.Collapse(_collapsedSize, _transitionDuration);
            }
        }
    }
    #endregion
}
