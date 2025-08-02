using Assets.Scripts;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class PlantSpawn : MonoBehaviour
{
    public GameObject PlantPrefab;
    public int numberOfPlants = 50;
    public float spawnRadius = 10f;
    private List<PlantView> plants = new();

    void Start()
    {
        SpawnPlants();
    }

    public void SpawnPlants()
    {
        for (int i = 0; i < numberOfPlants; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;

            GameObject plantGO = Instantiate(PlantPrefab, spawnPos, Quaternion.identity);
            PlantView plantView = plantGO.GetComponent<PlantView>();
            plantView.Initialize(new Plant());

            plants.Add(plantView);
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