using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Moments.Encoder;
using System.Collections.Generic;
using UnityEngine.Rendering;





#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

public class imgSaver : MonoBehaviour
{
    public Texture2D[] savePixelList;

    public GameObject completeUI;

    public void ResetScene()
    {
        SceneManager.LoadScene(0);
    }


    public void SaveImage()
    {
        SaveTexture(Data_Manager.dtm.selected_spr, MakeName("Sprite"));
        SaveTexture(Data_Manager.dtm.walkFrame, MakeName("Walk"));
        SaveTexture(Data_Manager.dtm.runFrame, MakeName("Run"));

        int count = 1;
        foreach (Texture2D tex in Data_Manager.dtm.danceFrames)
        {
            SaveTexture(tex, MakeName("Run" + count.ToString()));
            count++;
        }
        completeUI.SetActive(true);
    }

    [Header("스프라이트 이미지 갖고오기")]
    public Sprite_spliter sprites;

    public void SaveGIF()
    {
        SaveTexture(Data_Manager.dtm.selected_spr, MakeName("Sprite"));
        //GIF로 저장하는 코드 작성하기
        ExportGif(MakeTex(sprites.walk), "walk");
        ExportGif(MakeTex(sprites.run), "run");
        ExportGif(Data_Manager.dtm.danceFrames, "dance");
    }

    Texture2D[] MakeTex(Sprite[] sprites)
    {
        Texture2D[] textures = new Texture2D[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            textures[i] = sprites[i].texture;
        }
        return textures;
    }


    string MakeName(string name)
    {
        string date = DateTime.Now.ToString("yyyyMMddHHmm");
        string names = name + date + ".png";
        return names;
    }



#if UNITY_WEBGL
    // WebGL 파일 저장 JS 플러그인
    [DllImport("__Internal")]
    private static extern void DownloadFile(string filename, byte[] byteArray, int byteArrayLength);
#endif

    public static void SaveTexture(Texture2D texture, string fileName)
    {
        byte[] pngData = texture.EncodeToPNG();

#if UNITY_EDITOR
        // 에디터 모드: Assets/myImages 폴더에 저장
        string folderPath = "Assets/myImages";
        CreateFolderIfNeeded(folderPath);
        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, pngData);
        AssetDatabase.Refresh();
        Debug.Log($"에디터 저장 완료: {fullPath}");

#elif UNITY_WEBGL
        // WebGL: 브라우저 다운로드 호출
        DownloadFile(fileName, pngData, pngData.Length);
        Debug.Log("WebGL 다운로드 시작");

#else
        // Windows 빌드: 내 문서/PixelCraft 폴더에 저장
        string myDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string folderPath = Path.Combine(myDocuments, "PixelCraft");
        CreateFolderIfNeeded(folderPath);
        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log($"Windows 저장 완료: {fullPath}");
#endif
    }

    private static void CreateFolderIfNeeded(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
#if UNITY_EDITOR
            if (path.StartsWith("Assets/"))
                AssetDatabase.Refresh(); // 에디터에서 폴더 생성 시 리프레시
#endif
        }
    }

    //GIF로 저장하기
    public void ExportGif(Texture2D[] textures, string fileName, float frameDelay = 0.1f)
    {
        // 1. 저장 경로 결정
        string savePath = GetSavePath(fileName);

        // 2. GifEncoder 준비
        GifEncoder encoder = new GifEncoder();
        encoder.SetDelay((int)(frameDelay * 1000)); // ms 단위

        // 3. 파일 스트림 열고 GIF 생성 시작
        using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        {
            encoder.Start(fileStream);

            // 4. 각 Texture2D를 GifFrame으로 변환해 추가
            foreach (Texture2D tex in textures)
            {
                GifFrame frame = new GifFrame();
                frame.Width = tex.width;
                frame.Height = tex.height;
                frame.Data = tex.GetPixels32();
                encoder.AddFrame(frame);
            }

            // 5. GIF 파일 완성
            encoder.Finish();
        }

        Debug.Log("GIF 저장 완료: " + savePath);
    }

    private string GetSavePath(string fileName)
    {
#if UNITY_EDITOR
        string folder = Path.Combine(Application.dataPath, "GIFs");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, fileName);
#else
        string myDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string folder = Path.Combine(myDocuments, "PixelCraft");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, fileName);
#endif
    }


    //UI매니저에 버튼 입력으로 저장하는 어쩌구 만들기
}
