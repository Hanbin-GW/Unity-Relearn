using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemData item;               // 아이템 정보 (Scriptable Object)
    public UIInventory inventory;       // 인벤토리 메인 스크립트 참조
    public Image icon;                  // 아이템 아이콘 이미지 [1]
    public TextMeshProUGUI quantityText;// 아이템 개수 텍스트 [1]
    private Outline outline;            // 선택 시 강조할 테두리 [1]
    public int index;                   // 슬롯의 고유 번호 [1]
    public bool equipped;               // 장착 여부 [1]
    public int quantity;                // 아이템 개수 [1]
    
    private void Awake()
    {
        outline = GetComponent<Outline>(); // 테두리 컴포넌트 초기화 [1]
    }
    
    public void Set()
    {
        if (icon == null) icon = GetComponentInChildren<UnityEngine.UI.Image>();

        if (item != null && icon != null)
        {
            icon.sprite = item.icon; 
            icon.gameObject.SetActive(true);
        }
    }

    // 슬롯을 비우는 함수 [1]
    public void Clear()
    {
        item = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.gameObject.SetActive(false); // 아이템이 없으면 아이콘 숨기기
        }
        if (quantityText != null) quantityText.text = string.Empty;
    }
    
    // 슬롯 버튼을 눌렀을 때 실행 [1]
    public void OnClickButton()
    {
        inventory.SelectItem(index); // 인벤토리에 해당 아이템 선택 알림 [1]
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventory.SelectItem(index);
    }
}