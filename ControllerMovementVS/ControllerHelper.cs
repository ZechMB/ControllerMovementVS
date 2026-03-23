using SDL3;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    internal static class ControllerHelper
    {
        //what need:
        //turn using right stick
        //way to select controller (in game)
        //way to remap inputs
        //add jumpsneaksprint

        internal static short rawX = 0;
        internal static short rawY = 0;

        internal static void SetupGamePad(AnalogMovement am, ModSystem ms)
        {
            int gamepadCount = 0;
            uint[]? gamepads = SDL.GetGamepads(out gamepadCount);
            if (gamepadCount > 0 && gamepads is not null)
            {
                am.gamepad = SDL.OpenGamepad(gamepads[0]);
                if (am.gamepad is not null)
                {
                    ms.Mod.Logger.Notification("using gamepad named: " + SDL.GetGamepadName((nint)am.gamepad));
                    ms.Mod.Logger.Notification("gamepad type is: " + SDL.GetGamepadType((nint)am.gamepad));
                }
            }
            //ms.Mod.Logger.Notification("count = " + gamepadCount);
        }


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
                        SetupGamePad(am, mod);
                        mod.Mod.Logger.Notification("gamepad added");
                    }
                    else if (e.Type == (uint)SDL.EventType.GamepadRemoved)
                    {
                        SetupGamePad(am, mod);
                        mod.Mod.Logger.Warning("gamepad removed");
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
