using ServerApp;
using System;
using System.Collections.Generic;
using UnityEngine;
using USDM;
using GSDM;
using System.Collections;
using SDM;
using MSDM;
using System.Linq;
using System.Text;
using UnityEditor;
using Newtonsoft.Json;
using UnityEngine.Rendering.Universal;
using ESDM;
using UnityEngine.Rendering;

namespace DeathMatch
{
    public class ServerConnection : MonoBehaviour
    {
        #region Singleton
        public static ServerConnection Instance;

        private void Awake()
        {
            transform.SetParent(null);

            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        private UserServer _userServer;
        private GameServer _gameServer;
        private StyleServer _styleServer;
        private ServerInitial _serverInitial;
        private MarketServer _marketServer;
        private LogServer _logServer;
        private EventServer _eventServer;

        private int reconnectAttempts = 0;

        private ServerConnectionPresentor _presentor;
        private bool _userInfoUpdateNeeded = false;
        public UserInfo CurrentUserInfo { get; private set; }

        public bool ShowJSONLogs = true;

        private Queue<Action> actionsAfterReconnect = new Queue<Action>();
        private Dictionary<string, WaitingItem> waitingForAnswer = new Dictionary<string, WaitingItem>();

        public const string LOADING_DATA_TEXT = "<sprite index=0>";

        [Serializable]
        public class WaitingItem
        {
            private DateTime initTime;
            private Action actionOnFail;
            private float timeout;

            public DateTime InitializeTime { get => initTime; }
            public Action ActionOnFail { get => actionOnFail; }
            public float TimeOut { get => timeout; }

            public WaitingItem(Action actionOnFail, float timeOut = 6f)
            {
                initTime = DateTime.Now;
                this.actionOnFail = actionOnFail;
                timeout = timeOut;
            }

            public void UpdateTime()
            {
                initTime = DateTime.Now;
            }
        }

        public string Username
        {
            get => SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);
        }

        private void Start()
        {
            _userServer = new UserServer();
            _gameServer = new GameServer();
            _styleServer = new StyleServer();
            _serverInitial = new ServerInitial();
            _marketServer = new MarketServer();
            _logServer = new LogServer();
            _eventServer = new EventServer();

            _presentor = GetComponent<ServerConnectionPresentor>();

            IsConnected();
            CheckServerStatus();
        }

        private void Update()
        {
            foreach (var action in waitingForAnswer.ToList())
            {
                if ((DateTime.Now - action.Value.InitializeTime).Seconds <= action.Value.TimeOut)
                    continue;

                action.Value.ActionOnFail?.Invoke();

                if (ShowJSONLogs)
                    Debug.Log(action.Key + " retried");

                action.Value.UpdateTime();
            }
        }

        private void OnApplicationQuit()
        {
            ExitUserFromGame();
        }


        private void AddToWaitingList(string key, WaitingItem waitingItem)
        {
            if (!waitingForAnswer.ContainsKey(key))
                waitingForAnswer.Add(key, waitingItem);
        }

        private void RemoveFromWaitingList(string key)
        {
            waitingForAnswer.Remove(key);
        }

        [Obsolete]
        public void CreateUser(string userName, Gender gender, Action<string, string> successCallback, Action<string> failCallback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => CreateUser(userName, gender, successCallback, failCallback));
                return;
            }

            var genderChar = gender switch
            {
                Gender.Male => "M",
                Gender.Female => "F",
                _ => throw new NotImplementedException(),
            };

            StartCoroutine(_userServer.createUser(userName, genderChar, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (userInfo.success)
                    {
                        successCallback?.Invoke(userName, userInfo.message);
                    }
                    else
                    {
                        failCallback?.Invoke(userInfo.message);
                        Debug.Log($"ServerConnection::CreateUser:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::CreateUser::JSON:: {json}");

                    RemoveFromWaitingList(nameof(CreateUser));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(CreateUser), new WaitingItem(() => CreateUser(userName, gender, successCallback, failCallback)));
        }

        [Obsolete]
        public void CreateGuestUser(Action<UserInfo, string> successCallback, Action<string> failCallback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => CreateGuestUser(successCallback, failCallback));
                return;
            }

            StartCoroutine(_userServer.createGuestUser((string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (userInfo.success)
                    {
                        successCallback?.Invoke(userInfo, userInfo.message);
                    }
                    else
                    {
                        failCallback?.Invoke(userInfo.message);
                        Debug.Log($"ServerConnection::CreateGuestUser:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::CreateGuestUser::JSON:: {json}");

                    RemoveFromWaitingList(nameof(CreateGuestUser));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(CreateGuestUser), new WaitingItem(() => CreateGuestUser(successCallback, failCallback)));
        }

        [Obsolete]
        public void UpdatePlayerSet(string setCode, Action<List<SSDM.Set>> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UpdatePlayerSet(setCode, callback));
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "userName", Username },
                { "codeSet", setCode }
            };

            StartCoroutine(_userServer.setUserStyle(body, (string json) =>
            {
                try
                {
                    var userStyle = JsonUtility.FromJson<SSDM.Sets>(json);

                    if (userStyle.success)
                    {
                        _userInfoUpdateNeeded = true;
                        GameManager.Instance.LatestPlayerInfo = null;
                        callback?.Invoke(userStyle.sets);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::{nameof(UpdatePlayerSet)}:: {userStyle.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::{nameof(UpdatePlayerSet)}::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UpdatePlayerSet));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UpdatePlayerSet), new WaitingItem(() => UpdatePlayerSet(setCode, callback)));
        }

        [Obsolete]
        public void GetAllSets(Action<List<SSDM.Set>> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetAllSets(callback));
                return;
            }

            StartCoroutine(_styleServer.getAllStyles(Username, (string json) =>
            {
                try
                {
                    var styles = JsonUtility.FromJson<SSDM.Sets>(json);

                    if (styles.success)
                    {
                        callback?.Invoke(styles.sets);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::{nameof(GetAllSets)}:: {styles.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::{nameof(GetAllSets)}::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetAllSets));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetAllSets), new WaitingItem(() => GetAllSets(callback)));
        }

        [Obsolete]
        public void PurchaseSet(string setCode, Action<SSDM.Sets> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => PurchaseSet(setCode, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "userName", Username },
                { "codeSet", setCode }
            };

            StartCoroutine(_styleServer.bughtStyle(body, (string json) =>
            {
                try
                {
                    var us = JsonUtility.FromJson<SSDM.Sets>(json);

                    if (us.success)
                    {
                        _userInfoUpdateNeeded = true;
                        GameManager.Instance.LatestPlayerInfo = null;
                        callback?.Invoke(us);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::{nameof(PurchaseSet)}:: {us.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::{nameof(PurchaseSet)}::JSON:: {json}");

                    RemoveFromWaitingList(nameof(PurchaseSet));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(PurchaseSet), new WaitingItem(() => PurchaseSet(setCode, callback)));
        }

        [Obsolete]
        public void WatchAdForSet(string setCode, Action<SSDM.Sets> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => WatchAdForSet(setCode, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "userName", Username },
                { "codeSet", setCode }
            };

            StartCoroutine(_styleServer.adBughtStyle(body, (string json) =>
            {
                try
                {
                    var us = JsonUtility.FromJson<SSDM.Sets>(json);

                    if (us.success)
                    {
                        _userInfoUpdateNeeded = true;
                        GameManager.Instance.LatestPlayerInfo = null;
                        callback?.Invoke(us);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::{nameof(WatchAdForSet)}:: {us.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::{nameof(WatchAdForSet)}::JSON:: {json}");

                    RemoveFromWaitingList(nameof(WatchAdForSet));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(WatchAdForSet), new WaitingItem(() => WatchAdForSet(setCode, callback)));
        }

        [Obsolete]
        public void UpdateUserData(Action callback, Gender gender)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UpdateUserData(callback, gender));
                return;
            }

            string genderString = gender switch
            {
                Gender.Male => "m",
                Gender.Female => "f",
                _ => throw new NotImplementedException("ServerConnection:: UpdateUserData:: gender not implemented")
            };

            var data = new Dictionary<string, string>
            {
                { "UserName", Username },
                { "sex", genderString }
            };

            StartCoroutine(_userServer.updateSex(data, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (userInfo.success)
                    {
                        GameManager.Instance.LatestPlayerInfo = null;
                        _userInfoUpdateNeeded = true;
                        callback?.Invoke();
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::UpdateUserData:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::UpdateUserData::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UpdateUserData));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UpdateUserData), new WaitingItem(() => UpdateUserData(callback, gender)));
        }

        [Obsolete]
        public void DecreaseUserCoin(int count, Action<UserScore> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => DecreaseUserCoin(count, callback));
                return;
            }

            StartCoroutine(_userServer.DecreaseCoin(Username, count, (string json) =>
            {
                try
                {
                    var userscore = JsonUtility.FromJson<UserScore>(json);
                    if (userscore.success)
                    {
                        callback?.Invoke(userscore);
                        _userInfoUpdateNeeded = true;
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::DecreaseUserCoin:: {userscore.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::DecreaseUserCoin::JSON:: {json}");

                    RemoveFromWaitingList(nameof(DecreaseUserCoin));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(DecreaseUserCoin), new WaitingItem(() => DecreaseUserCoin(count, callback)));
        }

        [Obsolete]
        public void SaveGameData(string time, bool isWon, int rank, int coin, int exp, int trophy, Action<SaveGame> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SaveGameData(time, isWon, rank, coin, exp, trophy, callback));
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "userName", Username },
                { "EndTime", time },
                { "Win", isWon },
                { "Rank", rank },
                { "ReportType", true },
                { "coin", coin},
                { "Exp", exp},
                { "Trophy", trophy}
            };

            StartCoroutine(_gameServer.saveGame(body, (string json) =>
            {
                try
                {
                    var saveGameInfo = JsonUtility.FromJson<SaveGame>(json);
                    if (saveGameInfo.success)
                    {
                        callback?.Invoke(saveGameInfo);
                        //GetPlayerInfo();

                        _userInfoUpdateNeeded = true;
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::SaveGameData:: {saveGameInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SaveGameData::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SaveGameData));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(SaveGameData), new WaitingItem(() => SaveGameData(time, isWon, rank, coin, exp, trophy, callback)));
        }

        [Obsolete]
        public void GetPlayerInfo(Action<UserInfo> callback)
        {
            if (!_userInfoUpdateNeeded && CurrentUserInfo != null)
            {
                callback?.Invoke(CurrentUserInfo);
                return;
            }

            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetPlayerInfo(callback));
                return;
            }

            StartCoroutine(_userServer.getUserInfo(Username, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (userInfo.success)
                    {
                        CurrentUserInfo = userInfo;
                        _userInfoUpdateNeeded = false;
                        callback?.Invoke(userInfo);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::GetPlayerInfo:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetPlayerInfo::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetPlayerInfo));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(GetPlayerInfo), new WaitingItem(() => GetPlayerInfo(callback)));
        }

        [Obsolete]
        public void GetChangeUsernamePrice(Action<int> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetChangeUsernamePrice(callback));
                return;
            }

            StartCoroutine(_userServer.updateUserTariff(Username, (string json) =>
            {
                try
                {
                    var ut = JsonUtility.FromJson<UserTariff>(json);

                    if (ut.success)
                    {
                        callback?.Invoke(ut.cost);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::GetChangeUsernamePrice:: {ut.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetChangeUsernamePrice::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetChangeUsernamePrice));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetChangeUsernamePrice), new WaitingItem(() => GetChangeUsernamePrice(callback)));
        }

        [Obsolete]
        public void GetServerStatues(Action<ServerModel> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetServerStatues(callback));
                return;
            }

            StartCoroutine(_serverInitial.serverStatus((string json) =>
            {
                try
                {
                    var sd = JsonUtility.FromJson<ServerModel>(json);

                    callback?.Invoke(sd);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetServerStatues::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetServerStatues));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

            }));

            AddToWaitingList(nameof(GetServerStatues), new WaitingItem(() => GetServerStatues(callback)));
        }

        [Obsolete]
        public void SendCoinRequest(Action<CoinRequest> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendCoinRequest(callback));
                return;
            }

            StartCoroutine(_userServer.CoinRequest(Username, (string json) =>
            {
                try
                {
                    var coinReq = JsonUtility.FromJson<CoinRequest>(json);

                    if (coinReq.success)
                    {
                        callback?.Invoke(coinReq);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::SendCoinRequest:: {coinReq.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendCoinRequest::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendCoinRequest));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

            }));

            AddToWaitingList(nameof(SendCoinRequest), new WaitingItem(() => SendCoinRequest(callback)));
        }

        [Obsolete]
        public void GetQuestions(Action<Questions> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetQuestions(callback));
                return;
            }

            StartCoroutine(_gameServer.GetQuestions(Username, (string json) =>
            {
                try
                {
                    var question = JsonUtility.FromJson<Questions>(json);

                    if (question.success)
                    {
                        callback?.Invoke(question);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::SendCoinRequest:: {question.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetQuestions::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetQuestions));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetQuestions), new WaitingItem(() => GetQuestions(callback)));
        }

        [Obsolete]
        public void UpdateUsername(string newUsername, Action<UserInfo> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UpdateUsername(newUsername, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "OldUserName", Username },
                { "NewUserName", newUsername }
            };

            StartCoroutine(_userServer.updateUsername(body, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    GameManager.Instance.LatestPlayerInfo = null;
                    _userInfoUpdateNeeded = true;
                    callback?.Invoke(userInfo);

                    if (!userInfo.success)
                        Debug.Log($"ServerConnection::UpdateUsername:: {userInfo.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::UpdateUsername::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UpdateUsername));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UpdateUsername), new WaitingItem(() => UpdateUsername(newUsername, callback)));
        }

        [Obsolete]
        public void UpdateGender(Gender newGender, Action<UserInfo> callback)
        {
            if (newGender != Gender.Female || newGender != Gender.Male)
                return;

            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UpdateGender(newGender, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "UserName", Username }
            };

            // 'f' for female, 'm' for male
            var genderString = newGender switch
            {
                Gender.Female => "f",
                Gender.Male => "m",
                _ => throw new NotImplementedException(),
            };

            body.Add("Sex", genderString);

            StartCoroutine(_userServer.updateSex(body, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (userInfo.success)
                    {
                        callback?.Invoke(userInfo);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::UpdateGender:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::UpdateGender::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UpdateGender));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UpdateGender), new WaitingItem(() => UpdateGender(newGender, callback)));
        }

        [Obsolete]
        public void GetMatchMaking(Action<List<MatchMakingUser>> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetMatchMaking(callback));
                return;
            }

            StartCoroutine(_gameServer.GetMatchMaking(Username, (string json) =>
            {
                try
                {
                    var matchMaking = JsonUtility.FromJson<MatchMaking>(json);


                    if (matchMaking.success)
                    {
                        callback?.Invoke(matchMaking.users);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::GetMatchMaking:: {matchMaking.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetMatchMaking::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetMatchMaking));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetMatchMaking), new WaitingItem(() => GetMatchMaking(callback)));
        }

        [Obsolete]
        public void GetChangeGenderPrice(Action<int> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetChangeGenderPrice(callback));
                return;
            }

            StartCoroutine(_userServer.updateSexTariff(Username, (string json) =>
            {
                try
                {
                    var ct = JsonUtility.FromJson<SexTariff>(json);

                    if (ct.success)
                    {
                        callback?.Invoke(ct.cost);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::GetChangeGenderPrice:: {ct.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetChangeGenderPrice::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetChangeGenderPrice));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetChangeGenderPrice), new WaitingItem(() => GetChangeGenderPrice(callback)));
        }

        #region Leaderboard API's
        [Obsolete]
        public void GetLeaderboard(Action<LeaderBoard> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetLeaderboard(callback));
                return;
            }

            StartCoroutine(_gameServer.GetLeaderBoard(Username, (string json) =>
            {
                try
                {
                    var leaderBoard = JsonUtility.FromJson<LeaderBoard>(json);

                    if (leaderBoard.success)
                    {
                        callback?.Invoke(leaderBoard);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::GetLeaderboard:: {leaderBoard.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetLeaderboard::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetLeaderboard));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetLeaderboard), new WaitingItem(() => GetLeaderboard(callback)));
        }

        [Obsolete]
        public void ResetTrophies()
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ResetTrophies());
                return;
            }

            StartCoroutine(_userServer.resetTrophy(Username, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    if (!userInfo.success)
                    {
                        Debug.Log($"ServerConnection::ResetTrophies:: {userInfo.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ResetTrophies::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ResetTrophies));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(ResetTrophies), new WaitingItem(() => ResetTrophies()));
        }
        #endregion

        #region Save Phone Number API's
        [Obsolete]
        public void SetPhoneNumber(string phoneNumber, Action<UserPhone> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SetPhoneNumber(phoneNumber, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "userName", Username },
                { "phone", phoneNumber }
            };

            StartCoroutine(_userServer.setPhone(data, (string json) =>
            {
                try
                {
                    var userPhone = JsonUtility.FromJson<UserPhone>(json);

                    if (userPhone.success)
                    {
                        callback?.Invoke(userPhone);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::SetPhoneNumber:: {userPhone.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SetPhoneNumber::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SetPhoneNumber));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(SetPhoneNumber), new WaitingItem(() => SetPhoneNumber(phoneNumber, callback)));
        }

        [Obsolete]
        public void SendSMSCode(Action<UserPhone> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendSMSCode(callback));
                return;
            }

            StartCoroutine(_userServer.SendSmsCode(Username, (string json) =>
            {
                try
                {
                    var userPhone = JsonUtility.FromJson<UserPhone>(json);

                    if (userPhone.success)
                    {
                        callback?.Invoke(userPhone);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::SendSMSCode:: {userPhone.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendSMSCode::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendSMSCode));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(SendSMSCode), new WaitingItem(() => SendSMSCode(callback)));
        }

        [Obsolete]
        public void ActivePhoneNumber(string code, Action<UserPhone> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ActivePhoneNumber(code, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "userName", Username },
                { "code", code }
            };

            StartCoroutine(_userServer.activePhone(data, (string json) =>
            {
                try
                {
                    var userPhone = JsonUtility.FromJson<UserPhone>(json);

                    if (userPhone.success)
                    {
                        callback?.Invoke(userPhone);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::ActivePhoneNumber:: {userPhone.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ActivePhoneNumber::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ActivePhoneNumber));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(ActivePhoneNumber), new WaitingItem(() => ActivePhoneNumber(code, callback)));
        }

        [Obsolete]
        public void RecoverUserByPhoneNumber(string code, Action<UserPhone> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => RecoverUserByPhoneNumber(code, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "userName", Username },
                { "code", code }
            };

            StartCoroutine(_userServer.recoveryUserByPhone(data, (string json) =>
            {
                try
                {
                    var userPhone = JsonUtility.FromJson<UserPhone>(json);

                    if (userPhone.success)
                    {
                        callback?.Invoke(userPhone);
                    }
                    else
                    {
                        Debug.Log($"ServerConnection::RecoverUserByPhoneNumber:: {userPhone.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::RecoverUserByPhoneNumber::JSON:: {json}");

                    RemoveFromWaitingList(nameof(RecoverUserByPhoneNumber));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(RecoverUserByPhoneNumber), new WaitingItem(() => RecoverUserByPhoneNumber(code, callback)));
        }
        #endregion

        #region Registeration & Login
        [Obsolete]
        public void CheckUsername(string username, Action<SimpleModel> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => CheckUsername(username, callback));
                return;
            }

            StartCoroutine(_userServer.checkUserName(username, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    callback?.Invoke(sm);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::CheckUsername:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::CheckUsername::JSON:: {json}");

                    RemoveFromWaitingList(nameof(CheckUsername));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(CheckUsername), new WaitingItem(() => CheckUsername(username, callback)));
        }

        [Obsolete]
        public void RegisterUser(string username, string password, Action<UserInfo> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => RegisterUser(username, password, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "oldUserName", Username },
                { "newUserName", username },
                { "password", password }
            };

            StartCoroutine(_userServer.registerUser(data, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    _userInfoUpdateNeeded = true;
                    callback?.Invoke(userInfo);

                    if (!userInfo.success)
                        Debug.Log($"ServerConnection::RegisterUser:: {userInfo.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::RegisterUser::JSON:: {json}");

                    RemoveFromWaitingList(nameof(RegisterUser));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(RegisterUser), new WaitingItem(() => RegisterUser(username, password, callback), 10f));
        }

        [Obsolete]
        public void LoginUser(string username, string password, Action<UserInfo> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => LoginUser(username, password, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "userName", username},
                { "password", password }
            };

            StartCoroutine(_userServer.loginUser(data, (string json) =>
            {
                try
                {
                    var userInfo = JsonUtility.FromJson<UserInfo>(json);

                    _userInfoUpdateNeeded = true;
                    callback?.Invoke(userInfo);

                    if (!userInfo.success)
                        Debug.Log($"ServerConnection::LoginUser:: {userInfo.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::LoginUser::JSON:: {json}");

                    RemoveFromWaitingList(nameof(LoginUser));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(LoginUser), new WaitingItem(() => LoginUser(username, password, callback)));
        }

        [Obsolete]
        public void ChangePassword(string oldPassword, string newPassword, Action<SimpleModel> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ChangePassword(oldPassword, newPassword, callback));
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "userName", Username},
                { "oldPassword", oldPassword},
                { "newPassword", newPassword}
            };

            StartCoroutine(_userServer.changePassword(data, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    callback?.Invoke(sm);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::ChangePassword:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ChangePassword::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ChangePassword));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(ChangePassword), new WaitingItem(() => ChangePassword(oldPassword, newPassword, callback)));
        }

        [Obsolete]
        public void SendUserInfoViaPhoneNumber(string phoneNumber, Action<UserPhone> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendUserInfoViaPhoneNumber(phoneNumber, callback));
                return;
            }

            StartCoroutine(_userServer.smsUserInfo(phoneNumber, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<UserPhone>(json);

                    callback?.Invoke(sm);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::SendUserInfoViaPhoneNumber:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendUserInfoViaPhoneNumber::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendUserInfoViaPhoneNumber));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(SendUserInfoViaPhoneNumber), new WaitingItem(() => SendUserInfoViaPhoneNumber(phoneNumber, callback)));
        }
        #endregion

        [Obsolete]
        public void RequestForPrize(Action<bool> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => RequestForPrize(callback));
                return;
            }

            StartCoroutine(_userServer.CoinRequest(Username, (string json) =>
            {
                try
                {
                    var cr = JsonUtility.FromJson<CoinRequest>(json);

                    if (cr.success)
                    {
                        GameManager.Instance.LatestPlayerInfo = null;
                        _userInfoUpdateNeeded = true;
                    }
                    else
                        Debug.Log($"ServerConnection::RequestForPrize:: {cr.message}");

                    callback?.Invoke(cr.success);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::RequestForPrize::JSON:: {json}");

                    RemoveFromWaitingList(nameof(RequestForPrize));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(RequestForPrize), new WaitingItem(() => RequestForPrize(callback)));
        }

        [Obsolete]
        public void CheckVersion(Action<CheckVersion> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => CheckVersion(callback));
                return;
            }

            StartCoroutine(_logServer.CheckVersion(Username, Application.version, (string json) =>
            {
                try
                {
                    var checkVersion = JsonUtility.FromJson<CheckVersion>(json);

                    callback?.Invoke(checkVersion);

                    if (!checkVersion.success)
                        Debug.Log($"ServerConnection::CheckVersion:: {checkVersion.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::CheckVersion::JSON:: {json}");

                    RemoveFromWaitingList(nameof(CheckVersion));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(CheckVersion), new WaitingItem(() => CheckVersion(callback)));
        }

        [Obsolete]
        private void ExitUserFromGame()
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ExitUserFromGame());
                return;
            }

            StartCoroutine(_logServer.ExitUser(Username, (string json) =>
            {
                try
                {
                    var userExit = JsonUtility.FromJson<UserExit>(json);

                    if (!userExit.success)
                        Debug.Log($"ServerConnection::ExitUserFromGame:: {userExit.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ExitUserFromGame::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ExitUserFromGame));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(ExitUserFromGame), new WaitingItem(() => ExitUserFromGame()));
        }

        #region Market API's
        [Obsolete]
        public void GetRewardCoin(Action<RewardCoin> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetRewardCoin(callback));
                return;
            }

            StartCoroutine(_marketServer.rewardCoin(Username, (string json) =>
            {
                try
                {
                    var coin = JsonUtility.FromJson<RewardCoin>(json);

                    if (coin.success)
                        callback?.Invoke(coin);
                    else
                        Debug.Log($"ServerConnection::GetRewardCoin:: {coin.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetRewardCoin::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetRewardCoin));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(GetRewardCoin), new WaitingItem(() => GetRewardCoin(callback)));
        }

        [Obsolete]
        public void GetShopItems(Action<Shope> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetShopItems(callback));
                return;
            }

            StartCoroutine(_marketServer.getShope(Username, (string json) =>
            {
                try
                {
                    var data = JsonUtility.FromJson<Shope>(json);
                    callback?.Invoke(data);

                    if (!data.success)
                    {
                        Debug.Log($"ServerConnection::GetShopItems:: {data.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetShopItems::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetShopItems));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(GetShopItems), new WaitingItem(() => GetShopItems(callback)));
        }

        [Obsolete]
        public void SendMarketPayRequest(string packName, string marketName, Action<PaymentRequest> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendMarketPayRequest(packName, marketName, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "username", Username },
                { "name", packName },
                { "market", marketName }
            };

            StartCoroutine(_marketServer.requestMarketPay(body, (string json) =>
            {
                try
                {
                    var request = JsonUtility.FromJson<PaymentRequest>(json);

                    callback?.Invoke(request);

                    if (!request.success)
                    {
                        Debug.Log($"ServerConnection::SendMarketPayRequest:: {request.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendMarketPayRequest::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendMarketPayRequest));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(SendMarketPayRequest), new WaitingItem(() => SendMarketPayRequest(packName, marketName, callback)));
        }

        [Obsolete]
        public void SendMarketPaymentCompletion(string payload, string token, Action<PaymentCompleted> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendMarketPaymentCompletion(payload, token, callback));
                return;
            }

            var body = new Dictionary<string, string>
            {
                { "developerPayload", payload },
                { "PurchaseToken", token }
            };

            StartCoroutine(_marketServer.completeMarketPay(body, (string json) =>
            {
                try
                {
                    _userInfoUpdateNeeded = true;

                    var request = JsonUtility.FromJson<PaymentCompleted>(json);
                    callback?.Invoke(request);

                    if (!request.success)
                    {
                        Debug.Log($"ServerConnection::SendMarketPaymentCompletion:: {request.message}");
                    }

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendMarketPaymentCompletion::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendMarketPaymentCompletion));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(SendMarketPaymentCompletion), new WaitingItem(() => SendMarketPaymentCompletion(payload, token, callback)));
        }
        #endregion

        #region Ads
        /// <param name="pos">Bottom, Medium, Big</param>
        [Obsolete]
        public void GetAds(string pos, int count, Action<Ads> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetAds(pos, count, callback));
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "username", Username },
                { "position", pos },
                { "count", count }
            };

            StartCoroutine(_marketServer.getAds(body, (string json) =>
            {
                try
                {
                    var ads = JsonUtility.FromJson<Ads>(json);

                    if (!ads.success)
                        Debug.Log($"ServerConnection::GetAds:: {ads.message}");
                    else
                        callback?.Invoke(ads);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetAds::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetAds));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(GetAds), new WaitingItem(() => GetAds(pos, count, callback)));
        }

        [Obsolete]
        public void ClickAd(int adId, Action callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ClickAd(adId, callback));
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "username", Username },
                { "adId", adId}
            };

            StartCoroutine(_marketServer.clickAd(body, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::ClickAd:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ClickAd::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ClickAd));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }));

            AddToWaitingList(nameof(ClickAd), new WaitingItem(() => ClickAd(adId, callback)));
        }

        [Obsolete]
        public void DownloadImage(string url, Action<Sprite> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => DownloadImage(url, callback));
                return;
            }

            StartCoroutine(ServerInitial.downloadImg(url, (Texture2D texture) =>
            {
                try
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                    callback?.Invoke(sprite);
                    RemoveFromWaitingList(nameof(ClickAd));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);

                    throw;
                }
            }));

            AddToWaitingList(nameof(DownloadImage), new WaitingItem(() => DownloadImage(url, callback)));
        }
        #endregion

        #region Boosters
        [Obsolete]
        public void GetBoosters(Action<MSDM.Boosters> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetBoosters(callback));
                return;
            }

            StartCoroutine(_marketServer.GetBooster(Username, (string json) =>
            {
                try
                {
                    var booster = JsonUtility.FromJson<MSDM.Boosters>(json);

                    if (booster.success)
                        callback?.Invoke(booster);
                    else
                        Debug.Log($"ServerConnection::GetBoosters:: {booster.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetBoosters::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetBoosters));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetBoosters), new WaitingItem(() => GetBoosters(callback)));
        }

        [Obsolete]
        public void UseBooster(int boosterId)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UseBooster(boosterId));
                return;
            }

            var dic = new Dictionary<string, object>
            {
                { "UserName", Username },
                { "BoosterId", boosterId }
            };

            Debug.Log(JsonConvert.SerializeObject(dic));

            StartCoroutine(_marketServer.UseBooster(dic, (string json) =>
            {
                try
                {
                    var booster = JsonUtility.FromJson<MSDM.Boosters>(json);

                    if (!booster.success)
                        Debug.Log($"ServerConnection::UseBooster:: {booster.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::UseBooster::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UseBooster));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UseBooster), new WaitingItem(() => UseBooster(boosterId)));
        }

        [Obsolete]
        public void BuyBooster(int boosterId, Action<MSDM.Boosters> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => BuyBooster(boosterId, callback));
                return;
            }

            var dic = new Dictionary<string, object>
            {
                { "UserName", Username },
                { "BoosterId", boosterId }
            };

            StartCoroutine(_marketServer.BuyBooster(dic, (string json) =>
            {
                try
                {
                    var booster = JsonUtility.FromJson<MSDM.Boosters>(json);

                    if (booster.success)
                        callback?.Invoke(booster);
                    else
                        Debug.Log($"ServerConnection::BuyBooster:: {booster.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::BuyBooster::JSON:: {json}");

                    RemoveFromWaitingList(nameof(BuyBooster));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(BuyBooster), new WaitingItem(() => BuyBooster(boosterId, callback)));
        }
        #endregion

        #region Events
        [Obsolete]
        public void GetEvents(Action<ESDM.Events> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetEvents(callback));
                return;
            }

            StartCoroutine(_eventServer.Events(Username, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<ESDM.Events>(json);

                    if (!ev.success)
                        Debug.Log($"ServerConnection::GetEvents:: {ev.message}");
                    else
                        callback?.Invoke(ev);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetEvents::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetEvents));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetEvents), new WaitingItem(() => GetEvents(callback)));
        }

        [Obsolete]
        public void GetEventDetails(int eventId, Action<ESDM.EventInfo> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => GetEventDetails(eventId, callback));
                return;
            }

            StartCoroutine(_eventServer.EventInfo(eventId, Username, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<ESDM.EventDetaile>(json);

                    if (!ev.success)
                        Debug.Log($"ServerConnection::GetEventDetails:: {ev.message}");
                    else
                        callback?.Invoke(ev.eventInfo);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::GetEventDetails::JSON:: {json}");

                    RemoveFromWaitingList(nameof(GetEventDetails));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(GetEventDetails), new WaitingItem(() => GetEventDetails(eventId, callback)));
        }

        [Obsolete]
        public void RegisterEvent(int eventId, int typeNumber, Action<ESDM.Events> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => RegisterEvent(eventId, typeNumber, callback));
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "userName", Username },
                { "eventId", eventId },
                { "Type", typeNumber}
            };

            Debug.Log(JsonConvert.SerializeObject(body));

            StartCoroutine(_eventServer.RegisterEvent(body, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<ESDM.Events>(json);

                    if (!ev.success)
                        Debug.Log($"ServerConnection::RegisterEvent:: {ev.message}");
                    else
                        callback?.Invoke(ev);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::RegisterEvent::JSON:: {json}");

                    RemoveFromWaitingList(nameof(RegisterEvent));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(RegisterEvent), new WaitingItem(() => RegisterEvent(eventId, typeNumber, callback)));
        }

        [Obsolete]
        public void SendQuestionForEvent(ESDM.NewQuestionEventElement question, Action<ESDM.NewEventResponse> callback, Action<string> failMessage)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => SendQuestionForEvent(question, callback, failMessage));
                return;
            }

            question.UserName = Username;

            Debug.Log(JsonConvert.SerializeObject(question));

            StartCoroutine(_eventServer.SaveQuestoinEvent(question, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<ESDM.NewEventResponse>(json);

                    if (!ev.success)
                    {
                        failMessage?.Invoke(ev.message);
                        Debug.Log($"ServerConnection::SendQuestionForEvent:: {ev.message}");
                    }
                    else
                        callback?.Invoke(ev);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::SendQuestionForEvent::JSON:: {json}");

                    RemoveFromWaitingList(nameof(SendQuestionForEvent));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(SendQuestionForEvent), new WaitingItem(() => SendQuestionForEvent(question, callback, failMessage)));
        }

        [Obsolete]
        public void EventSaveGame(string endTime, int rank, int eventId, Action<ESDM.NewEventResponse> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => EventSaveGame(endTime, rank, eventId, callback));
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "username", Username },
                { "EndTime", endTime },
                { "Rank", rank },
                { "EventId", eventId }
            };

            StartCoroutine(_eventServer.SaveGameEvent(data, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<ESDM.NewEventResponse>(json);

                    if (!ev.success)
                        Debug.Log($"ServerConnection::EventSaveGame:: {ev.message}");
                    else
                        callback?.Invoke(ev);

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::EventSaveGame::JSON:: {json}");

                    RemoveFromWaitingList(nameof(EventSaveGame));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(EventSaveGame), new WaitingItem(() => EventSaveGame(endTime, rank, eventId, callback)));
        }

        [Obsolete]
        public void ClaimEventPrize(int eventId, Action<Claim> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ClaimEventPrize(eventId, callback));
                return;
            }

            StartCoroutine(_eventServer.ClaimEvent(eventId, Username, (string json) =>
            {
                try
                {
                    var ev = JsonUtility.FromJson<Claim>(json);

                    if (ev.success)
                        callback?.Invoke(ev);
                    else
                        Debug.Log($"ServerConnection::ClaimEventPrize:: {ev.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ClaimEventPrize::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ClaimEventPrize));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(ClaimEventPrize), new WaitingItem(() => ClaimEventPrize(eventId, callback)));
        }
        #endregion

        [Obsolete]
        public void ReportQuestion(int questionId, bool isCorrect, string userAnswer, int reportType)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => ReportQuestion(questionId, isCorrect, userAnswer, reportType));
                return;
            }

            var dic = new Dictionary<string, object>
            {
                { "QuestionId", questionId },
                { "userName", Username},
                { "isCorrect", isCorrect },
                { "answer", userAnswer },
                { "ReportType", reportType }
            };

            StartCoroutine(_gameServer.QuestionReport(dic, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::ReportQuestion:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::ReportQuestion::JSON:: {json}");

                    RemoveFromWaitingList(nameof(ReportQuestion));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(ReportQuestion), new WaitingItem(() => ReportQuestion(questionId, isCorrect, userAnswer, reportType)));
        }

        [Obsolete]
        public void QuestionLikeDislike(int questionId, bool like)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => QuestionLikeDislike(questionId, like));
                return;
            }

            var dic = new Dictionary<string, object>
            {
                { "QuestionId", questionId },
                { "userName", Username},
                { "like", like ? 1 : -1 }
            };

            StartCoroutine(_gameServer.QuestionLike(dic, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::QuestionLikeDislike:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::QuestionLikeDislike::JSON:: {json}");

                    RemoveFromWaitingList(nameof(QuestionLikeDislike));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(QuestionLikeDislike), new WaitingItem(() => QuestionLikeDislike(questionId, like)));
        }

        [Obsolete]
        public void UserComment(int rating, string comment, Action<bool> callback)
        {
            if (!IsConnected())
            {
                actionsAfterReconnect.Enqueue(() => UserComment(rating, comment, callback));
                return;
            }

            var dic = new Dictionary<string, object>
            {
                { "userName", Username },
                { "appVersion", Application.version },
                { "rank", Convert.ToInt16(rating) },
                { "comment", comment }
            };

            StartCoroutine(_logServer.UserComment(dic, (string json) =>
            {
                try
                {
                    var sm = JsonUtility.FromJson<SimpleModel>(json);

                    callback?.Invoke(sm.success);

                    if (!sm.success)
                        Debug.Log($"ServerConnection::UserComment:: {sm.message}");

                    if (ShowJSONLogs)
                        Debug.Log($"ServerConnection::UserComment::JSON:: {json}");

                    RemoveFromWaitingList(nameof(UserComment));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }));

            AddToWaitingList(nameof(UserComment), new WaitingItem(() => UserComment(rating, comment, callback)));
        }

        public bool IsConnected()
        {
            var connected = ServerInitial.checkInternetConnection();

            if (!connected)
            {
                _presentor.ActiveConnectionPanel(true);
                AudioManager.Instance.ErrorSFX();

                if (reconnectAttempts > 1)
                    _presentor.ActiveEnsureNetText(true);

                StartCoroutine(CheckConnectionAgainCoroutine());
            }
            else
            {
                _presentor.ResetUI();

                while (actionsAfterReconnect.Count > 0)
                {
                    var action = actionsAfterReconnect.Dequeue();
                    action?.Invoke();
                }
            }

            return connected;
        }

        [Obsolete]
        private void CheckServerStatus()
        {
            GetServerStatues((data) =>
            {
                if (data.reportState)
                {
                    _presentor.SetServerStatusPanelActivation(true);
                    _presentor.SetServerStatusTitle(data.reportTitle);

                    var desc = new StringBuilder();
                    desc.Append(data.reportMessage + "\n");
                    desc.Append($"غیرفعال تا {data.reportTime}");

                    _presentor.SetServerStatusDescription(desc.ToString());
                }
            });
        }

        private IEnumerator CheckConnectionAgainCoroutine()
        {
            var timer = 10;

            if (reconnectAttempts == 0)
                timer = 5;

            while (timer > 0)
            {
                timer--;

                if (reconnectAttempts > 0)
                    _presentor.SetTimerText($"{timer}");

                yield return new WaitForSeconds(1);
            }

            reconnectAttempts++;

            IsConnected();
        }
    }
}