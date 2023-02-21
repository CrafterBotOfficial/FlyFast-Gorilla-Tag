using BepInEx;
using BepInEx.Configuration;
using System.ComponentModel;
using UnityEngine;
using Utilla;

namespace FlyFastGorillaTag
{
    [BepInPlugin("com.FlyFastGorillaTag.Menu", "FlyFastGorillaTag", "3.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.7")]
    [ModdedGamemode]
    [Description("HauntedModMenu")]

    public class Main : BaseUnityPlugin
    {
        public static Main Instance;
        public static int Version = 0;

        public static ConfigEntry<float> ForwardFacingSpeed;
        public static ConfigEntry<float> Acceleration;
        public static ConfigEntry<bool> UseAcceleration;
        public static ConfigEntry<bool> LimitedSpeedOnGrip;

        private static GameObject FlyManagerObj;

        public static bool Override;
        private void Awake()
        {
            Instance = this;

            ForwardFacingSpeed = Config.Bind("Movement", "Forward Facing Speed", 2500f, "The speed at which the player will fly forward when the trigger is held down.");
            UseAcceleration = Config.Bind("Movement", "Use Acceleration", true, "If true, the player will speed by (0-60) like a car. If false, the player will instantly reach the max speed.");
            Acceleration = Config.Bind("Movement", "Acceleration", 150f, "The speed at which the player will accelerate when the trigger is held down. This doesn't apply if Use Acceleration is false.");
            LimitedSpeedOnGrip = Config.Bind("Movement", "Limited Speed On Grip", true, "If true, the player will fly at double the speed when the grip is held down. If false, the player will never stop accelerating when grip is held.");

            FlyManagerObj = new GameObject("FlyManager");

            DontDestroyOnLoad(FlyManagerObj);
            FlyManagerObj.AddComponent<Movement.Movement>();
            FlyManagerObj.SetActive(false);
        }

        [ModdedGamemodeJoin]
        private void RoomJoined(string GameMode)
        {
            FlyManagerObj.SetActive(true);
        }
        [ModdedGamemodeLeave]
        private void RoomLeft(string GameMode)
        {
            Movement.Movement.Instance.FixDrag();
            FlyManagerObj.SetActive(false);
        }

        private void OnEnable()
        {
            Override = false;
        }

        private void OnDisable()
        {
            Movement.Movement.Instance.FixDrag();
            Override = true;
        }
    }
}
