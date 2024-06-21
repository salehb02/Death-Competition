using UnityEngine;

public class BlockCustomizableItem : MonoBehaviour
{
    [Header("Main Config")]
    [SerializeField] private Sprite icon;

    public Sprite GetIcon()
    {
        return icon;
    }
}