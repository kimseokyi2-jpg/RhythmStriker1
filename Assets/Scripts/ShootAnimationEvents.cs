using UnityEngine;

// 캐릭터의 Animator가 붙은 오브젝트에 넣는 스크립트.
// 슛 애니메이션 클립에 Animation Event로 OnKickContact를 등록해두면,
// 발이 공에 닿는 정확한 프레임에 GameManager로 신호를 넘겨준다.
public class ShootAnimationEvents : MonoBehaviour
{
    public GameManager gameManager;

    public void OnKickContact()
    {
        if (gameManager != null)
            gameManager.LaunchBall();
    }
}
