using Assets.Scripts;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantSpawn : MonoBehaviour
{
    public GameObject PlantPrefab;
    public int maxNumberOfPlants = 100;
    public float spawnRadius = 10f;
    public float spawnInterval = 1.5f;
    private List<PlantView> plants = new();

    void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;

            GameObject plantGO = Instantiate(PlantPrefab, spawnPos, Quaternion.identity);
            PlantView plantView = plantGO.GetComponent<PlantView>();
            plantView.Initialize(new Plant());

            plants.Add(plantView);
        }

        StartCoroutine(SpawnPlantsOverTime());
    }

    IEnumerator SpawnPlantsOverTime()
    {
        while (true)
        {
            if (plants.Count < maxNumberOfPlants)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
                spawnPos.z = -1;

                GameObject plantGO = Instantiate(PlantPrefab, spawnPos, Quaternion.identity);
                PlantView plantView = plantGO.GetComponent<PlantView>();
                plantView.Initialize(new Plant());

                plants.Add(plantView);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
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
}