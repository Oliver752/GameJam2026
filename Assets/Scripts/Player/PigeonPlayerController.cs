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

    [Header("Visuals")]
    public float bankAngle = 45f;
    public float pitchSmoothing = 3f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    
    private Vector3 velocity;
    private float verticalVelocity;
    private bool isFlying;
    private bool isGliding;
    private float currentSpeed;
    private float currentPitch;
    private float flapTimer;
    private const float FLAP_COOLDOWN = 0.15f;

    private bool jumpPressed;
    private bool jumpHeld;

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
    }

    private void Update()
    {
        HandleLook();
        
        bool grounded = IsGrounded();
        
        if (grounded && !isFlying)
            HandleGroundMovement();
        else
            HandleFlightPhysics();
            
        ApplyMovement();
        UpdateVisuals();
        UpdateAnimator();
        
        jumpPressed = false;
    }

    private bool IsGrounded()
    {
        return controller.isGrounded && verticalVelocity <= 0.1f;
    }

    private void HandleGroundMovement()
    {
        velocity = Vector3.zero;
        verticalVelocity = -2f;
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
            
            Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0; camRight.y = 0;
                camForward.Normalize(); camRight.Normalize();
                move = camForward * move.z + camRight * move.x;
            }

            velocity = move * speed;

            Quaternion targetRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, groundRotationSpeed * Time.deltaTime);
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    private void TakeOff()
    {
        isFlying = true;
        currentSpeed = runSpeed;
        verticalVelocity = 5f;
        
        animator.SetTrigger(takeoffHash);
        animator.SetBool(isFlyingHash, true);
    }

    private void HandleFlightPhysics()
    {
        float pitchInput = -moveInput.y;
        float turnInput = moveInput.x;
        
        bool flapPressed = jumpPressed;
        bool glideHeld = jumpHeld;

        if (flapPressed && flapTimer <= 0f)
        {
            currentSpeed += flapThrust;
            verticalVelocity += flapLift;
            flapTimer = FLAP_COOLDOWN;
            
            Vector3 flapDirection = transform.forward;
            velocity += flapDirection * flapThrust * 0.5f;
            
            animator.SetTrigger(flapHash);
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

        CheckLanding();
    }

    private void CheckLanding()
    {
        float checkDistance = 0.3f;
        bool nearGround = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance);
        
        if ((controller.isGrounded || nearGround) && verticalVelocity <= 0 && isFlying)
        {
            Land();
        }
    }

    private void Land()
    {
        animator.SetTrigger(landHash);
        animator.SetBool(isFlyingHash, false);
        
        isFlying = false;
        isGliding = false;
        currentSpeed = 0f;
        currentPitch = 0f;
        verticalVelocity = -2f;
    }

    private void ApplyMovement()
    {
        controller.Move(velocity * Time.deltaTime);
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
        
        bool grounded = IsGrounded();
        bool moving = moveInput.sqrMagnitude > 0.01f;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        
        animator.SetBool(isGroundedHash, grounded);
        animator.SetBool(isMovingHash, moving);
        animator.SetBool(isFlyingHash, isFlying);
        animator.SetFloat(speedHash, normalizedSpeed);
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;

        float yaw = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * yaw);

        float pitch = lookInput.y * mouseSensitivity;
        cameraPitch -= pitch;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
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