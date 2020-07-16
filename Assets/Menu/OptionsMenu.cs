using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class OptionsMenu : MonoBehaviour
    {
        public GameObject optionsMenu;
        public GameObject mainMenu;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown qualityDropdown;
        public Toggle fullscreenToggle;
        public Slider mouseSensitivitySlider;
        public List<Resolution> resolutions;

        void Start()
        {
            resolutions = Screen.resolutions.Reverse().Where(r => r.refreshRate == Screen.currentResolution.refreshRate)
                .ToList();

            resolutionDropdown.ClearOptions();

            var currentResolutionIndex = resolutions.FindIndex(r =>
                Screen.currentResolution.height == r.height && Screen.currentResolution.width == r.width);

            resolutionDropdown.AddOptions(resolutions.Select(r => r.width + "x" + r.height).ToList());
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            fullscreenToggle.isOn = Screen.fullScreen;
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            if (PlayerPrefs.HasKey("mouseSensitivity"))
            {
                mouseSensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity");
            }
            else
            {
                PlayerPrefs.SetFloat("mouseSensitivity", 100);
                mouseSensitivitySlider.value = 100;
            }
        }

        public void Back()
        {
            optionsMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void Quality(int index)
        {
            QualitySettings.SetQualityLevel(index);
        }

        public void Fullscreen(bool fullScreen)
        {
            Screen.fullScreen = fullScreen;
        }

        public void ScreenResolution(int index)
        {
            var resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void MouseSensitivity(Single value)
        {
            PlayerPrefs.SetFloat("mouseSensitivity", value);
        }
    }
}