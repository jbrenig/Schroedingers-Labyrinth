using System;
using UnityEngine;

namespace Game
{
    public static class UserPreferences
    {
        public enum VolumeCategory
        {
            Music, Effects, Speech, Master
        }
        
        private static bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
        }

        private static void SetBool(string key, bool val)
        {
            PlayerPrefs.SetInt(key, val ? 1 : 0);
        }

        private static bool _isInit = false;

        public static void Init()
        {
            _drawGrid = GetBool("drawGrid", false);
            _normalizeEveryFrame = GetBool("normalizeEveryFrame", false);
            _disableDynamicAmbiance = GetBool("disableDynamicAmbience", false);
            _isInit = true;
        }

        private static bool _drawGrid;
        public static bool DrawGrid
        {
            get => _drawGrid;
            set
            {
                if (!_isInit) Init();
                _drawGrid = value;
                SetBool("drawGrid", value);       
            }
        }

        private static bool _normalizeEveryFrame;
        public static bool NormalizeEveryFrame
        {
            get => _normalizeEveryFrame;
            set
            {
                if (!_isInit) Init();
                _normalizeEveryFrame = value;
                SetBool("normalizeEveryFrame", value);
            }
        }

        public static bool IsNotFirstLaunch
        {
            get => GetBool("dataInit");
            set => SetBool("dataInit", value);
        }

        private static bool _disableDynamicAmbiance;
        public static bool DisableDynamicAmbiance
        {
            get => _disableDynamicAmbiance;
            set
            {
                if (!_isInit) Init();
                _disableDynamicAmbiance = value;
                SetBool("disableDynamicAmbience", value);
            }
        }

        public static int GetLevelHighscore(int level)
        {
            return PlayerPrefs.GetInt("highscoreLevel" + level, 0);
        }

        public static void ClearLevelHighscore(int level)
        {
            PlayerPrefs.SetInt("highscoreLevel" + level, 0);
            PlayerPrefs.SetInt("highscoreStars" + level, 0);
        }

        public static int UpdateLevelHighscore(int level, int score)
        {
            var oldScore = GetLevelHighscore(level);
            if (oldScore >= score) return oldScore;
            PlayerPrefs.SetInt("highscoreLevel" + level, score);
            return score;
        }
        
        public static int GetLevelStars(int level)
        {
            return PlayerPrefs.GetInt("highscoreStars" + level, 0);
        }

        public static int UpdateLevelStars(int level, GameController.GameResult stars)
        {
            return UpdateLevelStars(level, (int) stars);
        }
        
        public static int UpdateLevelStars(int level, int stars)
        {
            var oldScore = GetLevelStars(level);
            if (oldScore >= stars) return oldScore;
            PlayerPrefs.SetInt("highscoreStars" + level, stars);
            return stars;
        }

        public static float GetVolumeLin(VolumeCategory category)
        {
            return PlayerPrefs.GetFloat("volume_" + category, 1f);
        }

        public static void SetVolumeLin(VolumeCategory category, float value)
        {
            PlayerPrefs.SetFloat("volume_" + category, value);
        }
        
        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}