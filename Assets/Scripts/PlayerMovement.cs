using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [field: SerializeField] public float MoveSpeed { set; get; } = 5f;
    [field: SerializeField] public float RunSpeed { set; get; } = 10f;

    [field: SerializeField] public float JumpForce { set; get; } = 7f;
    [field: SerializeField] public float MouseSensitivity { set; get; } = 100f;
    [field: SerializeField] public LayerMask GroundLayer { set; get; }

    // [Header("Shooting Settings")] // Source [1-3] 참고
    // [field: SerializeField]
    // public int GunDamage { set; get; } = 1;
    //
    // [field: SerializeField] public float FireRate { set; get; } = 0.25f;
    // [field: SerializeField] public float WeaponRange { set; get; } = 50f;
    // [field: SerializeField] public float HitForce { set; get; } = 100f;
    // [field: SerializeField] public float ShootRecoil { set; get; } = 5.0f; // Source [4]

    [Header("References")]
    [field: SerializeField] public Transform PlayerCamera { set; get; }

    // [field: SerializeField] public Transform GunEnd { set; get; } = null!; // Source [5]
    [field: SerializeField] private Rigidbody Rigidbody { set; get; }
    [field: SerializeField] private Animator Animator { set; get; }
    // [field: SerializeField] public AudioClip ShootSound { set; get; } = null!; // Source [6]

    [Header("Input Actions")]
    [field: SerializeField] public InputActionReference MoveAction { set; get; } = null!;
    [field: SerializeField] public InputActionReference RunAction { set; get; } = null!;

    [field: SerializeField] public InputActionReference LookAction { set; get; }
    [field: SerializeField] public InputActionReference JumpAction { set; get; }
    [field: SerializeField] public InputActionReference CrouchAction { set; get; }
    // [field: SerializeField] public InputActionReference FireAction { set; get; }
    [Header("Advanced Movement")]
    private bool isSliding;
    private float slideTimer;
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchHeight = 1.0f; // 웅크렸을 때 높이
    [SerializeField] private float slideForce = 15f;    // 슬라이딩 힘
    [SerializeField] private float diveForce = 10f;
    
    private float originalHeight;
    private bool isCrouching;
    private float XRotation = 0f;
    private bool IsGrounded;
    private float NextFireTime;
    private AudioSource audioSource;
    private CapsuleCollider capsule;
    private Vector3 originalCenter;
    [SerializeField] Transform visualRoot;
    private Vector3 visualOriginalLocalPos;
    [SerializeField] private float crouchVisualOffsetY = -0.45f;
    [SerializeField] private float slideVisualOffsetY = -0.55f;    
    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponentInChildren<Animator>();

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        if (visualRoot == null && Animator != null)
            visualRoot = Animator.transform;

        if (visualRoot != null)
            visualOriginalLocalPos = visualRoot.localPosition;
    }
    
    private void OnEnable()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponentInChildren<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>(); // 사운드 소스 자동 추가

        // Only with this code will Unity start accepting keyboard/mouse inputs!
        if (MoveAction != null) MoveAction.action.Enable();
        if (LookAction != null) LookAction.action.Enable();
        if (JumpAction != null) JumpAction.action.Enable();
        if (RunAction != null) RunAction.action.Enable();
        // Event connect
        if (JumpAction != null) JumpAction.action.performed += OnJump;
        if (CrouchAction != null)
        {
            CrouchAction.action.Enable();
            CrouchAction.action.performed += OnCrouchStarted;
            CrouchAction.action.canceled += OnCrouchCanceled;
        }
        Cursor.lockState = CursorLockMode.Locked;

        // Event connect
        // JumpAction.action.performed += OnJump;
        // FireAction.action.performed += OnFire; // 사격 버튼 이벤트
    }
    
    private void OnDisable()
    {
        if (CrouchAction != null)
        {
            CrouchAction.action.performed -= OnCrouchStarted;
            CrouchAction.action.canceled -= OnCrouchCanceled;
            CrouchAction.action.Disable();
            if (JumpAction != null)
                JumpAction.action.performed -= OnJump;
        }
    }
    
    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        StartCrouch();
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        StopCrouch();
    }
    
    private void Update()
    {
        CheckGround();      // Make sure it's on the ground
        LookAround();       // 마우스로 시점 회전
        Move();             // Move WASD (Run the function you created below)
        UpdateAnimation();
    }
    private void Move()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;

                capsule.height = originalHeight;
                capsule.center = originalCenter;

                if (visualRoot != null)
                    visualRoot.localPosition = visualOriginalLocalPos;
            }
            return;
        }

        var moveInput = MoveAction.action.ReadValue<Vector2>();
        bool isRunning = RunAction != null && RunAction.action.IsPressed();
        float currentTargetSpeed = isRunning ? RunSpeed : MoveSpeed;

        var move = transform.right * moveInput.x + transform.forward * moveInput.y;
        var velocity = move * currentTargetSpeed;

        if (isCrouching)
            velocity /= 2f;

        velocity.y = Rigidbody.linearVelocity.y;
        Rigidbody.linearVelocity = velocity;
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded)
        {
            IsGrounded = false;
            Rigidbody.linearVelocity = new Vector3(
                Rigidbody.linearVelocity.x,
                JumpForce,
                Rigidbody.linearVelocity.z
            );
        }
    }
    private IEnumerator CameraShake(float duration, float intensity)
    {
        Vector3 originalPos = PlayerCamera.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            PlayerCamera.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        PlayerCamera.localPosition = originalPos;
    }
    
    private void StartCrouch()
    {
        bool canSlide = IsGrounded &&
                        (RunAction != null && RunAction.action.IsPressed()) &&
                        Rigidbody.linearVelocity.magnitude > MoveSpeed + 1f;

        if (canSlide)
        {
            StartSliding();
            return;
        }

        isCrouching = true;

        capsule.height = crouchHeight;
        capsule.center = new Vector3(originalCenter.x, crouchHeight * 0.5f, originalCenter.z);

        if (visualRoot != null)
            visualRoot.localPosition = visualOriginalLocalPos + new Vector3(0f, crouchVisualOffsetY, 0f);
    }

    private void StopCrouch()
    {
        if (isSliding) return;

        isCrouching = false;

        capsule.height = originalHeight;
        capsule.center = originalCenter;

        if (visualRoot != null)
            visualRoot.localPosition = visualOriginalLocalPos;
    }
    
    private void StartSliding()
    {
        isSliding = true;
        isCrouching = false;
        slideTimer = slideDuration;

        capsule.height = crouchHeight;
        capsule.center = new Vector3(originalCenter.x, crouchHeight * 0.5f, originalCenter.z);
        if (visualRoot != null)
            visualRoot.localPosition = visualOriginalLocalPos + new Vector3(0f, slideVisualOffsetY, 0f);
        Vector3 slideDir = transform.forward;
        Vector3 vel = Rigidbody.linearVelocity;
        vel.x = slideDir.x * slideForce;
        vel.z = slideDir.z * slideForce;
        Rigidbody.linearVelocity = vel;
    }
    private void UpdateAnimation()
    {
        var moveInput = MoveAction.action.ReadValue<Vector2>();
        bool isPressed = RunAction != null && RunAction.action.IsPressed();

        float inputMagnitude = moveInput.magnitude;
        bool hasInput = inputMagnitude > 0.15f;

        if (Animator != null)
        {
            bool isRunning = hasInput && isPressed && !isCrouching && IsGrounded;
            bool isWalking = hasInput && !isPressed && !isCrouching && IsGrounded;
            bool isJumping = !IsGrounded;
           // bool isJumping = !IsGrounded && Rigidbody.linearVelocity.y > 0.1f;
           // bool isFalling = !IsGrounded && Rigidbody.linearVelocity.y < -0.1f;

            Animator.SetBool("isJumping", isJumping);
            // Animator.SetBool("isFalling", isFalling); // 파라미터 있으면 사용
            Animator.SetBool("isRunning", isRunning);
            Animator.SetBool("isWalking", isWalking);
            Animator.SetBool("isCrouch", isCrouching && !isSliding);
            Animator.SetBool("isSliding", isSliding);

            if (hasInput)
            {
                float targetValue = isRunning ? 1.0f : 0.5f;
                Animator.SetFloat("Speed", targetValue, 0.05f, Time.deltaTime);
            }
            else
            {
                Animator.SetBool("isRunning", false);
                Animator.SetBool("isWalking", false);
                Animator.SetFloat("Speed", 0f);
            }
        }
    }
    private void CheckGround()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();

        Vector3 origin = transform.position + col.center;
        float rayDistance = (col.height * 0.5f) + 0.15f;

        IsGrounded = Physics.Raycast(origin, Vector3.down, rayDistance, GroundLayer);
    }
    public bool canLook = true; 

    private void LookAround()
    {
        if (!canLook) return; 

        var mouseInput = LookAction.action.ReadValue<Vector2>();
        float mouseX = mouseInput.x * MouseSensitivity * Time.deltaTime;
        float mouseY = mouseInput.y * MouseSensitivity * Time.deltaTime;

        XRotation -= mouseY;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        PlayerCamera.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}