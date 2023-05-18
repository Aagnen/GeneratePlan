using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using GH_IO.Serialization;

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
            pManager.AddIntegerParameter("Room Id", "Id", "The Room Id to extract", GH_ParamAccess.list, -1);
            pManager.AddTextParameter("Room Function", "F", "The Room Function to extract", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Chosen Rooms", "R", "The extracted Rooms", GH_ParamAccess.list);
            pManager.AddParameter(new RoomParam(), "Rest of Rooms", "!R", "Not extracted Rooms", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Room> iRooms = new List<Room>();
            HashSet<Room> rooms = new HashSet<Room>();
            List<int> roomId = new List<int>();
            List<string> roomFunction = new List<string>();

            if (!DA.GetDataList(0, iRooms)) return;
            DA.GetDataList(1, roomId);
            DA.GetDataList(2, roomFunction);

            for (int i = 0; i < iRooms.Count; i++) //deep copy just in case
            {
                rooms.Add(iRooms[i].Duplicate());
            }

            HashSet<Room> extractedRooms = new HashSet<Room>();
            HashSet<Room> restOfRooms = new HashSet<Room>(rooms); // Initialize restOfRooms with all rooms

            if (roomId.Count > 0)
            {
                foreach (var id in roomId)
                {
                    var roomsWithId = rooms.Where(room => room.Id == id);
                    foreach (var room in roomsWithId)
                    {
                        extractedRooms.Add(room);
                        restOfRooms.Remove(room);
                    }
                }
            }

            if (roomFunction.Count > 0)
            {
                foreach (var function in roomFunction)
                {
                    var roomsWithFunction = rooms.Where(room => room.Function == function);
                    foreach (var room in roomsWithFunction)
                    {
                        extractedRooms.Add(room);
                        restOfRooms.Remove(room);
                    }
                }
            }

            DA.SetDataList(0, extractedRooms);
            DA.SetDataList(1, restOfRooms);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("6D5217FE-6BFC-4D9F-B766-14ECBAD47CC9");
    }
}