using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GeneratePlan.Ghc.General
{
    public class GhcChooseRandom : GH_Component
    {

        public GhcChooseRandom()
          : base("Choose Random", "RandChoice",
            "Choose random element from the list",
            "DplMgr", "General")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("List", "List", "List", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Seed", "Seed", "Seed", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ChosenElement", "ChosenElement", "ChosenElement", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ChosenIndex", "ChosenIndex", "ChosenIndex", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> iList = new List<object>();
            DA.GetDataList("List", iList);

            int iSeed = 1;
            DA.GetData(1, ref iSeed);
            int outputIndex = GeneralMethods.ChooseRandomIndex(iList, iSeed);

            DA.SetData("ChosenElement", iList[outputIndex]);
            DA.SetData("ChosenIndex", outputIndex);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_uniwersalna;

        public override Guid ComponentGuid => new Guid("31f3c080-1481-49ad-af46-cf0a6fbc5666");
    }
}