using UnityEngine;

public class XpGem : MonoBehaviour
{
    public int xpAmount = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        XPManager xp = other.GetComponent<XPManager>();
        if (xp == null) return;

        xp.AddXP(xpAmount);
        Destroy(gameObject);
    }
}

