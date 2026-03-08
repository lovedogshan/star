using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // 怪物预制体数组
    public GameObject[] npcPrefabs; // NPC预制体数组
    public Transform spawnPoint; // 刷怪位置
    public int numberOfEnemies = 5; // 刷怪数量
    public int numberOfNPCs = 2; // 刷NPC数量

    private bool hasSpawned = false; // 是否已经刷怪

    // 当有其他物体进入触发器时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSpawned) // 检测是否是玩家并且尚未刷怪
        {
            Debug.Log("撞击");
            Transform spawnLocation = spawnPoint; // 获取刷新位置
            if (spawnLocation != null)
            {
                Debug.Log("生成");
                SpawnEnemies(spawnLocation.position);
                SpawnNPCs(spawnLocation.position);
                hasSpawned = true;
            }
        }
    }

    // 刷怪逻辑
    private void SpawnEnemies(Vector2 spawnPosition)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            // 随机选择一个怪物预制体
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            // 实例化怪物对象并放置在刷新位置
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // 刷NPC逻辑
    private void SpawnNPCs(Vector2 spawnPosition)
    {
        for (int i = 0; i < numberOfNPCs; i++)
        {
            // 随机选择一个NPC预制体
            GameObject npcPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
            // 实例化NPC对象并放置在刷新位置
            GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        }
    }
}

