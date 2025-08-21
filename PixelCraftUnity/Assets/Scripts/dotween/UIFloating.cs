using DG.Tweening;
using UnityEngine;

public class UIFloating : MonoBehaviour
{
    public RectTransform targetRect; // Inspector에서 할당하거나 GetComponent<RectTransform>()로 할당
    public float floatDistance = 30f; // 위아래 이동 거리
    public float duration = 1.2f;     // 한 번 오르내리는 데 걸리는 시간

    public bool isRevesed; //방향 뒤집기

    void Start()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        Vector2 originalPos = targetRect.anchoredPosition;

        if (isRevesed)
        {
            // 아래로 이동했다가 아래로 다시 돌아오는 트윈을 무한 반복
            targetRect.DOAnchorPosY(originalPos.y - floatDistance, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            // 위로 이동했다가 아래로 다시 돌아오는 트윈을 무한 반복
            targetRect.DOAnchorPosY(originalPos.y + floatDistance, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
