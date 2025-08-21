using DG.Tweening;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public Transform targetTransform;
    public float floatDistance = 1f;
    public float moveDuration = 1f;
    public float pauseDuration = 0.5f; // 양 끝에서 대기 시간
    public bool startDownward = false;

    void Start()
    {
        if (targetTransform == null)
            targetTransform = transform;

        Vector3 originalPos = targetTransform.position;
        float startY = startDownward ?
            originalPos.y - floatDistance :
            originalPos.y + floatDistance;

        // 초기 위치 설정 (방향에 따라 시작점 변경)
        targetTransform.position = new Vector3(
            originalPos.x,
            startY,
            originalPos.z
        );

        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();

        // 이동1 → 대기 → 이동2 → 대기 (무한 반복)
        seq.Append(targetTransform.DOMoveY(originalPos.y, moveDuration)
                  .SetEase(Ease.InOutSine))
           .AppendInterval(pauseDuration)
           .Append(targetTransform.DOMoveY(startY, moveDuration)
                  .SetEase(Ease.InOutSine))
           .AppendInterval(pauseDuration)
           .SetLoops(-1);
    }
}
