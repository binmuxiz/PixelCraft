using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShowmeRunway : MonoBehaviour
{
    // 1. 캐릭터 Sprite Renderer 컬러 black 상태로 나타내기

    // 2. 조명 3차례 순서대로 나타나기

    // 3. 캐릭터 Sprite 컬러 걷으면서 앞쪽으로 빼기

    // 4. 파티클도 같이

    public GameObject character;
    SpriteRenderer chaspt;
    public GameObject[] sportlights;
    public GameObject par;

    [Header("스포트라이트 딜레이")]
    [Range(300, 700)]
    public int lightdel = 300;

    [Header("스프라이트렌더 타임")]
    public float lightdur = 1.5f; // 색상 변화에 걸리는 시간(초)



    AudioSource song;
    public AudioClip[] sclip;

    private void Awake()
    {
        chaspt = character.GetComponent<SpriteRenderer>();
        song = GetComponent<AudioSource>();
    }


    private void Start()
    {
        BlackColor();
    }

    bool oneshot;
    public GameObject pressQ;
    public GameObject pressAD;

    private void Update()
    {
        if (UI_Controler.uiMg.isReady)
        {
            if (!oneshot)
            {
                oneshot = true;
                song.Stop();
                pressQ.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                pressQ.SetActive(false);
                UI_Controler.uiMg.isReady = false;
                //song.clip = sclip[1];
                //song.Play();
                LightOn().Forget();
            }
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            pressAD.SetActive(false);
        }
    }


    void BlackColor()
    {
        chaspt.color = Color.black;
    }


    //1초간 3박자가 정확하게... 0.3초마다 켜기
    async UniTaskVoid LightOn()
    {
        foreach (var light in sportlights)
        {
            await UniTask.Delay(lightdel);
            light.SetActive(true);
            song.clip = sclip[1];
            song.Play();
        }
        await UniTask.Delay(200);

        character.SetActive(true);
        FadeToWhiteAsync().Forget();
    }

    private async UniTaskVoid FadeToWhiteAsync()
    {
        Color startColor = Color.black;
        Color startColorA = Color.black; //알파값 조정을 위한 컬러
        Color endColor = Color.white;
        startColor.a = 0f;
        startColorA.a = 1f; //알파값도
        float alphadur = 1f;
        float elapsed = 0f;

        while (elapsed < alphadur)
        {
            float t = elapsed / alphadur;
            chaspt.color = Color.Lerp(startColor, startColorA, t);
            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }
        elapsed = 0f;
        while (elapsed < lightdur)
        {
            float t = elapsed / lightdur;
            chaspt.color = Color.Lerp(startColor, endColor, t);
            await UniTask.Yield(); // 다음 프레임까지 대기
            elapsed += Time.deltaTime;
        }

        // 마지막에 완전히 white로 보정
        chaspt.color = endColor;

        // 파티클 뿌리면서
        par.SetActive(true);

        // 스포트라이트 두짝 서서히 끄기
        Lightoff().Forget();
    }

    async UniTaskVoid Lightoff()
    {
        SpriteRenderer spL = sportlights[0].GetComponent<SpriteRenderer>();
        SpriteRenderer spR = sportlights[1].GetComponent<SpriteRenderer>();


        Color sportlightL = spL.color;
        Color sportlightR = spR.color;

        Color endcolor = Color.white;
        endcolor.a = 0f;

        float alphadur = 1.5f;
        float elapsed = 0f;

        while (elapsed < alphadur)
        {
            float t = elapsed / alphadur;
            spL.color = Color.Lerp(sportlightL, endcolor, t);
            spR.color = Color.Lerp(sportlightR, endcolor, t);

            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }
        elapsed = 0f;

        spL.color = endcolor;
        spR.color = endcolor;

        await UniTask.Yield();

        sportlights[0].SetActive(false);
        sportlights[1].SetActive(false);

        await UniTask.Delay(470);
        print("Delay!");

        song.Stop();
        song.clip = sclip[2];
        song.Play();

        pressAD.SetActive(true);

    }

}
