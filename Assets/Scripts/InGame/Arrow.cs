using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage;
    public float speed = 8f;
    private GameObject target;

    public void Init(GameObject target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }
    public int count = 0;
    void Update()
    {
        count++;
        if (count > 300) // 超過300幀後自動銷毀
        {
            Destroy(gameObject);
            return;
        }
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        // 只考慮2D平面(x, y)，忽略z軸
        Vector3 targetPos2D = target.transform.position;
        targetPos2D.y += 1.5f;
        targetPos2D.z = transform.position.z;
        Vector3 dir = (targetPos2D - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        transform.position += dir * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, targetPos2D) < 0.3f)
        {
            target.SendMessage("takeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
