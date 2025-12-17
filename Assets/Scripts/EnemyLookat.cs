using UnityEngine;

public class EnemyLookAt : MonoBehaviour
{
    [SerializeField] private Transform target;       // Camera hoặc Gun
    [SerializeField] private float turnSpeed = 360f; // độ/giây

    void LateUpdate(){
        if (!target) return;
        Vector3 dir = target.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion to = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, to, turnSpeed * Time.deltaTime);
    }
}
