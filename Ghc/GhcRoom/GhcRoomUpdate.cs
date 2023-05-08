using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomUpdate : GH_Component
    {
        public GhcRoomUpdate()
          : base("Update Room", "UpdateRoom",
              "Updates the room with new corner points by their Id (if provided) and recomputes its area and aspect ratio.",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Room", "R", "The room to update", GH_ParamAccess.item);
            pManager.AddParameter(new Point3dIdParam(), "Corner Points", "C", "List of Point3dId to replace the existing corner points", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Updated Room", "R", "The updated room", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Room iRoom = null;
            List<Point3dId> newCornerPoints = new List<Point3dId>();

            if (!DA.GetData(0, ref iRoom)) return;

            Room room = iRoom.Duplicate(); //NOTES: Naprawiło problem aktualizacji wstecznej

            room.UpdateCornersByID(newCornerPoints);
            room.UpdateArea();
            room.UpdateAspectRatio();

            DA.SetData(0, room);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("B5713F6D-5214-4893-8A44-36A42A0DD121");
    }
}