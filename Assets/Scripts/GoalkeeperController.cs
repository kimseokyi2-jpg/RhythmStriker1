using UnityEngine;

// 슛이 향하는 지점을 보고 왼쪽/오른쪽 다이빙 또는 (정면 높은 곳) 못 막는 반응을 재생
public class GoalkeeperController : MonoBehaviour
{
    public Camera referenceCamera;      // 좌우/높이 판단 기준 카메라 (비우면 Camera.main)
    public float centerThreshold = 0.5f; // 이 폭 안쪽이면 "정면"으로 간주
    public float highThreshold = 0.6f;   // 정면인데 이보다 높으면 "못 막는" 코스로 간주
    public float maxRange = 1.5f;        // 원래 자리에서 이 거리 밖으로는 절대 못 나감

    private Animator animator;
    private Vector3 homePosition;
    private Quaternion homeRotation;

    void Awake()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        animator = GetComponent<Animator>();

        if (referenceCamera == null)
            referenceCamera = Camera.main;
    }

    public void React(Vector3 ballTarget)
    {
        if (animator == null) return;

        Vector3 rightDir = referenceCamera != null ? referenceCamera.transform.right : Vector3.right;
        float horizontal = Vector3.Dot(ballTarget - homePosition, rightDir); // 화면 기준 왼쪽 음수 / 오른쪽 양수
        float vertical = ballTarget.y - homePosition.y;

        if (Mathf.Abs(horizontal) < centerThreshold && vertical > highThreshold)
            animator.SetTrigger("cantSave");
        else if (horizontal < 0f)
            animator.SetTrigger("diveRight"); // 캐릭터가 카메라를 보고 있어 좌우가 뒤집힘
        else
            animator.SetTrigger("diveLeft");
    }

    // 애니메이션(루트 모션)이 아무리 움직여도, 좌우로만 정해진 범위 안에서 움직이게 하고
    // 앞뒤/높이/회전은 원래 값으로 고정
    void LateUpdate()
    {
        Vector3 rightDir = referenceCamera != null ? referenceCamera.transform.right : Vector3.right;
        Vector3 offset = transform.position - homePosition;
        float sideAmount = Mathf.Clamp(Vector3.Dot(offset, rightDir), -maxRange, maxRange);

        transform.position = homePosition + rightDir * sideAmount;
        transform.rotation = homeRotation;
    }
}
