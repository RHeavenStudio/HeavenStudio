using UnityEngine;

namespace HeavenStudio.DiscordRPC
{
    public class DiscordRPC : MonoBehaviour
    {
        public static long clientID = 945877725984477205;

        private static void DiscordControllerCheck()
        {
            if (DiscordController.instance == null)
            {
                var discordController = new GameObject("DiscordController");
                var di = discordController.AddComponent<DiscordController>();
                DiscordController.instance = di;
                di.Connect();
            }
        }

        public static void Connect()
        {
            DiscordControllerCheck();
            DiscordController.instance.Connect();
        }

        public static void UpdateActivity(string state = null, string details = null, bool updateTime = false)
        {
            DiscordControllerCheck();
            DiscordController.instance.UpdateActivity(details, state, updateTime);
        }

        public static void Disconnect()
        {
            DiscordControllerCheck();
            DiscordController.instance.Disconnect();
            Destroy(DiscordController.instance.gameObject);
        }
    }
}