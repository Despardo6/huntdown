using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Huntdown.ConfigSettings;
using static Huntdown.Huntdown;
using Unity.Netcode;

namespace Huntdown
{
    public class Mission
    {
        private readonly string _name;
        private readonly Dictionary<EnemyKey, int> _enemiesToSpawn;
        private static List<EnemyAI> _aliveEnemies = new List<EnemyAI>();
        public static int _maxEnemies;
        private RewardPool _rewardPool;
        private readonly int _baseWeight;
        private int _weight;
        private static int _totalWeight;
        private static readonly int _missionChance = (int)ConfigEntries[(int)ConfigIndexes.GeneralPercentageChance].BoxedValue;
        private bool _enabled;

        private readonly List<EnemyKey> _randomEnemyChoices = new List<EnemyKey>
        {
            EnemyKey.Thumper,
            EnemyKey.BunkerSpider,
            EnemyKey.HoarderBug,
            EnemyKey.SnareFlea,
            EnemyKey.Nutcracker,
            EnemyKey.Masked,
            EnemyKey.Bracken,
            EnemyKey.Maneater
        };

        public Mission(string name, int baseWeight, Dictionary<EnemyKey, int> enemiesToSpawn, bool enabled, RewardPool rewardPool)
        {
            _name = name;
            _baseWeight = baseWeight;
            _weight = baseWeight;
            _enemiesToSpawn = enemiesToSpawn;
            _enabled = enabled;
            _rewardPool = rewardPool;
        }

        public void Start(ref SelectableLevel level, EnemyVent[] allVents)
        {
            _currentLevel = level;
            _allEnemyVents = allVents;
            SpawnEnemiesAtVent(ref level, RollRandomFarVent(allVents));
        }

        public void End()
        {
            if (_currentMission != null)
            {
                _aliveEnemies.Clear();
                _currentMission = null;
            }
        }

        public static EnemyVent RollRandomFarVent(EnemyVent[] allVents)
        {
            _logger.LogInfo("Rolling for a random faraway vent...");

            Vector3 entrancePos = RoundManager.FindMainEntrancePosition();
            float longestDist = 0f;
            List<EnemyVent> allFarVents = new List<EnemyVent>();


            _logger.LogInfo("MainEntrancePos: " + entrancePos);


            for (int i = 0; i < allVents.Length; i++)
            {
                _logger.LogInfo(i + "VentPosEntrancePos: " + allVents[i].transform.position);
                float dist = Vector3.Distance(entrancePos, allVents[i].transform.position);
                if (dist > longestDist)
                {
                    longestDist = dist;
                }
            }

            _logger.LogInfo("longest dist: " + longestDist);

            // Find the midpoint between the entrance and the furthest vent
            Vector3 midpoint = (entrancePos + allVents.Where(vent => Vector3.Distance(entrancePos, vent.transform.position) == longestDist).FirstOrDefault().transform.position) / 2f;

            _logger.LogInfo("midpoint: " + midpoint);

            // Calculate the halfway distance from the entrance to the midpoint
            float halfWayDist = Vector3.Distance(entrancePos, midpoint);

            _logger.LogInfo("halfway dist: " + halfWayDist);

            // Filter vents that are at least halfway from the entrance
            for (int i = 0; i < allVents.Length; i++)
            {
                _logger.LogInfo(i + "VentPosEntrancePos: " + allVents[i].transform.position);
                float dist = Vector3.Distance(entrancePos, allVents[i].transform.position);
                if (dist >= halfWayDist)
                {
                    _logger.LogInfo("dist: " + dist);
                    _logger.LogInfo("halfwaydist " + halfWayDist);
                    allFarVents.Add(allVents[i]);
                }
            }

            var rand = new System.Random();
            int roll = rand.Next(0, allFarVents.Count);
            _logger.LogInfo("Rolled " + roll + " out of " + allFarVents.Count + ".");
            return allFarVents[roll];
        }

        public static bool RollMissionChance()
        {
            _logger.LogInfo("Rolling for mission generation...");
            var rand = new System.Random();
            int roll = rand.Next(0, 100);
            _logger.LogInfo("Rolled " + roll + " out of " + _missionChance + ".");
            _logger.LogInfo("Mission received: " + (roll <= _missionChance) + ".");
            return (roll <= _missionChance);
        }

        private void ForceAIInside(EnemyAI enemyAI)
        {
            _logger.LogInfo("Forcing AI to work indoors.");
            enemyAI.enemyType.isOutsideEnemy = false;
            enemyAI.daytimeEnemyLeaving = false;
            enemyAI.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            // Removed: enemyAI.SyncPositionToClients();  // No longer needed.
        }

        private void SpawnUnnaturalEnemiesAtVent(ref SelectableLevel level, EnemyVent vent, EnemyKey enemyKey, int count)
        {
            _logger.LogInfo("Enemy does not spawn naturally on this moon. Temporarily adding it to spawnable enemies.");
            level.Enemies.Add(_storedEnemies[(int)enemyKey].SpawnableEnemy);
            for (int i = 0; i < count; i++)
            {
                RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, level.Enemies.IndexOf(_storedEnemies[(int)enemyKey].SpawnableEnemy));
                _aliveEnemies.Add(RoundManager.Instance.SpawnedEnemies[RoundManager.Instance.SpawnedEnemies.Count - 1]);
                EnemyAI enemyAI = _aliveEnemies[_aliveEnemies.Count - 1];

                if (enemyAI is MouthDogAI || enemyAI is BaboonBirdAI || enemyAI is ForestGiantAI)
                {
                    _logger.LogInfo("Dog/Baboon/Giant detected, forcing it to work inside.");
                    ForceAIInside(enemyAI);
                    _logger.LogInfo("Forced inside successfully.");
                }

                // --- Conditional Scale and HP Modification ---
                if (_currentMission != null && _currentMission.Name == "Facility Keeper")
                {
                    if (enemyAI != null)
                    {
                        // Calculate scale.
                        Vector3 newScale = (enemyAI.transform.localScale / 4);

                        // 1. Despawn
                        enemyAI.gameObject.GetComponent<NetworkObject>().Despawn(false);

                        // 2. Change Scale and HP
                        enemyAI.transform.localScale = newScale;
                        enemyAI.enemyHP = 5;

                        // 3. Respawn
                        enemyAI.gameObject.GetComponent<NetworkObject>().Spawn();
                    }
                }
                // --- End Conditional Modification ---

                ApplyGiantSizeUpgradedModifiers(enemyAI);
                ApplyTinySizeModifiers(enemyAI);

                ScanNodeProperties scanNode = enemyAI.gameObject.GetComponentInChildren<ScanNodeProperties>();
                if ((bool)ConfigEntries[(int)ConfigIndexes.ChangeSubtext].BoxedValue)
                {
                    scanNode.subText = "<color=white>TARGET</color>";
                }
                else
                {
                    scanNode.headerText = "<color=white>TARGET</color>";
                }
            }
            level.Enemies.Remove(_storedEnemies[(int)enemyKey].SpawnableEnemy);
        }
        private void ApplyGiantSizeUpgradedModifiers(EnemyAI enemyAI)
        {
            if (_currentMission != null && _currentMission.Name == "Giant Size: Upgraded")
            {
                if (enemyAI != null)
                {
                    // 1. Despawn (only if not already despawned - important for network synchronization)
                    if (enemyAI.gameObject.GetComponent<NetworkObject>().IsSpawned)
                    {
                        enemyAI.gameObject.GetComponent<NetworkObject>().Despawn(false);
                    }

                    // 2. Change Scale and HP
                    enemyAI.transform.localScale *= 2;

                    if (enemyAI is HoarderBugAI)
                    {
                        enemyAI.enemyHP *= 12;
                    }
                    else if (enemyAI is CentipedeAI)
                    {
                        enemyAI.enemyHP *= 17;
                    }
                    else if (enemyAI is MaskedPlayerEnemy)
                    {
                        enemyAI.enemyHP *= 5;
                    }
                    else if (enemyAI is CaveDwellerAI) // Maneater
                    {
                        enemyAI.enemyHP *= 2;
                    }
                    else
                    {
                        enemyAI.enemyHP *= 3;
                    }
                    // 3. Respawn
                    enemyAI.gameObject.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        private void ApplyTinySizeModifiers(EnemyAI enemyAI)
        {
            if (_currentMission?.Name == "Big Trouble Little Enemies" || _currentMission?.Name == "Who let the puppies out?")
            {
                if (enemyAI != null)
                {
                    // 1. Despawn (only if not already despawned - important for network synchronization)
                    if (enemyAI.gameObject.GetComponent<NetworkObject>().IsSpawned)
                    {
                        enemyAI.gameObject.GetComponent<NetworkObject>().Despawn(false);
                    }

                    // 2. Change Scale and HP
                    enemyAI.transform.localScale /= 3;
                    enemyAI.enemyHP = 1;

                    // 3. Respawn
                    enemyAI.gameObject.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        public void SpawnEnemiesAtVent(ref SelectableLevel level, EnemyVent vent)
        {
            _logger.LogInfo("Spawning mission targets...");
            _aliveEnemies.Clear();

            foreach (KeyValuePair<EnemyKey, int> entry in _enemiesToSpawn)
            {
                _logger.LogInfo($"Processing entry: {entry.Key}, Count: {entry.Value}"); // Debug log

                for (int i = 0; i < entry.Value; i++) // Loop for the NUMBER of enemies to spawn
                {
                    EnemyKey enemyKeyToSpawn = entry.Key; // Initialize with the configured key

                    if (enemyKeyToSpawn == EnemyKey.Random)
                    {
                        // Pick a random enemy from the list *INSIDE* the spawn loop
                        var rand = new System.Random();
                        int randomEnemyIndex = rand.Next(0, _randomEnemyChoices.Count);
                        enemyKeyToSpawn = _randomEnemyChoices[randomEnemyIndex];
                        _logger.LogInfo($"Randomly selected enemy: {_storedEnemies[(int)enemyKeyToSpawn].Name}"); // Debug log
                    }
                    else
                    {
                        _logger.LogInfo($"Spawning configured enemy: {_storedEnemies[(int)enemyKeyToSpawn].Name}"); //Debug log
                    }

                    // Now spawn the selected enemy (either the configured one or the random one)
                    if (!level.Enemies.Contains(_storedEnemies[(int)enemyKeyToSpawn].SpawnableEnemy))
                    {
                        SpawnUnnaturalEnemiesAtVent(ref level, vent, enemyKeyToSpawn, 1); // Spawn ONE at a time
                    }
                    else
                    {
                        // --- Existing Spawning Logic (Corrected for single-enemy spawn) ---
                        try
                        {
                            SpawnableEnemyWithRarity t_spawnableEnemy = _storedEnemies[(int)enemyKeyToSpawn].SpawnableEnemy;
                            GameObject enemyInstance = UnityEngine.Object.Instantiate(t_spawnableEnemy.enemyType.enemyPrefab);

                            ScanNodeProperties scanNode = enemyInstance.GetComponentInChildren<ScanNodeProperties>();
                            if (scanNode != null)
                            {
                                if ((bool)ConfigEntries[(int)ConfigIndexes.ChangeSubtext].BoxedValue)
                                {
                                    scanNode.subText = "<color=white>TARGET</color>";
                                }
                                else
                                {
                                    scanNode.headerText = "<color=white>TARGET</color>";
                                }
                            }

                            // Create a temporary SpawnableEnemyWithRarity for this single instance
                            SpawnableEnemyWithRarity tempSpawnable = new SpawnableEnemyWithRarity
                            {
                                enemyType = new EnemyType
                                {
                                    enemyPrefab = enemyInstance, // Use the instantiated prefab
                                    isOutsideEnemy = t_spawnableEnemy.enemyType.isOutsideEnemy,
                                },
                            };

                            level.Enemies.Add(tempSpawnable);
                            int enemyIndex = level.Enemies.IndexOf(level.Enemies.Last()); // Get index of *last* added enemy
                            RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, enemyIndex);
                            level.Enemies.RemoveAt(enemyIndex);
                            UnityEngine.Object.Destroy(enemyInstance); // Destroy the instance after spawning

                            _aliveEnemies.Add(RoundManager.Instance.SpawnedEnemies.Last()); // Add to alive enemies
                            EnemyAI enemyAI = _aliveEnemies.Last();

                            // --- Conditional Scale and HP Modification ---
                            if (_currentMission != null && _currentMission.Name == "Facility Keeper")
                            {
                                if (enemyAI != null)
                                {
                                    // Calculate scale.
                                    Vector3 newScale = (enemyAI.transform.localScale / 4);

                                    // 1. Despawn
                                    enemyAI.gameObject.GetComponent<NetworkObject>().Despawn(false);

                                    // 2. Change Scale and HP
                                    enemyAI.transform.localScale = newScale;
                                    enemyAI.enemyHP = 5;

                                    // 3. Respawn
                                    enemyAI.gameObject.GetComponent<NetworkObject>().Spawn();
                                }
                            }
                            // --- End Conditional Modification ---

                            ApplyGiantSizeUpgradedModifiers(enemyAI);
                            ApplyTinySizeModifiers(enemyAI);

                            if (enemyAI is MouthDogAI || enemyAI is BaboonBirdAI || enemyAI is ForestGiantAI)
                            {
                                _logger.LogInfo("Dog/Baboon/Giant detected, forcing it to work inside.");
                                ForceAIInside(enemyAI);
                                _logger.LogInfo("Forced inside successfully.");
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Could not spawn enemy.\n" + ex.Message);
                        }
                    }
                }
            }

            _maxEnemies = _aliveEnemies.Count;
        }


        public string Name
        {
            get { return _name; }
        }

        public Dictionary<EnemyKey, int> EnemiesToSpawn
        {
            get { return _enemiesToSpawn; }
        }

        public RewardPool RewardPool
        {
            get { return _rewardPool; }
            set { _rewardPool = value; }
        }

        public int BaseWeight
        {
            get { return _baseWeight; }
        }

        public int Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        public static int TotalWeight
        {
            get { return _totalWeight; }
            set { _totalWeight = value; }
        }

        public static int MissionChance
        {
            get { return _missionChance; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public static List<EnemyAI> AliveEnemies
        {
            get { return _aliveEnemies; }
            set { _aliveEnemies = value; }
        }
    }
}