using UnityEngine;

public enum ScannableImageType
{
    Chair,
    Heater,
    BucketOfManure,
    WateringCan,
}

public class ImageTypeIdentifier : MonoBehaviour
{
    #region Fields
    [SerializeField] private ScannableImageType _imageType;
    #endregion

    #region Methods
    private void Start()
    {
        GetComponent<DefaultTrackableEventHandler>().OnTargetFound.AddListener(() => ARManager.Instance.StartScanning(_imageType));
        GetComponent<DefaultTrackableEventHandler>().OnTargetLost.AddListener(() => ARManager.Instance.StopScanning());
    }
    #endregion
}
