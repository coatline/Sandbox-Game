using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBreakingVisual : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    ParticleSystem ps;
    SpriteRenderer sr;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();
    }

    // I could add a small amount of randomness
    public void BreakBlock(int x, int y, int blockHardness, int currentDurability)
    {
        EmitParticles(x, y);

        float percentageFinished = (float)currentDurability / (float)blockHardness;
        int index = (int)((frames.Length - 1) * percentageFinished);

        //if (Random.Range(0, 100) < 20)
        //{
        //    index += Random.Range(-1, 2);

        //    if (index >= frames.Length)
        //    {
        //        index = frames.Length - 1;
        //    }
        //    else if (index < 0)
        //    {
        //        index = 0;
        //    }
        //}


        sr.sprite = frames[index];
    }

    public void EmitParticles(int x, int y)
    {
        transform.position = new Vector3(x, y) + Vector3.one / 2;
        //ps.Emit(10);
    }

    public void Finish()
    {
        sr.sprite = null;
    }
}
