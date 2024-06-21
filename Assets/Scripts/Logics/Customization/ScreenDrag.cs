using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenDrag : MonoBehaviour, IDragHandler
{
    [SerializeField] private float Sensitivity;
    [SerializeField] private float MaxSpeed;
    [SerializeField] private float BrakeSpeed;
    [SerializeField] private GameObject[] Targets;

    private float _currentSpeed;

    public void OnDrag(PointerEventData eventData)
    {
        _currentSpeed += eventData.delta.x * -Sensitivity * Time.deltaTime;
    }

    private void Update()
    {
        _currentSpeed = Mathf.Lerp(_currentSpeed, 0, Time.deltaTime * BrakeSpeed);
        _currentSpeed = Mathf.Clamp(_currentSpeed, -MaxSpeed, MaxSpeed);

        foreach (var target in Targets)
        {
            target.transform.Rotate(Vector3.up * _currentSpeed, Space.Self);
        }
    }

    public void ResetSpeed()
    {
        _currentSpeed = 0;
    }
}