using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GeneratePlan.Ghc.General
{
    public class GhcRenumerateTree : GH_Component
    {
        public GhcRenumerateTree()
          : base("Renumerate Tree", "Renum",
              "Renumerate tree starting from 0, delete null and invalids",
              "DplMgr", "General")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree in", "Tree in", "Tree in", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree Out", "Tree Out", "Tree Out", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree(0, out GH_Structure<IGH_Goo> iTree);

            iTree = CleanTree(iTree);

            DA.SetDataTree(0, iTree);
        }

        public GH_Structure<T> CleanTree<T>(GH_Structure<T> tree) where T : IGH_Goo
        {
            // We are not allowed to modify the tree that comes out of the input.
            // So we'll just create a new one and populate it over time.
            var clean = new GH_Structure<T>();

            int k = 0;
            for (int p = 0; p < tree.PathCount; p++)
            {
                var list = tree.Branches[p];

                // Ignore empty branches.
                if (list.Count == 0)
                    continue;

                // Remove nulls and invalids from the branch,
                // this means make a copy of it as we are going to change it.
                list = new List<T>(list);
                list.RemoveAll(goo => goo == null);
                list.RemoveAll(goo => !goo.IsValid);

                // The list is empty after removing all nulls and invalids.
                if (list.Count == 0)
                    continue;

                clean.AppendRange(list, new GH_Path(k++));
            }

            return clean;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_uniwersalna;

        public override Guid ComponentGuid => new Guid("ABA25B3A-CC83-45F0-ADCC-AD27C9654A76");
    }
}