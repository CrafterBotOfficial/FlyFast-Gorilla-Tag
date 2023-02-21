using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace FlyFastGorillaTag.Movement
{
    public class Movement : MonoBehaviour
    {
        public static Movement Instance;
        private List<InputDevice> InputDeviceStatusList = new List<InputDevice>();

        private GorillaLocomotion.Player Player;
        private Rigidbody Rb;

        private bool RightGripButton;
        private bool RightHandPrimaryTrigger;
        private bool RightPrimaryButton;

        private float StoredDrag;
        private float Speed;
        private bool RightGripReleased;
        private float DragForce;
        private void Start()
        {
            Player = GorillaLocomotion.Player.Instance;
            // Harmony.CreateAndPatchAll(typeof(Movement));
            Instance = this;
            Rb = Player.GetComponent<Rigidbody>();
            DragForce = 100000;
        }

        // [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        private void FixedUpdate()
        {
            if (Main.Override) return;

            InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, InputDeviceStatusList);

            float PointerFinderValue = InputDeviceStatusList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float PointerFinder) ? PointerFinder : 0f;

            Debug.Log(PointerFinderValue);
            RightGripButton = InputDeviceStatusList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool RightGripValue) && RightGripValue;
            RightHandPrimaryTrigger = PointerFinderValue > 0.5f; // InputDeviceStatusList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool RightHandPrimaryTriggerValue) && RightHandPrimaryTriggerValue;
            RightPrimaryButton = InputDeviceStatusList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool RightPrimaryValue) && RightPrimaryValue;

            // Stop: Right Hand Trigger // I am using drag to prevent damaging other mods or other in game features
            StoredDrag = Rb.drag == DragForce ? StoredDrag : Rb.drag; // Prevents drag data from being list, not totally sure if the drag changes throughout the game, but this is a good precaution
            if (RightPrimaryButton) Rb.drag = DragForce;
            else if (Rb.drag == DragForce) Rb.drag = StoredDrag;

            // Fly-Forward: Right Hand Trigger
            float MaxSpeed = RightGripButton == true ? Main.ForwardFacingSpeed.Value * 2f : Main.ForwardFacingSpeed.Value;
            if (Main.UseAcceleration.Value)
                if (Main.LimitedSpeedOnGrip.Value) Speed = Speed >= MaxSpeed ? MaxSpeed : Speed + Main.Acceleration.Value;// * Time.deltaTime;
                else Speed += Main.Acceleration.Value;
            else Speed = MaxSpeed;
            if (RightHandPrimaryTrigger && !RightPrimaryButton)
                Rb.velocity = Player.headCollider.transform.forward * Speed * Time.deltaTime;

            // Reset Speed: Right Hand Grip Released, only when it is first released
            if (!RightGripButton && !RightGripReleased)
            {
                Speed = MaxSpeed;
                RightGripReleased = true;
            }
            else if (RightGripButton) RightGripReleased = false;

            // Debug.Log($"Speed: {Speed} MaxSpeed: {MaxSpeed}"); // Deprecated
        }

        public void FixDrag()
        {
            bool DragInvalid = Rb.drag == DragForce;
            if (!DragInvalid) return;
            Debug.LogError("[Drag Error]");
            if (StoredDrag == DragForce)
            {
                Debug.LogError("[Drag Error]: Failed to reset drag, please reset");
                return;
            }
            Rb.drag = StoredDrag;
        }
    }
}
