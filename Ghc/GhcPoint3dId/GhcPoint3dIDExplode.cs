using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcPoint3dId
{
    public class GhcPoint3dIDExplode : GH_Component
    {
        public GhcPoint3dIDExplode()
          : base("Explode Point3d", "PtID", "Splits a list of Point3dId objects into separate lists of Point3d objects and their corresponding IDs as strings.",
              "DplMgr", "Point3dId")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Point3dId List", "PtID", "List of Point3dId objects.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "List of Point3d objects.", GH_ParamAccess.list);
            pManager.AddTextParameter("IDs", "Id", "List of IDs as strings.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3dId> point3dIDList = new List<Point3dId>();

            if (!DA.GetDataList(0, point3dIDList))
            {
                return;
            }

            List<Point3d> points = new List<Point3d>();
            List<string> ids = new List<string>();

            foreach (Point3dId point3dID in point3dIDList)
            {
                points.Add(point3dID.Point);
                ids.Add(point3dID.Id.ToString());
            }

            DA.SetDataList(0, points);
            DA.SetDataList(1, ids);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID;

        public override Guid ComponentGuid => new Guid("4D347C7E-8155-4AB5-ABD7-C6E7AA83750C");
    }
}