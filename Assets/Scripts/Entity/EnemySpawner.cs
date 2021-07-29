using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] CameraFollowWithBarriers cam;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] WorldLoader wl;
    [SerializeField] DayNightCycle dnc;
    [SerializeField] int spawnCap;
    public float spawnDelay;
    float spawnRange;
    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        spawnRange = (wl.chunkSize * wl.excessChunksToLoad) + cam.CameraSizeInUnits().x;
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(Random.Range(2f, 4f));

        if (transform.childCount < spawnCap)
        {
            if (Random.Range(0, 2) == 0) { spawnRange = -spawnRange; }

            Vector3 position = cam.transform.position + new Vector3(spawnRange, 0, 10);

            if (position.y < GD.wd.worldHeight && position.y >= 0 && position.x < GD.wd.worldWidth && position.x >= 0)
            {
                while (GD.wd.blockMap[(int)position.x, (int)position.y, 0] != 0)
                {
                    position.y++;
                }

                var en = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
                en.player = this.player;
            }
        }

        StartCoroutine(Spawn());
    }

    void Update()
    {

    }
}
