using UnityEngine;
using Zenject;

namespace Controllers
{
    public class CameraController : MonoBehaviour
    {
        [Header("Setup")]
        public Transform target; // Assign the Rubik's Cube root object here

        [Header("Controls")]
        public float rotationSpeed = 4.0f;
        public float pitchMin = -45f;
        public float pitchMax = 80f;

        private CubeControls _controls;
        private Camera _camera;
        private bool _isRotating = false;
        private float _yaw = 0.0f;
        private float _pitch = 0.0f;
        
        [SerializeField] private LayerMask cubie;

        [Inject]
        public void Construct(CubeControls controls)
        {
            _controls = controls;
        }

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _controls.Player.Enable();
            _controls.Player.Click.performed += _ => OnClick();
            _controls.Player.Click.canceled += _ => OnRelease();
        }

        void Start()
        {
            _yaw = 0;
            _pitch = 0;
        }

        private void OnClick()
        {
            // Raycast to see if we hit the cube. If not, we can rotate.
            Ray ray = _camera.ScreenPointToRay(_controls.Player.MousePosition.ReadValue<Vector2>());
            if (!Physics.Raycast(ray, 100f, cubie)) // Notice the '!' - this is true if we hit NOTHING
            {
                _isRotating = true;
            }
        }

        private void OnRelease()
        {
            _isRotating = false;
        }

        void LateUpdate()
        {
            if (target == null || !_isRotating) return;

            Vector2 mouseDelta = _controls.Player.MouseDelta.ReadValue<Vector2>();

            _yaw += mouseDelta.x * rotationSpeed * Time.deltaTime;
            _pitch -= mouseDelta.y * rotationSpeed * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -Vector3.Distance(transform.position, target.position));
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}