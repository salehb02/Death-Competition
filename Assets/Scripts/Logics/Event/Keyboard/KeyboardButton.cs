using UnityEngine;
using UnityEngine.UI;

namespace DeathMatch
{
    [RequireComponent(typeof(Button))]
    public class KeyboardButton : MonoBehaviour
    {
        [SerializeField] private string letter;
        [SerializeField] private bool isBackspace = false;
        
        private Keyboard _keyboard;
        private Button _button;

        public Button GetButton { get => _button ?? GetComponent<Button>(); }

        private void Start()
        {
            _keyboard = GetComponentInParent<Keyboard>();
            _button = GetComponent<Button>();
            var rtlText = GetComponentInChildren<RTLTMPro.RTLTextMeshPro>();

            if (rtlText)
                rtlText.text = letter;

            if (!isBackspace)
            {
                _button.onClick.AddListener(AudioManager.Instance.KeyboardSFX);
                _button.onClick.AddListener(() => _keyboard.AddLetter(letter));
            }
            else
            {
                _button.onClick.AddListener(AudioManager.Instance.DeleteKeySFX);
                _button.onClick.AddListener(_keyboard.Backspace);
            }
        }
    }
}