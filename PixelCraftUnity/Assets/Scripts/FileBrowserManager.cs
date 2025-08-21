using Cysharp.Threading.Tasks;
using SFB;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;


//WebGL 빌드: 반드시 WebGL 빌드 설정에서 'Exceptions'을 'Full Without Stacktrace'로 설정
//UI 차단 방지: 길게 걸리는 작업 시 로딩 UI 표시 권장
//메모리 관리: 2048x2048 이상 큰 텍스처는 TextureCompression 사용 권장

public class FileBrowserManager
{
#if UNITY_WEBGL && !UNITY_EDITOR
    private UniTaskCompletionSource<Texture2D> _webglTextureTaskSource;
#endif

    // 파일 브라우저를 열고 Texture2D를 반환하는 메인 메서드
    public async UniTask<Texture2D> OpenFileBrowser()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _webglTextureTaskSource = new UniTaskCompletionSource<Texture2D>();
        OpenFileInput();
        return await _webglTextureTaskSource.Task;
#else
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        }, false);

        if (paths.Length == 0) return null;
        return await LoadTextureFromPath(paths[0]);
#endif
    }

    // 실제 텍스처 로딩 처리
    private async UniTask<Texture2D> LoadTextureFromPath(string path)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"이미지 로드 실패: {www.error}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Data_Manager.dtm.userinputphoto = texture;
            return texture;
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenFileInput();

    // WebGL에서 파일 선택 완료 시 호출
    public async void OnFileSelected(string url)
    {
        try
        {
            Texture2D texture = await LoadTextureFromPath(url);
            _webglTextureTaskSource?.TrySetResult(texture);
        }
        catch (Exception ex)
        {
            _webglTextureTaskSource?.TrySetException(ex);
        }
    }
#endif
    //====================================================================================================================

    // 파일 브라우저를 열고 비디오의 byte[] 반환
    public async UniTask<byte[]> OpenVideoFileBrowser()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL은 로컬 파일 시스템 접근이 다르므로 별도 처리 필요
        Debug.LogWarning("WebGL에서는 비디오 파일 byte[] 직접 로드가 어렵습니다.");
        return null;
#else
        var extensions = new[] {
            new ExtensionFilter("Video Files", "mp4", "mov", "avi", "mkv")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Video", "", extensions, false);

        if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            return null;

        string path = paths[0];

        if (!File.Exists(path))
        {
            Debug.LogError($"파일이 존재하지 않습니다: {path}");
            return null;
        }

        // 비동기적으로 파일을 읽어서 byte[] 반환
        return await LoadVideoBytesFromPath(path);
#endif
    }

    // 실제로 파일을 비동기로 로드
    private async UniTask<byte[]> LoadVideoBytesFromPath(string path)
    {
        return await UniTask.RunOnThreadPool(() => File.ReadAllBytes(path));
        // 대용량 파일을 UI 프리징 없이 읽도록 ThreadPool 사용
    }

}


