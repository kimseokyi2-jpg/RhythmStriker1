using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    public Transform targetPoint;   // 골대 안쪽 목표 지점
    public float flightDuration = 0.6f;
    public GoalNet goalNet;             // 도착 시 출렁일 그물 (비워두면 생략)
    public ParticleSystem launchEffect; // 차는 순간 재생할 이펙트 (비워두면 생략)
    public ParticleSystem arrivalEffect; // 도착 시 재생할 이펙트 (비워두면 생략)

    [Header("화려함")]
    public float arcHeight = 1.5f;     // 포물선으로 솟아오르는 높이
    public float spinSpeed = 720f;     // 초당 회전 각도

    [Header("골대 안 랜덤 위치")]
    public float targetOffsetLeft = 1.5f;  // GoalTarget 기준 왼쪽으로 최대 얼마나
    public float targetOffsetRight = 1.5f; // GoalTarget 기준 오른쪽으로 최대 얼마나
    public float targetSpreadY = 1f;       // 위로 흔들리는 범위 (0~이 값 사이)

    [Header("박자마다 살포시 던져 넣기")]
    public Camera referenceCamera;   // 화면 오른쪽 방향 기준 카메라 (비우면 Camera.main)
    public float rollInDistance = 5f; // 화면 밖 오른쪽에서 얼마나 멀리서 출발할지
    public float tossArcHeight = 0.8f; // 부드럽게 솟았다 내려오는 높이
    public float rollSpinSpeed = 150f;

    [Header("미스했을 때 빗나가기")]
    public float missDistance = 4f;   // 왼쪽으로 빗나가는 거리
    public float missDuration = 0.4f;
    public float missArcHeight = 0.5f;

    public bool IsBusy => shootRoutine != null;
    public Vector3 PlannedTarget { get; private set; } // 이번 슛이 향할 지점 (골키퍼 반응용)

    private Vector3 startPosition;
    private Coroutine shootRoutine;
    private Coroutine rollRoutine;
    private Rigidbody rb;

    void Awake()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true; // 날아가는 동안은 물리 끄고 코드로만 제어

        if (referenceCamera == null)
            referenceCamera = Camera.main;
    }

    // 박자 시작 시 GameManager가 호출: 화면 밖 오른쪽에서 대기 위치까지 굴러오게 함
    public void RollIn(float duration)
    {
        if (IsBusy) return; // 슛/바운스 중이면 굴러오는 연출 생략
        if (rollRoutine != null) StopCoroutine(rollRoutine);
        rollRoutine = StartCoroutine(RollInRoutine(duration));
    }

    IEnumerator RollInRoutine(float duration)
    {
        Vector3 rightDir = referenceCamera != null ? referenceCamera.transform.right : Vector3.right;
        Vector3 from = startPosition + rightDir * rollInDistance;
        Vector3 rollAxis = Vector3.Cross(Vector3.up, rightDir);

        transform.position = from;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float eased = 1f - (1f - p) * (1f - p); // 도착할수록 천천히, 살포시 놓이는 느낌

            Vector3 flat = Vector3.Lerp(from, startPosition, eased);
            float arc = Mathf.Sin(p * Mathf.PI) * tossArcHeight; // 부드러운 포물선
            transform.position = flat + Vector3.up * arc;

            transform.Rotate(rollAxis, rollSpinSpeed * Time.deltaTime, Space.World);
            yield return null;
        }
        transform.position = startPosition;
        rollRoutine = null;
    }

    public void Shoot()
    {
        if (rollRoutine != null) { StopCoroutine(rollRoutine); rollRoutine = null; }
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        transform.position = startPosition;

        if (targetPoint != null)
        {
            Vector3 rightDir = referenceCamera != null ? referenceCamera.transform.right : Vector3.right;
            Vector3 randomOffset = rightDir * Random.Range(-targetOffsetLeft, targetOffsetRight)
                                  + Vector3.up * Random.Range(0f, targetSpreadY);
            PlannedTarget = targetPoint.position + randomOffset;
        }
        else
        {
            PlannedTarget = startPosition + Vector3.forward * 10f;
        }

        shootRoutine = StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        Vector3 to = PlannedTarget;

        if (launchEffect != null)
            launchEffect.Play();

        float t = 0f;
        while (t < flightDuration)
        {
            t += Time.deltaTime;
            float p = t / flightDuration;

            Vector3 flatPos = Vector3.Lerp(startPosition, to, p);
            float arc = Mathf.Sin(p * Mathf.PI) * arcHeight; // 포물선: 중간에 최고점
            transform.position = flatPos + Vector3.up * arc;

            transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.World);

            yield return null;
        }
        transform.position = to;

        if (goalNet != null)
            goalNet.Ripple(to);

        if (arrivalEffect != null)
            arrivalEffect.Play();

        // 그물 맞고 나서 물리엔진에 맡겨서 통통 튀게 함 (이후로도 계속 물리 상태로 골대 근처에 남음)
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        shootRoutine = null; // 이 공은 여기서 임무 끝 - 이후 GameManager가 재사용하지 않음
    }

    // 미스했을 때: 놓친 느낌으로 왼쪽으로 빗나가서 뒹굴게 함
    public void Fumble()
    {
        if (rollRoutine != null) { StopCoroutine(rollRoutine); rollRoutine = null; }
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        shootRoutine = StartCoroutine(FumbleRoutine());
    }

    IEnumerator FumbleRoutine()
    {
        Vector3 rightDir = referenceCamera != null ? referenceCamera.transform.right : Vector3.right;
        Vector3 from = transform.position;
        Vector3 to = from - rightDir * missDistance + Vector3.forward * (missDistance * 0.3f);

        float t = 0f;
        while (t < missDuration)
        {
            t += Time.deltaTime;
            float p = t / missDuration;

            Vector3 flatPos = Vector3.Lerp(from, to, p);
            float arc = Mathf.Sin(p * Mathf.PI) * missArcHeight;
            transform.position = flatPos + Vector3.up * arc;

            transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.World);

            yield return null;
        }
        transform.position = to;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        shootRoutine = null;
    }
}
