using ImGuizmoNET;
using SDL3;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using static ControllerMovementVS.config.BindingHelper;

namespace ControllerMovementVS
{
    internal static class ControllerHelper
    {
        //ideas:
        //chat message for (dis)connects and battery low //capi.ShowChatMessage("Hello");
        //button face labels per controller type
        //button rebinding translations
        //axis as button
        //touchpad as mouse
        //support multiple gamepads
        //support joysticks
        //haptic feedback (on damage?)

        const string modid = "controllermovementvs";
        internal static uint[]? gamepads;
        public static List<string> gamepadNames = [];
        internal static short leftX = 0;
        internal static short leftY = 0;
        internal static short rightX = 0;
        internal static short rightY = 0;

        internal static int GetGamepads()
        {
            gamepads = SDL.GetGamepads(out int gamepadCount);
            gamepadNames.Clear();
            for (int i = 0; i < gamepadCount; i++)
            {
                if (gamepads is not null)
                {
                    string? name = SDL.GetGamepadNameForID(gamepads[i]);
                    if (name is not null)
                    {
                        gamepadNames.Add(name);
                    }
                }
            }
            return gamepadCount;
        }

        internal static bool SetGamepad(AnalogMovement am, int indexOfGamepad)
        {
            if (am.gamepad is not null)
            {
                SDL.CloseGamepad((nint)am.gamepad);
            }
            if (gamepads is not null && gamepads.Length >= indexOfGamepad + 1)
            {
                am.gamepad = SDL.OpenGamepad(gamepads[indexOfGamepad]);
                if (am.mod.configLibhelper is not null) am.mod.configLibhelper.gamepadSelectedIdx = indexOfGamepad;
                return true;
            }
            return false;
        }

        //sets newly connected gamepads as the one we want to use
        internal static void SetupNewGamePad(AnalogMovement am, ControllerMovementVSModSystem mod)
        {
            int numofgps = GetGamepads();
            if (numofgps > 0 && gamepads is not null)
            {
                SetGamepad(am, numofgps - 1);
                if (am.gamepad is not null)
                {
                    mod.Mod.Logger.Notification("Switching to new gamepad named: '{0}' of type: '{1}'", SDL.GetGamepadName((nint)am.gamepad), SDL.GetGamepadType((nint)am.gamepad).ToString());
                    string message = Lang.Get($"{modid}:SwitchingGamepad", SDL.GetGamepadName((nint)am.gamepad), SDL.GetGamepadType((nint)am.gamepad).ToString());
                    mod.capi?.ShowChatMessage(message);
                    mod.Mod.Logger.Chat(message);
                }
            }            
        }


        //get all sdl events
        internal static void PollEvents(AnalogMovement am, ControllerMovementVSModSystem mod, bool IsInitialized)
        {
            if (mod.sdlActivated)
            {
                while (SDL.PollEvent(out SDL.Event e))
                {
                    if (am.gamepad is not null)
                    {
                        //axes
                        if (e.Type == (uint)SDL.EventType.GamepadAxisMotion && e.GAxis.Which == SDL.GetJoystickID(SDL.GetGamepadJoystick((nint)am.gamepad)))
                        {
                            if (e.GAxis.Axis == (byte)SDL.GamepadAxis.LeftX) leftX = e.GAxis.Value;
                            else if (e.GAxis.Axis == (byte)SDL.GamepadAxis.LeftY) leftY = e.GAxis.Value;
                            else if (e.GAxis.Axis == (byte)SDL.GamepadAxis.RightX) rightX = e.GAxis.Value;
                            else if (e.GAxis.Axis == (byte)SDL.GamepadAxis.RightY) rightY = e.GAxis.Value;
                        }
                        //button down
                        else if (e.Type == (uint)SDL.EventType.GamepadButtonDown && e.GButton.Which == SDL.GetJoystickID(SDL.GetGamepadJoystick((nint)am.gamepad)))
                        {
                            if (IsRebinding) FinalizeRebind(e.GButton);
                            else
                            {
                                for (int i = 0; i < CurrentBindings.Count; i++)
                                {
                                    if ((byte)CurrentBindings[i].GamepadButton == e.GButton.Button)
                                    {
                                        var binding = CurrentBindings[i];
                                        binding.Activated = true;
                                        CurrentBindings[i] = binding;
                                        break;
                                    }
                                }
                            }
                        }
                        //button up
                        else if (e.Type == (uint)SDL.EventType.GamepadButtonUp && e.GDevice.Which == SDL.GetJoystickID(SDL.GetGamepadJoystick((nint)am.gamepad)))
                        {
                            for (int i = 0; i < CurrentBindings.Count; i++)
                            {
                                if (CurrentBindings[i].GamepadButton == (SDL.GamepadButton)e.GButton.Button)
                                {
                                    var binding = CurrentBindings[i];
                                    binding.Activated = false;
                                    CurrentBindings[i] = binding;
                                    break;
                                }
                            }
                        }
                        //low battery messages
                        else if (e.Type == (uint)SDL.EventType.JoystickBatteryUpdated && e.JDevice.Which == SDL.GetJoystickID(SDL.GetGamepadJoystick((nint)am.gamepad)))
                        {
                            var batt = e.JBattery.Percent;
                            if (batt <= 30 && batt % 5 == 0 && e.JBattery.State == SDL.PowerState.OnBattery)
                            {
                                string message = Lang.Get($"{modid}:ControllerBatteryIs", batt);
                                mod.capi?.ShowChatMessage(message);
                                mod.Mod.Logger.Chat(message);
                            }
                        }
                    }

                    //other events
                    if (e.Type == (uint)SDL.EventType.GamepadAdded && IsInitialized)
                    {
                        mod.Mod.Logger.Notification("gamepad added");
                        SetupNewGamePad(am, mod);
                    }
                    else if (e.Type == (uint)SDL.EventType.GamepadRemoved)
                    {
                        mod.Mod.Logger.Warning("gamepad removed");
                        string message = Lang.Get($"{modid}:GamepadDisconnected");
                        mod.capi?.ShowChatMessage(message);
                        mod.Mod.Logger.Chat(message);
                        SetupNewGamePad(am, mod);
                    }
                    else if (e.Type == (uint)SDL.EventType.Quit)
                    {
                        mod.sdlActivated = false;
                        mod.Mod.Logger.Warning("sdl has quit early");
                    }
                }
            }
        }
    }
}
