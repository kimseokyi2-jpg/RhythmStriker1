using UnityEngine;
using TMPro; // TextMeshPro 기능을 쓰기 위해 추가

public class InputTester : MonoBehaviour
{
    public float targetTime = 3f;
    public float startScale = 3f;
    public float endScale = 1f;
    public TextMeshProUGUI judgeText; // Inspector에서 연결할 UI 텍스트

    private float timer = 0f;
    private bool judged = false;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        judgeText.text = ""; // 시작할 땐 빈 텍스트
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / targetTime);
        float currentScaleFactor = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = originalScale * currentScaleFactor;

        if (Input.GetKeyDown(KeyCode.Space) && !judged)
        {
            judged = true;
            float diff = Mathf.Abs(timer - targetTime);

            if (diff <= 0.1f)
                judgeText.text = "PERFECT!";
            else if (diff <= 0.3f)
                judgeText.text = "GOOD";
            else
                judgeText.text = "MISS";
        }
    }
}