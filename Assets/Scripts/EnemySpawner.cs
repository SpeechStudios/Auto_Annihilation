using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public LayerMask NoSpawn;
    public LayerMask Ground;
    public GameObject EnemyPrefab;
    public float MaxSpawnRangeFromPlayer = 20f;
    public float MinSpawnRangeFromPlayer = 5f;
    private float Timer;
    [HideInInspector] public float SpawnTime;


    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer > SpawnTime)
        {
            SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        float randomXRangeFromPlayer = Random.Range(-MaxSpawnRangeFromPlayer, MaxSpawnRangeFromPlayer);
        float randomZRangeFromPlayer = Random.Range(-MaxSpawnRangeFromPlayer, MaxSpawnRangeFromPlayer);
        if (randomXRangeFromPlayer < MinSpawnRangeFromPlayer && randomXRangeFromPlayer > -MinSpawnRangeFromPlayer || randomZRangeFromPlayer < MinSpawnRangeFromPlayer && randomZRangeFromPlayer > -MinSpawnRangeFromPlayer)
        {
            return;
        }
        Vector3 randomLocation = new(transform.position.x + randomXRangeFromPlayer, transform.position.y + 50, transform.position.z + randomZRangeFromPlayer);
        if (Physics.Raycast(randomLocation, -Vector3.up, out RaycastHit hit, Mathf.Infinity, Ground))
        {
            if (Physics.CheckSphere(hit.point, 1, NoSpawn))
            {
                return;
            }
            Instantiate(EnemyPrefab, hit.point - (Vector3.up * 2), Quaternion.identity);
            Timer = 0;
        }

    }
}
