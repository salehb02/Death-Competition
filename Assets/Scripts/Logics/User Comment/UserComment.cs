using DeathMatch;
using UnityEngine;

public class UserComment : MonoBehaviour
{
    private const string IS_RATING_SUBMITTED_PREFS = "IS_RATING_SUBMITTED";
    private const string INIT_SHOW_PREFS = "RATING_INIT_SHOW";
    public const string PLAYED_MATCHES_COUNT_PREFS = "PLAYED_MATCHES_COUNT";

    private bool isSendingRating;
    private UserCommentPresentor presentor;

    private void Start()
    {
        presentor = GetComponent<UserCommentPresentor>();

        if (SaveManager.HasKey(IS_RATING_SUBMITTED_PREFS))
            return;

        if (!SaveManager.Get<bool>(INIT_SHOW_PREFS) && SaveManager.Get<int>(PLAYED_MATCHES_COUNT_PREFS) >= 5)
        {
            presentor.OpenRatingPanel();
            SaveManager.Set(PLAYED_MATCHES_COUNT_PREFS, 0);
            SaveManager.Set(INIT_SHOW_PREFS, true);
        }
        else if (SaveManager.Get<bool>(INIT_SHOW_PREFS) && SaveManager.Get<int>(PLAYED_MATCHES_COUNT_PREFS) >= 1)
        {
            presentor.OpenRatingPanel();
            SaveManager.Set(PLAYED_MATCHES_COUNT_PREFS, 0);
        }
    }

    [System.Obsolete]
    public void SendRating(int rating, string comment)
    {
        if (rating >= 4)
            return;

        if (isSendingRating)
            return;

        isSendingRating = true;

        ServerConnection.Instance.UserComment(rating, comment, (success) =>
        {
            isSendingRating = false;

            if (!success)
                return;

            SaveManager.Set(IS_RATING_SUBMITTED_PREFS, true);

            if (string.IsNullOrEmpty(comment))
            {
                presentor.CloseRatingPanel();
                presentor.OpenAskRatingInMarket();
                Invoke(nameof(OpenMarketRating), 3f);
            }
            else
            {
                presentor.SetSuccessfulMessage();
            }
        });
    }
    
    private void OpenMarketRating()
    {
        UMM.MarketIntents.OpenComments();
        presentor.CloseAskRatingInMarket();
    }
}