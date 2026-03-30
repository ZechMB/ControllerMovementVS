using ConfigLib;
using ImGuiNET;
using System.Numerics;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ControllerMovementVS.config
{
    internal class ConfigLibHelper
    {
        //const string modid = "ControllerMovementVS";
        const string modid = "controllermovementvs";
        readonly ICoreAPI api;
        readonly ControllerMovementVSModSystem mod;

        //private const string categorycontroller = $"{modid}:Config.Category.Crafting"; Lang.Get(categorycontroller);

        public ConfigLibHelper(ICoreAPI api, ControllerMovementVSModSystem modSystem)
        {
            this.api = api;
            mod = modSystem;
            LoadConfig();
            
            api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(modid, (id, buttons) =>
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
        bool lookUsingRightStick = false;
        bool swapLeftRightSticks = false;
        float lookSensitivityHorizontal = 0f;
        float lookSensitivityVertical = 0f;

        private void Edit(ICoreAPI api, Config config, string id)
        {
            //keep polling in case the game is paused so we register if a controller connects
            if (mod.am is not null && mod.capi is not null && mod.capi.IsGamePaused) ControllerHelper.PollEvents(mod.am, mod);

            //bool item_highlight = false;

            ImGui.TextWrapped(Lang.Get($"{modid}:SelectGamepad") + ":");
            if (ImGui.BeginListBox("", new Vector2(-1f, 4 * ImGui.GetTextLineHeightWithSpacing())))
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

            ImGui.Checkbox(Lang.Get($"{modid}:AutoSprint"), ref autoSprint);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:AutoSprintToolTip"));
            ImGui.DragFloat(Lang.Get($"{modid}:Deadzone"), ref deadzone, 0.01f, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:DeadzoneToolTip"));
            ImGui.NewLine();
            ImGui.Checkbox(Lang.Get($"{modid}:SwapLeftRightSticks"), ref swapLeftRightSticks);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:SwapLeftRightSticksToolTip"));
            ImGui.Checkbox(Lang.Get($"{modid}:LookUsingRightStick"), ref lookUsingRightStick);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookUsingRightStickToolTip"));
            ImGui.DragFloat(Lang.Get($"{modid}:LookSensitivityHorizontal"), ref lookSensitivityHorizontal, 0.01f, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookSensitivityHorizontalToolTip"));
            ImGui.DragFloat(Lang.Get($"{modid}:LookSensitivityVertical"), ref lookSensitivityVertical, 0.01f, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookSensitivityVerticalToolTip"));
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
                mod.config.LookUsingRightStick = lookUsingRightStick;
                mod.config.SwapLeftRightSticks = swapLeftRightSticks;
                mod.config.LookSensitivityHorizontal = lookSensitivityHorizontal;
                mod.config.LookSensitivityVertical = lookSensitivityVertical;
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
                lookUsingRightStick = mod.config.LookUsingRightStick;
                swapLeftRightSticks = mod.config.SwapLeftRightSticks;
                lookSensitivityHorizontal = mod.config.LookSensitivityHorizontal;
                lookSensitivityVertical = mod.config.LookSensitivityVertical;
            }
        }
    }
}
