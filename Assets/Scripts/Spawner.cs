using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] enemies;
    public float enemySpawnCoolDown;
    public float enemySpawnCoolTime;
    public int enemyCount;
    public int maxEnemies;
    public bool isSpawningEnemy;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (isSpawningEnemy)
        {
            Instantiate(enemies[Random.Range(0, enemies.Length)], transform.position, Quaternion.identity);
            enemyCount++;
            isSpawningEnemy = false;

        }

        if (!isSpawningEnemy)
        {
            enemySpawnCoolDown -= Time.deltaTime;

            if (enemySpawnCoolDown < 0)
            {
                enemySpawnCoolDown = enemySpawnCoolTime;
                isSpawningEnemy = true;
            }
        }

        //if we need to have a kill switch down the line
       if (!stopSpawning())
        {
            if (!isSpawningEnemy)
            {
                enemySpawnCoolDown -= Time.deltaTime;

                if (enemySpawnCoolDown < 0)
                {
                    enemySpawnCoolDown = enemySpawnCoolTime;
                    isSpawningEnemy = true;
                }
            }
        }
        else
        {
            isSpawningEnemy = false;
            enemySpawnCoolDown = 0;
        }




    }

    public bool stopSpawning()
    {
        if (enemyCount >= maxEnemies)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
