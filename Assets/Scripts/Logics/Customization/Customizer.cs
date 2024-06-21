using DG.Tweening;
using UnityEngine;

namespace DeathMatch
{
    public class Customizer : MonoBehaviour
    {
        [SerializeField] private PlayerCustomization avatar;
        [SerializeField] private BlockCustomization block;

        public PlayerCustomization Avatar { get => avatar; }
        public BlockCustomization Block { get => block; }

        private string currentAvatarSetCode;
        private string currentBlockSetCode;

        private CustomizationUI presentor;
        private ScreenDrag screenDrag;
        private bool isOnAvatar = true;

        private void Start()
        {
            presentor = FindObjectOfType<CustomizationUI>();
            screenDrag = FindObjectOfType<ScreenDrag>();

            block.StartCustomization();
        }

        public void PreviewAvatarSet(string setCode)
        {
            currentAvatarSetCode = setCode;

            if (Avatar.IsStylePurchased(setCode))
                SetUIOnPurchased(setCode);
            else
            {
                SetUIOnNotPurchased();

                if (Avatar.IsStyleLocked(setCode, out var neededLevel))
                    presentor.ShowLock(neededLevel);
                else
                    presentor.HideLock();
            }

            Avatar.PreviewStyle(setCode, false);
            AudioManager.Instance.ChangeClothSFX();

            void SetUIOnPurchased(string setCode)
            {
                presentor.SetSelectStyle(() => Avatar.SelectStyle(setCode));
            }

            void SetUIOnNotPurchased()
            {
                var purchaseMethod = Avatar.GetStylePurchaseMethod(currentAvatarSetCode);

                if (purchaseMethod is PurchaseByAd)
                {
                    var purchaseByAd = (PurchaseByAd)purchaseMethod;
                    presentor.SetPurchaseByAd(purchaseByAd.Watched, purchaseByAd.ToWatch, purchaseMethod.Purchase);
                }
                else if (purchaseMethod is PurchaseByCoin)
                {
                    var purchaseByCoin = (PurchaseByCoin)purchaseMethod;
                    presentor.SetPurchaseByCoin(purchaseByCoin.Price, purchaseMethod.Purchase);

                    purchaseByCoin.onPurchaseFailed += (message) =>
                    {
                        if (message == PurchaseByCoin.NOT_ENOUGH_COUNT)
                        {
                            presentor.OpenNotEnoughCoinPanel();
                        }
                    };
                }
            }
        }

        public void PreviewBlockSet(string setCode)
        {
            currentBlockSetCode = setCode;

            if (Block.IsStylePurchased(setCode))
                SetUIOnPurchased(setCode);
            else
                SetUIOnNotPurchased();

            if (Block.IsStyleLocked(setCode, out var neededLevel))
                presentor.ShowLock(neededLevel);
            else
                presentor.HideLock();

            Block.PreviewStyle(setCode, false);
            AudioManager.Instance.ChangeClothSFX();

            void SetUIOnPurchased(string setCode)
            {
                presentor.SetSelectStyle(() => Block.SelectStyle(setCode));
            }

            void SetUIOnNotPurchased()
            {
                var purchaseMethod = Block.GetStylePurchaseMethod(currentBlockSetCode);

                if (purchaseMethod is PurchaseByAd)
                {
                    var purchaseByAd = (PurchaseByAd)purchaseMethod;
                    presentor.SetPurchaseByAd(purchaseByAd.Watched, purchaseByAd.ToWatch, purchaseMethod.Purchase);
                }
                else if (purchaseMethod is PurchaseByCoin)
                {
                    var purchaseByCoin = (PurchaseByCoin)purchaseMethod;
                    presentor.SetPurchaseByCoin(purchaseByCoin.Price, purchaseMethod.Purchase);

                    purchaseByCoin.onPurchaseFailed += (message) =>
                    {
                        if (message == PurchaseByCoin.NOT_ENOUGH_COUNT)
                        {
                            presentor.OpenNotEnoughCoinPanel();
                        }
                    };
                }
            }
        }

        public void ChangeTabReset()
        {
            avatar.ResetPreview();
            block.ResetPreview();
            presentor.HideLock();
        }
    }
}