using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercent = 0.25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.HealPercent(healPercent);
        Destroy(gameObject);
    }
}

