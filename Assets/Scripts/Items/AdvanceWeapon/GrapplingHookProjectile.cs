using System.Collections;
using UnityEngine;

public class GrapplingHookProjectile : MonoBehaviour
{
    private GameObject player;
    private Vector3 direction;
    private float hookSpeed;
    private float pullSpeed;
    private float maxDistance;

    private bool hitSomething = false;
    private Vector3 hitPoint;

    private LineRenderer line;

    public void Init(GameObject player, Vector3 dir, float hookSpeed, float pullSpeed, float maxDistance)
    {
        this.player = player;
        this.direction = dir;
        this.hookSpeed = hookSpeed;
        this.pullSpeed = pullSpeed;
        this.maxDistance = maxDistance;

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    void Update()
    {
        if (player == null) { Destroy(gameObject); return; }

        // update line
        line.SetPosition(0, player.transform.position);
        line.SetPosition(1, transform.position);

        if (!hitSomething)
        {
            // bay về phía trước
            transform.position += direction * hookSpeed * Time.deltaTime;

            // check max distance
            if (Vector3.Distance(transform.position, player.transform.position) > maxDistance)
            {
                StartCoroutine(ReturnToPlayer());
            }
        }
        else
        {
            // kéo player về
            player.transform.position = Vector3.MoveTowards(
                player.transform.position, hitPoint, pullSpeed * Time.deltaTime);

            // nếu player đến nơi thì destroy
            if (Vector3.Distance(player.transform.position, hitPoint) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.gameObject == player) return;

        if (colision.CompareTag("MapObject"))
        {
            // bắn trúng 1 thứ -> hook dính lại
            hitSomething = true;
            hitPoint = transform.position;
        }
    }

    private IEnumerator ReturnToPlayer()
    {
        Vector3 start = transform.position;
        while (Vector3.Distance(transform.position, player.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, hookSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
