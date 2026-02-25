using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public ItemData item;             // 아이템 정보 (Scriptable Object)
    public UIInventory inventory;    // 인벤토리 메인 스크립트 참조
    public Image icon;               // 아이템 아이콘 이미지 [1]
    public TextMeshProUGUI quantityText; // 아이템 개수 텍스트 [1]
    private Outline outline;         // 선택 시 강조할 테두리 [1]
    public int index;                // 슬롯의 고유 번호 [1]
    public bool equipped;            // 장착 여부 [1]
    public int quantity;             // 아이템 개수 [1]
    
    private void Awake()
    {
        outline = GetComponent<Outline>(); // 테두리 컴포넌트 초기화 [1]
    }
    
    // 슬롯에 아이템 데이터를 채우는 함수 [1]
    public void Set()
    {
        icon.gameObject.SetActive(true);
        icon.sprite = item.icon;
        // 개수가 1개보다 많을 때만 숫자 표시 [1]
        quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        if (outline != null) outline.enabled = equipped; 
    }

    // 슬롯을 비우는 함수 [1]
    public void Clear()
    {
        item = null;
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;
    }
    
    // 슬롯 버튼을 눌렀을 때 실행 [1]
    public void OnClickButton()
    {
        inventory.SelectItem(index); // 인벤토리에 해당 아이템 선택 알림 [1]
    }
}