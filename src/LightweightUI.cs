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
        public const string BASEACTIONSWITCHON = "EnableBaseActionEdit";
        public const string ACTIONGROUPSWITCHON = "EnableActionGroupEdit";

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
#if DEBUG
            Debug.Log("Switch folder !");
#endif
            FolderAndActionGroup.ForEach(
                (e) =>
                {
                    if (e.IsFolder)
                    {
                        e.Show(true);
#if DEBUG
                        Debug.Log("Folder switched !");
#endif
                    }
                }
                );
            IsFolderVisible = !IsFolderVisible;
            CurrentControler = sender;
        }

        public void SwitchActionGroup(UIGroupManager sender)
        {
            //if it's a folder we need to show the action linked to it
            if (sender.IsFolder)
            {
                FolderAndActionGroup.ForEach(
                (e) =>
                {
                    if (e.IsFolder)
                    {
                        e.Show(e == sender);
                    }
                });


                int index = (sender.Events[ACTIONGROUPSWITCHON].guiName == "    AGM : General") ? 2 : 9;
                int max = (sender.Events[ACTIONGROUPSWITCHON].guiName == "    AGM : General") ? 9 : 19;

                for (; index < max; index++)
                {
#if DEBUG
                    Debug.Log("Dispay Action Group " + index.ToString() + " on " + FolderAndActionGroup.Count);
#endif

                    FolderAndActionGroup[index].Show(true);
                }

            }
            // otherwise it's an actual action group button which registered a modification, so we close all the action group.
            else
            {
                FolderAndActionGroup.ForEach(
                (e) =>
                {
                    e.Show(false);
                });

                BaseActions.ForEach(
                (e) =>
                {
                    e.Show(true);
                });
            }
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
                    e.Events[BASEACTIONSWITCHON].guiActive = vis;
                    e.Events[BASEACTIONSWITCHON].active = vis;
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

    class UIActionManager : CleanablePartModule
    {
        UIPartModuleManager origin;

        public BaseAction baseAction;

        internal void Initialize(BaseAction ba, UIPartModuleManager org)
        {
            this.Events[BASEACTIONSWITCHON].guiName = "  AGM : " + ba.guiName;
            this.Events[BASEACTIONSWITCHON].guiActive = false;
            this.Events[BASEACTIONSWITCHON].active = false;

            baseAction = ba;

            origin = org;
        }

        public override void Clean()
        {
            this.Events[BASEACTIONSWITCHON].guiActive = false;
            this.Events[BASEACTIONSWITCHON].active = false;
        }

        public override void Show(bool vis)
        {
            this.Events[BASEACTIONSWITCHON].guiActive = vis;
            this.Events[BASEACTIONSWITCHON].active = vis;
        }

        public override void Terminate()
        {
            throw new NotImplementedException();
        }

        [KSPEvent(name = BASEACTIONSWITCHON)]
        void EnableBaseActionEdit()
        {
            //if the clicked base action is selected
            //we remove all others actions to show the group action selector.
            foreach (UIActionManager mod in origin.BaseActions)
            {
                mod.Show(mod == this);
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

            Events[ACTIONGROUPSWITCHON].guiActive = false;
            Events[ACTIONGROUPSWITCHON].active = false;

            IsFolder = isfolder;
        }

        [KSPEvent(name = ACTIONGROUPSWITCHON)]
        public void EnableActionGroupEdit()
        {
            if (this.IsFolder)
            {
                origin.SwitchActionGroup(this);
            }
            else
            {
                if (!origin.CurrentControler.baseAction.IsInActionGroup(ActionGroup))
                    origin.CurrentControler.baseAction.AddActionToAnActionGroup(ActionGroup);
                else
                    origin.CurrentControler.baseAction.RemoveActionToAnActionGroup(ActionGroup);

                origin.SwitchActionGroup(this);
            }
        }

        public override void Clean()
        {
            Events[ACTIONGROUPSWITCHON].guiActive = false;
            Events[ACTIONGROUPSWITCHON].active = false;
        }

        public override void Show(bool vis)
        {
            this.Events[ACTIONGROUPSWITCHON].guiActive = vis;
            this.Events[ACTIONGROUPSWITCHON].active = vis;

            if (!IsFolder)
            {
#if DEBUG
                Debug.Log("Construct string");
                Debug.Log("Is Origin null : " + (origin == null).ToString());
                Debug.Log("Is Current Controler null : " + (origin.CurrentControler == null).ToString());

#endif


                string str;
                if (origin.CurrentControler.baseAction.IsInActionGroup(ActionGroup))
                    str = "      * " + ActionGroup.ToString() + " *";
                else
                    str = "      " + ActionGroup.ToString();

#if DEBUG
                Debug.Log("String done");
#endif

                this.Events[ACTIONGROUPSWITCHON].guiName = str;
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
#if DEBUG
            Debug.Log("Build modules");
#endif

            //the main part in root
            Part root = VesselManager.Instance.ActiveVessel.rootPart;
            if (!root.Modules.Contains("UIRootManager"))
            {
                currentRootModule = root.AddModule("UIRootManager") as UIRootManager;
            }

            //Give each part her own set of baseactionmanager
            foreach (Part p in VesselManager.Instance.GetParts())
            {

                UIPartModuleManager partManager = new UIPartModuleManager();
                partManager.Part = p;
                partManager.IsActionGroupVisible = false;
                partManager.IsFolderVisible = false;

                if (!p.Modules.Contains("UIActionManager"))
                {
#if DEBUG
                    Debug.Log("Build action modules");
#endif
                    List<UIActionManager> actions = new List<UIActionManager>();
                    foreach (BaseAction ba in BaseActionFilter.FromParts(p))
                    {
                        UIActionManager act = p.AddModule("UIActionManager") as UIActionManager;
                        act.Initialize(ba, partManager);
                        actions.Add(act);
                    }

                    List<UIGroupManager> list = new List<UIGroupManager>();

                    if (!p.Modules.Contains("UIGroupManager"))
                    {
#if DEBUG
                        Debug.Log("Build group modules");
#endif
                        UIGroupManager uig;

                        uig = p.AddModule("UIGroupManager") as UIGroupManager;
                        uig.Initialize(KSPActionGroup.None, partManager, true);
                        uig.Events[UIRootManager.ACTIONGROUPSWITCHON].guiName = "    AGM : General";
                        list.Add(uig);

                        uig = p.AddModule("UIGroupManager") as UIGroupManager;
                        uig.Initialize(KSPActionGroup.None, partManager, true);
                        uig.Events[UIRootManager.ACTIONGROUPSWITCHON].guiName = "    AGM : Custom";
                        list.Add(uig);

                        foreach (KSPActionGroup ag in Enum.GetValues(typeof(KSPActionGroup)))
                        {
                            if (ag == KSPActionGroup.None)
                                continue;
                            uig = p.AddModule("UIGroupManager") as UIGroupManager;
                            uig.Initialize(ag, partManager);
                            uig.Events[CleanablePartModule.ACTIONGROUPSWITCHON].guiName = "      AGM : " + ag.ToString();
                            list.Add(uig);
                        }
                    }
                    else
                    {
                        foreach (PartModule mod in p.Modules)
                        {
                            if (mod is UIGroupManager)
                                list.Add(mod as UIGroupManager);
                        }
                    }

                    partManager.FolderAndActionGroup = list;

                    partManager.BaseActions = actions;
                    currentRootModule.childsModules.Add(partManager);
                }
            }

            //
#if DEBUG
            Debug.Log("Modules build");
#endif

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
