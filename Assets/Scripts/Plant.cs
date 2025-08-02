using System;

namespace Assets.Scripts
{
    public class Plant
    {
        public float Growth { get; private set; } = 0f;
        public float MaxGrowthTime { get; private set; }
        public float NutritionValue { get; private set; }
        public bool IsAlive { get; private set; } = true;

        private float age = 0f;
        private float maxLifeTime;

        private float growthRate;

        public Plant(float maxGrowthTime = 30f, float maxLifeTime = 120f, float nutritionValue = 20f)
        {
            this.MaxGrowthTime = maxGrowthTime;
            this.maxLifeTime = maxLifeTime;
            this.NutritionValue = nutritionValue;

            growthRate = 1f / maxGrowthTime;
        }

        public void UpdatePlant(float deltaTime)
        {
            if (!IsAlive) return;

            age += deltaTime;

            if (Growth < 1f)
            {
                Growth += growthRate * deltaTime;
                if (Growth > 1f) Growth = 1f;
            }

            if (age >= maxLifeTime)
            {
                Die();
            }
        }

        public void Die()
        {
            IsAlive = false;
            Growth = 0f;
        }

        public float BeEaten()
        {
            if (!IsAlive) return 0f;

            float nutrition = NutritionValue * Growth;
            Die();
            return nutrition;
        }
    }
}