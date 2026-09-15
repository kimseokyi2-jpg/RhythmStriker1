using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Beat")]
    public float bpm = 120f;
    public float judgeWindow = 0.15f; // 박자와 이 시간(초) 이내로 맞추면 성공 판정
    public float animationReferenceBpm = 120f; // 지금 애니메이션 속도가 자연스러운 기준 bpm

    [Header("Idle Bob (박자마다 들썩임)")]
    public Transform character;      // 반응할 캐릭터
    public float bobScale = 0.08f;   // 리듬 탈 때 커지는 정도 (작을수록 은은함)
    public Transform goalkeeper;         // 준비 자세로 같이 들썩일 골키퍼 (비워두면 생략)
    public float goalkeeperBobScale = 0.05f;
    public GoalkeeperController goalkeeperController; // 슛 방향으로 다이빙 반응 (비워두면 생략)

    [Header("Hit / Miss 반응")]
    public ParticleSystem hitEffect;  // 성공 판정 순간 이펙트 (비워두면 생략)
    public Camera reactionCamera;     // 성공 시 살짝 펀치감을 줄 카메라 (비우면 Camera.main)
    public float cameraPunchDistance = 0.15f;
    public float cameraPunchDuration = 0.12f;

    [Header("점수")]
    public int scorePerHit = 100;
    public int comboBonusPerHit = 10; // 콤보 1당 추가 점수
    public TextMeshProUGUI scoreText; // 점수판 이미지 위에 겹쳐 표시할 실제 점수 텍스트
    public TextMeshProUGUI comboText;

    [Header("게임 오버")]
    public int missLimit = 3;               // 연속 미스 허용 횟수
    public float gameOverDisplayDuration = 3f;
    public MenuManager menuManager;         // 게임오버 후 메인 메뉴로 돌려보낼 대상
    public GameObject gameOverPanel;        // 게임오버 이미지 UI (평소엔 꺼둠)
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestComboText;

    [Header("공")]
    public GameObject ballPrefab;     // 박자마다 새로 생성할 공 프리팹
    public Transform ballSpawnPoint;  // 공이 나타날 준비 위치 (캐릭터 발 앞)
    public Transform ballTargetPoint; // 골대 안쪽 목표 지점 (씬 오브젝트라 프리팹에 저장 안 되므로 여기서 연결)
    public GoalNet ballGoalNet;
    public ParticleSystem ballLaunchEffect;
    public ParticleSystem ballArrivalEffect;

    private Animator characterAnimator;
    private float beatInterval;
    private float songTime = 0f;
    private float lastBeatTime = 0f;
    private Vector3 characterOriginalScale;
    private Vector3 goalkeeperOriginalScale;
    private int combo = 0;
    private int score = 0;
    private int maxCombo = 0;
    private BallController currentBall; // 지금 대기 중인, 아직 안 찬 공
    private Vector3 cameraOriginalPosition;
    private Coroutine cameraPunchRoutine;
    private int missStreak = 0;
    private bool isGameOver = false;

    void OnEnable()
    {
        // 새 판이 시작될 때마다(메뉴에서 재활성화될 때마다) 상태 초기화
        score = 0;
        combo = 0;
        maxCombo = 0;
        missStreak = 0;
        isGameOver = false;
        songTime = 0f;
        lastBeatTime = 0f;
        UpdateHud();
    }

    void UpdateHud()
    {
        if (scoreText != null) scoreText.text = score.ToString();
        if (comboText != null) comboText.text = combo.ToString();
    }

    void Start()
    {
        beatInterval = 60f / bpm;

        if (character != null)
        {
            characterOriginalScale = character.localScale;
            characterAnimator = character.GetComponent<Animator>();

            if (characterAnimator != null)
                characterAnimator.speed = bpm / animationReferenceBpm;
        }

        if (goalkeeper != null)
            goalkeeperOriginalScale = goalkeeper.localScale;

        if (reactionCamera == null)
            reactionCamera = Camera.main;
        if (reactionCamera != null)
            cameraOriginalPosition = reactionCamera.transform.localPosition;
    }

    void Update()
    {
        if (isGameOver) return;

        songTime += Time.deltaTime;

        if (songTime - lastBeatTime >= beatInterval)
        {
            lastBeatTime += beatInterval;

            // 지난 박자 동안 아무 반응 없이 그냥 지나친 공은 자동으로 미스 처리
            if (currentBall != null)
            {
                currentBall.Fumble();
                currentBall = null;
                RegisterMiss();
            }

            if (isGameOver) return;

            SpawnBallIfNeeded();

            if (currentBall != null)
                currentBall.RollIn(beatInterval);
        }

        Bob();

        if (Input.GetKeyDown(KeyCode.Space))
            TryShoot();
    }

    // 박자 안에서 사인파로 부드럽게 커졌다 작아지며 리듬을 타는 느낌을 줌
    void Bob()
    {
        if (character == null) return;

        float beatPhase = (songTime - lastBeatTime) / beatInterval; // 0~1
        float bounce = Mathf.Sin(beatPhase * Mathf.PI); // 박자 중간에 최대, 시작/끝에 0
        character.localScale = characterOriginalScale * (1f + bobScale * bounce);

        if (goalkeeper != null)
            goalkeeper.localScale = goalkeeperOriginalScale * (1f + goalkeeperBobScale * bounce);
    }

    void SpawnBallIfNeeded()
    {
        if (currentBall != null) return; // 아직 안 찬 공이 있으면 새로 안 만듦
        if (ballPrefab == null || ballSpawnPoint == null) return;

        GameObject instance = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        currentBall = instance.GetComponent<BallController>();

        if (currentBall != null)
        {
            currentBall.targetPoint = ballTargetPoint;
            currentBall.goalNet = ballGoalNet;
            currentBall.launchEffect = ballLaunchEffect;
            currentBall.arrivalEffect = ballArrivalEffect;
        }
    }

    void TryShoot()
    {
        float distFromLastBeat = songTime - lastBeatTime;
        float diff = Mathf.Min(distFromLastBeat, beatInterval - distFromLastBeat);

        if (diff <= judgeWindow)
            Hit();
        else
            Miss();
    }

    void Hit()
    {
        combo++;
        if (combo > maxCombo) maxCombo = combo;
        score += scorePerHit + comboBonusPerHit * (combo - 1);
        missStreak = 0;

        if (hitEffect != null)
            hitEffect.Play();

        if (characterAnimator != null)
            characterAnimator.SetTrigger("shoot");

        if (reactionCamera != null)
        {
            if (cameraPunchRoutine != null) StopCoroutine(cameraPunchRoutine);
            cameraPunchRoutine = StartCoroutine(CameraPunch());
        }

        UpdateHud();
    }

    IEnumerator CameraPunch()
    {
        Vector3 punchPos = cameraOriginalPosition + Vector3.back * cameraPunchDistance;
        float t = 0f;
        while (t < cameraPunchDuration)
        {
            t += Time.deltaTime;
            float p = t / cameraPunchDuration;
            reactionCamera.transform.localPosition = Vector3.Lerp(punchPos, cameraOriginalPosition, p);
            yield return null;
        }
        reactionCamera.transform.localPosition = cameraOriginalPosition;
        cameraPunchRoutine = null;
    }

    // 슛 애니메이션의 "발이 공에 닿는 프레임"에 Animation Event로 이 메서드를 호출
    public void LaunchBall()
    {
        if (currentBall == null) return;

        currentBall.Shoot();

        if (goalkeeperController != null)
            goalkeeperController.React(currentBall.PlannedTarget);

        currentBall = null; // 이 공은 골대 쪽에 남고, 다음 박자에 새 공이 생김
    }

    void Miss()
    {
        if (characterAnimator != null)
            characterAnimator.SetTrigger("miss");

        if (currentBall != null)
        {
            currentBall.Fumble();
            currentBall = null; // 이 공도 빗나간 채로 남고, 다음 박자에 새 공이 생김
        }

        RegisterMiss();
    }

    // 미스(수동/자동) 공통 처리: 콤보 끊기, 연속 미스 카운트, 한도 도달 시 게임오버
    void RegisterMiss()
    {
        combo = 0;
        missStreak++;
        UpdateHud();

        if (missStreak >= missLimit)
            TriggerGameOver();
    }

    void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = score.ToString();
        if (bestComboText != null) bestComboText.text = maxCombo.ToString();

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDisplayDuration);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (menuManager != null)
            menuManager.ShowMainMenu();

        enabled = false;
    }
}
