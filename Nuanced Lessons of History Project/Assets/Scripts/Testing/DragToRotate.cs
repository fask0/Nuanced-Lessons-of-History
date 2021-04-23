using UnityEngine;

public class DragToRotate : MonoBehaviour
{
    private Vector3 _mPrevPos = Vector3.zero;
    private Vector3 _mPosDelta = Vector3.zero;

    private bool _canUpdate;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                if (hit.transform.gameObject == transform.gameObject)
                    _canUpdate = true;
            _mPrevPos = Input.mousePosition;
        }

        if (!_canUpdate) return;

        _mPosDelta = Input.mousePosition - _mPrevPos;
        transform.Rotate(transform.up, Vector3.Dot(_mPosDelta, Camera.main.transform.right) * ((Vector3.Dot(transform.up, Vector3.up) >= 0) ? -1 : 1), Space.World);
        transform.Rotate(Camera.main.transform.right, Vector3.Dot(_mPosDelta, Camera.main.transform.up), Space.World);
        _mPrevPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0)) _canUpdate = false;
    }
}
