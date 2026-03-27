using ConfigLib;
using ImGuiNET;
using Vintagestory.API.Common;

namespace ControllerMovementVS
{
    internal class ConfigLibHelper
    {
        const string Modid = "ControllerMovementVS";
        ICoreAPI api;
        ControllerMovementVSModSystem mod;
        //private ref Config config;

        //private const string categorycontroller = $"{Modid}:Config.Category.Crafting"; Lang.Get(categorycontroller)

        public ConfigLibHelper(ICoreAPI api, ControllerMovementVSModSystem modSystem)
        {
            this.api = api;
            mod = modSystem;
            LoadConfig();
            
            api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Modid, (id, buttons) =>
            {
                if (buttons.Save) SaveConfig();
                if (buttons.Restore) LoadConfig();
                if (buttons.Defaults) setDefault();
                if (buttons.Reload) SaveConfig();
                if (mod.config is not null) Edit(api, mod.config, id);
            });
        }

        int gamepadSelectedIdx = 0;
        //int item_highlighted_idx = -1;
        bool autoSprint = true;
        float deadzone = 0f;

        private void Edit(ICoreAPI api, Config config, string id)
        {
            //keep polling in case the game is paused so we register if a controller connects
            if (mod.am is not null && mod.capi is not null && mod.capi.IsGamePaused) ControllerHelper.PollEvents(mod.am, mod);


            //bool item_highlight = false;

            ImGui.TextWrapped("select gamepad:");
            if (ImGui.BeginListBox(""))
            {
                for (int n = 0; n < ControllerHelper.gamepadNames.Count; n++)
                {
                    bool is_selected = (gamepadSelectedIdx == n);
                    if (ImGui.Selectable(ControllerHelper.gamepadNames[n], is_selected))
                    {
                        gamepadSelectedIdx = n;
                        config.GamepadIndex = n;
                        mod.Mod.Logger.Notification("choosing gamepad: " + n);
                    }
                    //if (item_highlight && ImGui.IsItemHovered())
                        //item_highlighted_idx = n;

                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (is_selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndListBox();
            }
            //ImGui.NewLine();
            ImGui.Checkbox("Auto sprint", ref autoSprint);
            ImGui.DragFloat("Deadzone", ref deadzone, 0.01f, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
        }

        private void SaveConfig()
        {
            if (mod.am is not null)
            {
                ControllerHelper.setGamepad(mod.am, gamepadSelectedIdx);
            }
            if (mod.config is not null)
            {
                mod.config.AutoSprint = autoSprint;
                mod.config.GamepadIndex = gamepadSelectedIdx;
                mod.config.DeadZone = deadzone;
                api.StoreModConfig<Config>(mod.config, "ControllerMovementVS.json");
            }
        }

        private void LoadConfig()
        {
            ConfigLoader.TryToLoadConfig(api, mod);
            if (mod.config is not null)
            {                         
                autoSprint = mod.config.AutoSprint;
                deadzone = mod.config.DeadZone;
                gamepadSelectedIdx = mod.config.GamepadIndex;
                if (mod.am is not null)
                {
                    ControllerHelper.setGamepad(mod.am, gamepadSelectedIdx);
                }
            }
        }

        private void setDefault()
        {
            mod.config = new();
            autoSprint = mod.config.AutoSprint;
            deadzone = mod.config.DeadZone;
            gamepadSelectedIdx = mod.config.GamepadIndex;
            if (mod.am is not null)
            {
                ControllerHelper.setGamepad(mod.am, gamepadSelectedIdx);
            }
        }
    }
}
