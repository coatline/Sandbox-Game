using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] CameraFollowWithBarriers cam;
    [SerializeField] Vector2 spawnDelay;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] DayNightCycle dnc;
    [SerializeField] WorldLoader wl;
    [SerializeField] int spawnCap;
    float spawnRange;
    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        spawnRange = (wl.chunkSize * wl.excessChunksToLoad) + cam.CameraSizeInUnits().x;
    }

    public void Night()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(Random.Range(spawnDelay.x, spawnDelay.y));

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
}
