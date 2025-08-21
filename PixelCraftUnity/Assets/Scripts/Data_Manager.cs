using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager dtm;

    public Texture2D userinputphoto; //유저가 보내는 사진
    public Texture2D user_cropped_photo; //자른 사진


    [Header("테스트용")]
    public Texture2D testOutputSprite; //테스트용 이미지 받는 변수

    [Header("1차 Sprite")]
    public Texture2D[] sprites;
    public Texture2D selected_spr;

    [Header("2차 SPrite")]
    public Texture2D walkFrame;
    public Texture2D runFrame;
    public Texture2D[] danceFrames;

    [Header("Split Frame")]
    public Sprite[] walk;
    public Sprite[] run;
    public Sprite[] dance;

    void Awake()
    {
        if (dtm == null) dtm = this;
        else Destroy(gameObject);
        //싱글턴

        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad상에 HttpManager 생성 :: input data 저장소");
    }

    public Texture2D ResizeTexture(Texture2D source, int width, int hight)
    {
        RenderTexture rt = new RenderTexture(width, hight, 0);
        Graphics.Blit(source, rt);

        Texture2D resizetex = new Texture2D(width, hight, source.format, false);
        resizetex.ReadPixels(new Rect(0, 0, width, hight), 0, 0);
        resizetex.Apply();

        RenderTexture.active = null;
        rt.Release();

        return resizetex;
    }

}
