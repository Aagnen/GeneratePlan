using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcPoint3dId
{
    public class GhcPoint3dIDDuplicate : GH_Component
    {
        public GhcPoint3dIDDuplicate()
          : base("DuplicatePoint3dID", "DupPtID", "Duplicates a Point3dId by Id and adds it to the list of all points",
              "DplMgr", "Point3dId")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "List of Point3dId", "P", "List of all Point3dId", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Id to Duplicate", "Id", "Id of the Point3dId to duplicate", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IfToDuplicate", "IfToDup", "If true, the specified Point3dId will be duplicated", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "List of Point3dId", "P", "List of all Point3dId, including the duplicated one if IfToDuplicate is true", GH_ParamAccess.list);
            pManager.AddIntegerParameter("New Point Id", "NewID", "Id of the new duplicated point, if IfToDuplicate is true", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3dId> points = new List<Point3dId>();
            int idToDuplicate = 0;
            bool ifToDuplicate = false;

            if (!DA.GetDataList(0, points)) return;
            if (!DA.GetData(1, ref idToDuplicate)) return;
            if (!DA.GetData(2, ref ifToDuplicate)) return;

            if (ifToDuplicate)
            {
                Point3dId pointToDuplicate = points.FirstOrDefault(point => point.Id == idToDuplicate);

                if (pointToDuplicate != null)
                {
                    int newId = points.Max(point => point.Id) + 1;
                    Point3dId newPoint = new Point3dId(newId, pointToDuplicate.Point);
                    points.Add(newPoint);
                    DA.SetDataList(0, points);
                    DA.SetData(1, newId);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Point with the specified Id not found");
                }
            }
            else
            {
                DA.SetDataList(0, points);
                DA.SetData(1, null);
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID;

        public override Guid ComponentGuid => new Guid("2A193832-FFBC-4D04-AE6E-1504BAE274D0");
    }
}