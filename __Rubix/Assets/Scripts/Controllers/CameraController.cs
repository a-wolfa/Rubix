using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Controllers
{
    public class CameraController : MonoBehaviour
    {
        [Header("Setup")]
        public Transform target;

        [Header("Controls")]
        public float rotationSpeed = 4.0f;
        public float pitchMin = -45f;
        public float pitchMax = 80f;
        
        [Header("Smoothing")]
        public float smoothTime = 0.1f;
        public float mouseSensitivity = 1.0f;
        public AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CubeControls _controls;
        private Camera _camera;
        private bool _isRotating = false;
        
        private float _yaw = 0.0f;
        private float _pitch = 0.0f;
        
        private float _targetYaw = 0.0f;
        private float _targetPitch = 0.0f;
        
        private float _yawVelocity = 0.0f;
        private float _pitchVelocity = 0.0f;

        private float _currentDistance;
        private Vector3 _negDistanceVector;
        
        private RaycastHit _raycastHit = new RaycastHit();

        private const float RaycastDistance = 100f;

        private Action<InputAction.CallbackContext> _clickPerformed;
        private Action<InputAction.CallbackContext> _clickCancelled;
        
        [SerializeField] private LayerMask cubie;

        [Inject]
        public void Construct(CubeControls controls)
        {
            _controls = controls;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            
            _clickPerformed = OnClickPerformed;
            _clickCancelled = OnClickCancelled;
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Click.performed += _clickPerformed;
            _controls.Player.Click.canceled += _clickCancelled;
        }

        private void OnDisable()
        {
            _controls.Player.Click.performed -= _clickPerformed;
            _controls.Player.Click.canceled -= _clickCancelled;
            _controls.Player.Disable();
        }

        private void Start()
        {
            var eulerAngles = transform.rotation.eulerAngles;
            _yaw = eulerAngles.y;
            _pitch = eulerAngles.x;
            
            _targetYaw = _yaw;
            _targetPitch = _pitch;
            
            if (target != null)
            {
                _currentDistance = Vector3.Distance(transform.position, target.position);
                _negDistanceVector = new Vector3(0.0f, 0.0f, -_currentDistance);
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 mousePos = _controls.Player.MousePosition.ReadValue<Vector2>();
            Ray ray = _camera.ScreenPointToRay(mousePos);
            
            if (!Physics.Raycast(ray, out _raycastHit, RaycastDistance, cubie))
            {
                _isRotating = true;
            }
        }

        private void OnClickCancelled(InputAction.CallbackContext ctx)
        {
            _isRotating = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (_isRotating)
            {
                Vector2 mouseDelta = _controls.Player.MouseDelta.ReadValue<Vector2>();
                
                
                float deltaScale = mouseSensitivity * rotationSpeed * Time.unscaledDeltaTime;
                
                _targetYaw += mouseDelta.x * deltaScale;
                _targetPitch -= mouseDelta.y * deltaScale;
                _targetPitch = Mathf.Clamp(_targetPitch, pitchMin, pitchMax);
            }
            
            float newYaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVelocity, smoothTime);
            float newPitch = Mathf.SmoothDampAngle(_pitch, _targetPitch, ref _pitchVelocity, smoothTime);
            
            
            if (!Mathf.Approximately(newYaw, _yaw) || !Mathf.Approximately(newPitch, _pitch))
            {
                _yaw = newYaw;
                _pitch = newPitch;
                
                Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
                
                _negDistanceVector.Set(0.0f, 0.0f, -_currentDistance);
                
                Vector3 position = rotation * _negDistanceVector + target.position;
                
                transform.SetPositionAndRotation(position, rotation);
            }
        }
        
        public void RecalculateDistance()
        {
            if (target != null)
            {
                _currentDistance = Vector3.Distance(transform.position, target.position);
            }
        }
    }
}