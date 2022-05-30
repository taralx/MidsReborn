using System;
using mrbBase.Base.Data_Classes;

namespace mrbBase.Base.Master_Classes
{
    public static class MidsContext
    {
        public const string AppName = "Mids' Reborn";
        private const int AppMajorVersion = 3;
        private const int AppMinorVersion = 3;
        private const int AppBuildVersion = 2;
        private const int AppRevisionVersion = 17;

        public const string AssemblyVersion = "3.3.2";
        public const string AssemblyFileVersion = "3.3.2.17";
        public static Version AppFileVersion { get; set; } = new(AppMajorVersion, AppMinorVersion, AppBuildVersion, AppRevisionVersion);

        public const string AppVersionStatus = "";
        public const string Title = "Mids' Reborn";

        public const int MathLevelBase = 49;
        public const int MathLevelExemp = -1;

        public static bool EnhCheckMode = false;

        //public static readonly Version AppVersion = new Version(AppMajorVersion, AppMinorVersion, AppBuildVersion, AppRevisionVersion);

        public static Archetype? Archetype;
        public static Character Character;
        public static Build Build;

        public static ConfigData Config => ConfigData.Current;
    }
}