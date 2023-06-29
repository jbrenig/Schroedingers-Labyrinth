namespace Game
{
    public static class SessionStorage
    {
        public static bool IsSceneRestart { get; set; } = false;
        public static int LevelRestartCounter = 0;
    }
}