using AnalogMovementVS;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    internal class AnalogMovement
    {
        readonly ICoreClientAPI capi;
        readonly Config config;
        readonly Mod Mod;
        internal AnalogMovement(ICoreClientAPI capi, Config config, ModSystem mod)
        {
            this.capi = capi;
            this.config = config;
            Mod = mod.Mod;
        }

        float moveX = 0;
        float moveY = 0;
        internal nint? gamepad;

        internal void ConsumeInputs()
        {
            if (capi.World.Player.Entity.Controls is EntityControlsAMfVS am && am.IsGameReadyForInput)
            {
                moveX = ControllerHelper.rawX / -32767f;
                moveY = ControllerHelper.rawY / -32767f;

                moveX = (Math.Abs(moveX) > config.DeadZone) ? moveX : 0;
                moveY = (Math.Abs(moveY) > config.DeadZone) ? moveY : 0;

                //Mod.Logger.Notification("gamepad = " + gamepad);
                //Mod.Logger.Notification("readingx " + moveX);
                //Mod.Logger.Notification("readingy " + moveY);

                // Update position
                if (config.AutoSprint) AutoSprint(am);
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
            else if (capi.World.Player.Entity.Controls is EntityControlsAMfVS am2 && !am2.IsGameReadyForInput)
            {
                //zero the controls if we enter a menu so we stop moving
                am2.amForwardBackward = 0;
                am2.amLeftRight = 0;
                am2.amSprint = false;
                //am2.amJump = false;
                //am2.amSneak = false;
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
