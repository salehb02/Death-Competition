using UnityEngine;

public class FootBlendShapeSetter : MonoBehaviour
{
    public int blendShapeValue;
    private SkinBlendShapeController controller;

    private void OnEnable()
    {
        controller = GetComponentInParent<SkinBlendShapeController>();
        controller.SetBlendShape(blendShapeValue);
    }
}