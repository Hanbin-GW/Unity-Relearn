using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;          // 전체 슬롯 배열 [2]
    public GameObject inventoryWindow; // 인벤토리 판넬 오브젝트 [2]
    public Transform slotPanel;       // 슬롯들이 배치된 부모 객체 [2]

    [Header("Selected Item Info")]    // 선택된 아이템 정보창 UI [2]
    private ItemSlot selectedItem;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;
    public GameObject equipButton;
    public GameObject useButton;
    public GameObject dropButton;
    public GameObject unEquipButton;
    
    private int selectedItemIndex;

    void Start()
    {
        inventoryWindow.SetActive(false); // 시작 시 인벤토리 닫기 [4]
        
        // 슬롯 자동 초기화: slotPanel의 자식 개수만큼 배열 생성 [4]
        slots = new ItemSlot[slotPanel.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
        }
        ClearSelectedItemWindow(); // 우측 정보창 비우기 [4]
    }
    // 탭 키 입력을 통한 토글 기능 [4]
    public void Toggle()
    {
        inventoryWindow.SetActive(!inventoryWindow.activeInHierarchy);
        
        // 마우스 커서 상태 제어 (FPS 컨트롤러와 연동 권장) [5, 6]
        if (inventoryWindow.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // 아이템 습득 시 호출되는 핵심 함수 [4, 7]
    public void AddItem(ItemData data)
    {
        // 1. 중복 가능한 아이템인지 확인하고 기존 슬롯에 합침 [7]
        if (data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                return;
            }
        }

        // 2. 비어있는 슬롯을 찾아 새로 추가 [7]
        ItemSlot emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }
    }

    // 모든 슬롯의 그래픽을 새로고침 [7]
    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null) slots[i].Set();
            else slots[i].Clear();
        }
    }
    
    ItemSlot GetItemStack(ItemData data)
    {
        for (int i = 0; i < slots.Length; i++) // 슬롯 배열 순회
        {   // 데이터와 슬롯의 아이템이 같다면 그리고 그 갯수가 최대값이 아니면
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i]; // 슬롯을 반환
            }
        }
        return null; // 아니면 스킵, 여기서 스킵일 경우 새롭게 슬롯을 만드는 방향으로 진행 될 것!
    }
    
    // 빈 슬롯 찾기 도우미 함수 [8]
    ItemSlot GetEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.item == null) return slot;
        }
        return null;
    }
    
    public void SelectItem(int index) // 슬롯 번호(index)를 받아옴 [1]
    {
        if (slots[index].item == null) return; // 아이템이 없는 빈 슬롯이면 무시 [1]

        selectedItem = slots[index]; // 선택된 아이템 슬롯 정보 저장 [1]
        selectedItemIndex = index;   // 선택된 아이템의 인덱스 저장 [1]

        // UI 텍스트 업데이트: 이름과 설명 [1]
        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;

        // 스탯 정보(체력 회복량 등) 초기화 후 출력 [1, 2]
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        for (int i = 0; i < selectedItem.item.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.item.consumables[i].type.ToString() + "\n";
            selectedItemStatValue.text += selectedItem.item.consumables[i].value.ToString() + "\n";
        }

        // 아이템 종류에 따라 버튼 활성화/비활성화 [2]
        // 소모품일 때만 '사용' 버튼 활성화
        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
    
        // 장착 가능 아이템이고 아직 장착 안 했을 때 '장착' 버튼 활성화
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !slots[index].equipped);
    
        // 이미 장착 중일 때만 '장착 해제' 버튼 활성화
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && slots[index].equipped);
    
        // '버리기' 버튼은 항상 켜둠 [2, 3]
        dropButton.SetActive(true);
    }
    // 정보창 비우기 [9]
    void ClearSelectedItemWindow()
    {
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        useButton.SetActive(false);
        dropButton.SetActive(false);
    }
}