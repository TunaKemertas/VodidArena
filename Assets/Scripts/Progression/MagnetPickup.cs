using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        XPManager xp = other.GetComponent<XPManager>();
        if (xp == null) return;

        Transform player = other.transform;
        XpGem[] gems = FindObjectsByType<XpGem>(FindObjectsSortMode.None);
        for (int i = 0; i < gems.Length; i++)
        {
            if (gems[i] == null) continue;
            gems[i].AttractTo(player, xp);
        }

        AudioManager.Instance?.PlayCollect();
        Destroy(gameObject);
    }
}
