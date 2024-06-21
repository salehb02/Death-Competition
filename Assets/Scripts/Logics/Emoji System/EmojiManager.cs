using System.Collections;
using System.Linq;
using UnityEngine;

namespace DeathMatch
{
    public class EmojiManager : MonoBehaviour
    {
        [SerializeField] private EmojiStorage emojiStorage;
        [SerializeField] private Vector2 emojiOffset;
        [SerializeField] private float delay = 0.5f;

        private GameObject objectsSpawnPoint;

        private void Start() => InitializeEmojisHolder();

        private void InitializeEmojisHolder()
        {
            var emojisHolder = new GameObject("Emojis");
            emojisHolder.transform.parent = FindObjectOfType<Canvas>().transform;
            emojisHolder.AddComponent<RectTransform>();
            emojisHolder.transform.localScale = Vector3.one;

            var rectTranstorm = emojisHolder.GetComponent<RectTransform>();
            rectTranstorm.anchorMin = Vector2.zero;
            rectTranstorm.anchorMax = Vector2.one;
            rectTranstorm.anchoredPosition = Vector2.zero;
            rectTranstorm.sizeDelta = Vector2.zero;

            objectsSpawnPoint = emojisHolder;
            emojisHolder.transform.SetAsFirstSibling();
        }

        public void ShowEmoji(Transform character, Emoji.EmojiType emojiType)
        {
            var emojisToShow = emojiStorage.emojis.Where(x => x.emojiType == emojiType).ToList();
            StartCoroutine(ShowEmojiCoroutine(character, emojiType));
        }

        private IEnumerator ShowEmojiCoroutine(Transform character, Emoji.EmojiType emojiType)
        {
            yield return new WaitForSeconds(delay);

            if (!objectsSpawnPoint)
                yield break;

            var emojisToShow = emojiStorage.emojis.Where(x => x.emojiType == emojiType).ToList();
            Instantiate(emojisToShow[Random.Range(0, emojisToShow.Count)], Camera.main.WorldToScreenPoint(character.position + new Vector3(emojiOffset.x, emojiOffset.y, 0)), Quaternion.identity, objectsSpawnPoint.transform);
        }
    }
}