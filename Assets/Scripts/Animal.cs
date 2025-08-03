using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Assets.Scripts
{
    public enum AnimalType
    {
        Herbivore,
        Predatory
    }

    public class Animal
    {
        public AnimalType TypeOfAnimal { get; private set; }
        public float Hunger {  get; set; }
        public float Age { get; set; }
        public float Speed { get; set; }
        public float HP { get; set; }
        public float VisionField { get; set; }
        public float ReproduceTime { get; set; }
        public bool isChase { get; set; }
        public bool isRunning { get; set; }
        public bool IsDead { get; private set; }
        public float Delta {  get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public UnityEngine.Transform Target { get; set; }
        public float NutritionValue { get; set; }
        private float ReproduceTries { get; set; }

        public enum AnimalState
        {
            Idle,
            Wander,
            SeekFood,
            Eating,
            Fleeing,
            Chasing,
            Reproducing,
            Dying,
            Group
        }

        public AnimalState CurrentState { get; set; }

        private System.Random randomParams = new System.Random();
        public AnimalAI AI = new AnimalAI();
        private float stateUpdateTimer = 0f;
        private float stateUpdateInterval = 3f;

        public Animal(AnimalType type)
        {
            TypeOfAnimal = type;

            if (type is AnimalType.Herbivore)
            {
                Hunger = randomParams.Next(1, 50);
                Age = 1;
                Speed = randomParams.Next(5, 10);
                HP = randomParams.Next(100, 300);
                VisionField = 15;
                ReproduceTime = 0;
                NutritionValue = randomParams.Next(1, 20);
                ReproduceTries = randomParams.Next(1, 3);
            }
            else if (type is AnimalType.Predatory)
            {
                Hunger = randomParams.Next(5, 75);
                Age = 1;
                Speed = randomParams.Next(5, 15);
                HP = randomParams.Next(50, 200);
                VisionField = 20;
                ReproduceTime = 0;
                ReproduceTries = randomParams.Next(1, 3);
            }

            Delta = 0.1f;
        }

        public void UpdateStates(float deltaTime)
        {
            stateUpdateTimer += deltaTime;

            if (stateUpdateTimer >= stateUpdateInterval)
            {
                Age += stateUpdateTimer;
                Hunger += stateUpdateTimer * 10;
                ReproduceTime += stateUpdateTimer * 5;

                if (Hunger >= 150 || Age >= 1500)
                {
                    Die();
                }

                if (Hunger <= 80)
                {
                    AI.Learn(AnimalAI.Action.EatPlant, +10f);
                    TryReproduce();
                }

                stateUpdateTimer = 0f;
            }
        }

        private void Die()
        {
            IsDead = true;
            CurrentState = AnimalState.Dying;
        }

        private Animal Mutate()
        {
            Animal offspring = new Animal(this.TypeOfAnimal);

            System.Random rand = new System.Random();

            float MutateValue(float value, float mutationRangePercent)
            {
                float delta = value * (mutationRangePercent / 100f);
                return value + UnityEngine.Random.Range(-delta, delta);
            }

            offspring.Speed = Mathf.Clamp(MutateValue(this.Speed, 10f), 1f, 20f);
            offspring.HP = Mathf.Clamp(MutateValue(this.HP, 10f), 10f, 500f);
            offspring.VisionField = Mathf.Clamp(MutateValue(this.VisionField, 10f), 5f, 50f);

            offspring.Age = 0;
            offspring.Hunger = 0;

            return offspring;
        }

        public Animal TryReproduce()
        {
            if (ReproduceTries <= 4)
            {
                if (ReproduceTime < 55f) return null;

                ReproduceTime = 0f;

                Animal child = Mutate();
                ReproduceTries++;
                return child;
            }
            return null;
        }

        public float BeEaten()
        {
            if (IsDead) return 0f;

            float nutrition = NutritionValue * Age;
            Die();
            return nutrition;
        }

        public void TakeDamage(float amount)
        {
            HP -= amount;
            if (HP <= 0)
            {
                Die();
            }
        }
    }
}
