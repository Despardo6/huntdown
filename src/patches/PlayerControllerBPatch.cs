using HarmonyLib;
using static Huntdown.Huntdown;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using UnityEngine;
using static Huntdown.Patches.RoundManagerPatch;

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void DebugMode(ref float ___sprintMeter, ref float ___sprintMultiplier)
        {
            if (_isHost)
            {
                ___sprintMeter = 1f;
                ___sprintMultiplier = 3f;
                
                if (Keyboard.current.digit1Key.wasPressedThisFrame && _framesPassedSinceInput >= 60)
                {
                    _framesPassedSinceInput = 0;
                    _logger.LogInfo("Spawning DEBUG target.");
                    SpawnTargetPatch(ref _currentLevel, ref _allEnemyVents);
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame && _framesPassedSinceInput >= 60)
                {
                    _framesPassedSinceInput = 0;
                    _logger.LogInfo("Killing DEBUG target.");
                    // enemyAI.KillEnemyClientRpc(false);

                    for (int i = 0; i < Mission.AliveEnemies.Count; i++)
                    {
                        Mission.AliveEnemies[i].KillEnemyClientRpc(false);
                    }
                }

                _framesPassedSinceInput++;
            }
        }

        /*[HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
        [HarmonyPrefix]
        public static void OverrideForDoggo(ref bool ___overrideIsInsideFactoryCheck)
        {
            ___overrideIsInsideFactoryCheck = true;
        }*/
    }

    [HarmonyPatch(typeof(Application))]
    public class ApplicationPatch
    {
        [HarmonyPatch(typeof(Application), "isEditor", MethodType.Getter)]
        [HarmonyPostfix]
        public static void ForceIsEditor(ref bool __result)
        {
            __result = true;
        }
    }
}
