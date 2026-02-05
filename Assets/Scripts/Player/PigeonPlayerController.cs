using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PigeonPlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform modelTransform;

    [Header("Ground Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float groundRotationSpeed = 10f;

    [Header("Flight Physics - Realistic Bird")]
    [Tooltip("Speed below which the bird stalls and falls")]
    public float stallSpeed = 8f;
    [Tooltip("Optimal gliding speed")]
    public float glideSpeed = 12f;
    [Tooltip("Maximum diving speed")]
    public float maxSpeed = 25f;
    [Tooltip("How fast flapping accelerates forward")]
    public float flapThrust = 8f;
    [Tooltip("Upward force from flap")]
    public float flapLift = 4f;
    [Tooltip("Air resistance - higher = slow down faster")]
    public float drag = 0.8f;
    [Tooltip("Gravity when flying (lower than normal for buoyancy)")]
    public float flightGravity = -9.8f;
    [Tooltip("How much speed generates lift (wing shape factor)")]
    public float liftCoefficient = 0.5f;
    [Tooltip("Max climb angle in degrees")]
    public float maxClimbAngle = 30f;
    [Tooltip("Max dive angle in degrees")]
    public float maxDiveAngle = 45f;

    [Header("Glide")]
    [Tooltip("Lift multiplier when holding space to glide")]
    public float glideLiftMultiplier = 1.5f;
    [Tooltip("Drag multiplier when gliding (lower = glide further)")]
    public float glideDragMultiplier = 0.3f;

    [Header("Camera")]
    public float mouseSensitivity = 0.15f;
    public float maxLookAngle = 80f;

    [Header("Visuals")]
    public float bankAngle = 45f;
    public float pitchSmoothing = 3f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    
    // Flight state
    private Vector3 velocity;
    private float verticalVelocity;
    private bool isFlying;
    private bool isGliding;
    private float currentSpeed;
    private float currentPitch;
    private float flapTimer;
    private const float FLAP_COOLDOWN = 0.15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleLook();
        
        if (IsGrounded() && !isFlying)
            HandleGroundMovement();
        else
            HandleFlightPhysics();
            
        ApplyMovement();
        UpdateVisuals();
    }

    private bool IsGrounded()
    {
        return controller.isGrounded && verticalVelocity <= 0;
    }

    private void HandleGroundMovement()
    {
        // Reset velocity when grounded
        velocity = Vector3.zero;
        verticalVelocity = -2f;
        currentSpeed = 0f;

        // Takeoff initiation - double tap space or hold to takeoff
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isFlying = true;
            currentSpeed = runSpeed;
            verticalVelocity = 5f;
            return;
        }

        // Normal walking - only move if there's input
        if (moveInput.sqrMagnitude > 0.01f)
        {
            float speed = moveInput.sqrMagnitude > 0.9f ? runSpeed : walkSpeed;
            
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

            // Rotate towards movement
            Quaternion targetRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, groundRotationSpeed * Time.deltaTime);
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    private void HandleFlightPhysics()
    {
        // Input reading
        float pitchInput = -moveInput.y; // W = dive, S = climb
        float turnInput = moveInput.x;   // A/D = turn
        bool flapPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        bool glideHeld = Keyboard.current.spaceKey.isPressed;

        // FLAP: Tap space for thrust and lift
        if (flapPressed && flapTimer <= 0f)
        {
            currentSpeed += flapThrust;
            verticalVelocity += flapLift;
            flapTimer = FLAP_COOLDOWN;
            
            // Small forward boost on flap
            Vector3 flapDirection = transform.forward;
            velocity += flapDirection * flapThrust * 0.5f;
        }
        flapTimer -= Time.deltaTime;

        // GLIDE: Hold space to extend wings and glide efficiently
        isGliding = glideHeld && !flapPressed;

        // Calculate target pitch based on input (climb/dive)
        float targetPitchAngle = 0f;
        if (pitchInput > 0.1f) // Climbing (S key)
            targetPitchAngle = -maxClimbAngle; // Negative = nose up
        else if (pitchInput < -0.1f) // Diving (W key)
            targetPitchAngle = maxDiveAngle; // Positive = nose down
            
        currentPitch = Mathf.Lerp(currentPitch, targetPitchAngle, pitchSmoothing * Time.deltaTime);

        // Speed changes based on pitch
        // Climbing slows you down, diving speeds you up
        float pitchFactor = -currentPitch / maxClimbAngle; // -1 to 1 range
        currentSpeed += pitchFactor * 5f * Time.deltaTime;

        // Apply drag (air resistance)
        float currentDrag = isGliding ? drag * glideDragMultiplier : drag;
        currentSpeed -= currentDrag * Time.deltaTime;

        // Clamp speed
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // LIFT PHYSICS
        // Lift = speed² * coefficient (realistic aerodynamics)
        // More speed = exponentially more lift
        float speedRatio = currentSpeed / glideSpeed;
        float baseLift = liftCoefficient * speedRatio * speedRatio;
        
        // Glide mode: better lift-to-drag ratio
        float lift = isGliding ? baseLift * glideLiftMultiplier : baseLift;

        // Stall condition: below stallSpeed, lift drops and you fall
        if (currentSpeed < stallSpeed)
        {
            lift *= (currentSpeed / stallSpeed); // Linear falloff
        }

        // Apply gravity and lift
        verticalVelocity += flightGravity * Time.deltaTime;
        verticalVelocity += lift * Time.deltaTime;

        // Terminal velocity limits
        verticalVelocity = Mathf.Clamp(verticalVelocity, -20f, 15f);

        // Calculate flight direction based on pitch
        Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        Vector3 flyDirection = transform.rotation * pitchRotation * Vector3.forward;

        // Set velocity
        velocity = flyDirection * currentSpeed;
        velocity.y = verticalVelocity;

        // Turning (yaw) - tighter turns at lower speeds
        float turnRate = Mathf.Lerp(80f, 40f, currentSpeed / maxSpeed);
        transform.Rotate(Vector3.up * turnInput * turnRate * Time.deltaTime);

        // Landing detection
        if (IsGrounded() && verticalVelocity <= 0)
        {
            isFlying = false;
            isGliding = false;
            currentSpeed = 0f;
            currentPitch = 0f;
        }
    }

    private void ApplyMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateVisuals()
    {
        // Bank/roll based on turning
        float turnInput = moveInput.x;
        float targetRoll = 0f;
        
        if (isFlying && Mathf.Abs(turnInput) > 0.1f)
        {
            targetRoll = -turnInput * bankAngle;
        }

        // Pitch visualization
        float visualPitch = isFlying ? currentPitch : 0f;

        if (modelTransform != null)
        {
            Quaternion targetRot = Quaternion.Euler(visualPitch, 0f, targetRoll);
            modelTransform.localRotation = Quaternion.Slerp(modelTransform.localRotation, targetRot, 5f * Time.deltaTime);
        }
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;

        // Horizontal look rotates bird
        float yaw = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * yaw);

        // Vertical look for camera only (bird pitch is controlled by W/S)
        float pitch = lookInput.y * mouseSensitivity;
        cameraPitch -= pitch;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    // Input System callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Keep this for compatibility but we read space directly in Update
    public void OnJump(InputValue value) { }
}