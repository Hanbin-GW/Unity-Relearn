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
        inventoryWindow.SetActive(!inventoryWindow.activeInHierarchy);

        // 플레이어 오브젝트를 태그로 찾아 스크립트를 가져옵니다.
        PlayerMovement player = GameObject.FindWithTag("Player")?.GetComponent<PlayerMovement>();

        if (inventoryWindow.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (player != null) player.canLook = false; // 시선 회전 금지
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (player != null) player.canLook = true;  // 시선 회전 허용
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
        selectedItemIndex = index;

        // 해당 슬롯에 아이템이 있는지 확인
        if (slots[index].item != null)
        {
            // 아이템이 있다면 버튼들을 활성화
            useButton.SetActive(true);
            dropButton.SetActive(true);
        
            // 장착 가능한 아이템인지에 따라 장착 버튼 활성화 (선택 사항)
            if (equipButton != null) equipButton.SetActive(true);
        }
        else
        {
            // 아이템이 없는 빈 슬롯이라면 버튼들을 숨김
            ClearSelectedItemWindow();
        }
    }

    public void ClearSelectedItemWindow()
    {
        useButton.SetActive(false);
        dropButton.SetActive(false);
        if (equipButton != null) equipButton.SetActive(false);
        if (unEquipButton != null) unEquipButton.SetActive(false);
    }
}