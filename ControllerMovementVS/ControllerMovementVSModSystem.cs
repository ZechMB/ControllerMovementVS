using AnalogMovementVS;
using SDL3;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    public class ControllerMovementVSModSystem : ModSystem
    {
        internal bool sdlActivated = false;
        private bool initialized = false;
        private ICoreClientAPI? capi;
        private AnalogMovement? am;
        private static Config? config;
        private long tickListenerId = 0;
        private long tickListenerId2 = 0;

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            tickListenerId = api.Event.RegisterGameTickListener(OnTick, 0);
            tickListenerId2 = api.Event.RegisterGameTickListener(EverySec, 1000);

            try
            {
                if (SDL.Init(SDL.InitFlags.Gamepad))
                {
                    Mod.Logger.Notification("sdl init good");
                    sdlActivated = true;
                    TryToLoadConfig();
                }
                else
                {
                    Mod.Logger.Warning("sdl init fail");
                    Mod.Logger.Warning($"SDL could not initialize: {SDL.GetError()}");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Error("failed sdl init: " + ex.Message);
            }
        }

        //can't be called until entitycontrols is set
        internal void Init()
        {
            initialized = true;
            if (capi is not null)
            {
                if (capi.World.Player.Entity.Controls is EntityControlsAMfVS amcontrols)
                {
                    if (config is not null)
                    {
                        am = new AnalogMovement(capi, config, this);
                        ControllerHelper.SetupGamePad(am, this);
                    }
                }
            }
        }

        internal void OnTick(float deltaTime)
        {
            if (sdlActivated)
            {
                if (!initialized)
                {
                    Init();
                }
                if (capi is not null && am is not null)
                {
                    ControllerHelper.PollEvents(am, this);
                    am.ConsumeInputs();
                }
            }
        }

        internal void EverySec(float deltaTime)
        {
            if (am is not null)
            {
                //ControllerHelper.SetupGamePad(am, this);
            }
        }

        private void TryToLoadConfig()
        {
            try
            {
                if (capi is not null)
                {
                    config = capi.LoadModConfig<Config>("ControllerMovementVS.json");
                    if (config == null)
                    {
                        config = new Config();
                    }
                    capi.StoreModConfig<Config>(config, "ControllerMovementVS.json");
                }
            }
            catch (Exception e)
            {
                //Couldn't load the mod config... Create a new one with default settings, but don't save it.
                Mod.Logger.Error("Could not load config! Loading default settings instead.");
                Mod.Logger.Error(e);
                config = new Config();
            }
            if (config is not null && config.DeadZone > 1f)
            {
                Mod.Logger.Warning("deadzone is set higher than expected max input of 1.0");
            }
        }

        public override void Dispose()
        {
            capi?.Event.UnregisterGameTickListener(tickListenerId);
            capi?.Event.UnregisterGameTickListener(tickListenerId2);
            if (sdlActivated) SDL.Quit();
            base.Dispose();
        }
    }
}
