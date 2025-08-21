using UnityEngine;

public class Mover_Clamp : MonoBehaviour
{
    [Header("클램프 포인트")]
    public float clampApos; //왼쪽 클램프 포지션
    public float clampBpos; //오른쪽 클램프 포지션
    [Header("Speed")]
    public float moveSpeed; //일반 스피드
    public float doublespeed; //배수
    float defspeed;

    public bool isCharacter;
    public GameObject character;
    public GameObject clampObg;

    public bool isclamped;

    float clampA;
    float clampB;

    Mover_Clamp characterclamp;

    private void Start()
    {
        characterclamp = character.GetComponent<Mover_Clamp>();
        clampA = clampObg.GetComponent<Mover_Clamp>().clampApos;
        clampB = clampObg.GetComponent<Mover_Clamp>().clampBpos;
        defspeed = moveSpeed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = moveSpeed * doublespeed;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = defspeed;
        }


        if (clampObg.transform.position.x <= clampA + 0.5 || clampObg.transform.position.x >= clampB - 0.5)
        {
            isclamped = true;
        }
        else isclamped = false;

        if (!isCharacter)
        {
            ObjectMove();
        }
        else
        {
            if (characterclamp.isclamped)
            {
                ObjectMove();
            }
        }
    }

    void ObjectMove()
    {
        float moveinput = Input.GetAxis("Horizontal");
        Vector3 position = transform.position;
        
        if (isCharacter) position.x += moveinput * moveSpeed * Time.deltaTime;
        else position.x -= moveinput * moveSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, clampApos, clampBpos);
        transform.position = position;
    }
}
