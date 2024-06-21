using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using GameAnalyticsSDK;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace DeathMatch
{
	public class PlayerCustomization : MonoBehaviour
	{
		[Header("Parts")]
		public GameObject avatarBase;
		public CustomizationItem[] styles;

		public bool IsPlayer { get; set; }
		public Gender Gender { get; set; }

		public List<SSDM.Set> serverSets = new List<SSDM.Set>();

		private Character character;

		public event Action OnLoadComplete;
		public event Action OnPurchaseCustomization;

		[Serializable]
		public struct UIInfo
		{
			public string Code;
			public Sprite Icon;
			public SSDM.Set Style;

			public UIInfo(string code, Sprite icon, SSDM.Set style)
			{
				Code = code;
				Icon = icon;
				Style = style;
			}
		}

		public void StartCustomization()
		{
			avatarBase.SetActive(false);
			character = GetComponent<Character>();

			UpdateCustomization();
		}

		public void UpdateCustomization()
		{
			if (!IsPlayer)
				return;

			LoadGender(LoadAvatar);
		}

		[Obsolete]
		private void LoadGender(Action callback)
		{
			// F for female
			// M for male
			// B for both

			if (GameManager.Instance.LatestPlayerInfo != null)
				OnGetData(GameManager.Instance.LatestPlayerInfo);
			else
				ServerConnection.Instance.GetPlayerInfo(OnGetData);

			void OnGetData(USDM.UserInfo info)
			{
				switch (info.data.user.sex.ToLower())
				{
					case "f":
						Gender = Gender.Female;
						break;
					case "m":
						Gender = Gender.Male;
						break;
					default:
						break;
				}

				callback?.Invoke();
			}
		}

		public void LoadAIStyle(List<GSDM.UserSet> sets)
		{
			DisableAllParts();

			foreach (var item in sets)
				styles.SingleOrDefault(x => x.name == item.code).gameObject.SetActive(true);

			EnableAvatar();
		}

		public void LoadRandomStyle()
		{
			DisableAllParts();

			styles[Random.Range(0, styles.Length)].gameObject.SetActive(true);

			EnableAvatar();
		}

		public void PreviewStyle(string setCode, bool invokeAction = true)
		{
			HideAvatar();
			DisableAllParts();

			var currentStyle = styles.SingleOrDefault(x => x.name == setCode);

			currentStyle.gameObject.SetActive(true);

			if (invokeAction)
				OnPurchaseCustomization?.Invoke();

			character.SetAnimationMode("Look_Cloth");

			EnableAvatar();
		}

		private void DisableAllParts()
		{
			foreach (var part in styles)
				part.gameObject.SetActive(false);

			LoadingCharacterIndicator.Instance.ShowLoadingIndicator(avatarBase.transform);
		}

		private void EnableAvatar()
		{
			avatarBase.SetActive(true);
			LoadingCharacterIndicator.Instance.HideLoadingIndicator();
		}

		public Sprite GetPartIcon(string setCode) => styles.SingleOrDefault(x => x.name == setCode).Icon;

		public List<UIInfo> GetUIInfos()
		{
			var items = new List<UIInfo>();

			foreach (var style in serverSets)
			{
				if (!IsServerSetsContains(style.code))
					continue;

				var icon = GetPartIcon(style.code);
				var info = new UIInfo(style.code, icon, style);
				items.Add(info);
			}

			return items;
		}

		[Obsolete]
		public void PurchaseStyle(string setCode, bool invokeAction = true)
		{
			ServerConnection.Instance.PurchaseSet(setCode, (data) =>
			{
				if (!data.success)
					return;

				SelectStyle(setCode);

				if (invokeAction)
					OnPurchaseCustomization?.Invoke();
			});
		}

		[Obsolete]
		public void WatchAdForSet(string setCode, bool invokeAction = true)
		{
			ServerConnection.Instance.WatchAdForSet(setCode, (data) =>
			{
				if (!data.success)
					return;

				UpdateCustomization();

				if (invokeAction)
					OnPurchaseCustomization?.Invoke();
			});
		}

		public void SelectStyle(string setCode)
		{
			ServerConnection.Instance.UpdatePlayerSet(setCode, OnUpdatePlayerStyle);

            // GameAnalytics events
            GameAnalytics.NewDesignEvent($"SelectSet:{setCode}");
        }

        private void OnUpdatePlayerStyle(List<SSDM.Set> sets)
		{
			LoadAvatar();

			if (GameManager.Instance.PrintLogs)
				Debug.Log("Player style updated successfuly!");
		}

		public bool IsStylePurchased(string setCode)
		{
			if (!IsServerSetsContains(setCode))
				return false;

			return serverSets.SingleOrDefault(x => x.code == setCode).bought;
		}

		public PurchaseMethod GetStylePurchaseMethod(string setCode)
		{
			if (!IsServerSetsContains(setCode))
				throw new NullReferenceException();

			var set = serverSets.SingleOrDefault(x => x.code == setCode);

			if (set.adPurchase > 0)
			{
				return new PurchaseByAd(setCode, set.adWatched, set.adPurchase, () => WatchAdForSet(setCode));
			}
			else if (set.virtualPayment.active)
			{
				return new PurchaseByCoin(setCode, set.virtualPayment.price, () => PurchaseStyle(setCode));
			}

			throw new Exception("No purchase method available");
		}

		public bool IsStyleLocked(string setCode, out int neededLevel)
		{
			if (!IsServerSetsContains(setCode))
                throw new NullReferenceException("Avatar style not exist");

            var currentServerStyle = serverSets.SingleOrDefault(x => x.code == setCode);

			neededLevel = currentServerStyle.level;
			return currentServerStyle.locked;
		}

		public bool IsStyleSelected(string setCode)
		{
			if (!IsServerSetsContains(setCode))
				return false;

			return serverSets.SingleOrDefault(x => x.code == setCode).chosen;
		}

		private bool IsServerSetsContains(string setCode)
		{
			if (!styles.Contains(styles.SingleOrDefault(x => x.name == setCode)))
				return false;

			return serverSets.Contains(serverSets.SingleOrDefault(x => x.code == setCode));
		}

		public void ChangeGender()
		{
			if (Gender == Gender.Male)
			{
				Gender = Gender.Female;
			}
			else if (Gender == Gender.Female)
			{
				Gender = Gender.Male;
			}

			ServerConnection.Instance.UpdateUserData(UpdateCustomization, Gender);

			// GameAnalytics events
			GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", 0, "ChangeGender", $"ChangeGender_{Gender}");
		}

		public void HideAvatar()
		{
			avatarBase.SetActive(false);
		}

		[Obsolete]
		private void LoadAvatar()
		{
			avatarBase.SetActive(false);

			LoadingCharacterIndicator.Instance.ShowLoadingIndicator(avatarBase.transform);

			if (SceneManager.GetActiveScene().name == GameManager.Instance.customizationScene)
			{
				ServerConnection.Instance.GetAllSets(OnLoadStyles);
			}
			else
			{
				var userStyle = GameManager.Instance.LatestPlayerInfo.data.userStyle;

				DisableAllParts();

				var selectedStyle = styles.SingleOrDefault(x => x.name == userStyle.code);

				if (selectedStyle == null)
					return;

				selectedStyle.gameObject.SetActive(true);

				EnableAvatar();
			}

			void OnLoadStyles(List<SSDM.Set> styles)
			{
				serverSets = styles.Where(x => x.category == "set").ToList();

                ApplyStyles(serverSets);
				OnLoadComplete?.Invoke();
			}
		}

		public void ApplyStyles(List<SSDM.Set> allStyles)
		{
			DisableAllParts();

			foreach (var style in allStyles)
			{
				if (!IsStylePurchased(style.code))
					continue;

				// Load styles
				var selectedStyle = styles.SingleOrDefault(x => x.name == style.code);

				if (selectedStyle == null)
					continue;

				selectedStyle.gameObject.SetActive(IsStyleSelected(style.code));
			}

			EnableAvatar();
		}

		public void ResetPreview()
		{
			ApplyStyles(serverSets);
        }

		public CustomizationItem GetSelectedStyle()
		{
			foreach (var style in styles)
			{
				if (style.gameObject.activeSelf)
					return style;
			}

			return null;
		}
	}
}