using UnityEngine;
using UnityEngine.UI;

public class Img_Crop : MonoBehaviour
{
    //예쁘게 임포트해온 이미지를 크롭하기
    public RawImage originalImage;
    public RectTransform cropFrame;

    private Vector2 lastMousePosition;

    public bool iscroped= false;

    void Update()
    {
        HandleFrameDrag(); //프레임은 계속 움직일 수 있도록
    }

    void HandleFrameDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            cropFrame.anchoredPosition += delta;
            lastMousePosition = Input.mousePosition;

            // 프레임이 이미지 밖으로 나가지 않도록 제한
            ClampFramePosition();
        }
    }

    void ClampFramePosition()
    {
        Vector2 framePos = cropFrame.anchoredPosition;
        Vector2 imageSize = originalImage.rectTransform.rect.size;
        Vector2 frameSize = cropFrame.rect.size;

        framePos.x = Mathf.Clamp(framePos.x, -imageSize.x / 2 + frameSize.x / 2, imageSize.x / 2 - frameSize.x / 2);
        framePos.y = Mathf.Clamp(framePos.y, -imageSize.y / 2 + frameSize.y / 2, imageSize.y / 2 - frameSize.y / 2);

        cropFrame.anchoredPosition = framePos;
    }


    public void CropImage()
    {
        Texture2D originalTexture = originalImage.texture as Texture2D;
        if (originalTexture == null) return;

        // 원본 텍스처를 읽기 가능한 새 텍스처로 복사
        Texture2D readableTexture = CreateReadableTexture(originalTexture);

        // 크롭 영역 계산
        Vector2 cropPosition = (cropFrame.anchoredPosition + originalImage.rectTransform.rect.size / 2) / originalImage.rectTransform.rect.size;
        Vector2 cropSize = cropFrame.rect.size / originalImage.rectTransform.rect.size;

        int x = Mathf.FloorToInt(cropPosition.x * readableTexture.width - (cropSize.x * readableTexture.width / 2));
        int y = Mathf.FloorToInt(cropPosition.y * readableTexture.height - (cropSize.y * readableTexture.height / 2));
        int width = Mathf.FloorToInt(cropSize.x * readableTexture.width);
        int height = Mathf.FloorToInt(cropSize.y * readableTexture.height);

        // 텍스처 크롭
        Color[] pixels = readableTexture.GetPixels(x, y, width, height);
        Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        //크롭된 이미지 크기 바꾸고
        //Texture2D result = new Texture2D(700, 700, TextureFormat.RGB24, false);
        //Graphics.ConvertTexture(croppedTexture, result);

        print("Cropped!");
        gameObject.SetActive(false); //게임오브제 꺼주고

        //가야 할 곳에 보내주기
        Data_Manager.dtm.user_cropped_photo = croppedTexture;
        iscroped = true;
    }


    //크롭 UI에 이미지 집어넣기
    public void MakeCropUI(Texture2D tex)
    {
        RectTransform rt = originalImage.rectTransform;
        rt.sizeDelta = new Vector2 (tex.width, rt.sizeDelta.y); //사이즈 새로 부여하긔
        originalImage.texture = tex;

    }


    #region 읽기 설정 변경
    private Texture2D CreateReadableTexture(Texture2D source)
    {
        // RenderTexture 생성 시 sRGB 읽기/쓰기를 활성화
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);  // sRGB 읽기/쓰기를 활성화

        // 원본 텍스처를 RenderTexture로 블릿(Blit)하여 복사
        Graphics.Blit(source, renderTex);

        // 이전 활성 RenderTexture 저장
        RenderTexture previous = RenderTexture.active;

        // 현재 활성 RenderTexture 설정
        RenderTexture.active = renderTex;

        // 새로운 Texture2D 생성 및 읽기
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();

        // 이전 활성 RenderTexture 복원
        RenderTexture.active = previous;

        // 임시 RenderTexture 해제
        RenderTexture.ReleaseTemporary(renderTex);

        return readableText;
    }
    #endregion

}
