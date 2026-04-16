using ConfigLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ControllerMovementVS.config
{
    internal class ConfigLibHelper
    {
        const string modid = "controllermovementvs";
        readonly ICoreAPI api;
        readonly ControllerMovementVSModSystem mod;

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

        internal int gamepadSelectedIdx = 0;
        bool autoSprint = true;
        float deadzone = 0f;
        bool lookUsingRightStick = false;
        bool swapLeftRightSticks = false;
        float lookSensitivityHorizontal = 0f;
        float lookSensitivityVertical = 0f;
        internal List<bool> rebinding = [];

        //ConfigLib render
        private void Edit(ICoreAPI api, Config config, string id)
        {
            //keep polling in case the game is paused so we register controller changes
            if (mod.am is not null && mod.capi is not null && mod.capi.IsGamePaused) ControllerHelper.PollEvents(mod.am, mod, true);

            ImGui.Text(Lang.Get($"{modid}:SaveReminder"));

            ImGui.TextWrapped(Lang.Get($"{modid}:SelectGamepad"));
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
                    if (is_selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndListBox();
            }
            ImGui.SeparatorText(Lang.Get($"{modid}:StickSettings"));
            ImGui.Checkbox(Lang.Get($"{modid}:AutoSprint"), ref autoSprint);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:AutoSprintToolTip"));
            ImGui.SliderFloat(Lang.Get($"{modid}:Deadzone"), ref deadzone, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:DeadzoneToolTip"));

            ImGui.NewLine();
            ImGui.Checkbox(Lang.Get($"{modid}:SwapLeftRightSticks"), ref swapLeftRightSticks);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:SwapLeftRightSticksToolTip"));
            ImGui.Checkbox(Lang.Get($"{modid}:LookUsingRightStick"), ref lookUsingRightStick);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookUsingRightStickToolTip"));
            ImGui.SliderFloat(Lang.Get($"{modid}:LookSensitivityHorizontal"), ref lookSensitivityHorizontal, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookSensitivityHorizontalToolTip"));
            ImGui.SliderFloat(Lang.Get($"{modid}:LookSensitivityVertical"), ref lookSensitivityVertical, 0.0f, 10.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            ImGui.SetItemTooltip(Lang.Get($"{modid}:LookSensitivityVerticalToolTip"));

            ImGui.SeparatorText("Button Rebindings");
            if (ImGui.BeginTable("rebind table", 4, ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.Borders))
            {
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Text("Action:");
                ImGui.TableNextColumn(); ImGui.Text("Button:");
                ImGui.TableNextColumn(); ImGui.TableNextColumn();
                var bindings = BindingHelper.CurrentBindings;
                for (int i = 0; i < rebinding.Count; i++)
                {
                    ImGui.PushID(i);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(bindings[i].ControlName);
                    ImGui.TableNextColumn();
                    string buttName = bindings[i].GamepadButton.ToString();
                    if (bindings[i].GamepadButton == SDL3.SDL.GamepadButton.Invalid) buttName = ""; //ignore invalids and put a blank
                    ImGui.Text(buttName);
                    ImGui.TableNextColumn();
                    bool temp = rebinding[i];
                    ImGui.Selectable(temp ? "Press a button to bind..." : "Rebind", ref temp);
                    rebinding[i] = temp;
                    ImGui.TableNextColumn();
                    bool temp2 = false;
                    ImGui.Selectable("Unbind", ref temp2);
                    if (temp2)
                    {
                        BindingHelper.Unbind(i);
                    }
                    ImGui.PopID();
                }
                ImGui.EndTable();

                //check if a rebind button was pressed
                int rebindCount = rebinding.Count(x => x);
                if (rebindCount == 1)
                {
                    int index = rebinding.FindIndex(x => x);
                    if (index != -1) BindingHelper.StartRebind(index, this);
                }
                else if (rebindCount == 2)
                {
                    int lastRebind = BindingHelper.GetIndexOfRebinding();
                    if (lastRebind != -1)
                    {
                        BindingHelper.CancelRebind();
                        rebinding[lastRebind] = false;
                        int index = rebinding.FindIndex(x => x);
                        if (index != -1) BindingHelper.StartRebind(index, this);
                    }
                    else BindingHelper.CancelRebind();
                }
                else if (rebindCount > 2)
                {
                    BindingHelper.CancelRebind();
                }
            }
            ImGui.PushID("end"); ImGui.SeparatorText(""); ImGui.PopID();
            ImGui.Text("SDL Activated = " + mod.sdlActivated);
            bool restartsdl = false;
            ImGui.SameLine();
            restartsdl = ImGui.Button("Restart SDL");
            if (restartsdl) mod.RestartSDL();
        }
               
        private void LoadConfig()
        {
            ConfigLoader.TryToLoadConfig(api, mod);
            CopyConfigToThis();

            if (mod.am is not null)
            {
                if (!ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx))
                {
                    mod.Mod.Logger.Notification("less controllers");
                    //if gamepad can't be set (because theres not as many connected) then default to 0
                    gamepadSelectedIdx = 0;
                }
            }
            if (mod.config is not null) BindingHelper.LoadBindingsFromConfig(mod.config, this);
        }

        private void SetDefault()
        {
            mod.config = new();
            CopyConfigToThis();

            if (mod.am is not null)
            {
                ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx);
            }
            BindingHelper.LoadBindingsFromConfig(mod.config, this);
        }

        private void SaveConfig()
        {
            if (mod.am is not null)
            {
                ControllerHelper.SetGamepad(mod.am, gamepadSelectedIdx);
            }
            if (mod.config is not null)
            {
                BindingHelper.SaveBindingsToConfig(mod.config);
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
