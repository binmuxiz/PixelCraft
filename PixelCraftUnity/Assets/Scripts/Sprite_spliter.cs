using System.Collections;
using UnityEngine;

public class Sprite_spliter : MonoBehaviour
{
    [Header("캐릭터 오브제 집어넣기")]
    public GameObject character; // 타겟 캐릭터
    //public GameObject test_sub; //테스트용 서브캐릭터 오브젝트

    [Header("추출용 이미지")]
    public Texture2D img_walk_target; // 프레임 추출할 이미지
    public Texture2D img_run_target;


    int walk_frame = 8;
    int run_frame = 10;

    int[] walkxpos = new int[] { 90, 400, 700, 1010, 1310, 1620, 1925, 2230 }; //90, 400, 710, 1020, 1330
    int[] runxpos = new int[] {0, 530, 1060, 1590, 2120, 2650, 3180, 3710, 4240, 4770 };


    SpriteRenderer sprender;
    //SpriteRenderer test_spriteRender;

    public Sprite[] walk;
    public Sprite[] run;
    public Sprite[] dance;


    private void Awake()
    {
        if (character != null)
        {
            sprender = character.GetComponent<SpriteRenderer>();
            if (sprender == null) print("Renderer Error!");
        }
    }

    public void MakeWalkClip()
    {
        Debug.Log("Walk Generate!");

        //로컬스케일로 리스케일하기
        character.transform.localScale = new Vector3(1f, 1f, 1f);

        Sprite[] frames = SplitSprites(img_walk_target, walk_frame, 300, 600, walkxpos, 90);
        Debug.Log("프레임 개수: " + frames.Length);
        if (frames.Length > 0)
            Debug.Log("첫 프레임 Rect: " + frames[0].rect);
        //OverrideClip(frames); //재생하는 코루틴
        walk = frames;
        Data_Manager.dtm.walk = frames;
    }

    public void MakeRunClip()
    {
        Debug.Log("Run Generate!");

        //로컬스케일로 리스케일하기
        character.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        Sprite[] frames = SplitSprites(img_run_target, run_frame, 430, 550, runxpos, 0);
        Debug.Log("프레임 개수: " + frames.Length);
        if (frames.Length > 0)
            Debug.Log("첫 프레임 Rect: " + frames[0].rect);
        //OverrideClip(frames); //재생하는 코루틴
        run = frames;
        Data_Manager.dtm.run = frames;
    }

    // 여러장의 텍스쳐 어레이를 사용해서 춤추는 프레임 만들어야해ㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐㅐ
    //1. 일단 sprite로 만들어
    public static Sprite[] ConvertTexture2DArrayToSpriteArray(Texture2D[] textureArray)
    {
        if (textureArray == null) return null;

        Sprite[] spriteArray = new Sprite[textureArray.Length];

        for (int i = 0; i < textureArray.Length; i++)
        {
            Texture2D tex = textureArray[i];
            if (tex == null)
            {
                spriteArray[i] = null; // null 처리
                continue;
            }

            // Texture2D 전체 영역을 Sprite로 변환
            spriteArray[i] = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f) // pivot: 가운데
            );
        }
        return spriteArray;
    }

    //2. 그다음 프레임으로 만들자
    public void MakeDanceClip()
    {
        Debug.Log("Dance Generate!");

        //로컬스케일로 리스케일하기
        character.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        Sprite[] frames = ConvertTexture2DArrayToSpriteArray(Data_Manager.dtm.danceFrames);
        Debug.Log("프레임 개수: " + frames.Length);
        if (frames.Length > 0)
            Debug.Log("첫 프레임 Rect: " + frames[0].rect);
        //OverrideClip(frames); //재생하는 코루틴
        dance = frames;
        Data_Manager.dtm.dance = frames;
    }

    #region 간접사용 함수 - 테스트 사용

    public void OverrideClip(Sprite[] frames)
    {
        // 애니메이터 대신 코루틴으로 직접 프레임 전환
        StartCoroutine(PlaySpriteAnimation(frames, 0.1f));
    }

    IEnumerator PlaySpriteAnimation(Sprite[] frames, float interval)
    {
        int currentFrame = 0;
        while (true)
        {
            sprender.sprite = frames[currentFrame];
            //Debug.Log($"프레임 {currentFrame} 표시됨");
            currentFrame = (currentFrame + 1) % frames.Length;
            yield return new WaitForSeconds(interval);
        }
    }

    // Sprite 분할
    public Sprite[] SplitSprites(Texture2D target, int columns, int frameWidth, int Framehight, int[] xpos, int ypos) // 타겟이미지, 프레임 수, 가로, 세로, x, y 위치
    {
        Sprite[] sprites = new Sprite[columns];

        for (int i = 0; i < columns; i++)
        {
            Rect rect = new Rect(
                xpos[i], // X 시작위치
                ypos, // Y 시작 위치
                frameWidth,
                Framehight
                );

            sprites[i] = Sprite.Create(
                target,
                rect,
                new Vector2(0.5f, 0.5f)
                );
        }
        return sprites;
    }
    #endregion

}
