//Copyright © 2013 Dagorn Julien (julien.dagorn@gmail.com)
//This work is free. You can redistribute it and/or modify it under the
//terms of the Do What The Fuck You Want To Public License, Version 2,
//as published by Sam Hocevar. See the COPYING file for more details.

using System.Collections.Generic;
using UnityEngine;

namespace ActionGroupManager
{
    /*
     * Main class.
     * Intercept the start call of KSP, handle UI class and The VesselPartManager.
     */ 
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ActionGroupManager : MonoBehaviour
    {
        //List of current UI handle
        Dictionary<string, UIObject> UiList;

        static ActionGroupManager _manager;

        public static ActionGroupManager Manager
        {
            get
            {
                return _manager;
            }
        }

        public bool ShowSettings { get; set; }

        public bool ShowMainWindow { get; set; }

        void Awake()
        {
#if DEBUG   
            Debug.Log("AGM : Action Group Manager is awake.");
#endif
        }

        void Start()
        {
            _manager = this;

            UiList = new Dictionary<string, UIObject>();

            LightweightUINew light = new LightweightUINew();
            light.Initialize();
            UiList.Add("Light", light);

            View viewMan = new View();
            viewMan.Initialize();
            UiList.Add("Main", viewMan);

            ShortcutNew shortcut = new ShortcutNew();
            shortcut.Initialize(viewMan);
            UiList.Add("Icon", shortcut);


            viewMan.SetVisible(SettingsManager.Settings.GetValue<bool>(SettingsManager.IsMainWindowVisible));

            //TESTING : TO REMOVE
            //TogglePanel panel = new TogglePanel();
            //panel.Initialize();
            //panel.SetVisible(true);
            //UiList.Add("Panel", panel);
            //ENDTESTING

#if DEBUG
            Debug.Log("AGM : Action Group Manager has started.");
#endif
        }      

        void Update()
        {
            //if (SettingsManager.Settings.GetValue<bool>(SettingsManager.AutomaticPartCheck, true) && Time.time - lastUpdate > (float)SettingsManager.Settings.GetValue<int>(SettingsManager.FrequencyOfAutomaticUpdate, 1))
            //{
            //    VesselManager.Instance.Update();
            //    lastUpdate = Time.time;
            //}

            if (ShowSettings && !UiList.ContainsKey("Settings"))
            {
                SettingsView setting = new SettingsView();
                setting.Initialize();
                setting.SetVisible(true);
                UiList.Add("Settings", setting);
            }
            else if (!ShowSettings && UiList.ContainsKey("Settings"))
            {
                UiList["Settings"].SetVisible(false);
                UiList["Settings"].Terminate();
                UiList.Remove("Settings");
            }
        }

        public void ToggleQuietMode()
        {
            bool b = SettingsManager.Settings.GetValue<bool>(SettingsManager.QuietMode, false);
            if(!b)
            {
                UIObject o;
                if (UiList.TryGetValue("Main", out o))
                {
                    o.Terminate();
                    o.SetVisible(false);
                    UiList.Remove("Main");
                }
                o=null;

                if (UiList.TryGetValue("Icon", out o))
                {
                    o.Terminate();
                    UiList.Remove("Icon");
                }
            }
            else
            {
                View viewMan = new View();
                viewMan.Initialize();
                UiList.Add("Main", viewMan);

                ShortcutNew shortcut = new ShortcutNew();
                shortcut.Initialize(viewMan);
                UiList.Add("Icon", shortcut);


                viewMan.SetVisible(SettingsManager.Settings.GetValue<bool>(SettingsManager.IsMainWindowVisible));
            }
            SettingsManager.Settings.SetValue(SettingsManager.QuietMode, !b);

        }

        public void UpdateIcon(bool val)
        {
            UIObject o;
            if (UiList.TryGetValue("Icon", out o))
                (o as ShortcutNew).SwitchTexture(val);
        }

        void OnDestroy()
        {
            //Terminate all UI
            foreach (KeyValuePair<string, UIObject> ui in UiList)
                ui.Value.Terminate();
            //Save settings to disk
            SettingsManager.Settings.save();

            VesselManager.Terminate();

#if DEBUG
            Debug.Log("AGM : Terminated.");
#endif
        }

    }
}

