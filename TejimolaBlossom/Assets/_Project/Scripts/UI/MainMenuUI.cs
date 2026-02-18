using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tejimola.Core;

namespace Tejimola.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button extrasButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject extrasPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image nahorTreeImage;

        [Header("Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Toggle fullscreenToggle;

        void Start()
        {
            SetupButtons();
            SetupTitle();
            CheckContinueButton();
            LoadSettings();
            AudioManager.Instance.PlayMusic("Audio/Music/menu", 1f);
        }

        void SetupTitle()
        {
            if (titleText != null)
            {
                titleText.text = "TEJIMOLA";
                titleText.fontSize = 72;
            }
            if (subtitleText != null)
            {
                subtitleText.text = "THE BLOSSOM FROM CLAY";
                subtitleText.fontSize = 28;
            }
        }

        void SetupButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGame);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinue);
            if (extrasButton != null)
                extrasButton.onClick.AddListener(OnExtras);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);
        }

        void CheckContinueButton()
        {
            if (continueButton != null)
                continueButton.interactable = SaveManager.Instance.HasSaveFile();
        }

        void OnNewGame()
        {
            GameManager.Instance.StartNewGame();
        }

        void OnContinue()
        {
            SaveManager.Instance.LoadGame();
        }

        void OnExtras()
        {
            mainPanel?.SetActive(false);
            extrasPanel?.SetActive(true);
        }

        void OnQuit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        public void OnBackToMain()
        {
            settingsPanel?.SetActive(false);
            extrasPanel?.SetActive(false);
            creditsPanel?.SetActive(false);
            mainPanel?.SetActive(true);
        }

        public void OnOpenSettings()
        {
            mainPanel?.SetActive(false);
            settingsPanel?.SetActive(true);
        }

        public void OnOpenCredits()
        {
            extrasPanel?.SetActive(false);
            creditsPanel?.SetActive(true);
        }

        void LoadSettings()
        {
            GameSettings settings = SaveManager.Instance.LoadSettings();
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.masterVolume;
                masterVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetMasterVolume(v));
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetMusicVolume(v));
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetSFXVolume(v));
            }
            if (subtitlesToggle != null)
                subtitlesToggle.isOn = settings.subtitlesEnabled;
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = settings.fullscreen;
                fullscreenToggle.onValueChanged.AddListener(v => Screen.fullScreen = v);
            }

            // Apply settings
            AudioManager.Instance.SetMasterVolume(settings.masterVolume);
            AudioManager.Instance.SetMusicVolume(settings.musicVolume);
            AudioManager.Instance.SetSFXVolume(settings.sfxVolume);
        }

        public void SaveSettings()
        {
            GameSettings settings = new GameSettings
            {
                masterVolume = masterVolumeSlider != null ? masterVolumeSlider.value : 1f,
                musicVolume = musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f,
                sfxVolume = sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.8f,
                subtitlesEnabled = subtitlesToggle != null && subtitlesToggle.isOn,
                fullscreen = fullscreenToggle != null && fullscreenToggle.isOn
            };
            SaveManager.Instance.SaveSettings(settings);
        }
    }
}
