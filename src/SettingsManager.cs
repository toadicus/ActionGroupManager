//Copyright © 2013 Dagorn Julien (julien.dagorn@gmail.com)
//This work is free. You can redistribute it and/or modify it under the
//terms of the Do What The Fuck You Want To Public License, Version 2,
//as published by Sam Hocevar. See the COPYING file for more details.


using KSP.IO;

namespace ActionGroupManager
{
    //Wrapper for PluginConfiguration
    class SettingsManager
    {
        public static PluginConfiguration Instance { get; private set; }

        public static readonly string IsMainWindowVisible = "IsMainWindowVisible";
        public static readonly string IsIconLocked = "IsIconLocked";
        public static readonly string MainWindowRect = "MainWindowRect";
        public static readonly string IconRect = "IconRect";
        public static readonly string AutomaticPartCheck = "AutomaticPartCheck";
        public static readonly string FrequencyOfAutomaticUpdate = "FrequencyOfAutomaticUpdate";
        public static readonly string OrderByStage = "OrderByStage";
        public static readonly string QuietMode = "QuietMode";
        public static readonly string RecapWindocRect = "RecapWindowRect";
        public static readonly string IsRecapWindowVisible = "IsRecapWindowVisible";
        public static readonly string OrderByModules = "OrderByModules";
        public static readonly string LastCheckedVersion = "LastCheckedVersion";
        public static readonly string LastCheckDate = "LastCheckDate";


        static SettingsManager()
        {
            Instance = PluginConfiguration.CreateForType<ActionGroupManager>();
            Instance.load();
        }

        static void Save()
        {
            Instance.save();
        }

    }
}
