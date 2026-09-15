using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("패널")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject startPromptPanel; // "스페이스바를 눌러 시작"
    public GameObject logo; // 인트로 화면 로고 (게임 시작 전까지만 표시)
    public GameObject scoreboardPanel; // 스코어/콤보 UI (게임 시작 후에만 표시)

    [Header("연결")]
    public GameManager gameManager;
    public AudioSource musicSource; // 설정 창에서 음량 조절할 음악 소스 (비워두면 생략)

    [Header("UI 효과음")]
    public AudioSource uiAudioSource; // 클릭음 재생용 (비워두면 자동으로 하나 추가)
    public AudioClip clickSound;

    [Header("로비 배경음 (삼바 + 관중 환호, 동시 재생)")]
    public AudioClip lobbyMusicClip;
    public AudioClip lobbyCheerClip;
    private AudioSource cheerSource;

    void Awake()
    {
        if (uiAudioSource == null)
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.clip = lobbyMusicClip;

        cheerSource = gameObject.AddComponent<AudioSource>();
        cheerSource.playOnAwake = false;
        cheerSource.loop = true;
        cheerSource.clip = lobbyCheerClip;
    }

    // 버튼 OnClick에 연결해서 클릭음 재생
    public void PlayClickSound()
    {
        if (clickSound != null && uiAudioSource != null)
            uiAudioSource.PlayOneShot(clickSound);
    }

    void PlayLobbyAmbience()
    {
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
            musicSource.Play();
        if (cheerSource != null && cheerSource.clip != null && !cheerSource.isPlaying)
            cheerSource.Play();
    }

    void StopLobbyAmbience()
    {
        if (musicSource != null) musicSource.Stop();
        if (cheerSource != null) cheerSource.Stop();
    }

    private bool waitingForStartInput = false;

    void Start()
    {
        if (gameManager != null)
            gameManager.enabled = false; // 시작 전엔 게임 로직 정지

        ShowMainMenu();
    }

    void Update()
    {
        if (waitingForStartInput && Input.GetKeyDown(KeyCode.Space))
            BeginGame();
    }

    public void ShowMainMenu()
    {
        waitingForStartInput = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (startPromptPanel != null) startPromptPanel.SetActive(false);
        if (logo != null) logo.SetActive(true);
        if (scoreboardPanel != null) scoreboardPanel.SetActive(false);
        PlayLobbyAmbience();
    }

    // "게임 시작" 버튼 OnClick에 연결
    public void OnStartGameClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startPromptPanel != null) startPromptPanel.SetActive(true);
        waitingForStartInput = true;
    }

    // "설정" 버튼 OnClick에 연결
    public void OnSettingsClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // 설정 창의 "뒤로" 버튼 OnClick에 연결
    public void OnSettingsBackClicked()
    {
        ShowMainMenu();
    }

    // 음량 슬라이더 OnValueChanged에 연결 (0~1)
    public void OnVolumeChanged(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;
        else
            AudioListener.volume = value;
    }

    // "종료" 버튼 OnClick에 연결
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서는 재생 모드 종료로 대체
#else
        Application.Quit();
#endif
    }

    void BeginGame()
    {
        waitingForStartInput = false;
        if (startPromptPanel != null) startPromptPanel.SetActive(false);
        if (logo != null) logo.SetActive(false);
        if (scoreboardPanel != null) scoreboardPanel.SetActive(true);
        StopLobbyAmbience();

        if (gameManager != null)
            gameManager.enabled = true;
    }
}
