using System;
using UnityEngine.SceneManagement;

namespace Lib
{
    public class Levels
    {

        public enum Type
        {
            Story, Experiment, MainMenu, LevelSelect, HighscoreOverview, Unknown
        }
        
        public const string MainMenu = "Main Menu";
        public const string LevelSelect = "Level Select";
        public const string HighscoreOverview = "Score Overview";

        public static Type CurrentLevelType()
        {
            if (SceneManager.GetActiveScene().name == MainMenu) return Type.MainMenu;
            if (SceneManager.GetActiveScene().name == LevelSelect) return Type.LevelSelect;
            if (SceneManager.GetActiveScene().name == HighscoreOverview) return Type.HighscoreOverview;
            if (Story.IsStoryLevel()) return Type.Story;
            // TODO: experiments
            return Type.Unknown;
        }

        public static bool IsSimulationLevel()
        {
            return SceneManager.GetActiveScene().name != MainMenu;
        }
    
        public static class Story
        {
            public const string Intro = "0_Intro";
            public const string Level1 = "1_Level1";
            public const string Level2 = "2_Level2";
            public const string Level3 = "3_Level3";
            public const string Level4 = "4_Level4";
            public const string Level5 = "5_Level5";
            public const string Level6 = "6_Level6";
            public const string Level7 = "7_Level7";

            public static readonly string[] LevelList =
            {
                Intro, Level1, Level2, Level3, Level4, Level5, Level6, Level7
            };
            
            public static string GetStoryLevel(int levelIndex)
            {
                if (levelIndex > LevelList.Length || levelIndex < 0) throw new IndexOutOfRangeException();
        
                return LevelList[levelIndex];
            }

            /// <summary>
            /// Returns true if the current level is a story level.
            /// </summary>
            /// <seealso cref="Levels.CurrentLevelType"/>
            public static bool IsStoryLevel()
            {
                return SceneManager.GetActiveScene().path.Contains("Story");
            }

            /// <summary>
            /// Returns true if the current level is the last story level. (Last level in <see cref="LevelList"/>)
            /// </summary>
            public static bool IsLastStoryLevel()
            {
                return SceneManager.GetActiveScene().name.Equals(LevelList[LevelList.Length - 1]);
            }

            /// <summary>
            /// Returns the index of the current story level in the level list
            /// </summary>
            /// <seealso cref="GetCurrentStoryLevelSceneNumber"/>
            /// <returns>-1 if invalid. Levellist Index otherwise</returns>
            /// <exception cref="InvalidOperationException"></exception>
            public static int GetCurrentStoryLevelIndex()
            {
                if (!IsStoryLevel()) throw new InvalidOperationException();
        
                var name = SceneManager.GetActiveScene().name;

                for (int i = 0; i < LevelList.Length; i++)
                {
                    if (LevelList[i].Equals(name)) return i;
                }

                return -1;
            }

            /// <summary>
            /// Returns the scene number of the given story level. Meaning the number that is part of the filename not the index in the level list.
            /// <seealso cref="GetCurrentStoryLevelIndex"/>
            /// </summary>
            /// <returns>-1 if invalid. Scene number otherwise</returns>
            public static int GetCurrentStoryLevelSceneNumber()
            {
                if (!IsStoryLevel()) throw new InvalidOperationException();
        
                var name = SceneManager.GetActiveScene().name;
                
                var underScore = name.IndexOf("_", StringComparison.Ordinal);
                if (underScore < 0) return -1;
                var strNumber = name.Substring(0, underScore);
        
                return int.Parse(strNumber);
            }

            /// <summary>
            /// Returns the next story level if any.
            /// </summary>
            /// <exception cref="IndexOutOfRangeException"> if there is no next story level</exception>
            public static string GetNextStoryLevel()
            {
                return GetStoryLevel(GetCurrentStoryLevelIndex() + 1);
            }
            
            public static int GetStoryLevelSceneNumber(int levelIndex)
            {
                var name = GetStoryLevel(levelIndex);
                return GetStoryLevelSceneNumber(name);
            }

            public static int GetStoryLevelSceneNumber(string name)
            {
                var underScore = name.IndexOf("_", StringComparison.Ordinal);
                if (underScore < 0) return -1;
                var strNumber = name.Substring(0, underScore);
        
                return int.Parse(strNumber);
            }
        }
    }
}
