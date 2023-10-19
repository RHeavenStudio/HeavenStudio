using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Discord;
using System;

namespace HeavenStudio.DiscordRPC
{
    public class DiscordController : MonoBehaviour
    {
        public Discord.Discord discord;

        public static DiscordController instance { get; set; }

        private long lastStartTime;

        private bool disconnected;

        private void Awake()
        {
        }

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }

        // private void OnApplicationQuit()
        // {
        //     Disconnect();
        // }

        public void Connect()
        {
            discord = new Discord.Discord(DiscordRPC.clientID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
            disconnected = false;
        }

        public void Disconnect()
        {
            if (discord != null)
            {
                discord.Dispose();
                disconnected = true;
            }
        }

        public void UpdateActivity(string stateText, string stateDetails, bool updateTime = false)
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Activity { };


            activity = new Activity
            {
                State = stateText,
                Details = stateDetails,
                Assets =
                {
                        LargeImage = "logo",
                        LargeText = "Together now!"
                },
                Instance = true,
            };

            if (updateTime == true)
            {
                lastStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                activity.Timestamps.Start = lastStartTime;
            }
            else
            {
                activity.Timestamps.Start = lastStartTime;
            }

            activityManager.UpdateActivity(activity, (result) => {
                if (result == Discord.Result.Ok)
                {
                    Debug.Log("Update Success!");
                }
                else
                {
                    Debug.Log("Update Failed");
                }
            });
        }

        public void ClearActivity()
        {
            var activityManager = discord.GetActivityManager();
            activityManager.ClearActivity((result) => {
                if (result == Discord.Result.Ok)
                {
                    Debug.Log("Clear Success!");
                }
                else
                {
                    Debug.Log("Clear Failed");
                }
            });
        }

        void Update()
        {
            if (discord != null && !disconnected)
            {
                discord.RunCallbacks();
            }
        }
    }
}