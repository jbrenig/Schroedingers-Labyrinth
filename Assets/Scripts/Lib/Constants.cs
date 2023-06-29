namespace Lib
{
    public static class Constants
    {
        /// <summary>
        /// Planck-Constant in J*s (or m^2 kg / s)
        /// </summary>
        public const double H = 6.62607015e-34;

        /// <summary>
        /// Planck Constant divided by 2 PI (in J*s)
        /// </summary>
        public const double HBar = H / (2 * System.Math.PI);
        
        public const double HBarSq = HBar * HBar;

        public const double Coulomb = 8.987511792314e9;
        
        public const double Epsilon0 = 8.854187812813e-12;
        
        /// <summary>
        /// One Electron volt in J
        /// </summary>
        public const double ElectronVolt = 1.602176634e-19;
        
        public static class Mass
        {
            /// <summary>
            /// Electron mass in kg
            /// </summary>
            public const double Electron = 9.10938356e-31;
        }

        public static class Scale
        {
            public const double Femto = 1e-15;
            public const double Pico = 1e-12;
            public const double Nano = 1e-9;
            public const double Micro = 1e-6;
            public const double Milli = 1e-3;
        }
    }
}