using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PlayerUIController : MonoBehaviour
    {
        private Slider healthSlider;
        private TMP_Text healthValue;
        private Slider bulletsSlider;
        private TMP_Text bulletsValue;
        private Slider staminaSlider;
        private TMP_Text staminaValue;

        void OnEnable()
        {
            healthSlider = transform.Find("Bottom/Container/Health/Slider").GetComponent<Slider>();
            healthValue = transform.Find("Bottom/Container/Health/Slider/Value").GetComponent<TMP_Text>();
            bulletsSlider = transform.Find("Bottom/Container/Bullets/Slider").GetComponent<Slider>();
            bulletsValue = transform.Find("Bottom/Container/Bullets/Slider/Value").GetComponent<TMP_Text>();
            staminaSlider = transform.Find("Bottom/Container/Stamina/Slider").GetComponent<Slider>();
            staminaValue = transform.Find("Bottom/Container/Stamina/Slider/Value").GetComponent<TMP_Text>();
        }

        public void SetHealth(int health)
        {
            healthSlider.value = health;
            healthValue.text = health.ToString();
        }

        public void SetBullets(float bullets, int maxBullets)
        {
            bulletsValue.text = (int) bullets + "/" + maxBullets;
            bulletsSlider.value = bullets - (int) bullets;
        }

        public void SetStamina(float stamina, int maxStamina)
        {
            staminaValue.text = (int) stamina + "/" + maxStamina;
            staminaSlider.value = stamina;
            staminaSlider.maxValue = maxStamina;
        }
    }
}