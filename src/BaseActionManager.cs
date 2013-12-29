//Copyright © 2013 Dagorn Julien (julien.dagorn@gmail.com)
//This work is free. You can redistribute it and/or modify it under the
//terms of the Do What The Fuck You Want To Public License, Version 2,
//as published by Sam Hocevar. See the COPYING file for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ActionGroupManager
{
    static class BaseActionFilter
    {
        public static IEnumerable<BaseAction> FromParts(Part part)
        {
            List<BaseAction> ret = new List<BaseAction>();

            foreach (BaseAction ba in part.Actions)
                ret.Add(ba);

            foreach (PartModule pm in part.Modules)
            {
                ret.AddRange(FromModule(pm));
            }

            return ret;
        }

        public static IEnumerable<BaseAction> FromModule(PartModule module)
        {
            List<BaseAction> ret = new List<BaseAction>();

            foreach (BaseAction ba in module.Actions)
            {
                ret.Add(ba);
            }

            return ret;
        }

        public static IEnumerable<BaseAction> FromParts(IEnumerable<Part> parts)
        {
            List<BaseAction> ret = new List<BaseAction>();
            foreach (Part p in parts)
            {
                ret.AddRange(FromParts(p));
            }
            return ret;
        }

        public static IEnumerable<BaseAction> FromParts(IEnumerable<Part> parts, KSPActionGroup ag)
        {
            return FromParts(parts).Where(
                (e) =>
                {
                    return e.IsInActionGroup(ag);
                });
        }

        public static IEnumerable<KSPActionGroup> GetActionGroupList(BaseAction bA)
        {
            List<KSPActionGroup> ret = new List<KSPActionGroup>();

            foreach (KSPActionGroup ag in Enum.GetValues(typeof(KSPActionGroup)) as KSPActionGroup[])
            {
                if (ag == KSPActionGroup.None)
                    continue;

                if (bA.IsInActionGroup(ag))
                    ret.Add(ag);
            }
            return ret;
        }
    }
}
