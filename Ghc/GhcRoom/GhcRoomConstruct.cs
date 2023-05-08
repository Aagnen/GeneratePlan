using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomConstruct : GH_Component
    {
        public GhcRoomConstruct()
          : base("Generate Room", "GenRm", "Generates a Room object from a list of Point3dId, function, and Id",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "Corner Points", "CP", "List of Point3dId defining the room's corner points", GH_ParamAccess.list);
            pManager.AddTextParameter("Function", "F", "Function of the room", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Id", "Id", "Id of the room", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Room", "R", "Generated Room object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3dId> cornerPoints = new List<Point3dId>();
            string function = string.Empty;
            int id = 0;

            if (!DA.GetDataList(0, cornerPoints)) { return; }
            if (!DA.GetData(1, ref function)) { return; }
            if (!DA.GetData(2, ref id)) { return; }

            Room room = new Room(cornerPoints, function, id);
            DA.SetData(0, new RoomGoo(room));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("010CCC78-71D4-43AC-973F-DFE51A4DEA1A");
    }
}