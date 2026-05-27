using UnityEngine;
using DG.Tweening;

public class XpGem : MonoBehaviour
{
    public int xpAmount = 5;
    public float magnetSpeed = 16f;
    public float magnetCollectDistance = 0.35f;

    private bool _collected;
    private bool _magneting;
    private Transform _magnetTarget;
    private XPManager _magnetXp;

    private void Update()
    {
        if (!_magneting || _collected) return;
        if (_magnetTarget == null)
        {
            _magneting = false;
            return;
        }

        if (GameManager.Instance != null &&
            (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        Vector3 target = _magnetTarget.position;
        transform.position = Vector3.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= magnetCollectDistance)
            CollectTo(_magnetXp);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected || _magneting) return;

        XPManager xp = other.GetComponent<XPManager>();
        if (xp == null) return;

        CollectTo(xp);
    }

    /// <summary>Magnet pickup: gem flies to the player, then is collected on arrival.</summary>
    public void AttractTo(Transform player, XPManager xp)
    {
        if (_collected || _magneting) return;
        if (player == null || xp == null) return;

        _magneting = true;
        _magnetTarget = player;
        _magnetXp = xp;
    }

    public void CollectTo(XPManager xp)
    {
        if (_collected) return;
        if (xp == null) return;

        _collected = true;
        _magneting = false;
        xp.AddXP(xpAmount);
        AudioManager.Instance?.PlayCollect();

        // Small pop before disappearing.
        transform.DOScale(transform.localScale * 1.35f, 0.12f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => Destroy(gameObject));
    }
}
