//Copyright © 2013 Dagorn Julien (julien.dagorn@gmail.com)
//This work is free. You can redistribute it and/or modify it under the
//terms of the Do What The Fuck You Want To Public License, Version 2,
//as published by Sam Hocevar. See the COPYING file for more details.

using System;
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

            VersionLogic();

            bool initial = SettingsManager.Instance.GetValue<bool>(SettingsManager.OrderByStage);
            bool final = GUILayout.Toggle(initial, "Order by stage", Style.ButtonToggleStyle);
            if (final != initial)
                SettingsManager.Instance.SetValue(SettingsManager.OrderByStage, final);

            initial = SettingsManager.Instance.GetValue<bool>(SettingsManager.OrderByModules);
            final = GUILayout.Toggle(initial, "Group by Modules", Style.ButtonToggleStyle);
            if (final != initial)
                SettingsManager.Instance.SetValue(SettingsManager.OrderByModules, final);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void VersionLogic()
        {
            GUILayout.Label("AGM Current version : " + Assembly.GetAssembly(typeof(ActionGroupManager)).GetName().Version.ToString(), Style.LabelExpandStyle);             

            string str = string.Empty;
            if (versionNumber == null)
                str = "Current.";
            else if (!versionNumber.isDone)
                str = "Checking ...";
            else if (versionNumber.isDone && ver == null)
            {
                ver = new System.Version(versionNumber.text.Substring(6, 7));
                SettingsManager.Instance.SetValue(SettingsManager.LastCheckedVersion, ver.ToString());
                str = ver.ToString();
            }

            GUILayout.Label("AGM Last version : " + str, Style.LabelExpandStyle);

            if (ver != null && Assembly.GetAssembly(typeof(ActionGroupManager)).GetName().Version.CompareTo(ver) < 0)
                if (GUILayout.Button("Update"))
                    Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/61263");
        }

        public override void Initialize(params object[] list)
        {
            settingsWindowPositon = new Rect(Screen.width / 2f - 100, Screen.height / 2f - 100, 200, 150);

            string version = SettingsManager.Instance.GetValue<string>(SettingsManager.LastCheckedVersion, string.Empty);
            DateTime date = SettingsManager.Instance.GetValue<DateTime>(SettingsManager.LastCheckDate, DateTime.Today);
            if (DateTime.Now > date.AddDays(1))
            {
                versionNumber = new WWW("https://raw.github.com/SirJu/ActionGroupManager/master/VERSION");
                SettingsManager.Instance.SetValue(SettingsManager.LastCheckDate, DateTime.Today);
            }
            else
            {
                SettingsManager.Instance.SetValue(SettingsManager.LastCheckedVersion, Assembly.GetAssembly(typeof(ActionGroupManager)).GetName().Version.ToString());
            }
        }

        public override void Terminate()
        {
            SettingsManager.Instance.save();
        }

        public override void Reset()
        {
        }
    }
}
