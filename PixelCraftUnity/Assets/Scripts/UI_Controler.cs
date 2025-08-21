using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controler : MonoBehaviour
{
    public static UI_Controler uiMg;
    ImagePrettyImporter impmg;
    FileBrowserManager fileBrowserManager;

    [Header("이미지")]
    public RawImage imp_img;
    public RawImage mask_img; //UI상에 보이게 하기 위해서 필요함 << 임포트 이미지의 부모오브제
    public RawImage[] conv_imgs; //1차로 만들어진 Sprite 이미지


    [Header("Active 제어 게임오브제 목록")]
    public GameObject mainLogo;
    public GameObject firstInnerContent;
    public GameObject generateBtn; //AI 생성 버튼
    public GameObject convers_list; //메인 스프라이트 선택 존
    public GameObject loaders; //로딩 이미지(창 아님!!!!)
    public GameObject firstUI;
    public GameObject topUI; //상단바
    public GameObject character;
    public TMP_Text[] ox; //업로드여부


    [Header("캐릭터 준비됐나?")]
    public bool isReady;


    private void Awake()
    {
        if (uiMg == null) uiMg = this;
        else Destroy(gameObject);
        //싱글턴

        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad상에 HttpManager 생성 :: input data 저장소");

        impmg = GetComponent<ImagePrettyImporter>();
    }


    public void StartMainpage()
    {
        mainLogo.SetActive(false);
        firstInnerContent.SetActive(true);
    }

    public async void ImportBTN()
    {
        Texture2D tex = await ImportImage(); //파일 탐색기 켜서 이미지 가져옴
        if (tex != null)
        {
            // 여기서 tex 사용 가능
            Data_Manager.dtm.userinputphoto = tex;
            Debug.Log("이미지 불러오기 성공: " + tex.width + "x" + tex.height);

            //이제 여기서 PrettyImporter 사용해야 함
            //크로퍼 사용하면 비동기인데 우짜지
            Texture2D result_tex = await impmg.ImageMakeTurminal(tex);
            imp_img.texture = result_tex;
            imp_img.gameObject.SetActive(true);
            mask_img.gameObject.SetActive(true); //UI에 이미지 띄우기

            //OX버튼 중 하나 바꿔주기
            ox[0].text = "O";
            ox[0].color = Color.green;

            generateBtn.SetActive(true);
        }
    }

    public async void ImpotVideo()
    {
        fileBrowserManager = new FileBrowserManager();
        //비디오 임포트하기
        byte[] video = await fileBrowserManager.OpenVideoFileBrowser();
        Video_Data.vdata.userinputvideo = video;

        //OX버튼 중 하나 바꿔주기
        ox[1].text = "O";
        ox[1].color = Color.green;
    }


    async UniTask<Texture2D> ImportImage()
    {
        FileBrowserManager manager = new FileBrowserManager();
        Texture2D selectedTexture = await manager.OpenFileBrowser();

        return selectedTexture;
    }

    public async void Img2Im()
    {
        loaders.SetActive(true);
        Texture2D[] texs = await Http_Manager.htpMg.Img2Task();
        loaders.SetActive(false);

        for (int i = 0; i < conv_imgs.Length; i++)
        {
            conv_imgs[i].texture = Data_Manager.dtm.sprites[i];
        }

        convers_list.SetActive(true);
    }


    //각 이미지를 클릭하면 실행되는 것들
    public async void GenerateAnimsOne()
    {
        convers_list.SetActive(false);
        loaders.SetActive(true);
        Data_Manager.dtm.selected_spr = Data_Manager.dtm.sprites[0];
        Texture2D[] tex = await Http_Manager.htpMg.Img2Anim();
        firstUI.SetActive(false);

        ChangeLED(); //전광판 텍스쳐 바꾸는 코드
        ledTarget.SetActive(true);


        //무대연출 관련 코드 집어넣을 수 있음
        character.SetActive(true);
        topUI.SetActive(true);
    }

    public async void GenerateAnimsTwo()
    {
        convers_list.SetActive(false);
        loaders.SetActive(true);
        Data_Manager.dtm.selected_spr = Data_Manager.dtm.sprites[1];
        Texture2D[] tex = await Http_Manager.htpMg.Img2Anim();
        firstUI.SetActive(false);

        ChangeLED(); //전광판 텍스쳐 바꾸는 코드
        ledTarget.SetActive(true);

        //무대연출 관련 코드 집어넣을 수 있음
        character.SetActive(true);
        topUI.SetActive(true);
    }

    public async void GenerateAnimsThree()
    {
        convers_list.SetActive(false );
        loaders.SetActive(true);
        Data_Manager.dtm.selected_spr = Data_Manager.dtm.sprites[2];
        Texture2D[] tex = await Http_Manager.htpMg.Img2Anim();
        //다 만들었당

        firstUI.SetActive(false);

        isReady = true;
        print("isReady :: " + isReady);
        
        ChangeLED(); //전광판 텍스쳐 바꾸는 코드
        ledTarget.SetActive(true);


        //무대연출 관련 코드 집어넣을 수 있음
        character.SetActive(true);
        topUI.SetActive(true);
    }

    [Header("전광판")]
    public GameObject ledTarget;

    void ChangeLED()
    {
        print("ChangeLED! 실행!");
        Renderer red = ledTarget.GetComponent<Renderer>();
        Texture newtex = Data_Manager.dtm.selected_spr;

        red.material.SetTexture("_MainTex", newtex);
    }


}
