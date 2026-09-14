using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("패널")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject startPromptPanel; // "스페이스바를 눌러 시작"
    public GameObject logo; // 인트로 화면 로고 (게임 시작 전까지만 표시)

    [Header("연결")]
    public GameManager gameManager;
    public AudioSource musicSource; // 설정 창에서 음량 조절할 음악 소스 (비워두면 생략)

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

        if (gameManager != null)
            gameManager.enabled = true;
    }
}
