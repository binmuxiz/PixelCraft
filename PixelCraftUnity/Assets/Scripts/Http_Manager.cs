using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;
using System.IO;

public class Http_Manager : MonoBehaviour
{
    public static Http_Manager htpMg; //스태틱 변수
    public string url; //통신 링크(은빈서버)
    //public string url_j; // 정훈서버 링크

    public Sprite_spliter characterSprite;

    private void Awake()
    {
        if (htpMg == null) htpMg = this;
        else Destroy(gameObject);
        //싱글턴

        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad상에 HttpManager 생성 :: 백엔드 통신관리 및 통신정보 저장소");
    }

    #region 테스트 데이터 통신

    public async UniTask<Texture2D> TestGenerateImg()
    {
        Debug.Log("TestGenerateImg 함수 실행! 통신 시작. . .");
        string urls = url + "/generate_image";
        print(urls);
        Texture2D tex = await AsyncTestGenerateImg(urls); //통신
        Data_Manager.dtm.testOutputSprite = tex;
        return tex;
    }

    public async UniTask<Texture2D> AsyncTestGenerateImg(string url)
    {
        byte[] imageBytes = Data_Manager.dtm.userinputphoto.EncodeToPNG();
        WWWForm formData = new WWWForm();

        formData.AddBinaryData("file", imageBytes, "image.png", "image/png");

        UnityWebRequest request = UnityWebRequest.Post(url, formData);
        request.timeout = 180;

        try
        {
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                print("Request Success");

                byte[] receiveBytes = request.downloadHandler.data;

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(receiveBytes);

                //Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    Data_Manager.dtm.testOutputSprite = texture;
                    // UI에 텍스쳐 띄우는 함수 실행하거나...
                    return texture;
                }
                else
                {
                    Debug.LogError("텍스쳐 null!");
                }
            }
            else Debug.LogError($"Get Texture통신 실패! 오류: {request.error}");
        }
        catch (Exception e)
        {
            Debug.LogError($"텍스처 다운로드 중 예외 발생: {e.Message}");
        }
        finally
        {
            Debug.Log("통신 과정 완료!");
        }
        return null;
    }
    #endregion

    #region 사진 - 3캐릭터
    public async UniTask<Texture2D[]> Img2Task()
    {
        Debug.Log("Img2Task 함수 실행! 통신 시작. . .");
        string urls = url + "/generate_pixel_character";
        print(urls);

        byte[] imageBytes = Data_Manager.dtm.user_cropped_photo.EncodeToPNG(); //유저가 넣은 사진을 기반으로 통신 시작
        WWWForm formData = new WWWForm();

        formData.AddBinaryData("image", imageBytes, "image.png", "image/png");

        UnityWebRequest request = UnityWebRequest.Post(urls, formData);
        request.timeout = 180;

        try
        {
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                print("Request Success");

                byte[] zipBytes = request.downloadHandler.data;

                Texture2D[] textures = Img2Unrwap(zipBytes); //압축 해제 함수 실행하기
                Data_Manager.dtm.sprites = textures;

                return textures; //텍스쳐 array 반환
            }
            else Debug.LogError($"Get Texture통신 실패! 오류: {request.error}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Zip 다운로드 중 예외 발생: {e.Message}");
        }
        finally
        {
            Debug.Log("통신 과정 완료!");
        }
        return null;
    }

    Texture2D[] Img2Unrwap(byte[] zipfile) // Zip 파일 받아서 압축 풀어 넘기기 
    {
        var imageList = new System.Collections.Generic.List<Texture2D>();

        using (MemoryStream zipStream = new MemoryStream(zipfile))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            int count = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".png") || entry.FullName.EndsWith(".jpg"))
                {
                    using (Stream entryStream = entry.Open())
                    {
                        byte[] buffer = new byte[entry.Length];
                        entryStream.Read(buffer, 0, buffer.Length);

                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(buffer);
                        imageList.Add(tex);

                        count++;
                    }
                }
            }
        }
        Debug.Log($"이미지 {imageList.Count}개를 성공적으로 로드했습니다.");
        return imageList.ToArray();
    }
    #endregion


    //선택한 사진을 보내고 이미지프레임 receive 받기
    //여기에 영상도 같이보내야겟는데?
    public async UniTask<Texture2D[]> Img2Anim()
    {
        Debug.Log("Img2Anim 함수 실행! 통신 시작. . .");
        string urls = url + "/generate_framesheet";
        print(urls);

        byte[] imageBytes = Data_Manager.dtm.selected_spr.EncodeToPNG(); //유저가 넣은 사진을 기반으로 통신 시작
        WWWForm formData = new WWWForm();

        formData.AddBinaryData("image", imageBytes, "image.png", "image/png");
        formData.AddBinaryData("video", Video_Data.vdata.userinputvideo, "video.mp4", "video/mp4"); //영상을 보내!!

        UnityWebRequest request = UnityWebRequest.Post(urls, formData);
        request.timeout = 500;

        try
        {
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                print("Request Success");

                byte[] zipBytes = request.downloadHandler.data;

                Texture2D[] textures = Anim2Unrwap(zipBytes); //압축 해제 함수 실행하기
                //이미 Anim2Unwrap 함수에서 데이터매니저 할당을 마침
                //split해서 캐릭터에 부여하면 될듯?
                UI_Controler.uiMg.isReady = true;
                print("Http에서 true 시도!");
                return textures;
            }
            else Debug.LogError($"Get Texture통신 실패! 오류: {request.error}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Zip 다운로드 중 예외 발생: {e.Message}");
        }
        finally
        {
            Debug.Log("통신 과정 완료!");
        }
        return null;
    }

    Texture2D[] Anim2Unrwap(byte[] zipfile) // Zip 파일 받아서 압축 풀고 정체를 확인하기
    {
        var imageList = new System.Collections.Generic.List<Texture2D>();

        using (MemoryStream zipStream = new MemoryStream(zipfile))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            int fileCount = archive.Entries.Count;
            Data_Manager.dtm.danceFrames = new Texture2D[fileCount-2];
            int framecount = 0;
            int count = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // 1. 파일명 추출 및 확장자 확인
                string fileName = Path.GetFileName(entry.FullName);
                if (string.IsNullOrEmpty(fileName)) continue; // 디렉토리 제외
                string[] fileNames = fileName.Split("_");

                print(fileName);


                if (entry.FullName.EndsWith(".png") || entry.FullName.EndsWith(".jpg"))
                {
                    using (Stream entryStream = entry.Open())
                    {
                        byte[] buffer = new byte[entry.Length];
                        entryStream.Read(buffer, 0, buffer.Length);

                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(buffer);
                        imageList.Add(tex);
                        print("Anim2Unrwap 압축 해제한 이미지 파일 타입! :: " + fileNames[0]);

                        if (fileNames[0] == "run")
                        {
                            Data_Manager.dtm.runFrame = tex;
                            characterSprite.img_run_target = tex;
                        }
                        else if (fileNames[0] == "walk")
                        {
                            //Texture2D newtex = Data_Manager.dtm.ResizeTexture(tex, 2048, 659);
                            Data_Manager.dtm.walkFrame = tex;
                            characterSprite.img_walk_target = tex;
                        }
                        else if (fileNames[0] == "dance")
                        {
                            print("dnace 데이터"+framecount+" 번 포착!");
                            //나머지는 전부 영상프레임이겠지 모
                            Data_Manager.dtm.danceFrames[framecount] = tex;
                            framecount++;
                        }
                    }
                }
            }
        }
        Debug.Log($"이미지 {imageList.Count}개를 성공적으로 로드했습니다.");
        UI_Controler.uiMg.firstUI.SetActive(false);
        return imageList.ToArray();
    }
}


