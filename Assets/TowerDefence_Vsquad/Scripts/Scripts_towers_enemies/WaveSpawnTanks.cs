using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnTanks : MonoBehaviour {

    public int WaveSize;
    public GameObject EnemyPrefab;
    public float EnemyInterval;
    public Transform spawnPoint;
    public float startTime;
    public Transform[] WayPoints;
    int enemyCount = 0;

    void Start()
    {
        InvokeRepeating("SpawnEnemy", startTime, EnemyInterval);
    }

    void Update()
    {
        if (enemyCount == WaveSize)
        {
            CancelInvoke("SpawnEnemy");
        }
    }

    void SpawnEnemy()
    {
        enemyCount++;
        GameObject enemy = GameObject.Instantiate(EnemyPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
        enemy.GetComponent<TankEnemy>().waypoints = WayPoints;

    }
}