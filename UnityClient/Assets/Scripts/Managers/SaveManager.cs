using System;
using UnityEngine;
using CuocDuaKyThu.Data;

namespace CuocDuaKyThu.Managers
{
    public class SaveManager : MonoBehaviour
    {
        private const string SettingsKey = "CuocDuaKyThu_Settings";
        
        private GameSettings _settings = new();
        public GameSettings CurrentSettings => _settings;

        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SettingsKey))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SettingsKey);
                    _settings = JsonUtility.FromJson<GameSettings>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveManager] Failed to load settings: {e.Message}. Using default.");
                    _settings = new GameSettings();
                }
            }
            else
            {
                _settings = new GameSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(_settings);
                PlayerPrefs.SetString(SettingsKey, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save settings: {e.Message}");
            }
        }

        public void SaveLastPlayerNames(string[] names)
        {
            _settings.lastPlayerNames = names;
            SaveSettings();
        }
    }
}
