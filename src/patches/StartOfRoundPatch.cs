using HarmonyLib;
using static Huntdown.Huntdown;

namespace Huntdown.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void IsPlayerHost(StartOfRound __instance)
        {
            _isHost = (__instance.IsServer || __instance.IsHost);
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPrefix]
        public static void ShipLeavePatch(StartOfRound __instance)
        {
            if (!_isHost) { return; }

            if (_currentMission != null)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=red>MISSION FAILED. TARGET WAS: </color><color=white>" + _currentMission.Name + "</color>", -1);
                _currentMission.End();
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StoreItems()
        {
            if (!_isHost) { return; }

            for (int i = 0; i < StartOfRound.Instance.allItemsList.itemsList.Count; i++)
            {
                for (int j = 0; j < _storedItems.Length; j++)
                {
                    if (StartOfRound.Instance.allItemsList.itemsList[i].ToString() == _storedItems[j].ID)
                    {
                        _storedItems[j].GameItem = StartOfRound.Instance.allItemsList.itemsList[i];
                        _storedItems[j].Name = StartOfRound.Instance.allItemsList.itemsList[i].itemName;
                        break;
                    }
                }
            }
        }
    }
}
