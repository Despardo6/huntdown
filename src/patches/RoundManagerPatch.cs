using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using static Huntdown.Mission;
using static Huntdown.Huntdown;
using static Huntdown.ConfigSettings;

//test

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        public static int CalculateTotalWeight()
        {
            _logger.LogInfo("Calculating total weight of all missions...");
            var total = 0;
            for (int i = 0; i < _possibleMissions.Length; i++)
            {
                if (_possibleMissions[i].Enabled)
                {
                    _logger.LogInfo("Adding " + _possibleMissions[i].Name + "'s weight of " + _possibleMissions[i].Weight + " to total weight.");
                    total += _possibleMissions[i].Weight;
                }
                else
                {
                    _logger.LogInfo(_possibleMissions[i].Name + " is a disabled mission, not adding weight.");
                }
                _logger.LogInfo("New total weight: " + total + ".");
            }
            _logger.LogInfo("Final total weight: " + total + ".");
            return total;
        }

        public static bool ChooseRandomMission(int weightTotal)
        {
            _logger.LogInfo("Choosing random mission...");
            var rand = new System.Random();
            int roll = rand.Next(0, weightTotal);
            bool targetChosen = false;
            for (int i = 0; i < _possibleMissions.Length; i++)
            {
                if (_possibleMissions[i].Enabled)
                {
                    _logger.LogInfo("Rolling for the mission " + _possibleMissions[i].Name + ".");
                    _logger.LogInfo("Rolled " + (roll - _possibleMissions[i].Weight));

                    if (((roll -= _possibleMissions[i].Weight) < 0) && (!targetChosen))
                    {
                        targetChosen = true;
                        _currentMission = _possibleMissions[i];
                        if ((bool)ConfigEntries[(int)ConfigIndexes.ToggleWeightDynamic].BoxedValue)
                        {
                            _possibleMissions[i].Weight = _possibleMissions[i].BaseWeight;
                        }
                        _logger.LogInfo("Mission chosen: " + _currentMission.Name + ".");
                    }
                    else
                    {
                        if ((bool)ConfigEntries[(int)ConfigIndexes.ToggleWeightDynamic].BoxedValue)
                        {
                            _possibleMissions[i].Weight += _possibleMissions[i].BaseWeight;
                        }
                    }
                }
            }
            return targetChosen;
        }

        // Checks if user is a host to prevent weirdness
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void SetIsHost()
        {
            _isHost = RoundManager.Instance.NetworkManager.IsHost;
            _logger.LogInfo("Is the player the host: " + _isHost + ".");

            if (_currentMission != null)
            {
                _currentMission.End();
            }
        }

        // Called after the vanilla game begins spawning enemies, intended to spawn the hunt target
        [HarmonyPatch("BeginEnemySpawning")]
        [HarmonyPostfix]
        public static void SpawnTargetPatch(ref SelectableLevel ___currentLevel, ref EnemyVent[] ___allEnemyVents)
        {
            // Ensures that a client is not using the mod to stop weird things from happening (I don't think anything bad would happen anyway but yeah)
            if (!_isHost || !RollMissionChance())
            {
                _currentMission = null;
                return;
            }
            
            //enemyAIList.Clear();

            TotalWeight = CalculateTotalWeight();

            if (!ChooseRandomMission(TotalWeight))
            {
                _logger.LogError("Error: Somehow no mission was selected, do you have all your missions disabled?.");
                return;
            }

            try
            {
                _logger.LogInfo("Attempting to generate mission '" + _currentMission.Name + "'.");
                _currentMission.Start(ref ___currentLevel, ___allEnemyVents);
                _logger.LogInfo("Mission '" + _currentMission.Name + "' generated successfully.");
            }
            catch
            (Exception ex)
            {
                _logger.LogError("There was an error when trying to generate the mission.\n" + ex.Message);
            }

            // Displays the target to the players in the chat
            if ((int)ConfigEntries[(int)ConfigIndexes.DisplayTarget].BoxedValue == (int)DisplayTargetSettings.Full)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=purple>TARGET: </color><color=white>" + _currentMission.Name + "</color>", -1);
            }
            else if ((int)ConfigEntries[(int)ConfigIndexes.DisplayTarget].BoxedValue == (int)DisplayTargetSettings.Some)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=purple>TARGET: </color><color=white>???</color>", -1);
            }

            // If the target has been hunted or not
            //bHunted = false;

            if ((int)ConfigEntries[(int)ConfigIndexes.DisplayTarget].BoxedValue == (int)DisplayTargetSettings.Full)
            {
                _terminalNode.displayText = "Your team's mission: " + _currentMission.Name;
            }
            else
            {
                _terminalNode.displayText = "You have the 'display target' setting off in your configuration, so your mission cannot be displayed using this command.";
            }
        }

        // Called every frame after other vanilla game stuff, intended to check if the hunt target has been killed
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void CheckEnemyDead()
        {
            // If the target has not already been hunted
            if ((_currentMission != null) && _isHost)
            {
                // Loops through every spawned enemy in the round
                //foreach (EnemyAI spawnedEnemy in RoundManager.Instance.SpawnedEnemies)
                //{

                for (int i = 0; i < AliveEnemies.Count; i++)
                {
                    // If the current spawned enemy in the round which is being checked is the hunt target, and the hunt target is dead
                    if (AliveEnemies[i].isEnemyDead)
                    {
                        try
                        {
                            _lastEnemyKilled = AliveEnemies[i];
                            AliveEnemies.RemoveAt(i);
                            HUDManager.Instance.AddTextToChatOnServer("(" + (_maxEnemies - AliveEnemies.Count) + "/" + _maxEnemies + ") <color=purple>TARGETS TERMINATED</color>") ;
                            _logger.LogInfo("The mission enemy '" + _lastEnemyKilled.name + "' was killed.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("A mission enemy was killed, but there was an error.\n" + ex.Message);
                        }
                        break;
                    }
                }

                if (AliveEnemies.Count == 0)
                {

                    _logger.LogInfo("Mission '" + _currentMission.Name + "'has been completed!");

                    if ((int)ConfigEntries[(int)ConfigIndexes.DisplayTarget].BoxedValue == (int)DisplayTargetSettings.Full)
                    {
                        _terminalNode.displayText = "Your team currently has no mission.";
                    }
                    else
                    {
                        _terminalNode.displayText = "You have the 'display target' setting off in your configuration, so your mission cannot be displayed using this command.";
                    }

                    var rand = new System.Random();
                    int x = rand.Next(0, _currentMission.RewardPool.Rewards.Count);
                    StoredItem reward = _currentMission.RewardPool.Rewards[x];

                    GameObject obj = Object.Instantiate(reward.GameItem.spawnPrefab, _lastEnemyKilled.serverPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                    GrabbableObject grabbableObj = obj.GetComponent<GrabbableObject>();
                    grabbableObj.transform.rotation = Quaternion.Euler(grabbableObj.itemProperties.restingRotation);
                    grabbableObj.fallTime = 0f;

                    var rand2 = new System.Random();
                    int y = rand.Next(0, 11) - 5;

                    if (reward.GameItem.isScrap)
                    {
                        if (_currentMission.RewardPool.Value + y > 0)
                        {
                            grabbableObj.SetScrapValue(_currentMission.RewardPool.Value + y);
                        }
                        else
                        {
                            grabbableObj.SetScrapValue(_currentMission.RewardPool.Value);
                        }
                    }

                    try
                    {
                        NetworkObject netObj = obj.GetComponent<NetworkObject>();
                        netObj.Spawn();

                        HUDManager.Instance.AddTextToChatOnServer("<color=purple>MISSION COMPLETED</color>\n<color=green>REWARD: </color>" + "<color=white>" + reward.Name + "</color>");

                        if (reward.GameItem.isScrap)
                        {
                            int resultBefore = 0;
                            Array.ForEach(_allScrapValue, value => resultBefore += value);
                            _logger.LogInfo("Value of total scrap before correction: " + resultBefore);

                            _logger.LogInfo("Length of AllScrapNetwork array: " + _allScrapNetwork.Length);
                            NetworkObjectReference[] correctedScrapNetworkArray = new NetworkObjectReference[_allScrapNetwork.Length + 1];
                            _logger.LogInfo("Length of correctedScrapNetworkArray array: " + correctedScrapNetworkArray.Length);
                            _allScrapNetwork.CopyTo(correctedScrapNetworkArray, 0);
                            correctedScrapNetworkArray[_allScrapNetwork.Length] = netObj;
                            int[] correctedScrapArray = new int[_allScrapValue.Length + 1];
                            _allScrapValue.CopyTo(correctedScrapArray, 0);
                            correctedScrapArray[_allScrapValue.Length] = grabbableObj.scrapValue;

                            _logger.LogInfo("Attempting to sync server and client scrap values.");
                            RoundManager.Instance.SyncScrapValuesClientRpc(correctedScrapNetworkArray, correctedScrapArray);

                            int resultAfter = 0;
                            Array.ForEach(correctedScrapArray, value => resultAfter += value);
                            _logger.LogInfo("Value of total scrap after correction: " + resultAfter);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Calculation of new total scrap in level failed.\n" + ex.Message);
                    }

                    _currentMission.End();
                }
            }
        }

        // Stores scrap generated on the level for use in syncing scrap between host and clients later
        [HarmonyPatch("waitForScrapToSpawnToSync")]
        [HarmonyPrefix]
        public static void StoreGeneratedScrap(ref NetworkObjectReference[] spawnedScrap, ref int[] scrapValues)
        {
            if (!_isHost)
            {
                return;
            }

            _logger.LogInfo("Storing values for scrap generated on new level.");

            // Scrap on server
            _allScrapNetwork = spawnedScrap;

            // Scrap on client
            _allScrapValue = scrapValues;
        }
    }
}
