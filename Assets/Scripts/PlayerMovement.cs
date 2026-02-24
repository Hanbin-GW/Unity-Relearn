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
    // [field: SerializeField] public InputActionReference FireAction { set; get; }

    private float XRotation = 0f;
    private bool IsGrounded;
    private float NextFireTime;
    private AudioSource audioSource;
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
        
        Cursor.lockState = CursorLockMode.Locked;

        // Event connect
        JumpAction.action.performed += OnJump;
        // FireAction.action.performed += OnFire; // 사격 버튼 이벤트
    }
    
    private void Update()
    {
        CheckGround();      // Make sure it's on the ground
        LookAround();       // 마우스로 시점 회전
        Move();             // Move WASD (Run the function you created below)
        UpdateAnimation();  // Playing walking animation
    }
    private void Move()
    {
        var moveInput = MoveAction.action.ReadValue<Vector2>();
        bool isRunning = RunAction != null && RunAction.action.IsPressed();
    
        // calculate the running speed here
        float currentTargetSpeed = isRunning ? RunSpeed : MoveSpeed;
    
        var moveX = moveInput.x;
        var moveZ = moveInput.y;

        var move = transform.right * moveX + transform.forward * moveZ;
        // you should multiply currentTargetSpeed!
        var velocity = move * currentTargetSpeed; 

        velocity.y = Rigidbody.linearVelocity.y;
        Rigidbody.linearVelocity = velocity;
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded)
        {
            Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, JumpForce, Rigidbody.linearVelocity.z);
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

    private void UpdateAnimation()
    {
        var moveInput = MoveAction.action.ReadValue<Vector2>();
        bool isPressed = RunAction != null && RunAction.action.IsPressed();
    
        // 조이스틱이나 키보드의 미세한 입력을 무시하기 위해 0.15f 정도로 설정합니다.
        float inputMagnitude = moveInput.magnitude;
        bool hasInput = inputMagnitude > 0.15f; 

        if (Animator != null)
        {
            // Performs a move decision only when there is input.
            // When running, clearly make isWalking false to prevent collisions.
            bool isRunning = hasInput && isPressed;
            bool isWalking = hasInput && !isPressed; 

            // Update animator parameters
            Animator.SetBool("isRunning", isRunning);
            Animator.SetBool("isWalking", isWalking);

            // Modify Speed value
            if (hasInput)
            {
                float targetValue = isRunning ? 1.0f : 0.5f;
                Animator.SetFloat("Speed", targetValue, 0.05f, Time.deltaTime);
            }
            else
            {
                // If there is no input, initialize all movement-related variables immediately.
                Animator.SetBool("isRunning", false);
                Animator.SetBool("isWalking", false);
                Animator.SetFloat("Speed", 0f);
            }
        }
    }
    private void CheckGround()
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, GroundLayer);
    }

    private void LookAround()
    {
        var mouseInput = LookAction.action.ReadValue<Vector2>();
        float mouseX = mouseInput.x * MouseSensitivity * Time.deltaTime;
        float mouseY = mouseInput.y * MouseSensitivity * Time.deltaTime;

        XRotation -= mouseY;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        PlayerCamera.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}