using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParticlePlay : MonoBehaviour
{
    public ParticleSystem particleSystems; // Inspector에서 할당
    public float interval = 1f;           // n초마다 재생

    void Start()
    {
        if (particleSystems == null)
            particleSystems = GetComponent<ParticleSystem>();

        PlayParticleLoopAsync().Forget();
    }

    private async UniTaskVoid PlayParticleLoopAsync()
    {
        while (true)
        {
            particleSystems.Play();
            await UniTask.Delay((int)(interval * 1000)); // interval 초 대기
        }
    }
}
