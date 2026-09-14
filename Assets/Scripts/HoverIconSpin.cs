using UnityEngine;
using UnityEngine.EventSystems;

// 버튼(또는 이 컴포넌트가 붙은 UI 오브젝트) 위에 마우스를 올리고 있는 동안
// 지정한 아이콘을 계속 회전시킨다.
public class HoverIconSpin : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform icon;
    public float spinSpeed = 180f; // 초당 회전 각도

    private bool hovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    void Update()
    {
        if (hovering && icon != null)
            icon.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }
}
