using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomExplode : GH_Component
    {
        public GhcRoomExplode()
          : base("Explode Room", "DecRm", "Deconstructs a Room object into its corner points, function, Id, and area",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Room", "R", "Room object to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Corner Points", "CP", "List of corner points of the room", GH_ParamAccess.list);
            pManager.AddTextParameter("Function", "F", "Function of the room", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Id", "Id", "Id of the room", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Area of the room", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RoomGoo roomGoo = new RoomGoo();
            if (!DA.GetData(0, ref roomGoo)) { return; }

            Room room = roomGoo.Value;
            DA.SetDataList(0, room.CornerPoints);
            DA.SetData(1, room.Function);
            DA.SetData(2, room.Id);
            DA.SetData(3, room.Area);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("98A5FC3E-2570-418B-B809-104A246BC111");
    }
}