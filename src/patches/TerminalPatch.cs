using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static Huntdown.Huntdown;

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]

        // Stores objects of type SpawnableEnemyWithRarity and SpawnableItemWithRarity which are used in the mod
        public static void StoreObjects(ref Terminal __instance, ref SelectableLevel[] ___moonsCatalogueList)
        {
            // Flag to prevent unnecessary storing of prefabs more than once
            if (!_objectsStored)
            {
                _logger.LogInfo("Attempting to store enemies and items for later use.");

                try
                {
                    List<SpawnableEnemyWithRarity> AllEnemies = new List<SpawnableEnemyWithRarity>();
                    SelectableLevel[] AllMoons = ___moonsCatalogueList;

                    foreach (SelectableLevel val in AllMoons)
                    {
                        AllEnemies.AddRange(val.Enemies);
                        AllEnemies.AddRange(val.OutsideEnemies);
                        foreach (SpawnableEnemyWithRarity enemy in AllEnemies)
                        {
                            if (_storedEnemies.Any(b => b.ID == enemy.enemyType.ToString()))
                            {
                                for (int i = 0; i < _storedEnemies.Length; i++)
                                {
                                    if ((_storedEnemies[i].ID == enemy.enemyType.ToString()) && (_storedEnemies[i].SpawnableEnemy == null))
                                    {
                                        _storedEnemies[i].SpawnableEnemy = enemy;
                                        _storedEnemies[i].Name = enemy.enemyType.enemyName;
                                        _logger.LogInfo("Stored: " + _storedEnemies[i].Name + ".");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                _objectsStored = true;
                _logger.LogInfo("Finished storing enemies and items.");
            }

            try
            {
                // Adds command for player to check their target mid-game
                _terminalNode.displayText = "Command not available - you must be the host and start the round to use this command.";
                _terminalNode.clearPreviousText = true;

                _terminalKeyword.word = "target";
                _terminalKeyword.specialKeywordResult = _terminalNode;

                TerminalKeyword[] newKeywordArray = new TerminalKeyword[__instance.terminalNodes.allKeywords.Length + 1];
                __instance.terminalNodes.allKeywords.CopyTo(newKeywordArray, 0);
                newKeywordArray[__instance.terminalNodes.allKeywords.Length] = _terminalKeyword;
                __instance.terminalNodes.allKeywords = newKeywordArray;
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't create terminal 'target' command.\n" + ex.Message);
            }
        }
    }
}
