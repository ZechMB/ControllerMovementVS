using SDL3;
using System.Collections.Generic;

namespace ControllerMovementVS.config
{
    public static class BindingHelper
    {
        public struct Binding(string ControlName, int GamepadButton)
        {
            //the action this bind represents
            public string ControlName = ControlName;
            //the int of the SDL.GamepadButton or 50/51 for trigger L/R
            public int GamepadButton = GamepadButton;
            //if button is currently pressed
            public bool Activated = false;

            public static implicit operator Binding((string, SDL.GamepadButton) v) => new(v.Item1, (int)v.Item2);
            public static implicit operator Binding((string, int) v) => new(v.Item1, v.Item2);

            public readonly string GetButtonName(nint gamepad)
            {
                if (GamepadButton == -1) return "";
                else if (GamepadButton == 50) return "Left Trigger";
                else if (GamepadButton == 51) return "Right Trigger";

                var button = (SDL.GamepadButton)GamepadButton;
                if (button == SDL.GamepadButton.South || button == SDL.GamepadButton.North || button == SDL.GamepadButton.West || button == SDL.GamepadButton.East)
                {
                    var temp = SDL.GetGamepadButtonLabel(gamepad, button).ToString();
                    return button.ToString() + " (" + temp + ")"; //'South (A)'
                }
                return button.ToString();
            }
        }

        internal static List<Binding> DefaultBindings = [("Jump", SDL.GamepadButton.South), ("Sneak", SDL.GamepadButton.Invalid), ("ToggleSneak", SDL.GamepadButton.RightStick),
            ("Sprint", SDL.GamepadButton.LeftStick), ("ToggleSprint", SDL.GamepadButton.Invalid), ("LeftMouse", 51), ("RightMouse", 50)];
        internal static List<Binding> CurrentBindings = [];

        static void LoadDefaultBindings()
        {
            CurrentBindings.Clear();
            CurrentBindings.AddRange(DefaultBindings);
        }

        internal static void LoadBindingsFromConfig(Config config, ConfigLibHelper clh)
        {
            if (config.PlayerBindings is not null && config.PlayerBindings.Count == DefaultBindings.Count) CurrentBindings = config.PlayerBindings;
            else LoadDefaultBindings();
            clh.rebinding.Clear();
            for (int i = 0; i < CurrentBindings.Count; i++)
            {
                clh.rebinding.Add(false);
            }
        }

        internal static void SaveBindingsToConfig(Config config)
        {
            //dont save button pressed state
            for (int i = 0;i < CurrentBindings.Count;i++)
            {
                Binding bind = CurrentBindings[i];
                bind.Activated = false;
                CurrentBindings[i] = bind;
            }
            config.PlayerBindings = CurrentBindings;
        }
        
        internal static bool IsRebinding = false;
        private static int indexOfRebinding = 0;
        private static ConfigLibHelper? clhelper = null;

        internal static void StartRebind(int indexOfBinding, ConfigLibHelper clh)
        {
            if (CurrentBindings.Count >= indexOfBinding)
            {
                indexOfRebinding = indexOfBinding;
                IsRebinding = true;
                clhelper = clh;
            }
        }

        internal static int GetIndexOfRebinding()
        {
            if (IsRebinding) return -1;
            return indexOfRebinding;
        }

        internal static void CancelRebind()
        {
            IsRebinding = false;
        }

        internal static void FinalizeRebind(byte button)
        {
            if (!IsRebinding) return;
            if (CurrentBindings.Count >= indexOfRebinding)
            {
                string name = CurrentBindings[indexOfRebinding].ControlName;
                CurrentBindings.RemoveAt(indexOfRebinding);
                CurrentBindings.Insert(indexOfRebinding, new Binding(name, button));
                clhelper?.rebinding[indexOfRebinding] = false;
            }
            IsRebinding = false;
        }

        internal static void Unbind(int IndexToUnbind)
        {
            if (CurrentBindings.Count >= IndexToUnbind)
            {
                string name = CurrentBindings[IndexToUnbind].ControlName;
                CurrentBindings.RemoveAt(IndexToUnbind);
                CurrentBindings.Insert(IndexToUnbind, new Binding(name, (int)SDL.GamepadButton.Invalid));
            }
        }

        internal static bool GetActivated(string ActionName)
        {
            int index = CurrentBindings.FindIndex(b => b.ControlName == ActionName);
            if (index != -1) return CurrentBindings[index].Activated;
            else return false;
        }

        internal static bool IsBindValid(string ActionName)
        {
            int index = CurrentBindings.FindIndex(b => b.ControlName == ActionName);
            if (index != -1 && CurrentBindings[index].GamepadButton != unchecked((byte)SDL.GamepadButton.Invalid))
            {
                return true;
            }
            else return false;
        }
    }    
}
