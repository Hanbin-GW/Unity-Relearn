using UnityEngine;

// 아이템의 타입을 정의하는 열거형 [3]
public enum ItemType {
    Resource,
    Equipable,
    Consumable
}

// 소모품의 효과 타입을 정의 [4]
public enum ConsumableType {
    Health,
}

// 소모품의 상세 효과를 담는 클래스 [3]
[System.Serializable]
public class ItemDataConsumable {
    public ConsumableType type;
    public float value;
}

[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject {
    [Header("Info")]
    public string displayName;      // 아이템의 표시 이름 [6]
    public string description;      // 아이템의 상세 설명 [6]
    public ItemType type;           // 아이템 종류 (자원, 장비, 소모품) [3]
    public Sprite icon;             // 인벤토리 슬롯에 표시될 아이콘 [1]
    public GameObject dropPrefab;   // 아이템을 버렸을 때 필드에 생성될 오브젝트 [2]
    
    [Header("Stacking")]
    public bool canStack;           // 중복 획득 시 겹치기 가능 여부 [2]
    public int maxStackAmount;      // 최대 중복 가능 개수 [6]

    [Header("Consumable")]
    public ItemDataConsumable[] consumables; // 소모품일 경우의 효과 정보 (체력 회복 등) [3]
}