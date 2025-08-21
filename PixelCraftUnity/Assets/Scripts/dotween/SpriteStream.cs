using UnityEngine;
using UnityEngine.UI;

public class SpriteStream : MonoBehaviour
{
    public RawImage rawImage;
    public float scrollSpeed = 0.5f;

    void Update()
    {
        Rect uvRect = rawImage.uvRect;
        uvRect.y += scrollSpeed * Time.deltaTime;
        uvRect.x += -scrollSpeed * Time.deltaTime;
        rawImage.uvRect = uvRect;
    }
}
