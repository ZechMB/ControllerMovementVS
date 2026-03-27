using SDL3;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    internal static class ControllerHelper
    {
        //what need:
        //turn using right stick
        //way to remap inputs
        //add jumpsneaksprint
        //translation support
        //maybe support multiple gamepads

        internal static uint[]? gamepads;
        public static List<string> gamepadNames = new List<string>();
        internal static short rawX = 0;
        internal static short rawY = 0;

        internal static int getGamepads()
        {
            //do i still need to call sdl.free?
            gamepads = SDL.GetGamepads(out int gamepadCount);
            gamepadNames.Clear();
            for (int i = 0; i < gamepadCount; i++)
            {
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                string? name = SDL.GetGamepadNameForID(gamepads[i]);
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (name is not null)
                {
                    gamepadNames.Add(name);
                }
            }
            return gamepadCount;
        }

        internal static void setGamepad(AnalogMovement am, int indexOfGamepad)
        {
            if (am.gamepad is not null)
            {
                SDL.CloseGamepad((nint)am.gamepad);
            }
            if (gamepads is not null && gamepads.Length >= indexOfGamepad)
            {
                am.gamepad = SDL.OpenGamepad(gamepads[indexOfGamepad]);
            }
        }

        //sets newly connected gamepads as the one we want to use
        internal static void SetupNewGamePad(AnalogMovement am, ModSystem ms)
        {
            int numofgps = getGamepads();
            if (numofgps > 0 && gamepads is not null)
            {
                setGamepad(am, numofgps - 1);
                if (am.gamepad is not null)
                {
                    ms.Mod.Logger.Notification("now using gamepad named: " + SDL.GetGamepadName((nint)am.gamepad) + " type: " + SDL.GetGamepadType((nint)am.gamepad));
                }
            }            
        }


        //get all sdl events
        internal static void PollEvents(AnalogMovement am, ControllerMovementVSModSystem mod)
        {
            if (mod.sdlActivated)
            {
                while (SDL.PollEvent(out SDL.Event e))
                {
                    if (e.Type == (uint)SDL.EventType.GamepadAxisMotion)
                    {
                        if (am.gamepad is not null)
                        {
                            rawX = SDL.GetGamepadAxis((nint)am.gamepad, SDL.GamepadAxis.LeftX);
                            rawY = SDL.GetGamepadAxis((nint)am.gamepad, SDL.GamepadAxis.LeftY);
                        }
                    }
                    else if (e.Type == (uint)SDL.EventType.GamepadAdded)
                    {
                        mod.Mod.Logger.Notification("gamepad added");
                        SetupNewGamePad(am, mod);                        
                    }
                    else if (e.Type == (uint)SDL.EventType.GamepadRemoved)
                    {
                        mod.Mod.Logger.Warning("gamepad removed");
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
