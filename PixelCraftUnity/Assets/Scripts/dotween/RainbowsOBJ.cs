using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RainbowsOBJ : MonoBehaviour
{
    public SpriteRenderer target; // ✅ Sprite 대신 SpriteRenderer 참조
    public float speed = 1f;

    [Header("색깔놀이")]
    [Range(0.0f, 1f)]
    public float sar;
    [Range(0.0f, 1f)]
    public float val;

    private void Start()
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

            Color rainbowColor = Color.HSVToRGB(hue, sar, val);
            rainbowColor.a = 0.3f;
            target.color = rainbowColor; // ✅ SpriteRenderer의 color 수정

            await UniTask.Yield();
        }
    }
}
