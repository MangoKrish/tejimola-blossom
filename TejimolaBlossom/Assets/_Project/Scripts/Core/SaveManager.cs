using System.IO;
using UnityEngine;
using Tejimola.Utils;

namespace Tejimola.Core
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private string SavePath => Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
        private string SettingsPath => Path.Combine(Application.persistentDataPath, GameConstants.SettingsFileName);

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SaveGame()
        {
            SaveData data = GameManager.Instance.GetSaveData();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to {SavePath}");
            EventManager.Instance.Publish(EventManager.Events.GameSaved);
        }

        public bool LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found.");
                return false;
            }

            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            GameManager.Instance.LoadSaveData(data);

            // Load the appropriate scene
            string sceneName = data.currentAct.ToString();
            SceneLoader.Instance.LoadScene(sceneName);

            EventManager.Instance.Publish(EventManager.Events.GameLoaded);
            Debug.Log("Game loaded successfully.");
            return true;
        }

        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        // Settings
        public void SaveSettings(GameSettings settings)
        {
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(SettingsPath, json);
        }

        public GameSettings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
                return new GameSettings();

            string json = File.ReadAllText(SettingsPath);
            return JsonUtility.FromJson<GameSettings>(json);
        }
    }

    [System.Serializable]
    public class GameSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float voiceVolume = 1f;
        public bool subtitlesEnabled = true;
        public int subtitleSize = 1; // 0=small, 1=medium, 2=large
        public bool highContrast = false;
        public string language = "en";
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = true;
    }
}
