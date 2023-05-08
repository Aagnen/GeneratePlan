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
    public class GhcRoomFind : GH_Component
    {
        public GhcRoomFind()
          : base("Find room", "RoomFind", "Extracts a list of Rooms from the list of Rooms by Room Id or Function",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "List of Rooms", "R", "List of Rooms", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Room Id", "Id", "The Room Id to extract", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("Room Function", "F", "The Room Function to extract", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Chosen Rooms", "R", "The extracted Rooms", GH_ParamAccess.list);
            pManager.AddParameter(new RoomParam(), "Rest of Rooms", "!R", "Not extracted Rooms", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Room> iRooms = new List<Room>();
            List<Room> rooms = new List<Room>();
            int roomId = -1;
            string roomFunction = "";

            if (!DA.GetDataList(0, iRooms)) return;
            DA.GetData(1, ref roomId);
            DA.GetData(2, ref roomFunction);

            for(int i=0; i<iRooms.Count; i++) //deep copy just in case
            {
                rooms.Add(iRooms[i].Duplicate());
            }

            List<Room> extractedRooms = new List<Room>();
            List<Room> restOfRooms = new List<Room>();

            if (roomId != -1)
            {
                extractedRooms = rooms.Where(room => room.Id == roomId).ToList();
                restOfRooms = rooms.Where(room => room.Id != roomId).ToList();
            }
            else if (!string.IsNullOrEmpty(roomFunction))
            {
                extractedRooms = rooms.Where(room => room.Function == roomFunction).ToList();
                restOfRooms = rooms.Where(room => room.Function != roomFunction).ToList();
            }

            DA.SetDataList(0, extractedRooms);
            DA.SetDataList(1, restOfRooms);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("6D5217FE-6BFC-4D9F-B766-14ECBAD47CC9");
    }
}