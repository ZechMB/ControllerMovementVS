using AnalogMovementVS;
using ControllerMovementVS.config;
using System;
using Vintagestory.API.Client;
using Vintagestory.Client;

namespace ControllerMovementVS
{
    internal class AnalogMovement
    {
        readonly ICoreClientAPI capi;
        internal readonly ControllerMovementVSModSystem mod;
        internal AnalogMovement(ICoreClientAPI capi, ControllerMovementVSModSystem mod)
        {
            this.capi = capi;
            this.mod = mod;
        }

        float moveX = 0;
        float moveY = 0;
        float lookX = 0;
        float lookY = 0;
        internal nint? gamepad;
        bool toggleSneak = false;
        bool toggleSprint = false;
        bool prevToggleSneakPressed = false;
        bool prevToggleSprintPressed = false;

        internal void ConsumeInputs()
        {
            if (capi.World.Player.Entity.Controls is EntityControlsAMfVS am && mod.config is not null)
            {
                float deadzone = mod.config.DeadZone;

                //looking
                if (mod.config.LookUsingRightStick && ScreenManager.Platform.IsFocused && !am.IsPauseMenuOpen)
                {
                    if (mod.config.SwapLeftRightSticks)
                    {
                        lookX = ControllerHelper.leftX;
                        lookY = ControllerHelper.leftY;
                    }
                    else
                    {
                        lookX = ControllerHelper.rightX;
                        lookY = ControllerHelper.rightY;
                    }
                    lookX /= -32767f;
                    lookY /= 32767f;
                    if (mod.config.InvertVerticalLook) lookY *= -1;
                    lookX = (Math.Abs(lookX) > deadzone) ? lookX : 0;
                    lookY = (Math.Abs(lookY) > deadzone) ? lookY : 0;
                    lookX *= mod.config.LookSensitivityHorizontal * .08f;
                    lookY *= mod.config.LookSensitivityVertical * .08f;
                    capi.World.Player.CameraYaw += lookX;
                    capi.World.Player.Entity.Pos.Pitch += lookY;
                }

                //moving
                
                if (mod.config.SwapLeftRightSticks)
                {
                    moveX = ControllerHelper.rightX;
                    moveY = ControllerHelper.rightY;
                }
                else
                {
                    moveX = ControllerHelper.leftX;
                    moveY = ControllerHelper.leftY;
                }
                moveX /= -32767f;
                moveY /= -32767f;

                moveX = (Math.Abs(moveX) > deadzone) ? moveX : 0;
                moveY = (Math.Abs(moveY) > deadzone) ? moveY : 0;

                // Update position
                if (mod.config.AutoSprint && !am.IsMounted) AutoSprint(am);
                else
                {
                    am.amForwardBackward = moveY;
                    am.amLeftRight = moveX;
                    bool shouldSprint = false;
                    if (BindingHelper.IsBindValid("Sprint")) shouldSprint = BindingHelper.GetActivated("Sprint");
                    if (BindingHelper.IsBindValid("ToggleSprint"))
                    {
                        if (!am.IsMounted)
                        {
                            bool pressed = BindingHelper.GetActivated("ToggleSprint");
                            if (pressed != prevToggleSprintPressed)
                            {
                                prevToggleSprintPressed = pressed;
                                if (pressed == true)
                                {
                                    toggleSprint = !toggleSprint;
                                }
                            }
                            if (!am.TriesToMove && toggleSprint && mod.config.AutoStopToggleSprint)
                            {
                                toggleSprint = false;
                            }
                        }
                        else //dont toggle if mounted
                        {
                            toggleSprint = BindingHelper.GetActivated("ToggleSprint");
                        }
                    }
                    am.amSprint = shouldSprint || toggleSprint;
                }

                //set jump & sneak
                var player = capi.World.Player;
                if (BindingHelper.IsBindValid("Jump")) am.amJump = BindingHelper.GetActivated("Jump") && (player.Entity.PrevFrameCanStandUp || player.WorldData.NoClip);
                bool shouldSneak = false;
                if (BindingHelper.IsBindValid("Sneak")) shouldSneak = BindingHelper.GetActivated("Sneak");
                if (BindingHelper.IsBindValid("ToggleSneak"))
                {
                    bool pressed = BindingHelper.GetActivated("ToggleSneak");
                    if (pressed != prevToggleSneakPressed)
                    {
                        prevToggleSneakPressed = pressed;
                        if (pressed == true)
                        {
                            toggleSneak = !toggleSneak;
                        }
                    }
                }
                am.amSneak = shouldSneak || toggleSneak;
            }
        }


        void AutoSprint(EntityControlsAMfVS am)
        {
            //calculate inputs
            float forwardback = moveY;
            float leftright = moveX;
            bool ShouldSprint = false;
            if (moveX > 0.5f || moveX < -0.5f || moveY > 0.5f || moveY < -0.5f)
            {
                ShouldSprint = true;
            }
            if ((moveY <= 0.5f || moveY >= -0.5f) && ShouldSprint == false)
            {
                forwardback *= 2f;
            }
            if ((moveX <= 0.5f || moveX >= -0.5f) && ShouldSprint == false)
            {
                leftright *= 2f;
            }

            //output controls
            am.amForwardBackward = forwardback;
            am.amLeftRight = leftright;
            am.amSprint = ShouldSprint;
        }
    }
}
