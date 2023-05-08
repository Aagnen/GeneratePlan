using System;
using System.Collections.Generic;
using GeneratePlan.Class;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomReplaceCorner : GH_Component
    {
        public GhcRoomReplaceCorner()
          : base("Replace room Corner", "ReplaceCorner",
              "Replaces a corner point in the iRoom with a new point",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("room", "R", "room to update", GH_ParamAccess.item);
            pManager.AddGenericParameter("Point to Replace", "P1", "Point3dId to be replaced in the iRoom's corners", GH_ParamAccess.item);
            pManager.AddGenericParameter("Replacement Point", "P2", "Point3dId to use as replacement", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Updated room", "R", "Updated iRoom with the corner point replaced", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Room iRoom = null;
            Point3dId pointToReplace = null;
            Point3dId replacementPoint = null;

            if (!DA.GetData(0, ref iRoom)) return;
            if (!DA.GetData(1, ref pointToReplace)) return;
            if (!DA.GetData(2, ref replacementPoint)) return;

            Room room = iRoom.Duplicate();

            room.ReplaceCorners(pointToReplace, replacementPoint);

            DA.SetData(0, room);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("F2014D00-FF23-4D4F-8E79-0CD699508849");
    }
}