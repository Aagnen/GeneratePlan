using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.Ghc.GhcRoom
{
    public class GhcRoomFindByPoint3dID : GH_Component
    {
        public GhcRoomFindByPoint3dID()
          : base("Find Rooms by Point Id", "FindRooms", "Finds all Rooms that include Point3dId with a specific Id",
              "DplMgr", "Rooms")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Rooms", "R", "List of Room objects", GH_ParamAccess.list);
            pManager.AddNumberParameter("Id", "Id", "Id to search for", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Rooms", "R", "List of Rooms that contain Point3dId with the specified Id", GH_ParamAccess.list);
            //pManager.AddLineParameter("Lines Before", "LB", "Lines consisting of found Point3dId and one point before", GH_ParamAccess.list);
            //pManager.AddLineParameter("Lines After", "LA", "Lines consisting of found Point3dId and one point after", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Room> iRooms = new List<Room>();
            List<Room> rooms = new List<Room>();
            int idToSearch = 0;

            if (!DA.GetDataList(0, iRooms)) return;
            if (!DA.GetData(1, ref idToSearch)) return;

            for (int i = 0; i < iRooms.Count; i++) //deep copy just in case
            {
                rooms.Add(iRooms[i].Duplicate());
            }

            var matchingRooms = new List<Room>();
            //var linesBefore = new List<Line>();
            //var linesAfter = new List<Line>();

            foreach (var room in rooms)
            {
                int index = room.IndexOfCornerByID(idToSearch);
                if (index >= 0)
                {
                    matchingRooms.Add(room);

                    //int indexBefore = (index - 1 + room.CornerPoints.Count) % room.CornerPoints.Count;
                    //int indexAfter = (index + 1) % room.CornerPoints.Count;

                    //linesBefore.Add(new Line(room.CornerPoints[indexBefore].Point, room.CornerPoints[index].Point));
                    //linesAfter.Add(new Line(room.CornerPoints[index].Point, room.CornerPoints[indexAfter].Point));
                }
            }

            DA.SetDataList(0, matchingRooms);
            //DA.SetDataList(1, linesBefore);
            //DA.SetDataList(2, linesAfter);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Room_methods;

        public override Guid ComponentGuid => new Guid("1C82D622-4807-4F5C-A425-726C1FC2BEFF");
    }
}