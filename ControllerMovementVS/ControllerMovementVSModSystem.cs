using AnalogMovementVS;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using SDL3;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using VSImGui;
using VSImGui.API;

namespace ControllerMovementVS
{
    public class ControllerMovementVSModSystem : ModSystem
    {
        internal bool sdlActivated = false;
        private bool initialized = false;
        internal ICoreClientAPI? capi;
        internal AnalogMovement? am;
        //private static Config? config;
        private long tickListenerId = 0;
        private long tickListenerId2 = 0;
        private ConfigLibHelper? configLibhelper;
        internal Config? config;

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
                    if (api.ModLoader.IsModEnabled("configlib"))
                    {
                        configLibhelper = new(api, this);
                    }
                    else
                    {
                        ConfigLoader.TryToLoadConfig(api, this);
                    }
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
                        am = new AnalogMovement(capi, this);
                        ControllerHelper.SetupNewGamePad(am, this);
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

        public override void Dispose()
        {
            capi?.Event.UnregisterGameTickListener(tickListenerId);
            capi?.Event.UnregisterGameTickListener(tickListenerId2);
            if (sdlActivated) SDL.Quit();
            base.Dispose();
        }
    }
}
