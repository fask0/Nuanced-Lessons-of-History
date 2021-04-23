using UnityEngine;

public class AssignListenerToEvent : MonoBehaviour
{
    public DefaultTrackableEventHandler[] trackableEventHandlers;

    void Start()
    {
        GameObject jeff = GameObject.Find("Jeff");

        if (!jeff) return;
        foreach (DefaultTrackableEventHandler handler in trackableEventHandlers)
            handler.OnTargetFound.AddListener(() => jeff.GetComponent<TestDialogue>().CheckTracker(handler.gameObject));
    }
}
