using UnityEngine;
using USDM;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System;
using ESDM;
using System.Linq;

//All Action For Connect To Server
namespace ServerApp
{
    class ServerAuth
    {
        //set Header Auth
        public static string authenticate()
        {
            string auth = "checkpoint" + ":" + "lilSwag";
            auth = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            return auth;
        }
        public static string url = "http://cpinfomath.ir/api/client/";//Basic Url
        public static string urlReport = "http://cpinfomath.ir:5001/api/client/";//Basic Url Report

        //Get Device Id
        public static string deviceId = SystemInfo.deviceUniqueIdentifier;
        // Get Android DeviceID (Backup Method)
        public static string GetDeviceID()
        {
            // Get Android ID
            AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
            string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
            // Get bytes of Android ID
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(android_id);
            // Encrypt bytes with md5
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            string device_id = hashString.PadLeft(32, '0');
            return device_id;
        }
    }

    //Pre Initial Server Start
    public class ServerInitial
    {
        public static string baseUrl => "http://cpinfomath.ir/";


        //************* Check Device Connection to Network (Check Net) ****************
        public static bool checkInternetConnection()
        {
            return !(Application.internetReachability == NetworkReachability.NotReachable);
        }
        //************* Check Ping Connection to Server (Check Ping) ****************
        public static bool pingServerConnection()
        {
            try
            {
                return 
                    new System.Net.NetworkInformation.Ping().Send("cpinfomath.ir").Status == System.Net.NetworkInformation.IPStatus.Success;

            }
            catch (System.Net.NetworkInformation.PingException)
            {
                return false;
            }
        }

        //************* Check Repair or Update Server (Server Check) ****************
        [System.Obsolete]
        public IEnumerator serverStatus(System.Action<string> callback)
        {
            string url = ServerAuth.urlReport + "RepairReport";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                    //yield return request.error;
                }
                else
                {
                    callback(request.downloadHandler.text);
                    //yield return request.downloadHandler.text;
                }
            }
        }

        //************* Download Image from Server (Download Image) ****************
        [System.Obsolete]
        public static IEnumerator downloadImg(string img,System.Action<Texture2D> callback)
        {
            string url = baseUrl + img;
            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
                

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    callback(null);
                }
                else
                {
                    if (www.isDone)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(www);
                        /*var rect = new Rect(0, 0, 600f, 600f);
                        var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));*/
                        callback(texture);
                    }
                }
            }
        }

    }
    // User Services Api,s
    public class UserServer
    {
        //*************Get All User Info****************
        [System.Obsolete]
        public IEnumerator getUserInfo(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/UserInfo?userName=" + userName;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                    //yield return request.error;
                }
                else
                {
                    //callback(request.downloadHandler.text);
                    callback(calculatLevelInfo(request.downloadHandler.text));//add level progress and callback
                    //yield return request.downloadHandler.text;
                }
            }
        }
        //*************Create New User (Signup User By Style) ****************
        [System.Obsolete]
        public IEnumerator createUser(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "CreateUser";
            string auth = ServerAuth.authenticate();
            string devId = ServerAuth.deviceId;
            body.Add("deviceId", devId);
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                    //yield return request.error;
                }
                else
                {
                    callback(request.downloadHandler.text);
                    //yield return request.downloadHandler.text;
                }
            }
        }
        //*************Create New User (Signup User Free Style) ****************
        [System.Obsolete]
        public IEnumerator createUser(string username, string sex, System.Action<string> callback)
        {
            string url = ServerAuth.url + "CreateUser";
            string auth = ServerAuth.authenticate();
            Dictionary<string, string> body = new Dictionary<string, string>();
            body.Add("userName", username);
            body.Add("sex", sex);
            body.Add("deviceId", ServerAuth.deviceId);
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                    //yield return request.error;
                }
                else
                {
                    callback(request.downloadHandler.text);
                    //yield return request.downloadHandler.text;
                }
            }
        }
        //************* Create Random Guest User (Signup User Random Sex & UserName & Free Style) ****************
        [System.Obsolete]
        public IEnumerator createGuestUser(System.Action<string> callback)
        {
                                    			#if UNITY_EDITOR
            string url = ServerAuth.url + "V2/CreateRandomUser?deviceId=" + RandomString(10);
#else
			string url = ServerAuth.url + "V2/CreateRandomUser?deviceId=" + ServerAuth.deviceId;
#endif

            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
		
		 public static string RandomString(int length)
        {
            var random = new System.Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
		
		
        //*************Change Info User (Update User Info) ****************
        [System.Obsolete]
        public IEnumerator updateUser(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateUser";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);

                }
            }
        }
        //************* Check Exist User (IsAllowedUser) ****************
        [System.Obsolete]
        public IEnumerator checkUserName(string userName,System.Action<string> callback)
        {
            string url = ServerAuth.url + "IsExistUser?Username=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Register User for Set Username And Password (Update User Info) ****************
        [System.Obsolete]
        public IEnumerator registerUser(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "RegisterUser";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);

                }
            }
        }
        //*************Login User By Password (Login User) ****************
        [System.Obsolete]
        public IEnumerator loginUser(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "LoginUser";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);

                }
            }
        }
        //*************Change Password for User (Change password) ****************
        [System.Obsolete]
        public IEnumerator changePassword(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "ChangeUserPassword";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);

                }
            }
        }
        //************* Check Exist User (IsAllowedUser) ****************
        [System.Obsolete]
        public IEnumerator smsUserInfo(string phone, System.Action<string> callback)
        {
            string url = ServerAuth.url + "SmsInfoUser?phone=" + phone;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Decrease Coin User (DecreaseCoin User For Buy Skinn or use Booster's) ****************
        [System.Obsolete]
        public IEnumerator DecreaseCoin(string username, int DecCoin, System.Action<string> callback)
        {
            string url = ServerAuth.url + "DecreaseCoin";
            string auth = ServerAuth.authenticate();
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("UserName", username);
            body.Add("Coin", DecCoin);
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Change or Set User Style (Change(Set) User Character)****************
        [System.Obsolete]
        public IEnumerator setUserStyle(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/ChangeSet";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Get User Style (Return User Style) ****************
        [System.Obsolete]
        public IEnumerator getUserStyle(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/UserSet?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Get User Score (Return User Score) ****************
        [System.Obsolete]
        public IEnumerator getUserScore(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/UserScore?userName=" + userName;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    //Add Level Progress
                    callback(calculateLevelScore(request.downloadHandler.text));
                }
            }
        }
        //*************Add Level Progresss (For UserInfo section) ****************
        string calculatLevelInfo(string json)
        {
            try
            {
                int[] minExps = { 0, 30, 50, 70, 170, 370, 770, 1770, 3770, 8700, 18770, 38770, 73770, 123700, 203770 };
                UserInfo userInfo = JsonUtility.FromJson<UserInfo>(json);

                if (userInfo == null || !userInfo.success)
                    return json;
                    
                Level level = userInfo.data.userScore.level;
                int exp = userInfo.data.userScore.exp;
                int min = exp - level.minExp - 1;
                int max = minExps[level.level + 1] - level.minExp;
                userInfo.data.userScore.level.inProgress = min;
                userInfo.data.userScore.level.maxProgress = max;
                userInfo.data.userScore.level.maxExp = minExps[level.level + 1] - 1;
                return JsonConvert.SerializeObject(userInfo).ToString();
            }
            catch (System.Exception)
            {
                return json;
            }
        }
        //*************Add Level Progresss (For UserScore Section) ****************
        string calculateLevelScore(string json)
        {
            try
            {
                int[] minExps = { 0, 30, 50, 70, 170, 370, 770, 1770, 3770, 8700, 18770, 38770, 73770, 123700, 203770 };
                UserScore userScore = JsonUtility.FromJson<UserScore>(json);
                Level level = userScore.data.score.level;
                int exp = userScore.data.score.exp;
                Debug.Log(level.level);
                int min = exp - level.minExp;
                int max = minExps[level.level + 1] - level.minExp;
                userScore.data.score.level.inProgress = min;
                userScore.data.score.level.maxProgress = max;
                userScore.data.score.level.maxExp = minExps[level.level + 1] - 1;
                return JsonConvert.SerializeObject(userScore).ToString();
            }catch(System.Exception)
            {
                return json;
            }
        }
        //************* Coin in Special Time Requst Model (Coin Request) ****************
        [System.Obsolete]
        public IEnumerator CoinRequest(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "SixRequest?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Get Tariff Change Sex for User (Update Sex Tariff) ****************
        [System.Obsolete]
        public IEnumerator updateSexTariff(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateSexTariff?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }  
        //************* Get Tariff Change Username for User (Update Sex Tariff) ****************
        [System.Obsolete]
        public IEnumerator updateUserTariff(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateUserTariff?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Change Username for New other (Change Username) ****************
        [System.Obsolete]
        public IEnumerator updateUsername(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateUserName";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Change Sex for Username (Change Sex) ****************
        [System.Obsolete]
        public IEnumerator updateSex(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateSex";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        //*************Change Info User (Update User Info) ****************
        [System.Obsolete]
        public IEnumerator resetTrophy(string username, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UpdateUser";
            string auth = ServerAuth.authenticate();
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("UserName", username);
            body.Add("Trophy", 0);
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);

                }
            }
        }
        //************* Set Phone for User (Set Phone) ****************
        [System.Obsolete]
        public IEnumerator setPhone(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "RequestVerifyPhone";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Active Phone for User (Active Phone) ****************
        [System.Obsolete]
        public IEnumerator activePhone(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "VerifyPhone";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Send Sms Code  for User (Send Sms Code) ****************
        [System.Obsolete]
        public IEnumerator SendSmsCode(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "SendSmsCodeRecovery?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Recovery By Sms Phone User (Recovery By Phone) ****************
        [System.Obsolete]
        public IEnumerator recoveryUserByPhone(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "RecoveryUserPhone";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
    }

    //Style Server Api's
    public class StyleServer
    {
        //*************Get All Styles for Users (Return All Styles) ****************
        [System.Obsolete]
        public IEnumerator getAllStyles(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/AllSets?userName=" + userName;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        //************* Bougth Style For Special User (Buy Style User) ****************
        [Obsolete]
        public IEnumerator bughtStyle(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/VirtualPaySet";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //************* Bougth Style For Special User By Ad (Buy Style User) ****************
        [Obsolete]
        public IEnumerator adBughtStyle(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/AdPaySet";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
    }

    // Game Services Api's
    public class GameServer
    {


        //************* Get Questions by All Info (Get Question's) ****************
        [System.Obsolete]
        public IEnumerator GetQuestions(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "GetQuestions?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }


        //************* Get Match Making for game  (Get Match Making) ****************
        [System.Obsolete]
        public IEnumerator GetMatchMaking(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/MatchMaking?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        //************* Get Leader Board for game  (Get Match Making) ****************
        [System.Obsolete]
        public IEnumerator GetLeaderBoard(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/LeaderBoard?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator QuestionReport(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/QuestionReport";
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);

                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }


        [System.Obsolete]
        public IEnumerator QuestionLike(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/QuestionLike";
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);

                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        //*************Save User Game's (Temp User Game Manager) ****************
        [System.Obsolete]
        public IEnumerator saveGame(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UserSaveGame";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
       
    }

    // Servers Api's
    public class MarketServer
    {
        //*************Add Coin Request plane at hours ****************
        [System.Obsolete]
        public IEnumerator rewardCoin(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/FreeCoin?userName=" + userName;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Get Shope Objects for User (Get Shoping Item) ****************
        [System.Obsolete]
        public IEnumerator getShope(string username,System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/Shope?userName=" + username;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Request for Check Payment Object (Request Market Pay) ****************
        [System.Obsolete]
        public IEnumerator requestMarketPay(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "RequestMarketPay";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Set Complet Pay and Coin for User (Request Market Pay) ****************
        [System.Obsolete]
        public IEnumerator completeMarketPay(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "CompleteMarketPay";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Request for Check Package Object (Request Package Pay) ****************
        [System.Obsolete]
        public IEnumerator requestPackagePay(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/RequestPackagePay";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Set Complete Pay and Package for User (Complete Package Pay) ****************
        [System.Obsolete]
        public IEnumerator completePackagetPay(Dictionary<string, string> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/CompletePackagePay";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        //*************Get Ad by Position and User (Get Ad) ****************
        [System.Obsolete]
        public IEnumerator getAds(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/GetRandomAd";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Set User Click for Ad (Ad Click) ****************
        [System.Obsolete]
        public IEnumerator clickAd(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/AdClick";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Get Boosters Object (Get Booster Items) ****************
        [System.Obsolete]
        public IEnumerator GetBooster(string username, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/GetBoosters?userName=" + username;
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Burn Booster for User (Use Booster) ****************
        [System.Obsolete]
        public IEnumerator UseBooster(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/UsingBooster";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //*************Bought Booster for User by Coin (Bought Booster) ****************
        [System.Obsolete]
        public IEnumerator BuyBooster(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/BoughtBooster";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }


    }

    public class EventServer
    {
        // GET All Events
        [System.Obsolete]
        public IEnumerator Events(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/GetEvents?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }
        //Get Sepecial Event Info By Code
        [System.Obsolete]
        public IEnumerator EventInfo(int id,string username, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/GetEventInfo?Id=" + id + "&&UserName=" + username;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator RegisterEvent(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/RegisterQuestionEvent";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator SaveQuestoinEvent(NewQuestionEventElement element, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/SaveElementQuestionEvent";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(element)) ;
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator SaveGameEvent(Dictionary<string, object> body, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/SaveElementGameEvent";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                yield return request.SendWebRequest();//send request
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator ClaimEvent(int id, string username, System.Action<string> callback)
        {
            string url = ServerAuth.url + "V2/ClaimEvent?Id=" + id + "&&UserName=" + username;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }


    }

    public class LogServer
    {
        [System.Obsolete]
        public IEnumerator CheckVersion(string userName,string version, System.Action<string> callback)
        {
            string url = ServerAuth.url + "CheckVersion?userName=" + userName + "&requestVersion=" + version;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        [System.Obsolete]
        public IEnumerator ExitUser(string userName, System.Action<string> callback)
        {
            string url = ServerAuth.url + "UserExit?userName=" + userName;
            string auth = ServerAuth.authenticate();

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("AUTHORIZATION", auth);
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }

        

        [Obsolete]
        public IEnumerator UserComment(Dictionary<string,object> body,System.Action<string> callback)
        {
            string url = ServerAuth.url + "UserComment";
            string auth = ServerAuth.authenticate();
            using (var request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
            {
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);

                request.SetRequestHeader("AUTHORIZATION", auth);
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    callback(request.error);
                }
                else
                {
                    callback(request.downloadHandler.text);
                }
            }
        }


    }
}


