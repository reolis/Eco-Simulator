using UnityEngine;
using Assets.Scripts;
using System.Collections.Generic;

public class HerbivoreAnimalProcess : MonoBehaviour
{
    private Animal animal;
    private GameObject animalPrefab, plantPrefab;
    private float timeSinceLastDecision = 0f;
    private float decisionInterval = 1f;

    private Vector2 currentDirection = Vector2.zero;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 2f;
    private Transform targetPlant;
    private Vector2 smoothedDirection = Vector2.zero;

    public void Initialize(Animal newAnimal, GameObject animalSt, GameObject plantSt)
    {
        animal = newAnimal;
        animalPrefab = animalSt;
        plantPrefab = plantSt;
    }

    void Update()
    {
        if (animal == null || animal.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        float delta = Time.deltaTime;
        animal.Delta = delta;
        animal.UpdateStates(delta);

        CheckForThreats();

        Animal child = animal.TryReproduce();
        if (child != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle.normalized * 1f;
            GameObject childGO = Instantiate(animalPrefab, spawnPos, Quaternion.identity);

            HerbivoreAnimalProcess childProcess = childGO.GetComponent<HerbivoreAnimalProcess>();
            childProcess.Initialize(child, animalPrefab, plantPrefab);
        }

        if (animal.CurrentState == Animal.AnimalState.SeekFood && targetPlant != null)
        {
            float dist = Vector2.Distance(transform.position, targetPlant.position);
            if (dist <= 0.5f)
            {
                ExecuteAction(AnimalAI.Action.EatPlant);
            }
        }

        timeSinceLastDecision += delta;

        if (timeSinceLastDecision >= decisionInterval)
        {
            AnimalAI.Action next;

            if (animal.AI.IsThreatNearby)
            {
                next = AnimalAI.Action.RunFromThreat;
            }
            else if (animal.TypeOfAnimal == AnimalType.Herbivore && animal.Hunger > 55)
            {
                next = AnimalAI.Action.SeekFood;
            }
            else
            {
                next = AnimalAI.Action.Wander;
            }

            ExecuteAction(next);
            timeSinceLastDecision = 0f;
        }

        MoveLogic();
    }

    private void ExecuteAction(AnimalAI.Action action)
    {
        switch (action)
        {
            case AnimalAI.Action.Wander:
                animal.CurrentState = Animal.AnimalState.Wander;
                break;
            case AnimalAI.Action.Sleep:
                animal.CurrentState = Animal.AnimalState.Idle;
                break;
            case AnimalAI.Action.EatPlant:
                animal.CurrentState = Animal.AnimalState.Eating;
                TryEatNearbyPlant();
                break;
            case AnimalAI.Action.SeekFood:
                animal.CurrentState = Animal.AnimalState.SeekFood;
                SeekFood();
                break;
            case AnimalAI.Action.RunFromThreat:
                animal.CurrentState = Animal.AnimalState.Fleeing;
                break;
            case AnimalAI.Action.BeGroup:
                animal.CurrentState = Animal.AnimalState.Wander;
                BeGrouped();
                break;
        }
    }

    private void MoveLogic()
    {
        if (animal.CurrentState == Animal.AnimalState.Dying || animal.CurrentState == Animal.AnimalState.Idle)
            return;

        if (animal.CurrentState != Animal.AnimalState.SeekFood)
        {
            directionChangeTimer += Time.deltaTime;

            if (directionChangeTimer >= directionChangeInterval || currentDirection == Vector2.zero)
            {
                currentDirection = Random.insideUnitCircle.normalized;
                directionChangeTimer = 0f;
            }
        }

        smoothedDirection = Vector2.Lerp(smoothedDirection, currentDirection, Time.deltaTime * 2f);

        float angle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 2f);

        float speedModifier = (animal.CurrentState == Animal.AnimalState.Eating) ? 0.2f : 0.5f;
        Vector2 noise = Random.insideUnitCircle * 0.05f;
        Vector2 finalDirection = (smoothedDirection + noise).normalized;

        transform.Translate(finalDirection * animal.Speed * speedModifier * Time.deltaTime, Space.World);
        transform.position = ClampPositionToCameraBounds(transform.position);
    }

    private void TryEatNearbyPlant()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var col in nearby)
        {
            PlantView plantView = col.GetComponent<PlantView>();
            if (plantView != null)
            {
                float nutrition = plantView.BeEaten();
                animal.Hunger = Mathf.Max(0f, animal.Hunger - nutrition);
                animal.CurrentState = Animal.AnimalState.Eating;
                break;
            }
        }
    }

    private void SeekFood()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, animal.VisionField);

        float minDist = float.MaxValue;
        targetPlant = null;

        foreach (var col in nearby)
        {
            PlantView plant = col.GetComponent<PlantView>();
            if (plant != null)
            {
                float dist = Vector2.Distance(transform.position, plant.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetPlant = plant.transform;
                }
            }
        }

        if (targetPlant != null)
        {
            Vector2 dir = (targetPlant.position - transform.position).normalized;
            currentDirection = dir;
        }
        else
        {
            animal.CurrentState = Animal.AnimalState.Wander;
            currentDirection = Random.insideUnitCircle.normalized;
        }
    }

    private void CheckForThreats()
    {
        Collider2D[] predators = Physics2D.OverlapCircleAll(transform.position, 5f, 0);
        animal.AI.IsThreatNearby = predators.Length > 0;

        if (predators.Length > 0)
        {
            Transform nearestPredator = predators[0].transform;
            float minDist = Vector2.Distance(transform.position, nearestPredator.position);

            foreach (var pred in predators)
            {
                float dist = Vector2.Distance(transform.position, pred.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestPredator = pred.transform;
                }
            }

            Vector2 fleeDir = (Vector2)(transform.position - nearestPredator.position).normalized;
            currentDirection = fleeDir;
        }
    }

    private void BeGrouped()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 10f, 6);

        List<Transform> nearbyAllies = new List<Transform>();
        foreach (var col in hits)
        {
            if (col.transform == this.transform) continue;

            HerbivoreAnimalProcess other = col.GetComponent<HerbivoreAnimalProcess>();
            if (other != null && other.animal.TypeOfAnimal == AnimalType.Herbivore && !other.animal.IsDead)
            {
                nearbyAllies.Add(other.transform);
            }
        }

        animal.AI.isRelativeNearby = nearbyAllies.Count >= 2;

        if (nearbyAllies.Count >= 2)
        {
            Transform nearest = nearbyAllies[0];
            float minDist = Vector2.Distance(transform.position, nearest.position);

            foreach (var ally in nearbyAllies)
            {
                float dist = Vector2.Distance(transform.position, ally.position);
                if (dist < minDist)
                {
                    nearest = ally;
                    minDist = dist;
                }
            }

            Vector2 dirToAlly = (nearest.position - transform.position).normalized;
            currentDirection = dirToAlly;
        }
    }

    private Vector2 ClampPositionToCameraBounds(Vector2 pos)
    {
        Camera cam = Camera.main;
        Vector2 min = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = cam.ViewportToWorldPoint(new Vector2(1, 1));

        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);

        return pos;
    }
}