using UnityEngine;

namespace DeathMatch
{
    public class CustomizationItem : MonoBehaviour
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private Gender gender;

        public Sprite Icon { get => icon; }
        public Gender Gender { get => gender; }
    }
}