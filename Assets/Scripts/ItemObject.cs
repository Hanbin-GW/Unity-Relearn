using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemData itemData;
    
    public string GetInteractText()
    {
        return $"{itemData.displayName}\n획득 [F]";
    }

    // 아이템을 획득했을 때의 처리를 담당합니다. [2, 3]
    public void OnPickup()
    {
        // 획득 시 사운드를 재생하거나 이펙트를 생성할 수도 있습니다.
        Destroy(gameObject);
    }
}