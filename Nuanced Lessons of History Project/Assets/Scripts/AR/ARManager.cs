using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine;
using UnityEngine.UI;

public class ARManager : Singleton<ARManager>
{
    #region Fields
    [Header("Main")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _vnPanel;

    [Header("AR")]
    [SerializeField] private Camera _arCamera;
    [SerializeField] private GameObject _arPanel;
    [SerializeField] private GameObject _vuforiaBank;
    [SerializeField] private GameObject _cameraReticleContainer;
    [SerializeField] private GameObject _progressBarContainer;
    [SerializeField] private float _scanTime;
    [SerializeField] private Color _correctFeedbackColor;
    [SerializeField] private Color _wrongFeedbackColor;

    [Header("Sidebar")]
    [SerializeField] private LocalizeStringEvent _infoLocalizedStringEvent;
    [SerializeField] private LocalizeStringEvent _hintLocalizedStringEvent;

    private ScannableImageScriptableObject[] _imagesToScan;
    private Image[] _cameraReticles;
    private Image[] _progressBars;
    private int _imagesToScanProgress = 0;
    private Coroutine _scannig = null;
    private bool _shouldFinishScanning = false;
    #endregion

    #region Methods
    private void Start()
    {
        _cameraReticles = _cameraReticleContainer.GetComponentsInChildren<Image>();
        _progressBars = _progressBarContainer.GetComponentsInChildren<Image>();
        _progressBarContainer.gameObject.SetActive(false);
    }

    public void PrepareImagesToScan(ScannableImageScriptableObject[] pImagesToScan, int pImagesToScanProgress = 0)
    {
        enableAR();
        _imagesToScan = pImagesToScan;
        _imagesToScanProgress = pImagesToScanProgress;
        _infoLocalizedStringEvent.StringReference = _imagesToScan[_imagesToScanProgress].Info;
        _hintLocalizedStringEvent.StringReference = _imagesToScan[_imagesToScanProgress].Hint;
    }

    public void StartScanning(ScannableImageType pScannableImageType)
    {
        if (_scannig != null)
        {
            StopCoroutine(_scannig);
            _scannig = null;
        }

        _scannig = StartCoroutine(scan(pScannableImageType));
    }

    public void CancelScan()
    {
        if (_scannig != null)
        {
            StopCoroutine(_scannig);
            _progressBarContainer.SetActive(false);
            _shouldFinishScanning = false;
            _scannig = null;
        }
        disableAR();
        DialogueManager.Instance.Repeat();
    }

    public void StopScanning()
    {
        _shouldFinishScanning = true;
    }

    private IEnumerator scan(ScannableImageType pScannableImageType)
    {
        _progressBarContainer.SetActive(true);
        setImageArrayFillProgress(_progressBars, 0);
        setImageArrayColor(_progressBars, Color.white);
        setImageArrayColor(_cameraReticles, Color.white);
        _shouldFinishScanning = false;

        Vector2[] sideModifiers = new Vector2[_progressBars.Length];
        for (int i = 0; i < _progressBars.Length; i++)
            sideModifiers[i] = new Vector2(1, 1).normalized / _progressBars[i].GetComponent<RectTransform>().rect.size.normalized;
        bool scanComplete = false;
        while (!_shouldFinishScanning)
        {
            float fillAmountIncrement = (Time.deltaTime / _scanTime) / _progressBars.Length;
            for (int i = 0; i < _progressBars.Length; i++)
                _progressBars[i].fillAmount += fillAmountIncrement * normalizedRactangleRadialFillModifier(_progressBars[i].fillOrigin, _progressBars[i].fillAmount, sideModifiers[i]);

            float totalFill = _progressBars[0].fillAmount * _progressBars.Length;
            if (totalFill >= 1) { scanComplete = true; break; }
            yield return new WaitForEndOfFrame();
        }

        if (scanComplete && _imagesToScan[_imagesToScanProgress].ImageType == pScannableImageType)
        {
            setImageArrayColor(_progressBars, _correctFeedbackColor);
            setImageArrayColor(_cameraReticles, _correctFeedbackColor);

            yield return new WaitForSeconds(1);

            if (_imagesToScanProgress + 1 < _imagesToScan.Length)
            {
                _imagesToScanProgress++;
                PrepareImagesToScan(_imagesToScan, _imagesToScanProgress);
            }
            else
            {
                disableAR();
                DialogueManager.Instance.Resume();
            }
        }
        else
        {
            setImageArrayColor(_progressBars, _wrongFeedbackColor);
            setImageArrayColor(_cameraReticles, _wrongFeedbackColor);
            yield return new WaitForSeconds(0.2f);
        }

        setImageArrayColor(_progressBars, Color.white);
        setImageArrayColor(_cameraReticles, Color.white);
        _progressBarContainer.SetActive(false);
        _scannig = null;
        _shouldFinishScanning = false;

        #region Local Methods
        float normalizedRactangleRadialFillModifier(int pOrigin, float pFillAmount, Vector2 pSideModifiers)
        {
            if (pOrigin == 0 || pOrigin == 2) //Bottom or Top
            {
                if (pFillAmount < 0.125f) return pSideModifiers.x;
                else if (pFillAmount < 0.375f) return pSideModifiers.y;
                else if (pFillAmount < 0.625f) return pSideModifiers.x;
                else if (pFillAmount < 0.875f) return pSideModifiers.y;
                else if (pFillAmount < 1) return pSideModifiers.x;
            }
            else if (pOrigin == 1 || pOrigin == 3)
            {
                if (pFillAmount < 0.125f) return pSideModifiers.y;
                else if (pFillAmount < 0.375f) return pSideModifiers.x;
                else if (pFillAmount < 0.625f) return pSideModifiers.y;
                else if (pFillAmount < 0.875f) return pSideModifiers.x;
                else if (pFillAmount < 1) return pSideModifiers.y;
            }

            return 1;
        }

        void setImageArrayColor(Image[] pImageArray, Color pColor)
        {
            for (int i = 0; i < pImageArray.Length; i++)
                pImageArray[i].color = pColor;
        }

        void setImageArrayFillProgress(Image[] pImageArray, float pFillAmount)
        {
            for (int i = 0; i < pImageArray.Length; i++)
                pImageArray[i].fillAmount = pFillAmount;
        }
        #endregion
    }

    private void enableAR()
    {
        _mainCamera.gameObject.SetActive(false);
        _vnPanel.SetActive(false);

        _arCamera.gameObject.SetActive(true);
        _arPanel.SetActive(true);
        _vuforiaBank.SetActive(true);
    }

    private void disableAR()
    {
        _arCamera.gameObject.SetActive(false);
        _arPanel.SetActive(false);
        _vuforiaBank.SetActive(false);

        _mainCamera.gameObject.SetActive(true);
        _vnPanel.SetActive(true);
    }
    #endregion
}
