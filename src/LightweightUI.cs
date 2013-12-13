using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ActionGroupManager
{
    abstract class CleanablePartModule : PartModule
    {

        public const string MAINSWITCHON = "EnableEdit";
        public const string MAINSWITCHOFF = "DisableEdit";
        public const string QUIETMODEON = "EnableQuietMode";
        public const string QUIETMODEOFF = "DisableQuietMode";
        public const string BASEACTIONSWITCH = "EnableBaseActionEdit";
        public const string ACTIONGROUPSWITCH = "EnableActionGroupEdit";

        public abstract void Clean();

        public abstract void Terminate();

        public abstract void Show(bool vis);
    }

    class UIPartModuleManager : CleanablePartModule
    {
        public Part Part { get; set; }
        public List<UIGroupManager> FolderAndActionGroup { get; set; }
        public List<UIActionManager> BaseActions { get; set; }
        public bool IsFolderVisible = false;
        public bool IsActionGroupVisible = false;
        public bool IsVisible = false;

        public UIActionManager CurrentControler;

        public void SwitchFolder(UIActionManager sender)
        {
            bool display;
            if (IsFolderVisible)
            {
                //If folder are already displayed, it may be that the user want to close the actual displayed folder, or clicked on another action.
                display = false;
            }
            else
            {
                display = true;
            }

            //We display both folders
            FolderAndActionGroup.ForEach(
                (e) =>
                {
                    if (e.IsFolder)
                    {
                        e.Show(display);
                    }
                }
                );
            IsFolderVisible = display;
            CurrentControler = IsFolderVisible ? sender : null;

        }

        public override void Terminate()
        {
            BaseActions.ForEach((e) => e.Terminate());
            FolderAndActionGroup.ForEach((e) => e.Terminate());
        }

        public override void Show(bool vis)
        {
            BaseActions.ForEach(
                (e) =>
                {
                    e.Events[BASEACTIONSWITCH].guiActive = vis;
                    e.Events[BASEACTIONSWITCH].active = vis;
                });
        }

        public override void Clean()
        {
            FolderAndActionGroup.ForEach((e) => e.Clean());
            BaseActions.ForEach((e) => e.Clean());
        }
    }

    class UIRootManager : CleanablePartModule
    {
        public List<CleanablePartModule> childsModules = new List<CleanablePartModule>();

        void Initialize()
        {
        }

        public override void Clean()
        {
            childsModules.ForEach((e) => e.Clean());
        }

        public override void Show(bool vis)
        {
            Events[MAINSWITCHON].guiActive = !vis;
            Events[MAINSWITCHON].active = !vis;

            Events[MAINSWITCHOFF].guiActive = vis;
            Events[MAINSWITCHOFF].active = vis;
        }


        public override void Terminate()
        {
            if (childsModules.Count > 0)
            {
                childsModules.ForEach(
                    (e) =>
                    {
                        e.Terminate();
                    }
                    );
            }
        }

        [KSPEvent(name = MAINSWITCHON, guiName = "AGM : Enable", guiActive = true, active = true)]
        public void EnableEdit()
        {
            Show(true);

            childsModules.ForEach(
                (e) =>
                {
                    e.Show(true);
                });
        }

        [KSPEvent(name = MAINSWITCHOFF, guiName = "AGM : Disable", guiActive = false, active = false)]
        public void DisableEdit()
        {
            this.Clean();


            Show(false);
        }
    }

    class UIQuietModeToggle : CleanablePartModule
    {
        public Action Toggled;

        public override void Clean()
        {
            throw new NotImplementedException();
        }

        public override void Terminate()
        {
            throw new NotImplementedException();
        }

        public override void Show(bool vis)
        {
            this.Events[QUIETMODEON].guiActive = !vis;
            this.Events[QUIETMODEON].active = !vis;

            this.Events[QUIETMODEOFF].guiActive = vis;
            this.Events[QUIETMODEOFF].active = vis;
        }

        [KSPEvent(name = QUIETMODEON, guiName = "AGM : Enable Quiet Mode", guiActive = true, active = true)]
        public void EnableQuietMode()
        {
            if (Toggled != null)
                Toggled();
            Show(true);
        }

        [KSPEvent(name = QUIETMODEOFF, guiName = "AGM : Disable Quiet Mode", guiActive = false, active = false)]
        public void DisableQuietMode()
        {
            if (Toggled != null)
                Toggled();
            Show(false);
        }
    }

    class UIActionManager : CleanablePartModule
    {
        UIPartModuleManager origin;

        public BaseAction baseAction;

        internal void Initialize(BaseAction ba, UIPartModuleManager org)
        {
            this.Events[BASEACTIONSWITCH].guiName = "  AGM : " + ba.guiName;
            this.Events[BASEACTIONSWITCH].guiActive = false;
            this.Events[BASEACTIONSWITCH].active = false;

            baseAction = ba;

            origin = org;
        }

        public override void Clean()
        {
            this.Events[BASEACTIONSWITCH].guiActive = false;
            this.Events[BASEACTIONSWITCH].active = false;
        }

        public override void Show(bool vis)
        {
            this.Events[BASEACTIONSWITCH].guiActive = vis;
            this.Events[BASEACTIONSWITCH].active = vis;
        }

        public override void Terminate()
        {
            throw new NotImplementedException();
        }

        [KSPEvent(name = BASEACTIONSWITCH)]
        void EnableBaseActionEdit()
        {
            //if the clicked base action is selected
            //we remove all others actions to show the group action selector.
                
            origin.BaseActions.ForEach((mod)=> mod.Show(origin.IsFolderVisible ? true : mod == this));

            if(origin.IsActionGroupVisible)
            {
                origin.FolderAndActionGroup.ForEach((e) => { if (!e.IsFolder) e.Show(false); origin.IsActionGroupVisible = false; });
            }

            origin.SwitchFolder(this);
        }
    }

    class UIGroupManager : CleanablePartModule
    {
        public KSPActionGroup ActionGroup { get; set; }
        public bool IsFolder { get; set; }

        public UIPartModuleManager origin;

        public void Initialize(KSPActionGroup ag, UIPartModuleManager org, bool isfolder = false)
        {
            origin = org;
            ActionGroup = ag;

            Events[ACTIONGROUPSWITCH].guiActive = false;
            Events[ACTIONGROUPSWITCH].active = false;

            IsFolder = isfolder;
        }

        [KSPEvent(name = ACTIONGROUPSWITCH)]
        public void EnableActionGroupEdit()
        {
            //if (!this.IsFolder)
            //{
            //    if (!origin.CurrentControler.baseAction.IsInActionGroup(ActionGroup))
            //        origin.CurrentControler.baseAction.AddActionToAnActionGroup(ActionGroup);
            //    else
            //        origin.CurrentControler.baseAction.RemoveActionToAnActionGroup(ActionGroup);

            //    origin.SwitchActionGroup(this);
            //}
            //else
            //{
            //    if (origin.IsFolderVisible)
            //    {
            //        origin.SwitchFolder(null);
            //    }
            //    else
            //    {
            //        origin.SwitchActionGroup(this);
            //    }
            //}

            if (this.IsFolder)
            {
                if (origin.IsActionGroupVisible)
                {
                    origin.FolderAndActionGroup.ForEach(
                        (e) =>
                        {
                            e.Show(e.IsFolder);
                        });

                    origin.IsActionGroupVisible = false;
                }
                else
                {
                    origin.FolderAndActionGroup.ForEach(
                        (e) =>
                        {
                            if (e.IsFolder && e != this)
                                e.Show(false);
                        });

                    int index = (this.Events[ACTIONGROUPSWITCH].guiName == "    AGM : General") ? 2 : 9;
                    int max = (this.Events[ACTIONGROUPSWITCH].guiName == "    AGM : General") ? 9 : 19;

                    for (; index < max; index++)
                    {
                        origin.FolderAndActionGroup[index].Show(true);
                    }

                    origin.IsActionGroupVisible = true;
                    
                }
            }
            else
            {
                if (!origin.CurrentControler.baseAction.IsInActionGroup(ActionGroup))
                    origin.CurrentControler.baseAction.AddActionToAnActionGroup(ActionGroup);
                else
                    origin.CurrentControler.baseAction.RemoveActionToAnActionGroup(ActionGroup);

                origin.FolderAndActionGroup.ForEach(
                (e) =>
                {
                    e.Show(false);
                });

                origin.BaseActions.ForEach(
                (e) =>
                {
                    e.Show(true);
                });

                origin.IsActionGroupVisible = true;
                origin.IsFolderVisible = false;
                origin.CurrentControler = null;
            }
        }

        public override void Clean()
        {
            Events[ACTIONGROUPSWITCH].guiActive = false;
            Events[ACTIONGROUPSWITCH].active = false;
        }

        public override void Show(bool vis)
        {
            this.Events[ACTIONGROUPSWITCH].guiActive = vis;
            this.Events[ACTIONGROUPSWITCH].active = vis;

            if (!IsFolder)
            {
                string str;
                if (origin.CurrentControler.baseAction.IsInActionGroup(ActionGroup))
                    str = "      * " + ActionGroup.ToString() + " *";
                else
                    str = "      " + ActionGroup.ToString();

                this.Events[ACTIONGROUPSWITCH].guiName = str;
            }
        }

        public override void Terminate()
        {
            throw new NotImplementedException();
        }
    }

    class LightweightUI : UIObject
    {
        UIRootManager currentRootModule;
        UIQuietModeToggle toggleMode;

        #region override
        public override void Initialize(params object[] list)
        {
#if DEBUG
            Debug.Log("Initialize Light UI");
#endif
            VesselManager.Instance.DatabaseUpdated += Instance_DatabaseUpdated;

            Instance_DatabaseUpdated(this, EventArgs.Empty);
        }

        void Instance_DatabaseUpdated(object sender, EventArgs e)
        {
            //the main part in root
            Part root = VesselManager.Instance.ActiveVessel.rootPart;
            if (!root.Modules.Contains("UIRootManager"))
            {
                //Make sure to make the controler part first in list
                if (root.Modules.Contains("UIActionManager"))
                {
                    List<UIActionManager> actionToRemove = new List<UIActionManager>();
                    foreach (PartModule mod in root.Modules)
                    {
                        if (mod is UIActionManager)
                        {
                            actionToRemove.Add(mod as UIActionManager);
                        }
                    }

                    if (actionToRemove.Count > 0)
                        actionToRemove.ForEach((mod) => root.Modules.Remove(mod));
                }

                if (root.Modules.Contains("UIGroupManager"))
                {
                    List<UIGroupManager> actionToRemove = new List<UIGroupManager>();
                    foreach (PartModule mod in root.Modules)
                    {
                        if (mod is UIGroupManager)
                        {
                            actionToRemove.Add(mod as UIGroupManager);
                        }
                    }

                    if (actionToRemove.Count > 0)
                        actionToRemove.ForEach((mod) => root.Modules.Remove(mod));
                }

                currentRootModule = root.AddModule("UIRootManager") as UIRootManager;

            }
            else
            {
                foreach (PartModule mod in root.Modules)
                {
                    if (mod is UIRootManager)
                        currentRootModule = mod as UIRootManager;
                }
            }


            //Insert the quiet mode toggle
            toggleMode = root.AddModule("UIQuietModeToggle") as UIQuietModeToggle;
            toggleMode.Show(SettingsManager.Settings.GetValue<bool>(SettingsManager.QuietMode, false));
            toggleMode.Toggled = new Action(ActionGroupManager.Manager.ToggleQuietMode);

            //Give each part her own set of baseactionmanager
            foreach (Part p in VesselManager.Instance.GetParts())
            {
                //Case of docked vessel : Remove any other UIRootManager
                if (p.Modules.Contains("UIRootManager") && p != root)
                {
                    UIRootManager rootToRemove = null;
                    foreach(PartModule mod in p.Modules)
                    {
                        if (mod is UIRootManager)
                        {
                            rootToRemove = mod as UIRootManager;
                            break;
                        }
                    }

                    if (rootToRemove != null)
                        p.RemoveModule(rootToRemove);
                }

                UIPartModuleManager partManager = new UIPartModuleManager();
                partManager.Part = p;
                partManager.IsActionGroupVisible = false;
                partManager.IsFolderVisible = false;


                List<UIActionManager> actions = new List<UIActionManager>();

                List<UIGroupManager> âctionGroupList = new List<UIGroupManager>();


                if (!p.Modules.Contains("UIActionManager"))
                {
                    foreach (BaseAction ba in BaseActionFilter.FromParts(p))
                    {
                        UIActionManager act = p.AddModule("UIActionManager") as UIActionManager;
                        act.Initialize(ba, partManager);
                        actions.Add(act);
                    }


                    if (!p.Modules.Contains("UIGroupManager"))
                    {
                        UIGroupManager uig;

                        uig = p.AddModule("UIGroupManager") as UIGroupManager;
                        uig.Initialize(KSPActionGroup.None, partManager, true);
                        uig.Events[UIRootManager.ACTIONGROUPSWITCH].guiName = "    AGM : General";
                        âctionGroupList.Add(uig);

                        uig = p.AddModule("UIGroupManager") as UIGroupManager;
                        uig.Initialize(KSPActionGroup.None, partManager, true);
                        uig.Events[UIRootManager.ACTIONGROUPSWITCH].guiName = "    AGM : Custom";
                        âctionGroupList.Add(uig);

                        foreach (KSPActionGroup ag in Enum.GetValues(typeof(KSPActionGroup)))
                        {
                            if (ag == KSPActionGroup.None)
                                continue;
                            uig = p.AddModule("UIGroupManager") as UIGroupManager;
                            uig.Initialize(ag, partManager);
                            uig.Events[CleanablePartModule.ACTIONGROUPSWITCH].guiName = "      AGM : " + ag.ToString();
                            âctionGroupList.Add(uig);
                        }
                    }
                    else
                    {
                        foreach (PartModule mod in p.Modules)
                        {
                            if (mod is UIGroupManager)
                                âctionGroupList.Add(mod as UIGroupManager);
                        }
                    }

                }
                else
                {
                    foreach (PartModule mod in p.Modules)
                    {
                        if (mod is UIActionManager)
                            actions.Add(mod as UIActionManager);
                    }
                }



                partManager.FolderAndActionGroup = âctionGroupList;

                partManager.BaseActions = actions;
                currentRootModule.childsModules.Add(partManager);

            }

        }

        public override void Terminate()
        {
            VesselManager.Instance.DatabaseUpdated -= Instance_DatabaseUpdated;
            if (currentRootModule != null)
                currentRootModule.Terminate();
        }

        public override void DoUILogic()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
