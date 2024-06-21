using UnityEngine;

public class CloudMovment : MonoBehaviour
{
    private void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime);
        CheckPosition();
    }

    private void CheckPosition()
    {
        if (transform.position.x > 25)
            transform.position = new Vector3(-20, transform.position.y, transform.position.z);
    }
}