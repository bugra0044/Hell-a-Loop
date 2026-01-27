using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("PowerUp Sistemi")]
    public PowerUpData activePowerUp;

    [Header("Temel Ayarlar")]
    public float acceleration = 50f;      // Normal hızlanma
    public float groundDrag = 5f;         // Normal sürtünme
    public float airDrag = 1f;            // Hava sürtünmesi

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
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Zemin ve Buz Kontrolü
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = groundCollider != null;

        if (isGrounded)
        {
            Debug.Log("Yerdeyim! Değdiğim objenin adı: " + groundCollider.gameObject.name + " Tag: " + groundCollider.tag);
            // Değdiğimiz objenin Tag'i "Ice" mı?
            onIce = groundCollider.CompareTag("Ice");
        }
        else
        {
            onIce = false;
        }

        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0;
        }

        HandlePhaseSystem();
        HandleWallBounce();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        UpdateAnimations();
        HandleSpriteFlip();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyFriction();
    }

    void ApplyMovement()
    {
        if (Mathf.Abs(moveInput) < 0.1f) return;

        float currentXSpeed = rb.linearVelocity.x;
        float targetLimit = currentMaxSpeed;
        bool sameDirection = Mathf.Sign(moveInput) == Mathf.Sign(currentXSpeed);

        if (Mathf.Abs(currentXSpeed) > targetLimit && sameDirection)
        {
            return;
        }

        // BUZDA MIYIZ?
        float currentAccel = onIce ? iceAcceleration : acceleration;
        rb.AddForce(new Vector2(moveInput * currentAccel, 0), ForceMode2D.Force);
    }

    void ApplyFriction()
    {
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            float dragAmount;

            if (isGrounded)
            {
                if (onIce)
                {
                    // Faz 3 ve üstünde buz erir, sürtünme iyice düşer
                    dragAmount = (currentPhase >= 3) ? meltedIceDrag : iceDrag;

                }
                else
                {
                    dragAmount = groundDrag;
                }
            }
            else
            {
                dragAmount = airDrag;
            }

            float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, 0, dragAmount * 10 * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
        }
    }

    // --- BURADAN AŞAĞISI SENİN ORİJİNAL KODUNLA AYNI ---

    void HandlePhaseSystem()
    {
        float rateMult = (activePowerUp != null) ? activePowerUp.machRateMultiplier : 1f;
        if (activePowerUp != null && activePowerUp.hasHammerTail) rateMult *= 0.5f;

        if (activePowerUp != null && activePowerUp.canReachPhase5)
            rateMult += (currentPhase * 0.1f);

        float currentRate = machIncreaseRate * rateMult;
        float maxLimit = (activePowerUp != null && activePowerUp.canReachPhase5) ? mach5Speed : mach4Speed;

        if (isRunning && Mathf.Abs(rb.linearVelocity.x) > walkSpeed - 1f && moveInput != 0 && isGrounded)
            currentMaxSpeed = Mathf.MoveTowards(currentMaxSpeed, maxLimit, currentRate * Time.deltaTime);
        else
            currentMaxSpeed = Mathf.MoveTowards(currentMaxSpeed, walkSpeed, currentRate * 2 * Time.deltaTime);

        float speedPct = Mathf.Abs(rb.linearVelocity.x) / maxLimit;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            float finalHorizontalBoost = (currentPhase >= 3) ? horizontalJumpBoost * moveInput : 0;
            float jumpMult = (activePowerUp != null) ? activePowerUp.jumpMultiplier : 1f;
            Vector2 jumpVector = new Vector2(finalHorizontalBoost, jumpForce * jumpMult);
            rb.AddForce(jumpVector, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    void HandleWallBounce()
    {
        // 1. Burnumuzun ucunda bir şeye değiyor muyuz?
        // (Yarıçapı 0.2'den 0.4'e çıkardım ki biraz daha uzaktan fark edip yok edebilelim)
        Collider2D hitObj = Physics2D.OverlapCircle(wallCheck.position, 0.4f, groundLayer);

        if (hitObj != null)
        {
            // 2. Değdiğimiz şey "Kırılabilir Buz" mu?
            BreakableIce ice = hitObj.GetComponent<BreakableIce>();

            // Eğer Buz ise VE Hızlıysak (Faz 3+)
            if (ice != null && currentPhase >= 3)
            {
                ice.Break(); // Buzu uzaktan patlat!
                return;      // AŞAĞIDAKİ SEKME KODUNU ÇALIŞTIRMA! Yoluna devam et.
            }

            // --- BURASI NORMAL DUVAR MANTIĞI ---
            // Eğer buz değilse (veya yavaşsak) ve duvara çarptıysak sektir
            if (currentPhase >= 3)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                float bounceDirection = -Mathf.Sign(transform.localScale.x);
                Vector2 bounceForce = new Vector2(bounceDirection * wallBounceForceX, wallBounceForceY);
                rb.AddForce(bounceForce, ForceMode2D.Impulse);

                // Debug.Log("Normal duvara tosladık!");
            }
        }
    }

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsRunning", isRunning);
    }

    void HandleSpriteFlip()
    {
        if (moveInput > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}