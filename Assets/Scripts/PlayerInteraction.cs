using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3.0f; // 상호작용 거리 [2]
    public LayerMask itemLayer;           // 아이템 레이어 설정
    public UIInventory inventory;         // 인벤토리 참조 [3]

    void Update()
    {
        // [F] 키를 눌렀을 때 실행
        if (Input.GetKeyDown(KeyCode.F))
        {
            PerformInteraction();
        }
    }

    void PerformInteraction()
    {
        RaycastHit hit;
        // 카메라 중심에서 앞으로 레이를 쏨 [4]
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance, itemLayer))
        {
            // 닿은 물체에 아이템 데이터가 있는지 확인 [5]
            ItemObject itemObj = hit.collider.GetComponent<ItemObject>();
            if (itemObj != null)
            {
                // 인벤토리에 추가 시도 [6]
                inventory.AddItem(itemObj.itemData);
                Destroy(hit.collider.gameObject); // 필드 아이템 제거 [7]
                Debug.Log(itemObj.itemData.displayName + "을(를) 획득했습니다.");
            }
        }
    }
}