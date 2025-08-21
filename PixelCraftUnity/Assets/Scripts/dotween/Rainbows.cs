using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Rainbows : MonoBehaviour
{
    public Graphic targetUI; // Image 또는 RawImage 등 Color 속성이 있는 UI 컴포넌트
    public float speed = 1f; // 색상 변화 속도

    void Start()
    {
        RainbowEffectAsync().Forget();
    }

    private async UniTaskVoid RainbowEffectAsync()
    {
        float hue = 0f;
        while (true)
        {
            hue += Time.deltaTime * speed;
            if (hue > 1f) hue -= 1f;

            // HSV(H, S, V) → RGB 변환, S=1, V=1이면 선명한 무지개
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
            targetUI.color = rainbowColor;

            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }
}
