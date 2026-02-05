using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PigeonPlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    [Tooltip("DRAG THE PIGEON OBJECT HERE")]
    public Transform modelTransform;

    private Animator animator;
    private CharacterController controller;

    [Header("Ground Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float groundRotationSpeed = 10f;
    [Tooltip("Minimum input to rotate (prevents spinning)")]
    public float rotationThreshold = 0.15f;

    [Header("Flight Physics - Realistic Bird")]
    public float stallSpeed = 8f;
    public float glideSpeed = 12f;
    public float maxSpeed = 25f;
    public float flapThrust = 8f;
    public float flapLift = 4f;
    public float drag = 0.8f;
    public float flightGravity = -9.8f;
    public float liftCoefficient = 0.5f;
    public float maxClimbAngle = 30f;
    public float maxDiveAngle = 45f;

    [Header("Glide")]
    public float glideLiftMultiplier = 1.5f;
    public float glideDragMultiplier = 0.3f;

    [Header("Camera")]
    public float mouseSensitivity = 0.15f;
    public float maxLookAngle = 80f;
    [Tooltip("Camera offset from player (behind and above)")]
    public Vector3 cameraOffset = new Vector3(0, 2f, -4f);
    [Tooltip("How fast camera follows player position")]
    public float cameraFollowSpeed = 10f;
    [Tooltip("How fast camera yaw aligns with player when landing")]
    public float cameraYawAlignSpeed = 5f;

    [Header("Visuals")]
    public float bankAngle = 45f;
    public float pitchSmoothing = 3f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.5f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    private float cameraYaw;
    private float targetCameraYaw; // Target yaw for smooth alignment
    
    private Vector3 velocity;
    private Vector3 cameraVelocity;
    private float verticalVelocity;
    private bool isFlying;
    private bool isGliding;
    private float currentSpeed;
    private float currentPitch;
    private float flapTimer;
    private const float FLAP_COOLDOWN = 0.15f;

    private bool jumpPressed;
    private bool jumpHeld;
    private bool isGrounded;
    private bool wasFlyingLastFrame; // Track state change

    private int isGroundedHash;
    private int isMovingHash;
    private int isFlyingHash;
    private int flapHash;
    private int landHash;
    private int takeoffHash;
    private int speedHash;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        if (modelTransform == null)
        {
            Debug.LogError("ERROR: 'Model Transform' is not assigned! Drag your Pigeon object into this field in the Inspector.", this);
            enabled = false;
            return;
        }
        
        animator = modelTransform.GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError($"ERROR: No Animator found on {modelTransform.name}!", this);
            enabled = false;
            return;
        }
        
        isGroundedHash = Animator.StringToHash("isGrounded");
        isMovingHash = Animator.StringToHash("isMoving");
        isFlyingHash = Animator.StringToHash("isFlying");
        flapHash = Animator.StringToHash("flap");
        landHash = Animator.StringToHash("land");
        takeoffHash = Animator.StringToHash("takeoff");
        speedHash = Animator.StringToHash("speed");

        if (groundLayer == 0)
        {
            groundLayer = ~LayerMask.GetMask("Player");
        }
        
        cameraYaw = transform.eulerAngles.y;
        targetCameraYaw = cameraYaw;
    }

    private void Update()
    {
        HandleLook();
        
        isGrounded = CheckGrounded();
        
        // Detect landing
        if (wasFlyingLastFrame && !isFlying)
        {
            // Just landed - smoothly align camera to player facing
            targetCameraYaw = transform.eulerAngles.y;
        }
        wasFlyingLastFrame = isFlying;
        
        if (isGrounded && !isFlying)
            HandleGroundMovement();
        else
            HandleFlightPhysics();
            
        ApplyMovement();
        UpdateVisuals();
        UpdateAnimator();
        UpdateCameraPosition();
        
        jumpPressed = false;
    }

    private bool CheckGrounded()
    {
        bool controllerGrounded = controller.isGrounded;
        
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool sphereHit = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, 
            out RaycastHit hit, groundCheckDistance, groundLayer);
        
        bool rayHit = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        
        Color debugColor = (sphereHit || rayHit) ? Color.green : Color.red;
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, debugColor);
        
        bool grounded = (controllerGrounded || sphereHit || rayHit) && verticalVelocity <= 0.5f;
        
        return grounded;
    }

    private void HandleGroundMovement()
    {
        verticalVelocity = -0.5f;
        velocity.y = verticalVelocity;
        currentSpeed = 0f;

        if (jumpPressed && !isFlying)
        {
            TakeOff();
            return;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            float speed = moveInput.sqrMagnitude > 0.9f ? runSpeed : walkSpeed;
            currentSpeed = speed;
            
            Vector3 move = Vector3.zero;
            
            if (cameraTransform != null)
            {
                // Use smoothed camera yaw for movement
                Vector3 camForward = new Vector3(
                    Mathf.Sin(cameraYaw * Mathf.Deg2Rad), 
                    0, 
                    Mathf.Cos(cameraYaw * Mathf.Deg2Rad)
                );
                Vector3 camRight = new Vector3(
                    Mathf.Cos(cameraYaw * Mathf.Deg2Rad), 
                    0, 
                    -Mathf.Sin(cameraYaw * Mathf.Deg2Rad)
                );
                
                move = camForward * moveInput.y + camRight * moveInput.x;
                move = Vector3.ClampMagnitude(move, 1f);
            }
            else
            {
                move = new Vector3(moveInput.x, 0f, moveInput.y);
                move = Vector3.ClampMagnitude(move, 1f);
            }

            if (move.sqrMagnitude > rotationThreshold * rotationThreshold)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, groundRotationSpeed * Time.deltaTime);
            }

            velocity.x = move.x * speed;
            velocity.z = move.z * speed;
        }
        else
        {
            velocity.x = 0;
            velocity.z = 0;
        }
    }

    private void TakeOff()
    {
        isFlying = true;
        currentSpeed = runSpeed;
        verticalVelocity = 5f;
        
        animator.SetBool(isFlyingHash, true);
        animator.SetTrigger(takeoffHash);
        
        StartCoroutine(ResetTriggerNextFrame(takeoffHash));
    }

    private System.Collections.IEnumerator ResetTriggerNextFrame(int triggerHash)
    {
        yield return null;
        animator.ResetTrigger(triggerHash);
    }

    private void HandleFlightPhysics()
    {
        float pitchInput = -moveInput.y;
        float turnInput = moveInput.x;
        
        bool flapPressed = jumpPressed;
        bool glideHeld = jumpHeld;

        if (isGrounded && verticalVelocity <= 0 && isFlying)
        {
            Land();
            return;
        }

        if (flapPressed && flapTimer <= 0f)
        {
            currentSpeed += flapThrust;
            verticalVelocity += flapLift;
            flapTimer = FLAP_COOLDOWN;
            
            Vector3 flapDirection = transform.forward;
            velocity += flapDirection * flapThrust * 0.5f;
            
            animator.SetTrigger(flapHash);
            StartCoroutine(ResetTriggerNextFrame(flapHash));
        }
        flapTimer -= Time.deltaTime;

        isGliding = glideHeld && !flapPressed;

        float targetPitchAngle = 0f;
        if (pitchInput > 0.1f)
            targetPitchAngle = -maxClimbAngle;
        else if (pitchInput < -0.1f)
            targetPitchAngle = maxDiveAngle;
            
        currentPitch = Mathf.Lerp(currentPitch, targetPitchAngle, pitchSmoothing * Time.deltaTime);

        float pitchFactor = -currentPitch / maxClimbAngle;
        currentSpeed += pitchFactor * 5f * Time.deltaTime;

        float currentDrag = isGliding ? drag * glideDragMultiplier : drag;
        currentSpeed -= currentDrag * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        float speedRatio = currentSpeed / glideSpeed;
        float baseLift = liftCoefficient * speedRatio * speedRatio;
        float lift = isGliding ? baseLift * glideLiftMultiplier : baseLift;

        if (currentSpeed < stallSpeed)
        {
            lift *= (currentSpeed / stallSpeed);
        }

        verticalVelocity += flightGravity * Time.deltaTime;
        verticalVelocity += lift * Time.deltaTime;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -20f, 15f);

        Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        Vector3 flyDirection = transform.rotation * pitchRotation * Vector3.forward;

        velocity = flyDirection * currentSpeed;
        velocity.y = verticalVelocity;

        float turnRate = Mathf.Lerp(80f, 40f, currentSpeed / maxSpeed);
        transform.Rotate(Vector3.up * turnInput * turnRate * Time.deltaTime);
    }

    private void Land()
    {
        animator.SetBool(isFlyingHash, false);
        animator.SetTrigger(landHash);
        StartCoroutine(ResetTriggerNextFrame(landHash));
        
        isFlying = false;
        isGliding = false;
        currentSpeed = 0f;
        currentPitch = 0f;
        verticalVelocity = -0.5f;
        
        // Don't snap cameraYaw - let it smooth to target in HandleLook
        targetCameraYaw = transform.eulerAngles.y;
    }

    private void ApplyMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform == null) return;
        
        // Smooth camera yaw toward target
        cameraYaw = Mathf.LerpAngle(cameraYaw, targetCameraYaw, cameraYawAlignSpeed * Time.deltaTime);
        
        // Calculate desired position
        Vector3 offset = new Vector3(
            Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraOffset.z + Mathf.Cos(cameraYaw * Mathf.Deg2Rad) * cameraOffset.x,
            cameraOffset.y,
            Mathf.Cos(cameraYaw * Mathf.Deg2Rad) * cameraOffset.z - Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraOffset.x
        );
        
        Vector3 targetPosition = transform.position + offset;
        
        // Smooth XZ position (horizontal)
        Vector3 currentPos = cameraTransform.position;
        Vector3 newPos = currentPos;
        
        // X and Z follow quickly
        newPos.x = Mathf.Lerp(currentPos.x, targetPosition.x, 10f * Time.deltaTime);
        newPos.z = Mathf.Lerp(currentPos.z, targetPosition.z, 10f * Time.deltaTime);
        
        // Y follows slowly (prevents landing shake)
        newPos.y = Mathf.Lerp(currentPos.y, targetPosition.y, 2f * Time.deltaTime);
        
        cameraTransform.position = newPos;
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    private void UpdateVisuals()
    {
        float turnInput = moveInput.x;
        float targetRoll = 0f;
        
        if (isFlying && Mathf.Abs(turnInput) > 0.1f)
        {
            targetRoll = -turnInput * bankAngle;
        }

        float visualPitch = isFlying ? currentPitch : 0f;

        if (modelTransform != null)
        {
            Quaternion targetRot = Quaternion.Euler(visualPitch, 0f, targetRoll);
            modelTransform.localRotation = Quaternion.Slerp(modelTransform.localRotation, targetRot, 5f * Time.deltaTime);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        bool moving = moveInput.sqrMagnitude > 0.01f;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        
        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetBool(isMovingHash, moving);
        animator.SetFloat(speedHash, normalizedSpeed);
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;

        // Mouse input updates target yaw, not immediate yaw
        targetCameraYaw += lookInput.x * mouseSensitivity * 100f * Time.deltaTime;
        
        // Clamp pitch
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
            jumpHeld = true;
        }
        else
        {
            jumpHeld = false;
        }
    }
}