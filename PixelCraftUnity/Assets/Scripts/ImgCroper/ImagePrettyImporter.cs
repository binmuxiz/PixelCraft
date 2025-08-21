using Cysharp.Threading.Tasks;
using SFB;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEngine.UI.Image;
using System.Windows.Forms;

public class ImagePrettyImporter : MonoBehaviour
{
    // 불러온 이미지 사이즈 체크하기
    // 600x1000 사이즈의 이미지를 권장하기

    [Header("크롭매니저")]
    public GameObject cropmanager;
    public Img_Crop imgcropper;

    public async UniTask<Texture2D> ImageMakeTurminal(Texture2D tex) //이미지 사이즈체커로 가져오고, 가로 길이 확인해서 처리하기
    {
        Texture2D thoustex = tex;
        if (tex.height != 1000) thoustex = ImageSizeCheck(tex); //1000이 아니면 비율 맞춰 1000 만들기

        if (thoustex.width > 600)
        {
            //크롭창 UI에 이미지 할당 먼저 하고. . .
            imgcropper.MakeCropUI(thoustex);

            cropmanager.SetActive(true); //이거 dotween 추가할 생각하기

            await UniTask.WaitUntil(() => imgcropper.iscroped); //수동 크롭이 끝날때까지 기다리
            imgcropper.iscroped = false;

            Texture2D resulttex = Data_Manager.dtm.user_cropped_photo;
            return resulttex; //아웃풋은 다 하도록해
        }
        else if (thoustex.width < 600)
        {
            //이미지 늘리기 로직
            Texture2D resulttex = LongTexMaker(thoustex);
            Data_Manager.dtm.user_cropped_photo = resulttex;
            return resulttex;
        }
        else
        {
            //황금비율 황밸
            Texture2D resulttex = thoustex;
            Data_Manager.dtm.user_cropped_photo = resulttex;
            return resulttex ;
        }
    }

    Texture2D ImageSizeCheck(Texture2D tex) //비율 맞춰 세로 1000으로 만들기
    {
        float aspect = (float)tex.width / tex.height;
        print("텍스쳐 가로 사이즈 :" + tex.width);
        print("텍스쳐 세로 사이즈 :" + tex.height);
        print("텍스쳐 비율 . . . " + aspect);

        int newHeight = 1000;
        int newWidth = Mathf.RoundToInt(newHeight * aspect); //가로 사이즈 자동 조정

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(tex, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D newTex = new Texture2D(newWidth, newHeight, tex.format, false);
        newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTex.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return newTex; // 세로 1000, 가로는 비율이 맞춰진 이미지
    }


    Texture2D LongTexMaker(Texture2D tex)
    {
        int width = tex.width;
        int height = tex.height;
        int minWidth = 600;

        // 새 Texture2D 생성 (흰색 배경)
        Texture2D newTex = new Texture2D(minWidth, height, tex.format, false);
        Color32[] whitePixels = new Color32[minWidth * height];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;
        newTex.SetPixels32(whitePixels);

        // 원본 이미지 픽셀을 왼쪽에 복사
        Color32[] originalPixels = tex.GetPixels32();
        for (int y = 0; y < height; y++)
        {
            int srcIndex = y * width;
            int dstIndex = y * minWidth;
            for (int x = 0; x < width; x++)
            {
                newTex.SetPixel(x, y, originalPixels[srcIndex + x]);
            }
        }
        newTex.Apply();
        return newTex;
    }
}
