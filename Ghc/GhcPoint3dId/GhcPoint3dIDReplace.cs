using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using Eto.Forms;

namespace GeneratePlan.Ghc.GhcPoint3dId
{
    public class GhcPoint3dIDReplace : GH_Component
    {
        public GhcPoint3dIDReplace()
          : base("ReplacePoint3dID", "RepPtID", "Replaces Point3dId with another in the list by their Id",
              "DplMgr", "Point3dId")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "List of Point3dId", "P", "List of all Point3dId", GH_ParamAccess.list);
            pManager.AddParameter(new Point3dIdParam(), "Points to Replace", "R", "List of Point3dId to replace", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Repaired List", "P", "Repaired list of all Point3dId", GH_ParamAccess.list);
            pManager.AddBooleanParameter("IfSuccessfull", "ifS", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3dId> allPoints = new List<Point3dId>();
            List<Point3dId> pointsToReplace = new List<Point3dId>();
            List<bool> ifSuccess = new List<bool>();

            if (!DA.GetDataList(0, allPoints)) return;
            if (!DA.GetDataList(1, pointsToReplace)) return;

            for(int i = 0; i < pointsToReplace.Count; i++)
            {
                ifSuccess.Add(pointsToReplace[i].ReplaceInListById(allPoints));
            }

            DA.SetDataList(0, allPoints);
            DA.SetDataList(1, ifSuccess);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID;

        public override Guid ComponentGuid => new Guid("51658BF9-AE9C-4079-A435-6A0DA317106F");
    }
}