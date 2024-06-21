using UnityEngine;

public class FallingCoin : MonoBehaviour
{
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float destroyAfter = 3f;

    private float currentSpeed;
    private float currentMaxSpeed;

    private void Start()
    {
        currentMaxSpeed = Random.Range(minSpeed, maxSpeed);
        Destroy(gameObject, destroyAfter);
    }

    private void Update()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, currentMaxSpeed * 2f, Time.deltaTime);

        transform.Translate(0, -Time.deltaTime * currentSpeed, 0);
    }
}