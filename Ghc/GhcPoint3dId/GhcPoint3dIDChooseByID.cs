using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcPoint3dId
{
    public class GhcPoint3dIDChooseByID : GH_Component
    {
        public GhcPoint3dIDChooseByID()
          : base("Get Point3dId by Id", "GetPointByID", "Gets a Point3dId from a list by Id",
              "DplMgr", "Point3dId")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Point3dId List", "P", "List of Point3dId objects", GH_ParamAccess.list);
            pManager.AddNumberParameter("Id", "Id", "Id to search for", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Point3dId", "P", "Point3dId with the matching Id", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var point3dIDList = new List<Point3dId>();
            double idToSearch = 0;

            if (!DA.GetDataList(0, point3dIDList)) return;
            if (!DA.GetData(1, ref idToSearch)) return;

            Point3dId matchingPoint3dID = point3dIDList.Find(p => p.Id == idToSearch); //NOTES: Find Point3dId by Id

            DA.SetData(0, matchingPoint3dID);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID;

        public override Guid ComponentGuid => new Guid("79C58044-4439-43A0-9FDC-CCBF34142AB2");
    }
}