using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject optionsMenu;
        public TMP_InputField nicknameInputField;

        private void Start()
        {
            if (PlayerPrefs.HasKey("nickname"))
            {
                nicknameInputField.text = PlayerPrefs.GetString("nickname");
            }
        }

        private void EnterValidNickname()
        {
            nicknameInputField.text = "";
            nicknameInputField.placeholder.GetComponent<TMP_Text>().color = Color.red;
        }
        public void Play()
        {
            if (!PlayerPrefs.HasKey("nickname") || PlayerPrefs.GetString("nickname").Trim().Length == 0)
            {
                EnterValidNickname();
                return;
            }
            SceneManager.LoadScene("GameWithMap");
        }
        public void Options()
        {
            gameObject.SetActive(false);
            optionsMenu.SetActive(true);
        }
        public void Quit()
        {
            Application.Quit();
        }

        public void Nickname(string value)
        {
            PlayerPrefs.SetString("nickname", value);
        }
    }
}
