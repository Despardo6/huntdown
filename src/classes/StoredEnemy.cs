using System.Collections.Generic;

namespace Huntdown
{
    public enum EnemyKey
    {
        SnareFlea,
        BunkerSpider,
        HoarderBug,
        Bracken,
        Thumper,
        Nutcracker,
        Masked,
        EyelessDog
    }

    public class StoredEnemy
    {
        private EnemyKey _key;
        private string _ID;
        private string _name;
        private SpawnableEnemyWithRarity _storedEnemy;

        public StoredEnemy(EnemyKey key, string id)
        {
            _key = key;
            _ID = id;
        }

        public EnemyKey Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public SpawnableEnemyWithRarity SpawnableEnemy
        {
            get { return _storedEnemy; }
            set { _storedEnemy = value; }
        }
    }
}
