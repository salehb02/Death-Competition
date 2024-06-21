using UnityEngine;

public class StickToGround : MonoBehaviour
{
    public LayerMask GroundLayerMask;

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position + Vector3.up, -transform.up, out var hit, Mathf.Infinity, GroundLayerMask))
        {
            transform.position = hit.point + Vector3.up * 0.05f;
        }
    }
}