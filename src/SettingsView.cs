//Copyright © 2013 Dagorn Julien (julien.dagorn@gmail.com)
//This work is free. You can redistribute it and/or modify it under the
//terms of the Do What The Fuck You Want To Public License, Version 2,
//as published by Sam Hocevar. See the COPYING file for more details.

using System.Reflection;
using UnityEngine;

namespace ActionGroupManager
{
    //Window to show available settings
    class SettingsView : UIObject
    {
        Rect settingsWindowPositon;
        WWW versionNumber;
        string lastVersion = "Checking ...";
        System.Version ver;

        public override void DoUILogic()
        {
            if (!IsVisible() || PauseMenu.isOpen || FlightResultsDialog.isDisplaying)
            {
                return;
            }

            GUI.skin = HighLogic.Skin;
            settingsWindowPositon = GUILayout.Window(this.GetHashCode(), settingsWindowPositon, DoMySettingsView, "Settings");
        }

        void DoMySettingsView(int id)
        {
            if (GUI.Button(new Rect(settingsWindowPositon.width - 24, 4, 20, 20), "X", Style.CloseButtonStyle))
            {
                ActionGroupManager.Manager.ShowSettings = false;
                return;
            }
            GUILayout.BeginVertical();

            GUILayout.Label("AGM Current version : " + Assembly.GetAssembly(typeof(ActionGroupManager)).GetName().Version.ToString(), Style.LabelExpandStyle);

            if (versionNumber.isDone && ver == null)
                ver = new System.Version(versionNumber.text.Substring(6, 7));

            GUILayout.Label("AGM Last version : " + (ver == null ? "Checking ..." : ver.ToString()), Style.LabelExpandStyle);

            if (ver != null && Assembly.GetAssembly(typeof(ActionGroupManager)).GetName().Version.CompareTo(ver) < 0)
                if (GUILayout.Button("Update"))
                    Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/61263");

            bool initial = SettingsManager.Settings.GetValue<bool>(SettingsManager.OrderByStage);
            bool final = GUILayout.Toggle(initial, "Order by stage", Style.ButtonToggleStyle);
            if (final != initial)
                SettingsManager.Settings.SetValue(SettingsManager.OrderByStage, final);

            initial = SettingsManager.Settings.GetValue<bool>(SettingsManager.OrderByModules);
            final = GUILayout.Toggle(initial, "Group by Modules", Style.ButtonToggleStyle);
            if (final != initial)
                SettingsManager.Settings.SetValue(SettingsManager.OrderByModules, final);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public override void Initialize(params object[] list)
        {
            settingsWindowPositon = new Rect(Screen.width / 2f - 100, Screen.height / 2f - 100, 200, 150);
            versionNumber = new WWW("https://raw.github.com/SirJu/ActionGroupManager/master/VERSION");
      
        }

        public override void Terminate()
        {
            SettingsManager.Settings.save();
        }

        public override void Reset()
        {
        }
    }
}
