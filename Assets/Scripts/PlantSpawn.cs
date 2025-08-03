using Assets.Scripts;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantSpawn : MonoBehaviour
{
    public GameObject PlantPrefab;
    public int maxNumberOfPlants = 100;
    public float spawnRadius = 10f;
    public float spawnInterval = 0.1f;
    private List<PlantView> plants = new();

    void Start()
    {
        for (int i = 0; i < 50; i++) SpawnPlant();
        StartCoroutine(SpawnPlantsOverTime());
    }

    IEnumerator SpawnPlantsOverTime()
    {
        while (true)
        {
            int missingPlants = maxNumberOfPlants - plants.Count;
            for (int i = 0; i < missingPlants; i++)
            {
                SpawnPlant();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool IsWithinBounds(Vector3 position)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = plants.Count - 1; i >= 0; i--)
        {
            PlantView plantView = plants[i];

            if (plantView == null)
            {
                plants.RemoveAt(i);
                continue;
            }

            plantView.PlantModel.UpdatePlant(deltaTime);

            if (plantView.PlantModel.IsAlive)
            {
                plantView.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, plantView.PlantModel.Growth);
            }
            else
            {
                Destroy(plantView.gameObject);
                plants.RemoveAt(i);
            }
        }
    }

    private void SpawnPlant()
    {
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.z = -1;

        if (!IsWithinBounds(spawnPos)) return;

        GameObject plantGO = Instantiate(PlantPrefab, spawnPos, Quaternion.identity);
        PlantView plantView = plantGO.GetComponent<PlantView>();
        plantView.Initialize(new Plant());
        plants.Add(plantView);
    }
}