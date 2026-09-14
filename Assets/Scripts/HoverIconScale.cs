using UnityEngine;
using UnityEngine.EventSystems;

// 버튼 위에 마우스를 올리고 있는 동안 지정한 아이콘이 커졌다가,
// 벗어나면 원래 크기로 돌아온다.
public class HoverIconScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform icon;
    public float hoverScale = 1.3f;
    public float speed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        if (icon != null)
        {
            originalScale = icon.localScale;
            targetScale = originalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    void Update()
    {
        if (icon == null) return;
        icon.localScale = Vector3.Lerp(icon.localScale, targetScale, Time.deltaTime * speed);
    }
}
