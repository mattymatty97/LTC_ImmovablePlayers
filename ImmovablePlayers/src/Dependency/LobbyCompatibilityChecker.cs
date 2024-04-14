using System;
using System.Runtime.CompilerServices;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;

namespace ImmovablePlayers.Dependency
{
    public static class LobbyCompatibilityChecker
    {
        public static bool Enabled { get { return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"); } }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            PluginHelper.RegisterPlugin(ImmovablePlayers.GUID, Version.Parse(ImmovablePlayers.VERSION), CompatibilityLevel.ServerOnly, VersionStrictness.None);
        }
        
    }
}