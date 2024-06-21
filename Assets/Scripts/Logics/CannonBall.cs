using System;
using System.Collections;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    private Action onCollision;

    public void Fire(Vector3 target, Action onCollision)
    {
        StartCoroutine(FireCoroutine(target, 40, 20));
        this.onCollision = onCollision;
    }

    private IEnumerator FireCoroutine(Vector3 targetPosition, float speed, float fireAngle)
    {
        var target_Distance = Vector3.Distance(transform.position, targetPosition);

        var projectile_Velocity = target_Distance / (Mathf.Sin(2 * fireAngle * Mathf.Deg2Rad) / speed);

        var Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(fireAngle * Mathf.Deg2Rad);
        var Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(fireAngle * Mathf.Deg2Rad);

        var flightDuration = target_Distance / Vx;

        transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
        var elapse_time = 0f;

        while (elapse_time < flightDuration)
        {
            transform.Translate(0, (Vy - (speed * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("Block"))
            return;

        onCollision?.Invoke();
        onCollision = null;
        Destroy(gameObject);
    }
}