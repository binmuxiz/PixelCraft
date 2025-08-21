using UnityEngine;

public class Video_Data : MonoBehaviour
{

    public static Video_Data vdata;

    public byte[] userinputvideo; // 유저가 업로드한 동영상


    void Awake()
    {
        if (vdata == null) vdata = this;
        else Destroy(gameObject);
        //싱글턴

        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad상에 HttpManager 생성 :: input data 저장소");
    }

}
