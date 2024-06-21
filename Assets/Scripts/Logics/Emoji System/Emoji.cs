using UnityEngine;

namespace DeathMatch
{
    public class Emoji : MonoBehaviour
    {
        public enum EmojiType { OnScoreUp, OnScoreDown, OnDie, OnAttack }
        public EmojiType emojiType;

        [SerializeField] private float destroyTime = 2f;

        private void Start() => Destroy(gameObject, destroyTime);
    }
}