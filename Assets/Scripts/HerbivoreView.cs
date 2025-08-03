using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class HerbivoreView : MonoBehaviour
    {
        public Animal AnimalModel { get; private set; }
        public Renderer Animal;
        private Color originalColor;

        public void Initialize(Animal model)
        {
            AnimalModel = model;
        }

        private void Start()
        {
            if (Animal != null)
                originalColor = Animal.material.color;
        }

        private void Update()
        {
            if (AnimalModel != null && AnimalModel.IsDead)
                Destroy(gameObject);
        }

        public float BeEaten()
        {
            if (AnimalModel == null) return 0f;

            float nutrition = AnimalModel.BeEaten();
            Destroy(gameObject);
            return nutrition;
        }

        public void TakeDamage(float amount)
        {
            AnimalModel?.TakeDamage(amount);
            StartCoroutine(FlashOnHit());
        }

        public void ReceiveAttack(float damage)
        {
            TakeDamage(damage);
        }

        private IEnumerator FlashOnHit()
        {
            if (Animal == null) yield break;

            Color hitColor;

            Animal.material.color = Color.yellow;
            yield return new WaitForSeconds(0.15f);
            if (UnityEngine.ColorUtility.TryParseHtmlString("#00FFD4", out hitColor))
            {
                Animal.material.color = hitColor;
            }
        }
    }
}