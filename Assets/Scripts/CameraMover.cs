using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    // カメラの移動量
    [SerializeField, Range(0.1f, 10.0f)]
    private float _positionStep = 2.0f;

    // マウス感度
    [SerializeField, Range(30.0f, 150.0f)]
    private float _mouseSensitive = 90.0f;

    // カメラ操作の有効/無効
    private bool _cameraMoveActive = true;
    private Transform _camTransform;
    private Vector3 _startMousePos;
    private Vector3 _presentCamRotation;
    private Vector3 _presentCamPos;
    private Quaternion _initialCamRotation;
    private bool _uiMessageActive;

    // Start is called before the first frame update
    void Start()
    {
        _camTransform = this.gameObject.transform;
        _initialCamRotation = this.gameObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        CamControlIsActive();

        if (_cameraMoveActive)
        {
            ResetCameraRotation();
            CameraRotationMouseControl();
            CameraSlideMouseControl();
            CameraPositionKeyControl();
        }
    }

    public void CamControlIsActive()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _cameraMoveActive = !_cameraMoveActive;
            if (!_uiMessageActive)
            {
                StartCoroutine(DisplayUIMessage());
            }
            Debug.Log("camcontrol:: " + _cameraMoveActive);
        }
    }

    private void ResetCameraRotation()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            this.gameObject.transform.rotation = _initialCamRotation;
            Debug.Log("Cam Rotate: " + _initialCamRotation.ToString());
        }

    }

    private void CameraRotationMouseControl()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _startMousePos = Input.mousePosition;
            _presentCamRotation.x = _camTransform.transform.eulerAngles.x;
            _presentCamRotation.y = _camTransform.transform.eulerAngles.y;
        }
        if(Input.GetMouseButton(0))
        {
            // (移動開始座標 - 現在の座標) / 解像度 で正規化
            float x = -(_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (_startMousePos.y - Input.mousePosition.y) / Screen.height;

            // 回転開始角度 + マウスの変化量 * マウス感度
            float eulerX = _presentCamRotation.x + y * _mouseSensitive;
            float eulerY = _presentCamRotation.y + x * _mouseSensitive;

            _camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
        }
    }

    private void CameraSlideMouseControl()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _startMousePos = Input.mousePosition;
            _presentCamPos = _camTransform.position;
        }

        if(Input.GetMouseButton(1))
        {
            // (移動開始座標 - 現在座標) / 解像度 で正規化
            float x = (_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (_startMousePos.y - Input.mousePosition.y) / Screen.height;

            x = x * _positionStep;
            y = y * _positionStep;

            Vector3 v = _camTransform.rotation * new Vector3(x, y, 0);
            _camTransform.position = v + _presentCamPos;
        }
    }

    private void CameraPositionKeyControl()
    {
        Vector3 campos = _camTransform.position;
        if (Input.GetKey(KeyCode.D))
        {
            campos += _camTransform.right * Time.deltaTime * _positionStep;
        }
        if (Input.GetKey(KeyCode.A))
        {
            campos -= _camTransform.right * Time.deltaTime * _positionStep;
        }
        if (Input.GetKey(KeyCode.E))
        {
            campos += _camTransform.up * Time.deltaTime * _positionStep;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            campos -= _camTransform.up * Time.deltaTime * _positionStep;
        }
        if (Input.GetKey(KeyCode.W))
        {
            campos += _camTransform.forward * Time.deltaTime * _positionStep;
        }
        if (Input.GetKey(KeyCode.S))
        {
            campos -= _camTransform.forward * Time.deltaTime * _positionStep;
        }
        _camTransform.position = campos;
    }

    private IEnumerator DisplayUIMessage()
    {
        _uiMessageActive = true;
        float time = 0;
        while(time < 2)
        {
            time += Time.deltaTime;
            yield return null;
        }
        _uiMessageActive = false;
    }

    private void OnGUI()
    {
        if (!_uiMessageActive)
        {
            return;
        }
        GUI.color = Color.black;
        if (_cameraMoveActive)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作: 有効");
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作: 無効");
        }
    }
}
