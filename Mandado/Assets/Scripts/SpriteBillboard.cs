using System;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    private Camera targetCamera;
    private bool _initialized = false;
    private Quaternion _lastRotation;

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.Initialized)
        {
            Initialize();
        }
        else
        {
            GameManager.InitializedEvent += Initialize;
        }
    }

    private void LateUpdate()
    {
        if (_initialized && targetCamera.transform.rotation != _lastRotation)
        {
            _lastRotation = targetCamera.transform.rotation;
            ForceBillboardUpdate();
        }
    }

    private void Initialize()
    {
        GameManager.InitializedEvent -= Initialize;

        if (GameManager.Instance != null)
        {
            targetCamera = GameManager.Instance.Camera;
            UpdateBillboard(targetCamera.transform);
            _initialized = true;
        }
        
    }

    private void OnEnable()
    {
        if(_initialized)
        {
            //targetCamera.OnCameraRotation += UpdateBillboard;
        }
    }
    
    private void OnDisable()
    {
        if(_initialized)
        {
            //targetCamera.OnCameraRotation -= UpdateBillboard;
        }
    }
    
    private void OnDestroy()
    {
        if(targetCamera != null)
        {
            //targetCamera.OnCameraRotation -= UpdateBillboard;
        }
    }

    public void ForceBillboardUpdate()
    {
        if(targetCamera != null)
        {
            UpdateBillboard(targetCamera.transform);
        }
    }

    private void UpdateBillboard(Transform cam)
    {
        if (cam == null || targetCamera == null || !gameObject.activeSelf)
        {
            return;
        }

        
        
        transform.eulerAngles = cam.transform.eulerAngles;
    }
}