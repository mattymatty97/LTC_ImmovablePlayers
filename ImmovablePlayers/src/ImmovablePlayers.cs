using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using ImmovablePlayers.Dependency;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace ImmovablePlayers
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", Flags:BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mattymatty.LobbyControl",  Flags:BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mattymatty.MattyFixes",  Flags:BepInDependency.DependencyFlags.SoftDependency)]
    internal class ImmovablePlayers : BaseUnityPlugin
    {
        public const string GUID = "mattymatty.ImmovablePlayers";
        public const string NAME = "ImmovablePlayers";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;
        
        private void Awake()
        {
            Log = Logger;
            try
            {
                    if (LobbyCompatibilityChecker.Enabled)
                        LobbyCompatibilityChecker.Init();
                    if (AsyncLoggerProxy.Enabled)
                        AsyncLoggerProxy.WriteEvent(NAME, "Awake", "Initializing");
                    
                    Log.LogInfo("Patching Methods");
                    var harmony = new Harmony(GUID);
                    harmony.PatchAll();
                    Log.LogInfo(NAME + " v" + VERSION + " Loaded!");
                    if (AsyncLoggerProxy.Enabled)
                        AsyncLoggerProxy.WriteEvent(NAME, "Awake", "Finished Initializing");
            }
            catch (Exception ex)
            {
                Log.LogError("Exception while initializing: \n" + ex);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        private static IEnumerable<CodeInstruction> StopPlayerPush(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var fldInfo = typeof(StartOfRound).GetField(nameof(StartOfRound.playersMask));
            
            Log.LogDebug($"fldInfo is {fldInfo}");
            for (var i = 0; i < codes.Count; i++)
            {
                var curr = codes[i];

                if (curr.LoadsField(fldInfo))
                {
                    var next = codes[i + 2];
                    if (next.IsStloc())
                    {
                        codes.InsertRange(i+1, new[]
                        {
                            new CodeInstruction(OpCodes.Pop)
                            {
                                blocks = next.blocks
                            },
                            new CodeInstruction(OpCodes.Ldc_I4_0)
                            {
                                blocks = next.blocks
                            }
                        });
                        ImmovablePlayers.Log.LogDebug("Patched Player Push!");
                        break;
                    }
                }
            }

            return codes;
        }

    }
}