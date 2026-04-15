using AnalogMovementVS;
using ControllerMovementVS.config;
using SDL3;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    public class ControllerMovementVSModSystem : ModSystem
    {
        internal bool sdlActivated = false;
        private bool initialized = false;
        internal ICoreClientAPI? capi;
        internal AnalogMovement? am;
        private long tickListenerId = 0;
        private ConfigLibHelper? configLibhelper;
        internal Config? config;

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            tickListenerId = api.Event.RegisterGameTickListener(OnTick, 0);

            //set native lib location to /native because vintage story wants them there            
            try
            {
                NativeLibrary.SetDllImportResolver(typeof(SDL).Assembly, (libraryName, assembly, searchPath) =>
                {
                    if (libraryName == "SDL3")
                    {
                        string path = "";
                        var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) path = Path.Combine(modDir, "native", "SDL3.dll");
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) path = Path.Combine(modDir, "native", "SDL3.so");
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) path = Path.Combine(modDir, "native", "SDL3.dylib");

                        try { return NativeLibrary.Load(path); }
                        catch (Exception ex) { Mod.Logger.Error($" Failed to load SDL native: {ex.Message}"); }
                    }
                    return IntPtr.Zero;
                });
            }
            catch { }//expected to fail if mod is reloaded (such as by going to the main menu and back into a world)
            sdlActivated = StartSDL();
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

        private bool StartSDL()
        {
            bool started = false;
            if (capi is null) return started;
            try
            {
                if (SDL.Init(SDL.InitFlags.Gamepad))
                {
                    Mod.Logger.Notification("sdl init good");
                    sdlActivated = true;
                    if (capi.ModLoader.IsModEnabled("configlib"))
                    {
                        configLibhelper ??= new(capi, this);
                    }
                    else
                    {
                        ConfigLoader.TryToLoadConfig(capi, this);
                    }
                    started = true;
                }
                else
                {
                    Mod.Logger.Warning($"sdl init error: {SDL.GetError()}");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Error("failed sdl init: " + ex.Message);
            }
            return started;
        }

        internal bool RestartSDL()
        {
            Mod.Logger.Notification("trying to restart SDL, previously activated = " + sdlActivated);
            if (sdlActivated)
            {
                sdlActivated = false;
                SDL.Quit();
            }
            return StartSDL();
        }

        public override void Dispose()
        {
            capi?.Event.UnregisterGameTickListener(tickListenerId);
            if (sdlActivated) SDL.Quit();
            base.Dispose();
        }
    }
}
