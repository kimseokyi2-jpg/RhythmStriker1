using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Beat")]
    public float bpm = 120f;
    public float judgeWindow = 0.15f; // 박자와 이 시간(초) 이내로 맞추면 성공 판정
    public float animationReferenceBpm = 120f; // 지금 애니메이션 속도가 자연스러운 기준 bpm

    [Header("Idle Bob (박자마다 들썩임)")]
    public Transform character;      // 반응할 캐릭터
    public float bobScale = 0.08f;   // 리듬 탈 때 커지는 정도 (작을수록 은은함)

    [Header("Hit / Miss 반응")]
    public ParticleSystem hitEffect;  // 성공 시 이펙트 (비워두면 생략)

    private Animator characterAnimator;
    private float beatInterval;
    private float songTime = 0f;
    private float lastBeatTime = 0f;
    private Vector3 characterOriginalScale;
    private int combo = 0;

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
    }

    void Update()
    {
        songTime += Time.deltaTime;

        if (songTime - lastBeatTime >= beatInterval)
            lastBeatTime += beatInterval;

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

        if (hitEffect != null)
            hitEffect.Play();

        if (characterAnimator != null)
            characterAnimator.SetTrigger("shoot");
    }

    void Miss()
    {
        combo = 0;

        if (characterAnimator != null)
            characterAnimator.SetTrigger("miss");
    }
}
