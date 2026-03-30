using ConfigLib;
using ImGuiNET;
using Vintagestory.API.Common;

namespace ControllerMovementVS.config
{
    internal class ConfigLibHelper
    {
        const string Modid = "ControllerMovementVS";
        readonly ICoreAPI api;
        readonly ControllerMovementVSModSystem mod;

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
                if (buttons.Defaults) SetDefault();
                if (buttons.Reload) SaveConfig();
                if (mod.config is not null) Edit(api, mod.config, id);
            });
        }

        int gamepadSelectedIdx = 0;
        //int item_highlighted_idx = -1;
        bool autoSprint = true;
        float deadzone = 0f;
        bool LookUsingRightStick = false;
        bool SwapLeftRightSticks = false;
        float LookSensitivityHorizontal = 0f;
        float LookSensitivityVertical = 0f;

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
                    bool is_selected = gamepadSelectedIdx == n;
                    if (ImGui.Selectable(ControllerHelper.gamepadNames[n], is_selected))
                    {
                        gamepadSelectedIdx = n;
                        config.GamepadIndex = n;
                        mod.Mod.Logger.Notification("selected gamepad: " + n);
                    }
                    //if (item_highlight && ImGui.IsItemHovered())
                        //item_highlighted_idx = n;

                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (is_selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndListBox();
            }
            
            ImGui.Checkbox("Auto sprint", ref autoSprint);
            ImGui.DragFloat("Deadzone", ref deadzone, 0.01f, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.NewLine();
            ImGui.Checkbox("SwapLeftRightSticks", ref SwapLeftRightSticks);
            ImGui.Checkbox("LookUsingRightStick", ref LookUsingRightStick);
            ImGui.DragFloat("LookSensitivityHorizontal", ref LookSensitivityHorizontal, 0.01f, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.DragFloat("LookSensitivityVertical", ref LookSensitivityVertical, 0.01f, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
        }
               
        private void LoadConfig()
        {
            ConfigLoader.TryToLoadConfig(api, mod);
            CopyConfigToThis();

            if (mod.am is not null)
            {
                ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx);
            }            
        }

        private void SetDefault()
        {
            mod.config = new();
            CopyConfigToThis();

            if (mod.am is not null)
            {
                ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx);
            }
        }

        private void SaveConfig()
        {
            if (mod.am is not null)
            {
                ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx);
            }
            if (mod.config is not null)
            {
                mod.config.AutoSprint = autoSprint;
                mod.config.GamepadIndex = gamepadSelectedIdx;
                mod.config.DeadZone = deadzone;
                mod.config.LookUsingRightStick = LookUsingRightStick;
                mod.config.SwapLeftRightSticks = SwapLeftRightSticks;
                mod.config.LookSensitivityHorizontal = LookSensitivityHorizontal;
                mod.config.LookSensitivityVertical = LookSensitivityVertical;
                api.StoreModConfig(mod.config, "ControllerMovementVS.json");
            }
        }

        private void CopyConfigToThis()
        {
            if (mod.config is not null)
            {
                autoSprint = mod.config.AutoSprint;
                deadzone = mod.config.DeadZone;
                gamepadSelectedIdx = mod.config.GamepadIndex;
                LookUsingRightStick = mod.config.LookUsingRightStick;
                SwapLeftRightSticks = mod.config.SwapLeftRightSticks;
                LookSensitivityHorizontal = mod.config.LookSensitivityHorizontal;
                LookSensitivityVertical = mod.config.LookSensitivityVertical;
            }
        }
    }
}
