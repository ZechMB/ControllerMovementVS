using System.Collections.Generic;

namespace ControllerMovementVS.config
{
    public class Config
    {
        //sprint when moving fast without pressing sprint button
        public bool AutoSprint = true;
        //(0 to 1) wont move/look if moving stick less than deadzone
        public float DeadZone = 0.15f;
        //turn off camera controls if you are using another method
        public bool LookUsingRightStick = true;
        //change look to left stick and move to right stick
        public bool SwapLeftRightSticks;
        //any value 0.0 or higher 
        public float LookSensitivityHorizontal = 1f;
        //any value 0.0 or higher 
        public float LookSensitivityVertical = 1f;        
        //used with configlib to select a gamepad
        public int GamepadIndex = 0;
        //used with configlib to save rebinded buttons
        public List<BindingHelper.Binding> PlayerBindings = [];
    }
}