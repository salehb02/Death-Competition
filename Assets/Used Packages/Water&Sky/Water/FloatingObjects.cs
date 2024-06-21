using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObjects : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float multiplier;

    private Vector3 newPos;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        newPos = originalPos;
        newPos.y += Mathf.Sin(Mathf.PI * speed * Time.fixedTime) * multiplier;
        transform.position = newPos;
    }
}
