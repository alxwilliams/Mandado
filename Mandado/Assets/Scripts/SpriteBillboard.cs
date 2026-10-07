using System;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _trueLookDirection;

    private Sprite _frontSprite;
    private Sprite _backSprite;
    private Camera targetCamera;
    private bool _initialized = false;
    private Quaternion _lastRotation;

    public float TrueLookDirection
    {
        get => _trueLookDirection;
        set => _trueLookDirection = value;
    }

    public Sprite FrontSprite
    {
        get => _frontSprite;
        set => _frontSprite = value;
    }

    public Sprite BackSprite
    {
        get => _backSprite;
        set => _backSprite = value;
    }

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
        /*if (_initialized && targetCamera.transform.rotation != _lastRotation)
        {
            _lastRotation = targetCamera.transform.rotation;
            ForceBillboardUpdate();
        }*/
    }


    private void Initialize()
    {
        GameManager.InitializedEvent -= Initialize;

        if (GameManager.Instance != null)
        {
            targetCamera = GameManager.Instance.MainCamera;
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
        GameManager.InitializedEvent -= Initialize;
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
        if (gameObject == null || cam == null || targetCamera == null || !gameObject.activeSelf)
        {
            return;
        }

        transform.eulerAngles = cam.transform.eulerAngles;

        if (_backSprite != null)
        {

            float facingRad = _trueLookDirection * Mathf.Deg2Rad;
            Vector3 facingDirection = new Vector3(Mathf.Sin(facingRad), 0f, Mathf.Cos(facingRad));

            Vector3 camDirection = (cam.position - transform.position).normalized;
            camDirection.y = 0f;

            float dot = Vector3.Dot(facingDirection, camDirection);

            if (dot > 0f)
            {
                _spriteRenderer.sprite = _frontSprite;
            }
            else
            {
                _spriteRenderer.sprite = _backSprite;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        Gizmos.color = new Color(1, 0, 1, 1f);
        
        Vector3 lookVector = Quaternion.Euler(0f, _trueLookDirection, 0f) * Vector3.forward;

        Gizmos.DrawLine(position, position + lookVector * 100f);

    }
}