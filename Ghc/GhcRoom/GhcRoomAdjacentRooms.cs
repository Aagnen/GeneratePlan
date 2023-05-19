using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomAdjacentRooms : GH_Component
    {
        public GhcRoomAdjacentRooms()
          : base("Find Adjacent Rooms", "Adjacent R", "Finds adjacent rooms in a list to the main room",
              "DplMgr", "Rooms")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Room", "R", "Room object to deconstruct", GH_ParamAccess.item);
            pManager.AddParameter(new RoomParam(), "All Rooms", "AR", "Rooms to find naighbours from", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Adjacent Rooms", "AdjR", "List of adjacent rooms to the main room", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input retrieval
            Room mainRoom = null;
            List<Room> allRooms = new List<Room>();

            if (!DA.GetData<Room>(0, ref mainRoom)) { return; }
            if (!DA.GetDataList<Room>(1, allRooms)) { return; }

            // The HashSet is used to avoid duplicate rooms and to use the 'UpdateAdjacentRooms' method as is
            HashSet<Room> allRoomsHashSet = new HashSet<Room>(allRooms);

            // Find adjacent rooms
            mainRoom.UpdateAdjacentRooms(allRoomsHashSet);

            // Extract the adjacent rooms from the dictionary values to a list
            List<Room> adjacentRooms = new List<Room>(mainRoom.AdjacentRooms.Values);

            // Output
            DA.SetDataList(0, adjacentRooms);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("8B0C4B77-4830-4E8F-A64E-8DFB7E1ED1CF");
    }
}