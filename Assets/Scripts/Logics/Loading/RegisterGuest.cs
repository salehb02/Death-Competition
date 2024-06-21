using DeathMatch;
using UnityEngine;
using USDM;

public class RegisterGuest : MonoBehaviour
{
    public bool WaitingForGuestCreation { get; private set; } = true;

    public void Register()
    {
        if (SaveManager.HasKey(SaveManager.PLAYER_USERNAME))
        {
            WaitingForGuestCreation = false;
            return;
        }

        ServerConnection.Instance.CreateGuestUser(OnSuccess, OnFail);
    }

    private void OnSuccess(UserInfo info, string message)
    {
        SaveManager.Set(SaveManager.PLAYER_USERNAME, info.data.user.userName);
        SaveManager.Set(SaveManager.IS_GUEST, true);
        WaitingForGuestCreation = false;
    }

    private void OnFail(string message)
    {
        Debug.Log($"RegisterGuest::OnFail::{message}");
    }
}