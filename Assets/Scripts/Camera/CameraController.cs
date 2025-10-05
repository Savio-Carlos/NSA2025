using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nasa.Fazendas.CameraSystem
{
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float fastPanMultiplier = 2f;
        [SerializeField] private bool useEdgeScrolling = false;
        [SerializeField, Min(0f)] private float edgeScrollMargin = 16f;

        [Header("Rotation & Tilt")]
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float tiltSpeed = 90f;
        [SerializeField] private Vector2 tiltLimits = new Vector2(20f, 80f);

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 200f;
        [SerializeField] private Vector2 zoomRange = new Vector2(15f, 120f);

        [Header("Bounds")]
        [SerializeField] private bool useMovementBounds = false;
        [SerializeField] private Bounds movementBounds = new Bounds(Vector3.zero, new Vector3(200f, 0f, 200f));

        [Header("Ground Alignment (optional)")]
        [SerializeField] private bool alignToGround = false;
        [SerializeField] private LayerMask groundLayerMask = Physics.DefaultRaycastLayers;
        [SerializeField] private float groundRaycastHeight = 1000f;
        [SerializeField] private float groundOffset = 2f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference panAction;
        [SerializeField] private InputActionReference rotateAction;
        [SerializeField] private InputActionReference zoomAction;
        [SerializeField] private InputActionReference fastPanAction;
        [SerializeField] private InputActionReference rotateModifierAction;
        [SerializeField] private InputActionReference tiltModifierAction;
        [SerializeField] private InputActionReference pointerPositionAction;

        public event Action<Vector3> PositionChanged;
        public event Action<Vector3> RotationChanged;

        private Camera cameraComponent;
        private Transform cachedTransform;

        private Vector3 panInput;
        private float rotationInput;
        private float tiltInput;
        private float zoomInput;
        private bool isFastPanning;
        private float currentTilt;

        public Camera ControlledCamera => cameraComponent;
        public Transform CachedTransform => cachedTransform;

        private void OnValidate()
        {
            if (tiltLimits.y < tiltLimits.x)
            {
                float minTilt = tiltLimits.y;
                tiltLimits.y = tiltLimits.x;
                tiltLimits.x = minTilt;
            }

            if (zoomRange.y < zoomRange.x)
            {
                float minZoom = zoomRange.y;
                zoomRange.y = zoomRange.x;
                zoomRange.x = minZoom;
            }

            panSpeed = Mathf.Max(0f, panSpeed);
            fastPanMultiplier = Mathf.Max(1f, fastPanMultiplier);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            tiltSpeed = Mathf.Max(0f, tiltSpeed);
            zoomSpeed = Mathf.Max(0f, zoomSpeed);
            edgeScrollMargin = Mathf.Max(0f, edgeScrollMargin);
            groundRaycastHeight = Mathf.Max(1f, groundRaycastHeight);
            groundOffset = Mathf.Max(0f, groundOffset);
        }

        private void Awake()
        {
            cachedTransform = transform;
            cameraComponent = GetComponent<Camera>();
            currentTilt = Mathf.Clamp(NormalizeAngle(cachedTransform.eulerAngles.x), tiltLimits.x, tiltLimits.y);
        }

        private void OnEnable()
        {
            if (controlsEnabled)
            {
                EnableActions();
            }
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void Update()
        {
            GatherMovementInput();
            GatherRotationInput();
            GatherZoomInput();
        }

        private void LateUpdate()
        {
            ApplyRotation();
            ApplyMovementAndZoom();
        }

        public void SetControlsEnabled(bool enabled)
        {
            if (controlsEnabled == enabled)
            {
                return;
            }

            controlsEnabled = enabled;
            if (controlsEnabled)
            {
                if (isActiveAndEnabled)
                {
                    EnableActions();
                }
            }
            else
            {
                DisableActions();
                panInput = Vector3.zero;
                rotationInput = 0f;
                tiltInput = 0f;
                zoomInput = 0f;
            }
        }

        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set => SetControlsEnabled(value);
        }

        private bool controlsEnabled = true;

        private void GatherMovementInput()
        {
            if (!controlsEnabled)
            {
                panInput = Vector3.zero;
                isFastPanning = false;
                return;
            }

            Vector2 panValue = ReadVector2(panAction);
            var input = new Vector3(panValue.x, 0f, panValue.y);

            if (useEdgeScrolling)
            {
                Vector3 edgeInput = GetEdgeScrollInput();
                input.x = Mathf.Approximately(input.x, 0f) ? edgeInput.x : input.x;
                input.z = Mathf.Approximately(input.z, 0f) ? edgeInput.z : input.z;
            }

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            panInput = input;
            isFastPanning = IsActionPressed(fastPanAction);
        }

        private void GatherRotationInput()
        {
            if (!controlsEnabled)
            {
                rotationInput = 0f;
                tiltInput = 0f;
                return;
            }

            Vector2 rotationValue = ReadVector2(rotateAction);
            bool rotateModifierPressed = !HasAction(rotateModifierAction) || IsActionPressed(rotateModifierAction);
            bool tiltModifierPressed = !HasAction(tiltModifierAction) || IsActionPressed(tiltModifierAction);

            rotationInput = rotateModifierPressed ? rotationValue.x : 0f;
            tiltInput = tiltModifierPressed ? rotationValue.y : 0f;
        }

        private void GatherZoomInput()
        {
            if (!controlsEnabled)
            {
                zoomInput = 0f;
                return;
            }

            zoomInput = ReadFloat(zoomAction);
        }

        private void ApplyRotation()
        {
            if (!controlsEnabled)
            {
                return;
            }

            float yaw = cachedTransform.eulerAngles.y;

            if (Mathf.Abs(rotationInput) > Mathf.Epsilon)
            {
                yaw += rotationInput * rotationSpeed * Time.deltaTime;
            }

            if (Mathf.Abs(tiltInput) > Mathf.Epsilon)
            {
                currentTilt = Mathf.Clamp(currentTilt - tiltInput * tiltSpeed * Time.deltaTime, tiltLimits.x, tiltLimits.y);
            }

            Vector3 newEuler = new Vector3(currentTilt, yaw, 0f);
            Quaternion oldRotation = cachedTransform.rotation;
            cachedTransform.rotation = Quaternion.Euler(newEuler);

            if (cachedTransform.rotation != oldRotation)
            {
                RotationChanged?.Invoke(cachedTransform.eulerAngles);
            }
        }

        private void ApplyMovementAndZoom()
        {
            if (!controlsEnabled)
            {
                return;
            }

            Vector3 position = cachedTransform.position;

            if (panInput.sqrMagnitude > Mathf.Epsilon)
            {
                Vector3 right = cachedTransform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 forward = cachedTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                float speed = panSpeed * (isFastPanning ? fastPanMultiplier : 1f);
                position += (right * panInput.x + forward * panInput.z) * speed * Time.deltaTime;
            }

            if (Mathf.Abs(zoomInput) > Mathf.Epsilon)
            {
                float targetHeight = Mathf.Clamp(position.y - zoomInput * zoomSpeed * Time.deltaTime, zoomRange.x, zoomRange.y);
                position.y = targetHeight;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, zoomRange.x, zoomRange.y);
            }

            if (alignToGround)
            {
                position.y = Mathf.Max(position.y, SampleGroundHeight(position) + groundOffset);
            }

            if (useMovementBounds && movementBounds.size != Vector3.zero)
            {
                Vector3 min = movementBounds.min;
                Vector3 max = movementBounds.max;
                position.x = Mathf.Clamp(position.x, min.x, max.x);
                position.z = Mathf.Clamp(position.z, min.z, max.z);
            }

            if (position != cachedTransform.position)
            {
                cachedTransform.position = position;
                PositionChanged?.Invoke(position);
            }
        }

        private Vector3 GetEdgeScrollInput()
        {
            if (!useEdgeScrolling || edgeScrollMargin <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 result = Vector3.zero;
            if (!TryGetPointerPosition(out Vector2 pointerPosition))
            {
                return Vector3.zero;
            }

            if (pointerPosition.x <= edgeScrollMargin)
            {
                result.x = -1f;
            }
            else if (pointerPosition.x >= Screen.width - edgeScrollMargin)
            {
                result.x = 1f;
            }

            if (pointerPosition.y <= edgeScrollMargin)
            {
                result.z = -1f;
            }
            else if (pointerPosition.y >= Screen.height - edgeScrollMargin)
            {
                result.z = 1f;
            }

            if (result.sqrMagnitude > 1f)
            {
                result.Normalize();
            }

            return result;
        }

        private float SampleGroundHeight(Vector3 position)
        {
            float startHeight = groundRaycastHeight;
            if (position.y + groundOffset > startHeight)
            {
                startHeight = position.y + groundOffset;
            }

            Vector3 origin = new Vector3(position.x, startHeight, position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRaycastHeight * 2f, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point.y;
            }

            return position.y;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }
            else if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }

        private void EnableActions()
        {
            EnableAction(panAction);
            EnableAction(rotateAction);
            EnableAction(zoomAction);
            EnableAction(fastPanAction);
            EnableAction(rotateModifierAction);
            EnableAction(tiltModifierAction);
            EnableAction(pointerPositionAction);
        }

        private void DisableActions()
        {
            DisableAction(panAction);
            DisableAction(rotateAction);
            DisableAction(zoomAction);
            DisableAction(fastPanAction);
            DisableAction(rotateModifierAction);
            DisableAction(tiltModifierAction);
            DisableAction(pointerPositionAction);
        }

        private static void EnableAction(InputActionReference reference)
        {
            InputAction action = GetAction(reference);
            if (action != null && !action.enabled)
            {
                action.Enable();
            }
        }

        private static void DisableAction(InputActionReference reference)
        {
            InputAction action = GetAction(reference);
            if (action != null && action.enabled)
            {
                action.Disable();
            }
        }

        private static InputAction GetAction(InputActionReference reference)
        {
            return reference != null ? reference.action : null;
        }

        private static bool HasAction(InputActionReference reference)
        {
            return GetAction(reference) != null;
        }

        private static bool IsActionPressed(InputActionReference reference)
        {
            InputAction action = GetAction(reference);
            return action != null && action.enabled && action.IsPressed();
        }

        private static Vector2 ReadVector2(InputActionReference reference)
        {
            InputAction action = GetAction(reference);
            return action != null && action.enabled ? action.ReadValue<Vector2>() : Vector2.zero;
        }

        private static float ReadFloat(InputActionReference reference)
        {
            InputAction action = GetAction(reference);
            return action != null && action.enabled ? action.ReadValue<float>() : 0f;
        }

        private bool TryGetPointerPosition(out Vector2 position)
        {
            InputAction action = GetAction(pointerPositionAction);
            if (action != null && action.enabled)
            {
                position = action.ReadValue<Vector2>();
                return true;
            }

            if (Mouse.current != null && Mouse.current.enabled)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = Vector2.zero;
            return false;
        }
    }
}

