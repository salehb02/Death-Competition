using UnityEngine;
using TMPro;

namespace Gameplay.Core
{
	public class FPSCounter : MonoBehaviour
	{
		private float deltaTime;
		public TextMeshProUGUI text;


		private void Start()
		{
			text.enabled = GameManager.Instance.ShowFPS;
		}

		private void Update()
		{
			CalculateFPS();
		}

		private void CalculateFPS()
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float fps = 1f / deltaTime;
			text.text = ((int)fps).ToString() + " FPS" + "\n" + Screen.currentResolution;
		}
	}
}