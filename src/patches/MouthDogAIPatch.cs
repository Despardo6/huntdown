using GameNetcodeStuff;
using HarmonyLib;
using static Huntdown.Huntdown;
using UnityEngine;
using Unity.Netcode;

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(MouthDogAI))]
    internal class MouthDogAIPatch
    {
        public static void EnterLunge(MouthDogAI __instance, Ray ray, RaycastHit rayHit, RoundManager roundManager)
        {
            __instance.SwitchToBehaviourState(3);
            __instance.endingLunge = false;
            ray = new Ray(__instance.transform.position + Vector3.up, __instance.transform.forward);
            Vector3 pos = ((!Physics.Raycast(ray, out rayHit, 17f, StartOfRound.Instance.collidersAndRoomMask)) ? ray.GetPoint(17f) : rayHit.point);
            pos = roundManager.GetNavMeshPosition(pos);
            __instance.SetDestinationToPosition(pos);
            __instance.agent.speed = 13f;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        public static bool FixIndoorDogBehaviourForClients(MouthDogAI __instance, ref Collider other, bool ___inKillAnimation, ref Collider ___debugCollider, ref bool ___inLunge, Ray ___ray, RaycastHit ___rayHit, RoundManager ___roundManager)
        {
            if (!__instance.enemyType.isOutsideEnemy && _isHost)
            {
                if (!__instance.isEnemyDead && !___inKillAnimation && !(__instance.stunNormalizedTimer >= 0f))
                {
                    PlayerControllerB playerControllerB = other.gameObject.GetComponent<PlayerControllerB>();
                    if(!(playerControllerB != null))
                    {
                        _logger.LogError("Player returned null");
                        return false;
                    }

                    Vector3 vector = Vector3.Normalize((__instance.transform.position + Vector3.up - playerControllerB.gameplayCamera.transform.position) * 100f);
                    if (Physics.Linecast(__instance.transform.position + Vector3.up + vector * 0.5f, playerControllerB.gameplayCamera.transform.position, out var hitInfo, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                    {
                        if (!(hitInfo.collider == ___debugCollider))
                        {
                            Debug.Log("Eyeless dog collide, linecast obstructed: " + hitInfo.collider.gameObject.name);
                            ___debugCollider = hitInfo.collider;
                            return false;
                        }
                        return false;
                    }
                    else if (__instance.currentBehaviourStateIndex == 3)
                    {
                        playerControllerB.inAnimationWithEnemy = __instance;
                        __instance.KillPlayerServerRpc((int)playerControllerB.playerClientId);
                        return false;
                    }
                    else if (__instance.currentBehaviourStateIndex == 0 || __instance.currentBehaviourStateIndex == 1)
                    {
                        __instance.SwitchToBehaviourState(2);
                        __instance.ChangeOwnershipOfEnemy(NetworkManager.Singleton.LocalClientId);
                        __instance.SetDestinationToPosition(playerControllerB.transform.position);
                        return false;
                    }
                    else if (__instance.currentBehaviourStateIndex == 2 && !___inLunge)
                    {
                        __instance.transform.LookAt(other.transform.position);
                        __instance.transform.localEulerAngles = new Vector3(0f, __instance.transform.eulerAngles.y, 0f);
                        ___inLunge = true;
                        EnterLunge(__instance, ___ray, ___rayHit, ___roundManager);
                        return false;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
