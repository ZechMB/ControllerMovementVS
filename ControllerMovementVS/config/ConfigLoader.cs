using System;
using Vintagestory.API.Common;

namespace ControllerMovementVS.config
{
    internal static class ConfigLoader
    {
        internal static void TryToLoadConfig(ICoreAPI api, ControllerMovementVSModSystem modSystem)
        {
            try
            {
                modSystem.config = api.LoadModConfig<Config>("ControllerMovementVS.json");
                if (modSystem.config == null)
                {
                    modSystem.config = new();
                }
                //save just in case new config variables were added or no config found
                api.StoreModConfig(modSystem.config, "ControllerMovementVS.json");
            }
            catch (Exception e)
            {
                //Couldn't load the mod config... Create a new one with default settings, but don't save it.
                modSystem.Mod.Logger.Error("Could not load config! Loading default settings instead.");
                modSystem.Mod.Logger.Error(e);
                modSystem.config = new();
            }
            if (modSystem.config is not null && modSystem.config.DeadZone > 1f)
            {
                modSystem.Mod.Logger.Warning("deadzone is set higher than expected max input of 1.0");
            }
        }
    }
}
