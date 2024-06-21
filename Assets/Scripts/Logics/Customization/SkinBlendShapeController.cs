using UnityEngine;

public class SkinBlendShapeController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;
    private int value;

    public void SetBlendShape(int value)
    {
        this.value = value;
    }

    private void LateUpdate()
    {
        if (value == -1)
            skinnedMesh.SetBlendShapeWeight(0, 0);
        else
            skinnedMesh.SetBlendShapeWeight(value, 100);
    }
}