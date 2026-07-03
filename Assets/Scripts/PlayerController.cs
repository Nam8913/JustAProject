using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject focusObject;
    public Camera followCamera;

    [SerializeField] float freeMoveSpeed = 12f;
    [SerializeField] Vector3 freeModeCameraOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] float followSmoothTime = 0.15f;
    [SerializeField] Vector3 followModeCameraOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] float zoomSpeed = 0.75f;
    [SerializeField] float minZoom = 3f;
    [SerializeField] float maxZoom = 20f;
    [SerializeField] float lookHoldThreshold = 0.5f;
    [SerializeField] float lookMinDistance = 0.5f;
    [SerializeField] float lookMaxDistance = 10.5f;
    [SerializeField] float lookMoveSpeed = 6f;

    CameraState currentState;
    [SerializeField] string currentStateName = "None";
    readonly FreeMoveState freeMoveState = new FreeMoveState();
    readonly FollowTargetState followTargetState = new FollowTargetState();
    bool ownsFocusObject = false;
    Vector3 followVelocity;
    Vector3 lookOffset;
    float rightMouseHoldTime;
    bool isLookModeActive;

    public void SetFocusObject(GameObject obj)
    {
        if (obj == null)
        {
            SetNoneFocusMode();
            return;
        }

        SwitchToFollowMode(obj);
    }

    public void SetNoneFocusMode()
    {
        SwitchToFreeMode();
    }

    void Awake()
    {
        ResolveCameraReference();
    }

    void Start()
    {
        ResolveCameraReference();

        if (followCamera == null)
        {
            Debug.LogError("Main Camera not found in the scene. Please ensure there is a camera tagged as 'MainCamera'.");
            enabled = false;
            return;
        }

        GameService.MainCamera = followCamera;

        if (focusObject == null)
        {
            SetNoneFocusMode();
        }
        else
        {
            SwitchToFollowMode(focusObject);
        }

        RefreshCurrentStateName();
    }

    void Update()
    {
        if (followCamera == null || currentState == null)
        {
            return;
        }

        if (currentState == followTargetState)
        {
            UpdateLookInput();
        }
        else
        {
            ResetLookInteraction(false);
        }

        HandleZoom();
        currentState.Update(this);
        RefreshCurrentStateName();
    }

    void FixedUpdate()
    {
        if (followCamera == null || currentState == null)
        {
            return;
        }

        currentState.FixedUpdate(this);
    }

    void LateUpdate()
    {
        if (followCamera == null || currentState == null)
        {
            return;
        }

        currentState.LateUpdate(this);

        RefreshCurrentStateName();
    }

    void OnDestroy()
    {
        if (ownsFocusObject && focusObject != null)
        {
            Destroy(focusObject);
        }
    }

    void ResolveCameraReference()
    {
        if (followCamera != null)
        {
            return;
        }

        followCamera = GameService.MainCamera != null ? GameService.MainCamera : Camera.main;
    }

    void SwitchToFollowMode(GameObject target)
    {
        if (target == null)
        {
            SwitchToFreeMode();
            return;
        }

        if (currentState == followTargetState && focusObject == target)
        {
            followTargetState.SetTarget(target.transform);
            ownsFocusObject = false;
            followVelocity = Vector3.zero;
            return;
        }

        if (ownsFocusObject && focusObject != null)
        {
            Destroy(focusObject);
        }

        focusObject = target;
        ownsFocusObject = false;
        followTargetState.SetTarget(target.transform);
        followVelocity = Vector3.zero;
        ResetLookInteraction(true);
        TransitionTo(followTargetState);
    }

    void SwitchToFreeMode()
    {
        ResolveCameraReference();

        if (followCamera == null)
        {
            Debug.LogError("Main Camera not found in the scene. Please ensure there is a camera tagged as 'MainCamera'.");
            return;
        }

        if (currentState == freeMoveState && ownsFocusObject && focusObject != null)
        {
            freeMoveState.SetAnchor(focusObject.transform);
            return;
        }

        if (ownsFocusObject && focusObject != null)
        {
            Destroy(focusObject);
        }

        GameObject cameraFollower = new GameObject("CameraFollower");
        cameraFollower.transform.position = followCamera.transform.position - freeModeCameraOffset;
        focusObject = cameraFollower;
        ownsFocusObject = true;
        freeMoveState.SetAnchor(cameraFollower.transform);
        followVelocity = Vector3.zero;
        ResetLookInteraction(true);
        TransitionTo(freeMoveState);
    }

    void TransitionTo(CameraState nextState)
    {
        if (currentState == nextState)
        {
            RefreshCurrentStateName();
            return;
        }

        currentState?.Exit(this);
        currentState = nextState;
        RefreshCurrentStateName();
        currentState?.Enter(this);
    }

    internal void MoveFreeAnchor(Vector2 moveInput)
    {
        if (focusObject == null)
        {
            return;
        }

        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * freeMoveSpeed * Time.deltaTime;
        focusObject.transform.position += delta;
    }

    internal void SnapCameraToFreeAnchor()
    {
        if (focusObject == null)
        {
            return;
        }

        followCamera.transform.position = focusObject.transform.position + freeModeCameraOffset;
    }

    internal void FollowTarget()
    {
        if (focusObject == null)
        {
            SwitchToFreeMode();
            return;
        }

        Vector3 desiredPosition = focusObject.transform.position + followModeCameraOffset;

        if (isLookModeActive)
        {
            if (lookOffset == Vector3.zero)
            {
                return;
            }

            desiredPosition += lookOffset;
            followCamera.transform.position = Vector3.MoveTowards(
                followCamera.transform.position,
                desiredPosition,
                lookMoveSpeed * Time.deltaTime
            );
            return;
        }

        if (followSmoothTime <= 0f)
        {
            followCamera.transform.position = desiredPosition;
            return;
        }

        followCamera.transform.position = Vector3.SmoothDamp(
            followCamera.transform.position,
            desiredPosition,
            ref followVelocity,
            followSmoothTime
        );
    }

    internal void HandleZoom()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        if (scrollDelta == Vector2.zero)
        {
            return;
        }

        float normalizedScroll = Mathf.Clamp(scrollDelta.y, -1f, 1f);
        if (Mathf.Approximately(normalizedScroll, 0f))
        {
            return;
        }

        if (followCamera.orthographic)
        {
            followCamera.orthographicSize = Mathf.Clamp(
                followCamera.orthographicSize - (normalizedScroll * zoomSpeed),
                minZoom,
                maxZoom
            );
            return;
        }

        float zoomDelta = normalizedScroll * zoomSpeed;
        freeModeCameraOffset.z = Mathf.Clamp(freeModeCameraOffset.z + zoomDelta, -maxZoom, -minZoom);
        followModeCameraOffset.z = Mathf.Clamp(followModeCameraOffset.z + zoomDelta, -maxZoom, -minZoom);
    }

    internal Vector2 ReadDirectionalInput()
    {
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput.x += 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                moveInput.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                moveInput.y -= 1f;
            }
        }

        if (Gamepad.current != null)
        {
            moveInput += Gamepad.current.leftStick.ReadValue();
        }

        return Vector2.ClampMagnitude(moveInput, 1f);
    }

    void UpdateLookInput()
    {
        if (Mouse.current == null)
        {
            ResetLookInteraction(false);
            return;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            rightMouseHoldTime += Time.deltaTime;
            if (rightMouseHoldTime >= lookHoldThreshold && !isLookModeActive)
            {
                isLookModeActive = true;
            }

            if (isLookModeActive)
            {
                UpdateLookTarget();
            }

            return;
        }

        ResetLookInteraction(true);
    }

    void UpdateLookTarget()
    {
        if (followCamera == null || focusObject == null || Mouse.current == null)
        {
            lookOffset = Vector3.zero;
            RefreshCurrentStateName();
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 cameraScreenCenter = followCamera.pixelRect.center;
        float targetDepth = followCamera.WorldToScreenPoint(focusObject.transform.position).z;

        Vector3 cameraCenterWorld = followCamera.ScreenToWorldPoint(new Vector3(cameraScreenCenter.x, cameraScreenCenter.y, targetDepth));
        Vector3 mouseWorld = followCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, targetDepth));
        Vector3 direction = mouseWorld - cameraCenterWorld;
        direction.z = 0f;

        float distance = direction.magnitude;
        if (distance < lookMinDistance)
        {
            lookOffset = Vector3.zero;
            RefreshCurrentStateName();
            return;
        }

        lookOffset = Vector3.ClampMagnitude(direction, lookMaxDistance);
        RefreshCurrentStateName();
    }

    void ResetLookInteraction(bool clearOffset)
    {
        rightMouseHoldTime = 0f;
        isLookModeActive = false;

        if (clearOffset)
        {
            lookOffset = Vector3.zero;
        }

        RefreshCurrentStateName();
    }

    Vector3 GetMouseWorldPositionOnPlane(float planeZ)
    {
        if (followCamera == null || Mouse.current == null)
        {
            return focusObject != null ? focusObject.transform.position : Vector3.zero;
        }

        Ray ray = followCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, planeZ));

        if (plane.Raycast(ray, out float enterDistance))
        {
            return ray.GetPoint(enterDistance);
        }

        return focusObject != null ? focusObject.transform.position : followCamera.transform.position;
    }

    void RefreshCurrentStateName()
    {
        string stateName = currentState != null ? currentState.StateName : "None";
        currentStateName = currentState == followTargetState && isLookModeActive ? $"{stateName} + PZLook" : stateName;
    }

    abstract class CameraState
    {
        public virtual string StateName => GetType().Name;

        public virtual void Enter(PlayerController controller)
        {
        }

        public virtual void Exit(PlayerController controller)
        {
        }
        public virtual void FixedUpdate(PlayerController controller)
        {
        }

        public abstract void Update(PlayerController controller);
        
        public abstract void LateUpdate(PlayerController controller);
    }

    sealed class FreeMoveState : CameraState
    {
        public override string StateName => "NoTarget / FreeMove";

        Transform anchor;

        public void SetAnchor(Transform newAnchor)
        {
            anchor = newAnchor;
        }

        public override void Update(PlayerController controller)
        {
            if (anchor == null)
            {
                controller.SwitchToFreeMode();
                return;
            }

            Vector2 moveInput = controller.ReadDirectionalInput();
            if (moveInput == Vector2.zero)
            {
                return;
            }

            controller.MoveFreeAnchor(moveInput);
        }

        public override void LateUpdate(PlayerController controller)
        {
            if (anchor == null)
            {
                controller.SwitchToFreeMode();
                return;
            }

            controller.SnapCameraToFreeAnchor();
        }
    }

    sealed class FollowTargetState : CameraState
    {
        public override string StateName => "HasTarget / FollowTarget";

        Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public override void Update(PlayerController controller)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                controller.SwitchToFreeMode();
            }
        }

        public override void FixedUpdate(PlayerController controller)
        {
            Rigidbody2D targetRigidbody = target != null ? target.GetComponent<Rigidbody2D>() : null;
            if (targetRigidbody == null)
            {
                return;
            }

            targetRigidbody.linearVelocity = controller.ReadDirectionalInput();
        }
        public override void LateUpdate(PlayerController controller)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                controller.SwitchToFreeMode();
                return;
            }

            controller.FollowTarget();
        }
    }
}
