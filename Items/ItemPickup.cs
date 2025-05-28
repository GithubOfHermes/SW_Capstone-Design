using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum PickupType { Heart, Coin }

    [Header("기본 설정")]
    public PickupType pickupType = PickupType.Heart;
    public int healAmount = 1;
    public int goldAmount = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupType == PickupType.Coin)
            {
                PickupEffectHandler.Apply(pickupType, healAmount, goldAmount);
                Destroy(transform.root.gameObject); // Coin 게임 오브젝트 파괴
            }
            else
            {
                PickupController.Register(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickupController.Unregister(this);
        }
    }

    private void OnDisable()
    {
        PickupController.Unregister(this);
    }
}

