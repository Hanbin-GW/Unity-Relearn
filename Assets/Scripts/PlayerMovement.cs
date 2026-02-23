using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [field: SerializeField]
    public float MoveSpeed { set; get; } = 5f;

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
    [field: SerializeField]
    public Transform PlayerCamera { set; get; }

    // [field: SerializeField] public Transform GunEnd { set; get; } = null!; // Source [5]
    [field: SerializeField] private Rigidbody Rigidbody { set; get; }
    [field: SerializeField] private Animator Animator { set; get; }
    // [field: SerializeField] public AudioClip ShootSound { set; get; } = null!; // Source [6]

    [Header("Input Actions")]
    [field: SerializeField]
    public InputActionReference MoveAction { set; get; } = null!;

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

            // 🌟 [핵심 해결책] 조작 키 스위치 켜기 (Enable)
        // 이 코드가 있어야만 유니티가 키보드/마우스 입력을 받아들이기 시작합니다!
        if (MoveAction != null) MoveAction.action.Enable();
        if (LookAction != null) LookAction.action.Enable();
        if (JumpAction != null) JumpAction.action.Enable();

        // 이벤트 연결
        if (JumpAction != null) JumpAction.action.performed += OnJump;
        
        Cursor.lockState = CursorLockMode.Locked;

        // 이벤트 연결
        JumpAction.action.performed += OnJump;
        // FireAction.action.performed += OnFire; // 사격 버튼 이벤트
    }
    
    private void Update()
    {
        CheckGround();      // 땅에 닿아있는지 확인
        LookAround();       // 마우스로 시점 회전
        Move();             // WASD 이동 (아래에 만든 함수 실행)
        UpdateAnimation();  // 걷는 애니메이션 재생
    }
    private void Move()
    {
        var moveInput = MoveAction.action.ReadValue<Vector2>();
        var moveX = moveInput.x;
        var moveZ = moveInput.y;

        var move = transform.right * moveX + transform.forward * moveZ;
        var velocity = move * MoveSpeed;
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

    // // --- 사격 및 피드백 로직 (Source [1, 4, 7-9] 통합) ---
    // private void OnFire(InputAction.CallbackContext context)
    // {
    //     if (Time.time >= NextFireTime)
    //     {
    //         NextFireTime = Time.time + FireRate;
    //         Shoot();
    //     }
    // }
    //
    // private void Shoot()
    // {
    //     // 1. 사운드 재생 [Source 522, 583]
    //     if (ShootSound != null) audioSource.PlayOneShot(ShootSound);
    //
    //     // 2. 레이캐스트 발사 [Source 505, 532-533]
    //     Vector3 rayOrigin = PlayerCamera.position;
    //     if (Physics.Raycast(rayOrigin, PlayerCamera.forward, out RaycastHit hit, WeaponRange))
    //     {
    //         Debug.Log("Hit: " + hit.collider.name); // Source [8]
    //
    //         // 적에게 데미지 전달 [Source 549]
    //         // hit.collider.GetComponent<ShootableBox>()?.Damage(GunDamage);
    //
    //         // 물리적인 힘 가하기 [Source 550]
    //         hit.rigidbody?.AddForce(-hit.normal * HitForce);
    //     }
    //
    //     // 3. 총기 반동 및 카메라 흔들림 [Source 576, 579]
    //     ApplyRecoil();
    //     StartCoroutine(CameraShake(0.1f, 0.05f));
    // }
    //
    // private void ApplyRecoil() // 발사 시 카메라가 위로 튀는 효과 [Source 576]
    // {
    //     XRotation -= ShootRecoil * 0.1f;
    //     PlayerCamera.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
    // }

    private IEnumerator CameraShake(float duration, float intensity) // 화면 흔들림 [Source 579]
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

    // --- 유틸리티 및 애니메이션 ---
    private void UpdateAnimation() // 이전 대화에서 설정한 isWalking 제어
    {
        var moveInput = MoveAction.action.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;
        if (Animator != null) Animator.SetBool("isWalking", isMoving);
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