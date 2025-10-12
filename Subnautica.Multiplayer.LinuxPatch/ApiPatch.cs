using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Subnautica.API.Features;

namespace Subnautica.Multiplayer.LinuxPatch
{
    public class ApiPatch
    {
        public static string patchVersion = "1.0.2";
            
        private static Harmony _harmony;
        // Subnautica.Client gets loaded before Subnautica.Events causing a crash if patched too early
        private static bool eventsLoaded = false;
        private static bool clientLoaded = false;
        private static bool clientPatched = false;

        static ApiPatch()
        {
            // Log.Info("Subnautica Linux patch initialized.");
            _harmony = new Harmony("com.troplo.subnauticabzmp.linuxpatch.api");
            _harmony.PatchAll();
            
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }
         
    // Some assemblies are not loaded yet, so we can't patch them.
    // Whenever an assembly is loaded, we will see if it's the one we want and patch.
    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        var name = args.LoadedAssembly.GetName().Name;

        if (name == "Subnautica.Events")
            eventsLoaded = true;

        if (name == "Subnautica.Client")
            clientLoaded = true;

        if (eventsLoaded && clientLoaded && !clientPatched)
        {
            clientPatched = true;
            Assembly clientAssembly = args.LoadedAssembly;
            if (clientAssembly.GetName().Name != "Subnautica.Client")
            {
                clientAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Subnautica.Client");
            }

            if (clientAssembly != null)
                TryPatch(clientAssembly);
        }
    }

    
    private static void TryPatch(Assembly assembly)
    {
        if (assembly.GetName().Name == "Subnautica.Client")
        {
            PatchIsDeveloperModeOn(assembly);
            PatchMethod(
                assembly,
                "Subnautica.Client.Core.NetworkServer",
                "StartServer",
                new HarmonyMethod(typeof(ApiPatch).GetMethod(
                    nameof(ReplaceStartServerPort),
                    BindingFlags.NonPublic | BindingFlags.Static
                ))
            );
            PatchMethod(
                assembly,
                "Subnautica.Client.Modules.MultiplayerMainMenu",
                "OnAddServerSaveButtonClick",
                new HarmonyMethod(typeof(ApiPatch).GetMethod(
                    nameof(ReplaceStartServerPort),
                    BindingFlags.NonPublic | BindingFlags.Static
                ))
            );
            PatchMethod(
                assembly,
                "Subnautica.Client.Modules.MultiplayerMainMenu",
                "OnAddServerButtonClick",
                new HarmonyMethod(typeof(ApiPatch).GetMethod(
                    nameof(ReplaceStartServerPort),
                    BindingFlags.NonPublic | BindingFlags.Static
                ))
            );
        }
    }
    
    private static void PatchMethod(Assembly assembly, string typeName, string methodName, HarmonyMethod transpiler)
    {
        var type = assembly.GetType(typeName);
        if (type == null) return;

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (method == null) return;

        _harmony.Patch(method, transpiler: transpiler);
    }
    
    // Generic transpiler factory
    private static IEnumerable<CodeInstruction> CreateTranspiler(IEnumerable<CodeInstruction> instructions, int[] args)
    {
        int originalValue = args[0];
        int newValue = args[1];

        foreach (var instr in instructions)
        {
            if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == originalValue)
                instr.operand = newValue;

            yield return instr;
        }
    }
    
    private static IEnumerable<CodeInstruction> ReplaceStartServerPort(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instr in instructions)
        {
            if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == 666)
                instr.operand = 24032;
            yield return instr;
        }
    }
    
    private static void PatchIsDeveloperModeOn(Assembly assembly)
    {
        var type = assembly.GetType("Subnautica.Client.Synchronizations.Processors.Player.ConsoleCommandProcessor");
        if (type == null) return; // type may not exist in this version

        var method = type.GetMethod("IsDeveloperModeOn", BindingFlags.Instance | BindingFlags.Public);
        if (method == null) return;

        var harmony = new Harmony("com.troplo.subnauticabzmp.devmode");

        // Prefix: always return true and skip the original method
        harmony.Patch(method, prefix: new HarmonyMethod(typeof(ApiPatch).GetMethod(nameof(IsDeveloperModeOnPrefix))));
    }
    
    
    public static bool IsDeveloperModeOnPrefix(ref bool __result)
    {
        __result = true;
        return false; // skip original
    }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.InviteCode), "GetApiUrl")]
    public class InviteCode_GetApiUrl_Patch
    {
        static void Postfix(ref string __result)
        {
            // Log.Info("[InviteCode_GetApiUrl_Patch] called.");
            __result = "http://subnauticamultiplay.troplo.com/api/";
        }
    }

    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "Initialize")]
    public class Netbird_Init_Patch
    {
        static bool Prefix(ref bool __result)
        {
            // Log.Info("[Netbird_Init_Patch] called.");
            __result = true;
            return false; 
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "IsWaitingInstallation")]
    public class Netbird_IsWaitingInstallation_Patch
    {
        static bool Prefix(ref bool __result)
        {
            // Log.Info("[Netbird_IsWaitingInstallation_Patch] called.");
            __result = false;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "IsWaitingLogin")]
    public class Netbird_IsWaitingLogin_Patch
    {
        static bool Prefix(ref bool __result)
        {
            // Log.Info("[Netbird_IsWaitingLogin_Patch] called.");
            __result = false;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "IsAnyError")]
    public class Netbird_IsAnyError_Patch
    {
        static bool Prefix(ref bool __result)
        {
            // Log.Info("[Netbird_IsAnyError_Patch] called.");
            __result = false;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "GetPeerIp")]
    public class Netbird_GetPeerIp_Patch
    {
        static bool Prefix(ref string __result)
        {
            string text = null;
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "-peerIp" && i + 1 < commandLineArgs.Length)
                {
                    text = commandLineArgs[i + 1];
                }
            }
            // Log.Info("Peer id: " + text);
            // Log.Info("[Netbird_GetPeerIp_Patch] called, using peerip " + text);
            __result = text;
            return false;
        }
    }

    [HarmonyPatch(typeof(Subnautica.API.Features.Netbird.Netbird), "GetPeerId")]
    public class Netbird_GetPeerId_Patch
    {
        static bool Prefix(ref string __result)
        {
            string text = null;
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "-peerId" && i + 1 < commandLineArgs.Length)
                {
                    text = commandLineArgs[i + 1];
                }
            }

            // Log.Info("Peer id: " + text);
            // Log.Info("[Netbird_GetPeerId_Patch] called, using peerid " + text);
            __result = text;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "Connect")]
    public class NetBirdApi_Connect_Patch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "ExecuteCommand")]
    public class NetBirdApi_ExecuteCommand_Patch
    {
        static bool Prefix()
        {
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "IsHostConnected")]
    public class NetBirdApi_IsHostConnected_Patch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "IsHostConnectionActive")]
    public class NetBirdApi_IsHostConnectionActive_Patch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "IsReady")]
    public class NetBirdApi_IsReady_Patch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.NetBirdApi), "RemoveAndUpdateInstall")]
    public class NetBirdApi_RemoveAndUpdateInstall_Patch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    // === USERNAME & DISPLAY INFORMATION ===
    [HarmonyPatch(typeof(Subnautica.API.Features.Tools), "GetLoggedInName")]
    public class Tools_GetLoggedInName_Patch
    {
        static bool Prefix(ref string __result)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "-username" && i + 1 < commandLineArgs.Length)
                {
                    __result = commandLineArgs[i + 1];
                    return false;
                }
            }
            __result = "UNSET";
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Subnautica.API.Features.Tools), "GetLoggedId")]
    public class Tools_GetLoggedInId_Patch
    {
        static bool Prefix(ref string __result)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "-userId" && i + 1 < commandLineArgs.Length)
                {
                    __result = commandLineArgs[i + 1];
                    return false;
                }
            }
            
            __result = "UNSET";
            return false;
        }
    }
    
    // [HarmonyPatch(typeof(Subnautica.Client.Synchronizations.Processors.Player.ConsoleCommandProcessor), "IsDeveloperModeOn")]
    // public class Sync_IsDeveloperModeOn_Patch
    // {
    //     static bool Prefix(ref bool __result)
    //     {
    //         __result = true;
    //         return false;
    //     }
    // }
    //
    // === MISC ===
    [HarmonyPatch(typeof(Subnautica.API.Features.Settings), "GetWatermarkText")]
    public class Tools_GetWatermarkText_Patch
    {
        static bool Prefix(ref string __result)
        {
            __result = string.Format("<size=18>{0} (by BOT Benson, Troplo patch v{1})</size>", Tools.GetLauncherVersion(true, true), ApiPatch.patchVersion);
            return false;
        }
    }
    
    // === SERVER & CLIENT ===
    //
    // [HarmonyPatch(typeof(Subnautica.Client.Core.NetworkServer), "StartServer")]
    // class Patch_StartServer_Port
    // {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         foreach (var instr in instructions)
    //         {
    //             if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == 666)
    //             {
    //                 instr.operand = 24032;
    //             }
    //
    //             yield return instr;
    //         }
    //     }
    // }
    
    // [HarmonyPatch(typeof(Subnautica.Client.Modules.MultiplayerMainMenu), "OnAddServerButtonClick")]
    // class Patch_OnAddServerButtonClick_Port
    // {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         foreach (var instr in instructions)
    //         {
    //             if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == 666)
    //             {
    //                 instr.operand = 24032;
    //             }
    //
    //             yield return instr;
    //         }
    //     }
    // }
    //
    // [HarmonyPatch(typeof(Subnautica.Client.Modules.MultiplayerMainMenu), "OnAddServerSaveButtonClick")]
    // class Patch_OnAddServerSaveButtonClick_Port
    // {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         foreach (var instr in instructions)
    //         {
    //             if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == 666)
    //             {
    //                 instr.operand = 24032;
    //             }
    //
    //             yield return instr;
    //         }
    //     }
    // }
}