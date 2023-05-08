using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcPoint3dId
{
    public class GhcPoint3dIDConstruct : GH_Component
    {
        public GhcPoint3dIDConstruct()
          : base("ConstructPoint3d", "PtsID",
                "Combines lists of Point3d objects and their corresponding IDs into a list of Point3dId objects.",
              "DplMgr", "Point3dId")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "List of Point3d.", GH_ParamAccess.list);
            pManager.AddNumberParameter("IDs", "Id", "List of numbers that will become IDs.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Point3dId List", "PtID", "List of Point3dId objects.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();
            List<double> ids = new List<double>();

            if (!DA.GetDataList(0, points) || !DA.GetDataList(1, ids))
            {
                return;
            }

            if (points.Count != ids.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of points and IDs must be equal.");
                return;
            }

            List<Point3dId> point3dIDList = new List<Point3dId>();

            for (int i = 0; i < points.Count; i++)
            {
                point3dIDList.Add(new Point3dId((int)ids[i], points[i]));
            }

            DA.SetDataList(0, point3dIDList);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID;

        public override Guid ComponentGuid => new Guid("0E84D6C7-98D7-4320-B283-0CD289203EA2");

    }
}