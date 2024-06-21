using DeathMatch;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance;

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

        if (!_isInitialized)
            InitializeAudioSources();

        SceneManager.activeSceneChanged += OnSceneChange;
        OnSceneChange(new Scene(), new Scene());
        OnAudioSettingsChanged();
    }
    #endregion

    private bool logCallers = false;

    private Options _options;
    private bool _isInitialized = false;

    private AudioSource _musicADS;
    private AudioSource _sfxADS;

    private void InitializeAudioSources()
    {
        _musicADS = new GameObject("Music").AddComponent<AudioSource>();
        _musicADS.transform.SetParent(transform);
        _musicADS.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _musicADS.loop = true;
        _musicADS.playOnAwake = false;
        _musicADS.spatialBlend = 0;
        _musicADS.Stop();

        _sfxADS = new GameObject("SFX").AddComponent<AudioSource>();
        _sfxADS.transform.SetParent(transform);
        _sfxADS.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _sfxADS.loop = false;
        _sfxADS.playOnAwake = false;
        _sfxADS.spatialBlend = 0;
        _sfxADS.Stop();

        _isInitialized = true;
    }

    private void OnSceneChange(Scene arg0, Scene arg1)
    {
        FindOptions();
        SetupMusicClips();
    }

    private void FindOptions()
    {
        _options = FindObjectOfType<Options>();

        if (_options)
            _options.OnMusicChange += OnAudioSettingsChanged;
    }

    private void OnAudioSettingsChanged()
    {
        _musicADS.mute = SaveManager.Get<int>(Options.MUSIC_MUTE_PREFS) == 1 ? true : false;
        _musicADS.volume = SaveManager.Get<float>(Options.MUSIC_VOLUME_PREFS);
        _musicADS.loop = true;

        if (!_musicADS.isPlaying)
            _musicADS.Play();

        _sfxADS.volume = SaveManager.Get<float>(Options.SFX_VOLUME_PREFS);
        _sfxADS.mute = SaveManager.Get<int>(Options.SFX_MUTE_PREFS) == 1 ? true : false;
    }

    private void SetupMusicClips()
    {
        var isGameplay = false;
        var isMenu = false;

        if (SceneManager.GetActiveScene().name == GameManager.Instance.gameplayScene)
            isGameplay = true;

        if (SceneManager.GetActiveScene().name == GameManager.Instance.mainMenuScene)
            isMenu = true;

        if (isMenu)
        {
            if (!GameManager.Instance.musics.menu)
            {
                _musicADS.Stop();
                return;
            }

            if (_musicADS.clip == GameManager.Instance.musics.menu && _musicADS.isPlaying)
                return;

            _musicADS.clip = GameManager.Instance.musics.menu;
            _musicADS.Play();
            return;
        }

        if (isGameplay)
        {
            if (!GameManager.Instance.musics.gameplay)
            {
                _musicADS.Stop();
                return;
            }

            if (_musicADS.clip == GameManager.Instance.musics.gameplay && _musicADS.isPlaying)
                return;

            _musicADS.clip = GameManager.Instance.musics.gameplay;
            _musicADS.Play();
            return;
        }

        _musicADS.Stop();
    }

    public void WrongAsnwerSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.wrongAnswer);

    public void CorrectAnswerSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.correctAnswer);

    public void LoseSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.lose);

    public void ChangeClothSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.changeCloth);

    public void StartButtonSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.startBtn);

    public void CancelSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.cancel);

    public void ConfirmSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.confirm);

    public void SettingCloseSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.settingClose);

    public void SettingPopUpSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.settingPopUp);

    public void ErrorSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.error);

    public void QuestionNotificationSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.questionNotification);

    public void TimeOverSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.timeOver);

    public void EndGameSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.endGame);
    public void EndPanelSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.endPanel);

    public void FallingSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.falling);

    public void SharkAttakSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.sharkAttack);

    public void DeleteKeySFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.deleteKey);

    public void ClickButtonSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.buttons[Random.Range(0, GameManager.Instance.SFXBank.buttons.Length)]);

    public void BlockFallSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.blockFalls[Random.Range(0, GameManager.Instance.SFXBank.blockFalls.Length)]);

    public void KeyboardSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.keyboards[Random.Range(0, GameManager.Instance.SFXBank.keyboards.Length)]);

    public void WaterFallSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.waterFalls[Random.Range(0, GameManager.Instance.SFXBank.waterFalls.Length)], 0.7f);

    public void AnswerSendSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.answerSends[Random.Range(0, GameManager.Instance.SFXBank.answerSends.Length)]);

    public void MoneyDecreaseSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.moneyDecreases[Random.Range(0, GameManager.Instance.SFXBank.moneyDecreases.Length)], 0.5f);

    public void BoosterSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.boosters[Random.Range(0, GameManager.Instance.SFXBank.boosters.Length)]);

    public void BlockRiseSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.blockRises[Random.Range(0, GameManager.Instance.SFXBank.blockRises.Length)]);

    public void SwimSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.swims[Random.Range(0, GameManager.Instance.SFXBank.swims.Length)]);

    public void LaughSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.laughs[Random.Range(0, GameManager.Instance.SFXBank.laughs.Length)]);

    public void FearSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.fears[Random.Range(0, GameManager.Instance.SFXBank.fears.Length)]);

    public void MaleScreamSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.maleScream, 0.6f);

    public void FemaleScreamSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.femaleScream, 0.6f);

    public void FlipCardSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.cardFlip);

    public void RewardsScreenIntroSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.rewardsScreenIntro);

    public void PirateShipEntranceSFX() => CustomPlayOneShot(GameManager.Instance.SFXBank.pirateShipEntrance);

    public void CustomPlayOneShot(AudioClip audioClip, float volume = 1)
    {
        if (!audioClip)
            return;

        if (logCallers)
            UnityEngine.Debug.Log(new StackFrame(1).GetMethod().Name);

        _sfxADS?.PlayOneShot(audioClip, volume);
    }
}