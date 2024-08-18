using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Huntdown.ConfigSettings;
using static Huntdown.Huntdown;

namespace Huntdown
{
    public enum MissionKey
    {
        SnareFlea,
        BunkerSpider,
        HoarderBug,
        Bracken,
        Thumper,
        Nutcracker,
        Masked,
        GoodBoy,
        BugMafia,
        Butler
    }

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
            List <EnemyVent> allFarVents = new List<EnemyVent>();


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
            _logger.LogInfo("Forcing dog AI to work indoors.");
            enemyAI.enemyType = UnityEngine.Object.Instantiate(enemyAI.enemyType);
            enemyAI.enemyType.isOutsideEnemy = false;
            enemyAI.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            enemyAI.SyncPositionToClients();
        }

        private void SpawnUnnaturalEnemiesAtVent(ref SelectableLevel level, EnemyVent vent, KeyValuePair<EnemyKey, int> entry)
        {
            _logger.LogInfo("Enemy does not spawn naturally on this moon. Temporarily adding it to spawnable enemies.");
            level.Enemies.Add(_storedEnemies[(int)entry.Key].SpawnableEnemy);
            for (int i = 0; i < entry.Value; i++)
            {
                RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, level.Enemies.IndexOf(_storedEnemies[(int)entry.Key].SpawnableEnemy));
                // level.Enemies.Add(_storedEnemies[(int)entry.Key].SpawnableEnemy);
                _aliveEnemies.Add(RoundManager.Instance.SpawnedEnemies[RoundManager.Instance.SpawnedEnemies.Count - 1]);
                EnemyAI enemyAI = _aliveEnemies[_aliveEnemies.Count - 1];

                if (enemyAI is MouthDogAI)
                {
                    _logger.LogInfo("Dog detected, forcing it to work inside.");
                    ForceAIInside(enemyAI);
                    _logger.LogInfo("Forced doggo inside successfully.");
                }

                if (enemyAI.gameObject.GetComponentInChildren<ScanNodeProperties>())
                {
                    enemyAI.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = "<color=white>TARGET</color>";
                }
            }
            level.Enemies.Remove(_storedEnemies[(int)entry.Key].SpawnableEnemy);
        }

        public void SpawnEnemiesAtVent(ref SelectableLevel level, EnemyVent vent)
        {
            _logger.LogInfo("Spawning mission targets...");
            foreach (KeyValuePair<EnemyKey, int> entry in _enemiesToSpawn)
            {
                _logger.LogInfo("Spawning " + entry.Value + " " + _storedEnemies[(int)entry.Key].Name + ".");
                if (!level.Enemies.Contains(_storedEnemies[(int)entry.Key].SpawnableEnemy))
                {
                    SpawnUnnaturalEnemiesAtVent(ref level, vent, entry);
                }
                else
                {
                    for (int i = 0; i < entry.Value; i++)
                    {
                        try
                        {
                            //RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, level.Enemies.IndexOf(_storedEnemies[(int)entry.Key].SpawnableEnemy));
                            //EnemyAI tempAI = RoundManager.Instance.SpawnedEnemies[RoundManager.Instance.SpawnedEnemies.Count - 1];

                            //tempAI.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = "<color=white>TARGET</color>";

                            SpawnableEnemyWithRarity t_spawnableEnemy = _storedEnemies[(int)entry.Key].SpawnableEnemy;
                            t_spawnableEnemy.enemyType = UnityEngine.Object.Instantiate(t_spawnableEnemy.enemyType);
                            t_spawnableEnemy.enemyType.enemyPrefab = UnityEngine.Object.Instantiate(t_spawnableEnemy.enemyType.enemyPrefab);
                            t_spawnableEnemy.enemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "<color=white>TARGET</color>";
                            t_spawnableEnemy.enemyType.enemyPrefab.transform.localScale = (t_spawnableEnemy.enemyType.enemyPrefab.transform.localScale / 2f);
                            level.Enemies.Add(t_spawnableEnemy);
                            RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, level.Enemies.IndexOf(level.Enemies.Last()));
                            level.Enemies.Remove(level.Enemies.Last());

                            // RoundManager.Instance.SpawnEnemyOnServer(vent.transform.position, 0f, level.Enemies.IndexOf(_storedEnemies[(int)entry.Key].SpawnableEnemy));
                            _logger.LogInfo("Enemy spawned successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Could not spawn enemy.\n" + ex.Message);
                        }

                        // level.Enemies.Add(_storedEnemies[(int)entry.Key].SpawnableEnemy);
                        _aliveEnemies.Add(RoundManager.Instance.SpawnedEnemies[RoundManager.Instance.SpawnedEnemies.Count - 1]);
                        EnemyAI enemyAI = _aliveEnemies[_aliveEnemies.Count - 1];

                        if (enemyAI is MouthDogAI)
                        {
                            _logger.LogInfo("Dog detected, forcing it to work inside.");
                            ForceAIInside(enemyAI);
                            _logger.LogInfo("Forced doggo inside successfully.");
                        }

                        try
                        {
                            if (enemyAI.gameObject.GetComponentInChildren<ScanNodeProperties>())
                            {
                                enemyAI.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = "<color=white>TARGET</color>";
                                _logger.LogInfo("Applied header text to enemy's scan node successfully.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("EnemyAI does not have a gameObject, or something is wrong with this function.\n" + ex.Message);
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
