using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using static Huntdown.Huntdown;

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(HoarderBugAI))]
    internal class HoarderBugAIPatch
    {

        private static float _blunderbugShootTimer = 3f;
        private static float _blunderbugAngryTimer= 0f;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void WeaponiseBugs(HoarderBugAI __instance)
        {
            if (!_isHost)
            {
                return;
            }

            for (int i = 0; i < Mission.AliveEnemies.Count; i++)
            {
                if (__instance == Mission.AliveEnemies[i])
                {
                    if (_currentMission.Name == "Bug Mafia")
                    {
                        for (int j = 0; j < StartOfRound.Instance.allItemsList.itemsList.Count; j++)
                        {
                            if (StartOfRound.Instance.allItemsList.itemsList[j].ToString() == _storedItems[(int)ItemKey.Shovel].ID)
                            {
                                GameObject obj = Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[j].spawnPrefab, __instance.serverPosition, Quaternion.identity, StartOfRound.Instance.propsContainer);
                                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                                

                                GrabbableObject component = netObj.gameObject.GetComponent<GrabbableObject>();
                                __instance.targetItem = null;
                                //__instance.targetItem = component;
                                HoarderBugAI.HoarderBugItems.Add(new HoarderBugItem(component, HoarderBugItemStatus.Owned, __instance.nestPosition));
                                __instance.heldItem = HoarderBugAI.HoarderBugItems[HoarderBugAI.HoarderBugItems.Count - 1];
                                //__instance.heldItem = null;
                                component.parentObject = __instance.grabTarget;
                                component.hasHitGround = false;
                                component.isHeldByEnemy = true;
                                component.grabbableToEnemies = false;
                                component.grabbable = true;
                                component.GrabItemFromEnemy(__instance);
                                component.EnablePhysics(enable: false);
                                netObj.Spawn();
                                __instance.GrabItemServerRpc(netObj);
                            }
                        }
                    }

                    else if (_currentMission.Name == "Blunderbug")
                    {
                        for (int j = 0; j < StartOfRound.Instance.allItemsList.itemsList.Count; j++)
                        {
                            if (StartOfRound.Instance.allItemsList.itemsList[j].ToString() == _storedItems[(int)ItemKey.Shotgun].ID)
                            {
                                GameObject obj = Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[j].spawnPrefab, __instance.serverPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                                

                                GrabbableObject component = netObj.gameObject.GetComponent<GrabbableObject>();
                                __instance.targetItem = null;
                                //__instance.targetItem = component;
                                HoarderBugAI.HoarderBugItems.Add(new HoarderBugItem(component, HoarderBugItemStatus.Owned, __instance.nestPosition));
                                __instance.heldItem = HoarderBugAI.HoarderBugItems[HoarderBugAI.HoarderBugItems.Count - 1];
                                //__instance.heldItem = null;
                                component.parentObject = __instance.grabTarget;
                                component.hasHitGround = false;
                                component.isHeldByEnemy = true;
                                component.grabbableToEnemies = false;
                                component.grabbable = false;
                                component.GrabItemFromEnemy(__instance);
                                component.EnablePhysics(enable: false);
                                netObj.Spawn();
                                __instance.GrabItemServerRpc(netObj);
                            }
                        }
                    }
                }
            }
        }

        /* [HarmonyPatch("GrabItem")]
        [HarmonyPrefix]
        public static bool ForcePreventGrab(HoarderBugAI __instance)
        {
            if (!_isHost)
            {
                return true;
            }

            for (int i = 0; i < Mission.AliveEnemies.Count; i++)
            {
                if (__instance == Mission.AliveEnemies[i] && (_currentMission.Name == "Bug Mafia" || _currentMission.Name == "Blunderbug"))
                {
                    // logger.LogInfo("Trying to grab something, but preventing it.");
                    return false;
                }
            }
            return true;
        }*/

        [HarmonyPatch("DropItem")]
        [HarmonyPrefix]
        public static bool ForcePreventDrop(HoarderBugAI __instance)
        {
            if (!_isHost)
            {
                return true;
            }

            for (int i = 0; i < Mission.AliveEnemies.Count; i++)
            {
                if (__instance == Mission.AliveEnemies[i] && !__instance.isEnemyDead && (_currentMission.Name == "Bug Mafia" || _currentMission.Name == "Blunderbug"))
                {
                    // logger.LogMessage("Tried to drop item, but it was covered in glue!");
                    // ___waitingAtNest = false;
                    __instance.targetItem = null;
                    return false;
                }
            }
            return true;
        }

        private static void ShootGun(ShotgunItem item, HoarderBugAI instance)
        {
            if (item.safetyOn) { item.safetyOn = false; }
            if (item.shellsLoaded == 0) { item.shellsLoaded = 2; }

            Vector3 position = (instance.watchingPlayer).transform.position;
            Vector3 forward = item.shotgunRayPoint.forward;
            item.ShootGunServerRpc(position, forward);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void BlunderShoot(HoarderBugAI __instance)
        {
            for (int i = 0; i < Mission.AliveEnemies.Count; i++)
            {
                if (__instance == Mission.AliveEnemies[i] && _currentMission.Name == "Blunderbug")
                {
                    //_logger.LogInfo("blunderbug has existed for a frame");
                    _blunderbugShootTimer += Time.deltaTime;
                    //_logger.LogInfo("blunderbug has existed for a frame_2");

                    if (__instance.heldItem != null)
                    {
                        //_logger.LogInfo("target item exists");
                        GrabbableObject obj = __instance.heldItem.itemGrabbableObject;

                        if ((obj.GetComponent<ShotgunItem>() != null) && (__instance.angryTimer > 0f))
                        {
                            _blunderbugAngryTimer += Time.deltaTime;

                            if (_blunderbugAngryTimer >= 1f && _blunderbugShootTimer >= 3f)
                            {
                                ShootGun((ShotgunItem)obj, __instance);
                                _blunderbugShootTimer = 0f;
                            }
                        }
                        else if (__instance.angryTimer <= 0)
                        {
                            _blunderbugAngryTimer = 0f;
                        }
                    }
                }
            }
        }
    }
}
