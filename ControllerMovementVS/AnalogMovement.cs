using AnalogMovementVS;
using System;
using Vintagestory.API.Client;
using Vintagestory.Client;

namespace ControllerMovementVS
{
    internal class AnalogMovement
    {
        readonly ICoreClientAPI capi;
        readonly ControllerMovementVSModSystem mod;
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

        internal void ConsumeInputs()
        {
            float deadzone = 0;
            if (mod.config is not null) deadzone = mod.config.DeadZone;

            //looking
            if (mod.config is not null && mod.config.LookUsingRightStick && ScreenManager.Platform.IsFocused)
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
                lookX = lookX / -32767f;
                lookY = lookY / 32767f;
                lookX = (Math.Abs(lookX) > deadzone) ? lookX : 0;
                lookY = (Math.Abs(lookY) > deadzone) ? lookY : 0;
                lookX *= mod.config.LookSensitivityHorizontal * .08f;
                lookY *= mod.config.LookSensitivityVertical * .08f;
                capi.World.Player.CameraYaw += lookX;
                capi.World.Player.Entity.Pos.Pitch += lookY;
                //mod.Mod.Logger.Notification("camy " + capi.World.Player.CameraPitch);
                //mod.Mod.Logger.Notification("looky " + lookY);
            }

            //moving
            if (capi.World.Player.Entity.Controls is EntityControlsAMfVS am && mod.config is not null)
            {
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
                moveX = moveX / -32767f;
                moveY = moveY / -32767f;

                moveX = (Math.Abs(moveX) > deadzone) ? moveX : 0;
                moveY = (Math.Abs(moveY) > deadzone) ? moveY : 0;

                //Mod.Logger.Notification("gamepad = " + gamepad);
                //Mod.Logger.Notification("readingx " + moveX);
                //Mod.Logger.Notification("readingy " + moveY);

                // Update position
                if (mod.config.AutoSprint) AutoSprint(am);
                else
                {
                    am.amForwardBackward = moveY;
                    am.amLeftRight = moveX;
                    //am.amSprint = capi.Input.IsHotKeyPressed("sprint") || (am.Sprint && am.TriesToMove && ClientSettings.ToggleSprint);
                }

                //set jump & sneak
                //var player = capi.World.Player;
                //am.amJump = capi.Input.IsHotKeyPressed("jump") && (player.Entity.PrevFrameCanStandUp || player.WorldData.NoClip);
                //am.amSneak = capi.Input.IsHotKeyPressed("sneak");
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

            /*
            if (config.ReverseSprint && capi.Input.IsHotKeyPressed("sprint"))
            {
                ShouldSprint = false;
            }
            */

            //output controls
            am.amForwardBackward = forwardback;
            am.amLeftRight = leftright;
            am.amSprint = ShouldSprint;
        }
    }


}
