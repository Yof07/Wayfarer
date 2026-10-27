using UnityEngine;

namespace ithappy
{
    public class EditorLikeCameraController : MonoBehaviour
    {
        [Header("Movement Settings")] 
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _fastMoveMultiplier = 2f;
        [SerializeField] private float _rotationSpeed = 2f;

        [Header("Zoom Settings")] 
        [SerializeField] private float _zoomSpeed = 10f;
        [SerializeField] private float _minZoomDistance = 2f;
        [SerializeField] private float _maxZoomDistance = 50f;

        private Transform _cameraTransform;
        private Transform _pivot;
        private Vector3 _moveVector;
        private bool _isRotating;
        private float _rotationX; // Отслеживаем угол вращения по X отдельно

        private void Awake()
        {
            _pivot = new GameObject("Camera Pivot").transform;
            _pivot.position = transform.position;
            _pivot.rotation = transform.rotation;
            
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            _cameraTransform.SetParent(_pivot);
            _cameraTransform.localPosition = new Vector3(0, 0, -10f);
            _cameraTransform.LookAt(_pivot.position);

            _rotationX = _pivot.eulerAngles.x; // Инициализируем начальный угол
        }

        private void LateUpdate()
        {
            HandleMovement();
            HandleRotation();
            HandleZoom();
        }

        private void HandleMovement()
        {
            float speed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? _fastMoveMultiplier : 1f);
            
            _moveVector.x = Input.GetAxis("Horizontal");
            _moveVector.z = Input.GetAxis("Vertical");
            _moveVector.y = 0;
            
            _pivot.Translate(_moveVector * (speed * Time.deltaTime), Space.Self);
            
            if (Input.GetKey(KeyCode.Q))
            {
                _pivot.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);
            }

            if (Input.GetKey(KeyCode.E))
            {
                _pivot.Translate(Vector3.up * (speed * Time.deltaTime), Space.World);
            }
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _isRotating = true;
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _isRotating = false;
            }

            if (_isRotating)
            {
                float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed;

                // Вращение по Y (горизонталь) — работает как обычно
                float rotationY = _pivot.eulerAngles.y + mouseX;

                // Вращение по X (вертикаль) — с ограничением и плавностью
                _rotationX -= mouseY;
                _rotationX = Mathf.Clamp(_rotationX, -89f, 89f); // Ограничиваем угол

                // Применяем поворот
                _pivot.rotation = Quaternion.Euler(_rotationX, rotationY, 0);
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 zoomDirection = _cameraTransform.localPosition.normalized;
                float currentDistance = _cameraTransform.localPosition.magnitude;
                float newDistance = Mathf.Clamp(currentDistance - scroll * _zoomSpeed, _minZoomDistance, _maxZoomDistance);

                _cameraTransform.localPosition = zoomDirection * newDistance;
            }
        }
    }
}