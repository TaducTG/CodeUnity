using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath), typeof(AIDestinationSetter))]
public class EnemyAIDistanceToggle : MonoBehaviour
{
    public Transform player;                 // Kéo thả hoặc tự tìm theo tag
    public float detectionRadius = 6f;       // vào phạm vi này thì bật
    public float loseRadius = 8f;            // ra phạm vi này thì tắt (nên > detectionRadius)
    public float checkInterval = 0.2f;       // kiểm tra mỗi 0.2s cho nhẹ máy

    AIPath aiPath;
    AIDestinationSetter dest;
    bool isChasing;
    float timer;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        dest = GetComponent<AIDestinationSetter>();
        ToggleChase(false);
    }

    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
    }

    void Update()
    {
        if (!player) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        float detectSqr = detectionRadius * detectionRadius;
        float loseSqr = loseRadius * loseRadius;

        if (!isChasing && sqrDist <= detectSqr) ToggleChase(true);
        else if (isChasing && sqrDist >= loseSqr) ToggleChase(false);
    }

    void ToggleChase(bool on)
    {
        isChasing = on;
        if (on)
        {
            dest.target = player;
            aiPath.enabled = true;
            aiPath.canMove = true;
        }
        else
        {
            aiPath.canMove = false;
            aiPath.enabled = false;
            dest.target = null;
        }
    }
}
