using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    private Animator animator;
    private Rigidbody mainRigidbody;
    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;

    void Start()
    {
        animator = GetComponent<Animator>();
        mainRigidbody = GetComponent<Rigidbody>();
        
        // 자식 오브젝트들의 래그돌용 물리 컴포넌트들을 가져옴 [13]
        ragdollColliders = GetComponentsInChildren<Collider>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        // 시작할 때는 래그돌 기능을 꺼둠
        SetRagdoll(false);
    }

    public void SetRagdoll(bool state)
    {
        // 래그돌 활성화 시 애니메이터는 꺼야 함 [14]
        animator.enabled = !state;

        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state; // 물리 연산 활성화/비활성화 [15]
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = state; // 충돌체 활성화
        }
        
        // 메인 리지드바디는 반대로 설정하여 겹침 방지
        if (mainRigidbody != null) mainRigidbody.isKinematic = state;
    }

    // 체력이 0이 되었을 때 호출 예시 [16, 17]
    public void OnDie()
    {
        SetRagdoll(true);
        Debug.Log("플레이어가 쓰러졌습니다.");
    }
}