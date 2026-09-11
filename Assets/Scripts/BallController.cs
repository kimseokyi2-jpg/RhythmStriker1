using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    public Transform targetPoint;   // 골대 안쪽 목표 지점
    public float flightDuration = 0.6f;
    public float resetDelay = 0.5f;
    public GoalNet goalNet;             // 도착 시 출렁일 그물 (비워두면 생략)
    public ParticleSystem arrivalEffect; // 도착 시 재생할 이펙트 (비워두면 생략)

    [Header("화려함")]
    public float arcHeight = 1.5f;     // 포물선으로 솟아오르는 높이
    public float spinSpeed = 720f;     // 초당 회전 각도

    private Vector3 startPosition;
    private Coroutine shootRoutine;

    void Awake()
    {
        startPosition = transform.position;
    }

    public void Shoot()
    {
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        shootRoutine = StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        Vector3 to = targetPoint != null ? targetPoint.position : startPosition + Vector3.forward * 10f;

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

        yield return new WaitForSeconds(resetDelay);
        transform.position = startPosition;
        shootRoutine = null;
    }
}
