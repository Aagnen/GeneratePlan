using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomAddCorner : GH_Component
    {
        public GhcRoomAddCorner()
          : base("AddCornerToRoom", "AddCrnr", "Adds a corner to the room and sorts all corners around the room center",
              "DplMgr", "Rooms")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Room", "R", "Room to add corner to", GH_ParamAccess.item);
            pManager.AddParameter(new Point3dIdParam(), "Corner", "C", "Corner to add to the room", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Modified Room", "R", "Modified room with new corner", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Room iRoom = null;
            Point3dId newCorner = null;

            if (!DA.GetData(0, ref iRoom)) return;
            if (!DA.GetData(1, ref newCorner)) return;

            Room newRoom = iRoom.Duplicate();

            newRoom.CornerPoints.Add(newCorner); //adds corner, calculates area, aspect ratio, and sorts point

            DA.SetData(0, newRoom);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("1A1606A6-F471-484E-9DC1-C562BDB42A02");
    }
}