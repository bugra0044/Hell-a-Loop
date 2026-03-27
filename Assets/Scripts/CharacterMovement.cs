using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("PowerUp Sistemi")]
    public PowerUpData activePowerUp;

    [Header("Temel Ayarlar")]
    public float acceleration = 50f;      // Normal hızlanma
    public float groundDrag = 5f;         // Normal sürtünme
    public float airDrag = 1f;            // Hava sürtünmesi
    public float airborneGravity = 3f;    // Havada yerçekimi çarpanı

    [Header("Buz Seviyesi (Kat 9) Ayarları")]
    public float iceAcceleration = 80f;   // Buzda daha hızlı ivmelenme
    public float iceDrag = 1f;            // Buzda kayma hissi
    public float meltedIceDrag = 0.2f;    // Faz 3+ olunca su üstünde kayma
    private bool onIce;                   // Buzda mıyız?

    [Header("Mach ve Hız Limitleri")]
    public float walkSpeed = 10f;
    public float mach4Speed = 35f;
    public float machIncreaseRate = 10f;
    public float currentMaxSpeed;
    public int currentPhase = 1;
    public float mach5Speed = 50f;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 15f;
    public float horizontalJumpBoost = 5f;
    private int jumpCount = 0;

    [Header("Duvar ve Sekme Ayarları")]
    public Transform wallCheck;
    public float wallBounceForceX = 15f;
    public float wallBounceForceY = 5f;
    private bool isTouchingWall;

    [Header("Durum Kontrolleri")]
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private bool isRunning;
    private SpriteRenderer sprite;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        currentMaxSpeed = walkSpeed;

        // Auto-Fix: Physics Material to prevent bouncing/sticking
        PhysicsMaterial2D slipMat = new PhysicsMaterial2D();
        slipMat.friction = 0f;
        slipMat.bounciness = 0f;
        rb.sharedMaterial = slipMat;
        GetComponent<Collider2D>().sharedMaterial = slipMat;

        // Auto-Fix: Freeze Rotation so we don't tip over
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("Physics Auto-Configured: Zero Friction, Rotation Frozen.");
        
        // PARENT SCALE CHECK
        if (transform.parent != null)
        {
            Vector3 pScale = transform.parent.lossyScale;
            if (Mathf.Abs(pScale.x - pScale.y) > 0.01f)
            {
                Debug.LogError($"UYARI: Karakterinizin Parent objesi ('{transform.parent.name}') orantısız scale'e sahip ({pScale})! Bu durum dönünce karakteri yamultur/esnetir. Lütfen Parent scale'ini (1,1,1) yapın.");
            }
        }

        if (footstepSource != null) footstepSource.loop = true;
    }

    private Vector2 groundNormal = Vector2.up;
    private float lastJumpTime;
    private bool wasGrounded;

    [Header("Prediction & Snapping")]
    public float predictionOffset = 1.0f; // How far ahead to look for slopes
    public float stepHeight = 0.5f;       // Max height we can auto-step
    public float stepSmooth = 0.1f;       // Smoothing for step snap (optional)
    public float minLoopSpeed = 10f;      // Speed required to stick to walls/loops

    [Header("Gelişmiş Loop Ayarları")]
    public float groundCheckOffset = 0.5f; // Distance for front/back raycasts
    public float loopLaunchCutoff = 0.5f; // Normal Y value below which we consider "Launching"
    public LayerMask whatIsGround; // Ensure we use this consistent naming if needed, but script uses groundLayer
    
    // State
    private Vector2 groundNormalBackend; // Calculated in FixedUpdate
    private float timeSinceGrounded;
    private bool disableGroundSnapping; // For launching

    [Header("Loop Launch Ayarları")]
    public float launchControlDuration = 0.5f; // Duration for input disable
    private float inputDisableEndTime;
    private float iceBreakOverrideEndTime; // New: Timer for breaking ice after launch

    [Header("Efekt Nesneleri")]
    public GameObject flamerObject; // Buz kırıcı etkisi olan obje

    [Header("Ses Efektleri")]
    public AudioSource footstepSource; // Yürüme/Koşma sesi için (Loop olmalı)
    public AudioSource sfxSource;      // Zıplama/Düşme sesi için (OneShot)
    public AudioClip walkClip;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip swingClip;     // Sallanma sesi (Loop)
    public AudioClip launchClip;    // Loop Fırlatma sesi (OneShot)
    public AudioClip iceBreakClip;  // Buz kırma sesi (OneShot)
    public float runPitchMultiplier = 1.5f; // Koşarken ses ne kadar hızlansın?
    
    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // --- BUZ KIRMA DURUMU KONTROLÜ ---
        // Faz 3+ veya Launch Override varsa flamer aktif olsun
        bool canBreakIce = (currentPhase >= 3 && Mathf.Abs(moveInput) > 0.1f) || (Time.time < iceBreakOverrideEndTime);
        
        if (flamerObject != null)
        {
            flamerObject.SetActive(canBreakIce);
        }

        // --- INPUT DISABLE (Loop Launch) ---
        // Block user input for a short time after launch
        if (Time.time < inputDisableEndTime)
        {
            moveInput = 0f;
        }

        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Jump Input
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= inputDisableEndTime)
        {
            Jump();
        }

        UpdateAnimations();
        
        // --- LOCAL FLIP ---
        if (moveInput > 0) 
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0) 
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            
        HandlePhaseSystem(); // Keep existing phase logic
        HandleAudio();
    }

    void HandleAudio()
    {
        // 0. Oyun Başlamadıysa Ses Çalmasın
        if (Time.timeScale == 0)
        {
            if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        if (footstepSource == null) return;

        // 1. SALLANMA DURUMU (Swinging)
        if (isSwinging)
        {
            if (swingClip != null)
            {
                if (footstepSource.clip != swingClip)
                {
                    footstepSource.clip = swingClip;
                    footstepSource.pitch = 1f; // Swing pitch'i normal olsun
                    footstepSource.Play();
                }
            }
            return; // Sallanıyorsak yürüme sesine geçmesin
        }

        if (walkClip == null) return;

        // 2. YÜRÜME / KOŞMA DURUMU
        // Yerdeysek ve hareket ediyorsak ses çal (ve swing değilsek)
        if (isGrounded && Mathf.Abs(moveInput) > 0.1f)
        {
            if (footstepSource.clip != walkClip || !footstepSource.isPlaying)
            {
                footstepSource.clip = walkClip;
                footstepSource.Play();
            }

            // Koşma durumuna göre pitch ayarla
            if (isRunning)
            {
                footstepSource.pitch = runPitchMultiplier;
            }
            else
            {
                footstepSource.pitch = 1f;
            }
        }
        else
        {
            // Duruyorsak veya havadaysak sesi durdur
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    void FixedUpdate()
    {
        CheckGroundAndRotation();
        CheckHeadCollision();
        HandleWallCollision();
        
        ApplyMovement();
        ApplyFriction();
        ApplyAdhesion();

        wasGrounded = isGrounded;
    }

    void CheckGroundAndRotation()
    {
        if (disableGroundSnapping)
        {
            isGrounded = false;
            rb.gravityScale = airborneGravity;
            // Re-enable snapping after a short delay or if we fall down
            if (Time.time > lastJumpTime + 0.2f) disableGroundSnapping = false;
            return;
        }

        // Disable ground check while swinging to allow free movement
        if (isSwinging)
        {
            isGrounded = false;
            rb.gravityScale = airborneGravity; 
            return;
        }

        // --- DUAL RAYCAST SYSTEM ---
        // Calculate offsets relative to valid rotation
        Vector3 rightOffset = transform.right * groundCheckOffset;
        Vector3 upOffset = transform.up * 0.2f; // Lift slightly to avoid clipping

        // Front and Back Raycasts
        Vector2 originCenter = groundCheck.position + upOffset;
        Vector2 originFront = (Vector2)(groundCheck.position + rightOffset) + (Vector2)upOffset;
        Vector2 originBack  = (Vector2)(groundCheck.position - rightOffset) + (Vector2)upOffset;

        float checkDistance = 1.0f; // Slightly longer to catch curves
        
        RaycastHit2D hitCenter = Physics2D.Raycast(originCenter, -transform.up, checkDistance, groundLayer);
        RaycastHit2D hitFront  = Physics2D.Raycast(originFront, -transform.up, checkDistance, groundLayer);
        RaycastHit2D hitBack   = Physics2D.Raycast(originBack, -transform.up, checkDistance, groundLayer);

        bool centerHit = hitCenter.collider != null;
        bool frontHit = hitFront.collider != null;
        bool backHit = hitBack.collider != null;

        // Determine if strictly grounded (at least one valid hit)
        isGrounded = centerHit || frontHit || backHit;

        if (isGrounded)
        {
            timeSinceGrounded = 0f;
            jumpCount = 0; // Reset jump count when grounded
            onIce = (centerHit && hitCenter.collider.CompareTag("Ice")) || 
                    (frontHit && hitFront.collider.CompareTag("Ice"));

            // --- NORMAL AVERAGING ---
            Vector2 combinedNormal = Vector2.zero;
            int count = 0;

            if (centerHit) { combinedNormal += hitCenter.normal; count++; }
            if (frontHit)  { combinedNormal += hitFront.normal; count++; }
            if (backHit)   { combinedNormal += hitBack.normal; count++; }
            
            if (count > 0) combinedNormal /= count;
            groundNormalBackend = combinedNormal.normalized;

            // --- ROTATION ALIGNMENT ---
            // Replaced FromToRotation with Euler to prevent 3D skewing artifacts
            float angle = Mathf.Atan2(groundNormalBackend.y, groundNormalBackend.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            
            float rotSpeed = 720f; // Fast alignment
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime);
            
            // Fix Gravity on Loops
            rb.gravityScale = 0f; 

            // Yere düşme sesi
            if (!wasGrounded && sfxSource != null && landClip != null)
            {
                // Sadece yere çarptığımızda çal (sürekli değil)
                // wasGrounded FixedUpdate sonunda güncelleniyor, o yüzden burası güvenli
                sfxSource.PlayOneShot(landClip);
            } 
        }
        else
        {
             // --- LOOP LAUNCH CHECK (REMOVED - Replaced by LoopLauncher Script) ---
             timeSinceGrounded += Time.fixedDeltaTime;
             
             // Smoothly rotate back to upright IF we are not in a loop launch trajectory
             // Actually, for better feeling, let physics handle rotation in air, or slow correct
             rb.gravityScale = airborneGravity;
             
             // Slow align to upright
             transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, 180f * Time.fixedDeltaTime);
        }
    }

    void CheckHeadCollision()
    {
        // Simple fix: If head hits ceiling, rotate to match checks
        // Prevents getting stuck when jumping into slanted ceilings
        Vector2 headOrigin = (Vector2)transform.position + (Vector2)(transform.up * 1.0f);
        RaycastHit2D headHit = Physics2D.Raycast(headOrigin, transform.up, 0.5f, groundLayer);

        if (!isGrounded && headHit.collider != null)
        {
             // We hit a ceiling! Align to it
             Vector2 ceilingNormal = headHit.normal;
             Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, ceilingNormal);
             transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.fixedDeltaTime);
        }
    }

    void ApplyAdhesion()
    {
        if (isSwinging) return; // Don't stick to walls while swinging
        if (disableGroundSnapping || !isGrounded) return;

        // Calculate Speed
        float currentSpeed = rb.linearVelocity.magnitude;

        // --- SPEED BASED ADHESION ---
        // If we are upside down (or steep) AND too slow -> FALL
        bool isSteep = groundNormalBackend.y < 0.5f;
        
        if (isSteep && currentSpeed < minLoopSpeed)
        {
            // Fall off the loop
            rb.gravityScale = 3f;
            return; // No adhesion
        }

        // Apply down force relative to SURFACES
        float stickForce = 50f + (currentSpeed * 2f);
        rb.AddForce(-groundNormalBackend * stickForce, ForceMode2D.Force);
    }

    void ApplyMovement()
    {
        if (isSwinging) return; // Physics handled by Joint
        if (Mathf.Abs(moveInput) < 0.1f) return;

        // --- VELOCITY LIMIT CHECK (Local X) ---
        float localVelX = transform.InverseTransformDirection(rb.linearVelocity).x;
        float targetLimit = currentMaxSpeed;
        bool sameDirection = Mathf.Sign(moveInput) == Mathf.Sign(localVelX);

        if (Mathf.Abs(localVelX) > targetLimit && sameDirection) return;

        // BUZDA MIYIZ?
        float currentAccel = onIce ? iceAcceleration : acceleration;
        
        // --- MOVE DIRECTION ---
        Vector3 forceDir = transform.right * moveInput;
        rb.AddForce(forceDir * currentAccel, ForceMode2D.Force);
    }

    void ApplyFriction()
    {
        if (isSwinging) return; // No friction while swinging
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            float dragAmount;

            if (isGrounded)
            {
                if (onIce) dragAmount = (currentPhase >= 3) ? meltedIceDrag : iceDrag;
                else dragAmount = groundDrag;
            }
            else dragAmount = airDrag;

            // Local velocity dampening
            Vector2 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            float newLocalX = Mathf.MoveTowards(localVel.x, 0, dragAmount * 10 * Time.fixedDeltaTime);
            
            Vector2 newLocalVel = new Vector2(newLocalX, localVel.y);
            rb.linearVelocity = transform.TransformDirection(newLocalVel);
        }
    }

    // --- BURADAN AŞAĞISI SENİN ORİJİNAL KODUNLA AYNI ---

    void HandlePhaseSystem() // Omitted for brevity, assumed unchanged
    {
        float rateMult = (activePowerUp != null) ? activePowerUp.machRateMultiplier : 1f;
        if (activePowerUp != null && activePowerUp.hasHammerTail) rateMult *= 0.5f;

        if (activePowerUp != null && activePowerUp.canReachPhase5)
            rateMult += (currentPhase * 0.1f);

        float currentRate = machIncreaseRate * rateMult;
        float maxLimit = (activePowerUp != null && activePowerUp.canReachPhase5) ? mach5Speed : mach4Speed;

        float currentSpeed = Mathf.Abs(transform.InverseTransformDirection(rb.linearVelocity).x);

        if (isRunning && currentSpeed > walkSpeed - 1f && moveInput != 0 && isGrounded)
            currentMaxSpeed = Mathf.MoveTowards(currentMaxSpeed, maxLimit, currentRate * Time.deltaTime);
        else
            currentMaxSpeed = Mathf.MoveTowards(currentMaxSpeed, walkSpeed, currentRate * 2 * Time.deltaTime);

        float speedPct = currentSpeed / maxLimit;
        if (activePowerUp != null && activePowerUp.canReachPhase5)
        {
            if (speedPct < 0.20f) currentPhase = 1;
            else if (speedPct < 0.40f && isRunning) currentPhase = 2;
            else if (speedPct < 0.60f && isRunning) currentPhase = 3;
            else if (speedPct < 0.80f && isRunning) currentPhase = 4;
            else if (speedPct < 5f && isRunning) currentPhase = 5;
        }
        else
        {
            if (speedPct < 0.33f) currentPhase = 1;
            else if (speedPct < 0.66f && isRunning) currentPhase = 2;
            else if (speedPct < 0.90f && isRunning) currentPhase = 3;
            else if (speedPct < 5f && isRunning) currentPhase = 4;
        }
    }

    void Jump()
    {
        int maxJumps = (activePowerUp != null && activePowerUp.canDoubleJump) ? 2 : 1;
        if (isGrounded || jumpCount < maxJumps)
        {
            lastJumpTime = Time.time;
            disableGroundSnapping = true; // Crucial: Stop sticking immediately 
            
            Vector2 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            localVel.y = 0;
            rb.linearVelocity = transform.TransformDirection(localVel);

            float finalHorizontalBoost = (currentPhase >= 3) ? horizontalJumpBoost * moveInput : 0;
            float jumpMult = (activePowerUp != null) ? activePowerUp.jumpMultiplier : 1f;
            
            Vector3 localJump = new Vector3(finalHorizontalBoost, jumpForce * jumpMult, 0);
            rb.AddForce(transform.TransformDirection(localJump), ForceMode2D.Impulse);
            jumpCount++;

            // Zıplama Sesi
            if (sfxSource != null && jumpClip != null)
            {
                sfxSource.PlayOneShot(jumpClip);
            }
        }
    }

    void HandleWallCollision()
    {
        // 1. Check for wall/obstacle ahead using OverlapCircleAll to catch overlapping colliders
        Collider2D[] hitObjs = Physics2D.OverlapCircleAll(wallCheck.position, 0.4f, groundLayer);

        foreach (Collider2D hitObj in hitObjs)
        {
            // A. Check for Breakable Ice
            BreakableIce ice = hitObj.GetComponent<BreakableIce>();
            // Break ONLY if moving (user request: "move at it directly")
            // OR if we have the launch override
            bool canBreak = (currentPhase >= 3 && Mathf.Abs(moveInput) > 0.1f) || (Time.time < iceBreakOverrideEndTime);
            
            if (ice != null && canBreak)
            {
                ice.Break(); 

                // Buz Kırma Sesi
                if (sfxSource != null && iceBreakClip != null)
                {
                    sfxSource.PlayOneShot(iceBreakClip);
                }

                return; // Stop processing collision if we broke something
            }
        }

        // Re-get for Physics checks (Wall vs Slope) - Just grab the first non-trigger or valid ground
        Collider2D collisionHit = Physics2D.OverlapCircle(wallCheck.position, 0.4f, groundLayer);
        
        if (collisionHit != null)
        {
            // A. Skip Ice (already handled or not breakable)
            if (collisionHit.GetComponent<BreakableIce>() != null) return;

            // B. SLOPE VS WALL CHECK
            // Use Raycast to get the normal of the surface we are hitting
            Vector2 direction = transform.right * Mathf.Sign(transform.localScale.x);
            RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, 0.5f, groundLayer);

            if (hit.collider != null)
            {
                // Calculate angle of the surface relative to PLAYER UP (Local)
                // Using Vector2.up caused vertical walls (loops) to read as 90 deg (Obstacle)
                float angle = Vector2.Angle(hit.normal, transform.up);
                
                // Debug visualization
                Debug.DrawRay(wallCheck.position, direction, Color.yellow);
                
                // If it's a slope relative to us (e.g. < 75 degrees), treat as walkable
                // If it's a slope relative to us (e.g. < 75 degrees), treat as walkable
                if (angle < 75f) 
                {
                    return; 
                }

                // C. BOUNCE (Only if we are NOT trying to move into it, or it's a bouncy wall)
                // Moved inside check to ensure we only bounce on confirmed steep walls
                if (currentPhase >= 3)
                {
                    rb.linearVelocity = Vector2.zero;
                    float bounceDirection = -Mathf.Sign(moveInput);
                    Vector2 bounceForce = new Vector2(bounceDirection * wallBounceForceX, wallBounceForceY);
                    rb.AddForce(bounceForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    void UpdateAnimations()
    {
        // Use local speed for animation
        float localSpeed = Mathf.Abs(transform.InverseTransformDirection(rb.linearVelocity).x);
        anim.SetFloat("Speed", localSpeed);
        anim.SetBool("IsRunning", isRunning);
    }

    // HandleSpriteFlip removed (integrated into Update with SpriteRenderer.flipX)

    void HandleStepClimb()
    {
        if (moveInput == 0 || !isGrounded) return;

        float dir = Mathf.Sign(moveInput);
        
        // 1. Raycast at feet level (Low)
        Vector2 originLow = transform.position - (transform.up * 0.4f); // Slightly above pivot/feet
        RaycastHit2D hitLow = Physics2D.Raycast(originLow, transform.right * dir, 0.5f, groundLayer);

        if (hitLow.collider != null)
        {
            // 2. Raycast at Head/Knee level (High)
            Vector2 originHigh = transform.position - (transform.up * 0.45f) + (Vector3)(transform.up * stepHeight);
            
            // Debug.DrawLine(originLow, originLow + (Vector2)(transform.right * dir * 0.5f), Color.red);
            // Debug.DrawLine(originHigh, originHigh + (Vector2)(transform.right * dir * 0.6f), Color.blue);

            RaycastHit2D hitHigh = Physics2D.Raycast(originHigh, transform.right * dir, 0.6f, groundLayer);

            // If Low hits (wall) but High misses (air), it's a step!
            if (hitHigh.collider == null)
            {
                 // Teleport up slightly
                 rb.position += (Vector2)(transform.up * 0.15f);
            }
        }
    }

    // --- EXTERNAL LAUNCHER METHOD ---
    public void ApplyExternalLaunch(Vector2 velocity, bool fixRotation)
    {
        rb.linearVelocity = velocity;
        
        // Disable Controls to preserve momentum
        inputDisableEndTime = Time.time + launchControlDuration;
        
        // Allow breaking ice for 1 second
        iceBreakOverrideEndTime = Time.time + 1.0f;
        
        disableGroundSnapping = true;
        
        if (fixRotation)
        {
            // Reset rotation to upright (0,0,0) so player lands on feet
            transform.rotation = Quaternion.identity;
        }

        // Fırlatma Sesi
        if (sfxSource != null && launchClip != null)
        {
            sfxSource.PlayOneShot(launchClip);
        }
    }

    // --- GRAPPLING HOOK SUPPORT ---
    private bool isSwinging;
    public void SetSwinging(bool swinging)
    {
        isSwinging = swinging;
        // Opsiyonel: Sallanırken animasyonu değiştir?
        // anim.SetBool("IsSwinging", swinging);
    }
    
    // Getter for hooking script
    public Rigidbody2D GetRigidbody() { return rb; }
    public bool IsSwinging() { return isSwinging; }
}