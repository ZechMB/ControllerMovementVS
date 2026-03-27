namespace ControllerMovementVS
{
    public class Config
    {
        //sprint when moving fast without pressing sprint button
        public bool AutoSprint = true;
        //(0 to 1) wont move if moving stick less than deadzone
        public float DeadZone = 0.15f;
        //used internally to select a gamepad
        public int GamepadIndex = 0;
    }
}