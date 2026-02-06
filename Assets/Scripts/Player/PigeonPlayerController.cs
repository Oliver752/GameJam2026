using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PigeonPlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    [Tooltip("DRAG THE PIGEON OBJECT HERE")]
    public Transform modelTransform;
    [Tooltip("Where poop spawns from")]
    public Transform poopSpawnPoint;

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
    [Tooltip("How much faster you fall when holding S")]
    public float diveMultiplier = 2f;
    [Tooltip("Maximum fall speed (negative is down)")]
    public float maxFallSpeed = -30f;
    [Tooltip("Maximum upward speed")]
    public float maxRiseSpeed = 15f;
    [Tooltip("Tighter turn radius at low speeds")]
    public float minTurnRate = 60f;
    public float maxTurnRate = 120f;

    [Header("Glide")]
    public float glideLiftMultiplier = 1.5f;
    public float glideDragMultiplier = 0.3f;

    [Header("Impact Bounce")]
    [Tooltip("Speed threshold for bounce")]
    public float bounceSpeedThreshold = 15f;
    [Tooltip("How much speed is kept after bounce")]
    public float bounceRetention = 0.3f;
    [Tooltip("Upward boost from bounce")]
    public float bounceUpward = 5f;
    [Tooltip("Stun duration after bounce")]
    public float bounceStunDuration = 0.5f;

    [Header("Poop")]
    [Tooltip("Poop prefab to spawn")]
    public GameObject poopPrefab;
    [Tooltip("Poop drop force")]
    public float poopDropForce = 5f;
    [Tooltip("Cooldown between poops")]
    public float poopCooldown = 0.5f;
    [Tooltip("Key for pooping")]
    public KeyCode poopKey = KeyCode.F;

    [Header("Camera")]
    public float mouseSensitivity = 0.15f;
    public float maxLookAngle = 80f;
    public Vector3 cameraOffset = new Vector3(0, 2f, -4f);
    public float cameraFollowSpeed = 10f;
    public float cameraYawAlignSpeed = 5f;

    [Header("Visuals")]
    public float bankAngle = 45f;
    public float pitchSmoothing = 3f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.5f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Particle Effects")]
    public ParticleSystem impactParticlesPrefab;
    public TrailRenderer speedTrail;
[Tooltip("Speed threshold for trail")]
public float trailSpeedThreshold = 15f;
[Tooltip("Time for trail to fade when stopped")]
public float trailFadeTime = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip flapSound;
    public AudioClip windSound;
    public AudioClip landSound;
    [Tooltip("Wind volume at max speed")]
    public float maxWindVolume = 0.5f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    private float cameraYaw;
    private float targetCameraYaw;
    
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
    private bool wasFlyingLastFrame;
    private bool isDiving;
    private bool isStunned;
    private float stunTimer;
    private float poopTimer;
    private bool isWalking;

    private int isGroundedHash;
    private int isMovingHash;
    private int isFlyingHash;
    private int flapHash;
    private int landHash;
    private int takeoffHash;
    private int speedHash;

    private bool hitWallThisFrame;
    private Vector3 wallHitNormal;
    private float wallHitSpeed;
    private Vector3 wallHitPoint; // Add with other wall variables

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        if (modelTransform == null)
        {
            Debug.LogError("ERROR: 'Model Transform' is not assigned!", this);
            enabled = false;
            return;
        }
        
        animator = modelTransform.GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError($"No Animator found on {modelTransform.name}!", this);
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
if (speedTrail != null)
    {
        speedTrail.emitting = false;
    }

    }

    private void Update()
{
    HandleLook();
    
    isGrounded = CheckGrounded();
    
    // Handle stun from bounce FIRST
    if (isStunned)
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            isStunned = false;
        }
        else
        {
            // While stunned, only apply physics and camera
            ApplyMovement();
            UpdateCameraPosition();
            UpdateParticles();
            return; // Skip all other updates
        }
    }
    
    // Check for wall hit BEFORE movement
    if (hitWallThisFrame && isFlying && !isStunned)
    {
        Debug.Log("Processing wall bounce!");
        CheckImpactBounce(true);
        hitWallThisFrame = false;
    }
    
    // Check for landing bounce
    if (wasFlyingLastFrame && !isFlying && isGrounded)
    {
        CheckImpactBounce(false);
    }
    
    wasFlyingLastFrame = isFlying;
    
    // Only do normal flight/ground logic if not bouncing this frame
    if (!isStunned)
    {
        if (isGrounded && !isFlying)
            HandleGroundMovement();
        else
            HandleFlightPhysics();
    }
    
    // Apply movement (uses velocity set by bounce or flight physics)
    ApplyMovement();
    
    // Visual updates
    UpdateVisuals();
    UpdateAnimator();
    UpdateCameraPosition();
    UpdateParticles();
    UpdateAudio();
    HandlePoop();
    
    jumpPressed = false;
}

    private bool CheckGrounded()
    {
        bool controllerGrounded = controller.isGrounded;
        
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool sphereHit = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, 
            out RaycastHit hit, groundCheckDistance, groundLayer);
        
        bool rayHit = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        
        return (controllerGrounded || sphereHit || rayHit) && verticalVelocity <= 0.5f;
    }

    private void HandleGroundMovement()
    {
        verticalVelocity = -0.5f;
        velocity.y = verticalVelocity;
        currentSpeed = 0f;
        isWalking = false;

        if (jumpPressed && !isFlying)
        {
            TakeOff();
            return;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            float speed = moveInput.sqrMagnitude > 0.9f ? runSpeed : walkSpeed;
            currentSpeed = speed;
            isWalking = true;
            
            Vector3 move = Vector3.zero;
            
            if (cameraTransform != null)
            {
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
        
        PlaySound(flapSound);
        
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
        
        // Check for dive (holding S)
        isDiving = pitchInput < -0.5f;

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
            PlaySound(flapSound);
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

        // Apply gravity with dive multiplier
        float gravity = isDiving ? flightGravity * diveMultiplier : flightGravity;
        verticalVelocity += gravity * Time.deltaTime;
        
        verticalVelocity += lift * Time.deltaTime;
        
        // Clamp vertical velocity
        verticalVelocity = Mathf.Clamp(verticalVelocity, maxFallSpeed, maxRiseSpeed);

        Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        Vector3 flyDirection = transform.rotation * pitchRotation * Vector3.forward;

        velocity = flyDirection * currentSpeed;
        velocity.y = verticalVelocity;

        // Smoother turn rate based on speed
        float speedFactor = Mathf.InverseLerp(stallSpeed, maxSpeed, currentSpeed);
        float turnRate = Mathf.Lerp(minTurnRate, maxTurnRate, speedFactor);
        transform.Rotate(Vector3.up * turnInput * turnRate * Time.deltaTime);
    }

    private void CheckImpactBounce(bool isWall)
{
    float impactSpeed = isWall ? wallHitSpeed : currentSpeed;
    
    if (impactSpeed > bounceSpeedThreshold)
    {
        // Bounce!
        isStunned = true;
        stunTimer = bounceStunDuration;
        
        if (impactParticlesPrefab != null)
{
    Vector3 spawnPos = isWall ? wallHitPoint : transform.position + Vector3.up * 0.5f;
    Vector3 particleDir = isWall ? wallHitNormal : Vector3.up;

    ParticleSystem ps = Instantiate(
        impactParticlesPrefab,
        spawnPos,
        Quaternion.LookRotation(particleDir)
    );

    ps.Play();
    Destroy(ps.gameObject, 3f); // cleanup
}

        
        if (isWall)
        {
            // Bounce off wall
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            Vector3 reflectDir = Vector3.Reflect(horizontalVelocity.normalized, wallHitNormal);
            
            currentSpeed = impactSpeed * bounceRetention;
            velocity.x = reflectDir.x * currentSpeed;
            velocity.z = reflectDir.z * currentSpeed;
            verticalVelocity = bounceUpward * 0.5f;
            velocity.y = verticalVelocity;
            
            // Push away from wall more aggressively
            transform.position += wallHitNormal * 1.0f;
        }
        else
        {
            verticalVelocity = bounceUpward;
            currentSpeed *= bounceRetention;
            velocity = -transform.forward * currentSpeed;
            velocity.y = verticalVelocity;
        }
        
        PlaySound(landSound);
    }
    else if (!isWall)
    {
        PlaySound(landSound);
    }
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
    isDiving = false;
    hitWallThisFrame = false; // Reset wall hit flag
    
    targetCameraYaw = transform.eulerAngles.y;
}

    private void HandlePoop()
    {
        poopTimer -= Time.deltaTime;
        
        if (Input.GetKeyDown(poopKey) && poopTimer <= 0f)
        {
            DropPoop();
            poopTimer = poopCooldown;
        }
    }

    private void DropPoop()
    {
        if (poopPrefab == null) return;
        
        Vector3 spawnPos = poopSpawnPoint != null ? poopSpawnPoint.position : transform.position - Vector3.up * 0.5f;
        
        GameObject poop = Instantiate(poopPrefab, spawnPos, Quaternion.identity);
        
        Rigidbody poopRb = poop.GetComponent<Rigidbody>();
        if (poopRb != null)
        {
            // Drop with some forward momentum from bird
            Vector3 dropForce = Vector3.down * poopDropForce + velocity * 0.3f;
            poopRb.AddForce(dropForce, ForceMode.Impulse);
        }
        
        // Destroy after 5 seconds
        Destroy(poop, 5f);
        
        Debug.Log("Poop dropped!");
    }

    private void ApplyMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform == null) return;
        
        cameraYaw = Mathf.LerpAngle(cameraYaw, targetCameraYaw, cameraYawAlignSpeed * Time.deltaTime);
        
        Vector3 offset = new Vector3(
            Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraOffset.z + Mathf.Cos(cameraYaw * Mathf.Deg2Rad) * cameraOffset.x,
            cameraOffset.y,
            Mathf.Cos(cameraYaw * Mathf.Deg2Rad) * cameraOffset.z - Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraOffset.x
        );
        
        Vector3 targetPosition = transform.position + offset;
        
        Vector3 currentPos = cameraTransform.position;
        Vector3 newPos = currentPos;
        
        newPos.x = Mathf.Lerp(currentPos.x, targetPosition.x, 10f * Time.deltaTime);
        newPos.z = Mathf.Lerp(currentPos.z, targetPosition.z, 10f * Time.deltaTime);
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

    private void UpdateParticles()
{
    if (speedTrail == null) return;

    // Use actual velocity magnitude for air speed
    float airSpeed = velocity.magnitude;

    if (isFlying && airSpeed > trailSpeedThreshold)
    {
        // Trail is active
        speedTrail.emitting = true;
    }
    else
    {
        // Stop emitting - trail will naturally fade based on its Time setting
        speedTrail.emitting = false;
    }
}


    private void UpdateAudio()
    {
        if (audioSource == null) return;
        
        // Wind sound based on speed
        if (isFlying)
        {
            float windVolume = Mathf.Lerp(0, maxWindVolume, currentSpeed / maxSpeed);
            if (!audioSource.isPlaying || audioSource.clip != windSound)
            {
                audioSource.clip = windSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            audioSource.volume = windVolume;
        }
        else if (isWalking && walkSound != null)
        {
            // Walking sound (simplified - ideally use footsteps)
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true;
                audioSource.volume = 0.3f;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip == windSound || audioSource.clip == walkSound)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
{
    Debug.Log($"OnControllerColliderHit! isFlying: {isFlying}, isStunned: {isStunned}");
    
    if (!isFlying || isStunned) 
    {
        Debug.Log("Skipping - not flying or stunned");
        return;
    }
    
    float dot = Vector3.Dot(hit.normal, Vector3.up);
    bool isWall = Mathf.Abs(dot) < 0.5f;
    
    Debug.Log($"Hit normal: {hit.normal}, Dot: {dot}, isWall: {isWall}");
    
    float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
    Debug.Log($"Horizontal speed: {horizontalSpeed}, threshold: {bounceSpeedThreshold}");
    
    if (isWall && horizontalSpeed > bounceSpeedThreshold && !hitWallThisFrame)
    {
        Debug.Log("WALL HIT DETECTED - Setting flag!");
        hitWallThisFrame = true;
        wallHitNormal = hit.normal;
        wallHitSpeed = horizontalSpeed;
        wallHitPoint = hit.point;
    }
}

    private void HandleLook()
    {
        if (cameraTransform == null) return;

        targetCameraYaw += lookInput.x * mouseSensitivity * 100f * Time.deltaTime;
        
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