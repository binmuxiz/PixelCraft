using UnityEngine;

public class Character_Animator : MonoBehaviour
{
    Sprite_spliter spt;
    SpriteRenderer sr;


    float timer;
    //walk interval
    int currentWalkFrame;
    public float walkinterval;

    //run interval
    int currentRunFrame;
    public float runinterval;

    bool isrun;


    bool isDance = false;
    int currentDanceFrame;
    [Header("댄스 인터벌")]
    public float danceinterval = 0.18f; // 필요시 Inspector에서 조정
    float danceTimer;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        spt = GetComponent<Sprite_spliter>();
    }

    private void Start()
    {
        spt.MakeWalkClip();
        spt.MakeRunClip();
        spt.MakeDanceClip();
        sr.sprite = spt.walk[0];
        transform.localScale = new Vector3(1f, 1f, 1f);
    }


    //private void Update()
    //{
    //    Flip(); //캐릭터 뒤집기 판정

    //    if (Input.GetAxis("Horizontal") != 0)
    //    {
    //        if (Input.GetKey(KeyCode.LeftShift))
    //        {
    //            RunFramePlay();
    //            isrun = true;
    //        }
    //        else
    //        {
    //            WalkFranePlay();
    //            isrun= false;
    //        }
    //    }
    //}

    private void Update()
    {
        Flip(); //캐릭터 뒤집기 판정

        // E키를 한 번 누르면 Dance 상태로 진입
        if (Input.GetKeyDown(KeyCode.E))
        {
            isDance = true;
            transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            transform.localPosition = new Vector3(0f, 0.4f, -1f);
            currentDanceFrame = 0;
            danceTimer = 0f;
        }

        // Dance 중이라면 수평 입력이 없을 때만 Dance 유지
        float horizontal = Input.GetAxis("Horizontal");
        bool hasMoveInput = Mathf.Abs(horizontal) > 0.01f;

        if (isDance && !hasMoveInput)
        {
            Dance();
            return;
        }
        // 수평 입력이 생기면 Dance는 종료, 이후 걷기/뛰기
        if (hasMoveInput)
        {
            isDance = false;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                RunFramePlay();
                isrun = true;
            }
            else
            {
                WalkFranePlay();
                isrun = false;
            }
        }
    }

    void Flip()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal > 0) sr.flipX = false;
        else if (horizontal < 0) sr.flipX = true;
    }


    void WalkFranePlay()
    {
        FrameWork(spt.walk, ref currentWalkFrame, walkinterval); // ref 추가
    }

    void RunFramePlay()
    {
        FrameWork(spt.run, ref currentRunFrame, runinterval); // ref 추가
    }

    // ref 키워드로 참조 전달
    void FrameWork(Sprite[] frames, ref int currentFrame, float interval)
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            currentFrame = (currentFrame + 1) % frames.Length; // ✅
            sr.sprite = frames[currentFrame];

            if(isrun) transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            else transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        }
    }

    // 🔽 Dance 애니메이션 처리(Dance 스프라이트 배열 필요)
    void Dance()
    {
        if (spt.dance == null || spt.dance.Length == 0) return; // dance 스프라이트가 준비되어 있어야 함

        danceTimer += Time.deltaTime;
        if (danceTimer >= danceinterval)
        {
            danceTimer -= danceinterval;
            currentDanceFrame = (currentDanceFrame + 1) % spt.dance.Length;
            sr.sprite = spt.dance[currentDanceFrame];
        }
    }
}
