using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;          
    public GameObject inventoryWindow; 
    public Transform slotPanel;

    [Header("Selected Item Info")]    
    private ItemSlot selectedItem;
    // --- 필요 없는 텍스트 변수들을 완전히 제거했습니다 ---
    public TextMeshProUGUI selectedItemName;        // 아이템 이름 텍스트
    public TextMeshProUGUI selectedItemDescription; // 아이템 설명 텍스트
    public TextMeshProUGUI selectedItemStatName;    // 스탯 종류 이름 (예: 체력)
    public TextMeshProUGUI selectedItemStatValue;   // 스탯 수치 (예: 20)
    public GameObject equipButton;
    public GameObject useButton;
    public GameObject dropButton;
    public GameObject unEquipButton;
    [field: SerializeField] public InputActionReference ToggleAction { get; set; } = null!;
    private int selectedItemIndex;
    
    private void OnEnable()
    {
        // 액션 활성화
        ToggleAction.action.Enable();
        // Tab 키 등이 눌렸을 때 Toggle 함수가 실행되도록 연결
        ToggleAction.action.performed += OnToggleInput;
    }

    private void OnDisable()
    {
        // 이벤트 연결 해제 및 액션 비활성화
        ToggleAction.action.performed -= OnToggleInput;
        ToggleAction.action.Disable();
    }

    // 입력이 들어오면 실행되는 중간 함수
    private void OnToggleInput(InputAction.CallbackContext context)
    {
        Toggle(); // 기존에 만들어둔 Toggle 함수 호출
    }

    void Start()
    {
        // 인스펙터 연결 확인용 방어 코드
        if (inventoryWindow == null || slotPanel == null)
        {
            Debug.LogError("Inventory Window 또는 Slot Panel이 연결되지 않았습니다!");
            return;
        }

        // inventoryWindow.SetActive(false); 
        
        slots = new ItemSlot[slotPanel.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            if (slots[i] != null) // ItemSlot 스크립트가 있을 때만 실행
            {
                slots[i].index = i;
                slots[i].inventory = this;
            }
        }
        ClearSelectedItemWindow(); 
    }

    public void Toggle()
    {
        // 1. 인벤토리 창만 토글합니다.
        bool isOpen = !inventoryWindow.activeSelf;
        inventoryWindow.SetActive(isOpen);
        

        // 2. 창을 닫을 때 버튼들을 초기화하거나 정보창을 비웁니다.
        if (!isOpen)
        {
            ClearSelectedItemWindow(); // 소스 [2] 참고: 선택창 비우기 함수 호출
        }

        // 플레이어 제어 로직
        PlayerMovement player = GameObject.FindWithTag("Player")?.GetComponent<PlayerMovement>();

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (player != null) player.canLook = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (player != null) player.canLook = true;
        }
    }

    
    public void AddItem(ItemData data)
    {
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

        ItemSlot emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                if (slots[i].item != null) slots[i].Set();
                else slots[i].Clear();
            }
        }
    }
    
    ItemSlot GetItemStack(ItemData data)
    {
        for (int i = 0; i < slots.Length; i++) 
        {
            if (slots[i] != null && slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }
    
    ItemSlot GetEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.item == null) return slot;
        }
        return null;
    }
    
    public void SelectItem(int index)
    {
        // 1. 해당 슬롯에 아이템이 없으면 아무것도 하지 않음
        if (slots[index].item == null) return;

        selectedItemIndex = index;
        ItemData data = slots[index].item;

        // 2. 이름 및 설명 텍스트 업데이트 [1]
        selectedItemName.text = data.displayName;
        selectedItemDescription.text = data.description;

        // 3. 아이템 능력치(Stat) 정보 표시 [2]
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;
        for (int i = 0; i < data.consumables.Length; i++)
        {
            selectedItemStatName.text += data.consumables[i].type.ToString() + "\n";
            selectedItemStatValue.text += data.consumables[i].value.ToString() + "\n";
        }

        // 4. 아이템 타입에 따른 버튼 활성화 설정 [2]
        // 소모품일 때만 사용 버튼 표시
        useButton.SetActive(data.type == ItemType.Consumable);
    
        // 장비 아이템인 경우 장착 여부에 따라 장착/해제 버튼 교체 표시
        bool isEquipable = data.type == ItemType.Equipable;
        equipButton.SetActive(isEquipable && !slots[index].equipped);
        unEquipButton.SetActive(isEquipable && slots[index].equipped);
    
        // 버리기 버튼은 아이템이 있다면 항상 표시
        dropButton.SetActive(true);
    }

    public void ClearSelectedItemWindow()
    {
        useButton.SetActive(false);
        dropButton.SetActive(false);
        if (equipButton != null) equipButton.SetActive(false);
        if (unEquipButton != null) unEquipButton.SetActive(false);
    }
}