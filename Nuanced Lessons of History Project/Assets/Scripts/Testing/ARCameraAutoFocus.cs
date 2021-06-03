using UnityEngine;
using Vuforia;

public class ARCameraAutoFocus : MonoBehaviour
{
    #region Fields
    private bool _vuforiaStarted = false;
    #endregion

    #region Methods
    private void Start()
    {
        VuforiaARController vuforia = VuforiaARController.Instance;

        if (vuforia != null)
            vuforia.RegisterVuforiaStartedCallback(startAfterVuforia);
    }

    private void startAfterVuforia()
    {
        _vuforiaStarted = true;
        setAutoFocus();
    }

    private void OnApplicationPause(bool pAuse)
    {
        if (pAuse) return;

        if (_vuforiaStarted)
            setAutoFocus();
    }

    private void setAutoFocus()
    {
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
        {
            Debug.Log("Autofocus set");
        }
        else
        {
            Debug.Log("this device doesn't support auto focus");
        }
    }
    #endregion
}
