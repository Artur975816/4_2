using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    private Vector3 target;

    private void Start()
    {
        target = pointB != null ? pointB.position : transform.position + Vector3.right * 2f;
    }

    private void Update()
    {
        if (pointA == null || pointB == null)
        {
            transform.position = new Vector3(
                transform.position.x + Mathf.Sin(Time.time * speed) * Time.deltaTime,
                transform.position.y,
                transform.position.z);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.05f)
            target = target == pointA.position ? pointB.position : pointA.position;
    }
}
