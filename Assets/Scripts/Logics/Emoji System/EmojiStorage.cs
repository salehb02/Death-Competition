using UnityEngine;

namespace DeathMatch
{
    [CreateAssetMenu(menuName = "Death Competition/New Emoji Storage")]
    public class EmojiStorage : ScriptableObject
    {
        public Emoji[] emojis;
    }
}