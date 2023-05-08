using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using System.Linq;
using System.Text;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino.Collections;

namespace GeneratePlan.Ghc.GhcMovement
{
    public class GhcProcessNewPointsInRooms : GH_Component
    {
        public GhcProcessNewPointsInRooms()
          : base("Process new points in rooms", "PRP",
              "Include moved points in both the main point list and Room list. Check if point duplicated points need to be included or not.",
              "DplMgr", "Process movement")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "All Rooms", "R", "List of all Rooms", GH_ParamAccess.list);
            pManager.AddParameter(new Point3dIdParam(), "Main point list", "R", "List of all Points", GH_ParamAccess.list);

            pManager.AddParameter(new Point3dIdParam(), "Original point A", "Aorg", "Chosen point3dId A before movement WITH NEW INDEX. This point may be included.", GH_ParamAccess.item);
            pManager.AddParameter(new Point3dIdParam(), "Moved point A", "Amov", "Moved point3dId A as a copy with new index. This point will always be included i affected rooms", GH_ParamAccess.item);
            pManager.AddParameter(new Point3dIdParam(), "Original point B", "Borg", "Chosen point3dId B before movement WITH NEW INDEX. This point may be included.", GH_ParamAccess.item);
            pManager.AddParameter(new Point3dIdParam(), "Moved point B", "Bmov", "Moved point3dId B as a copy with new index. This point will always be included i affected rooms", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Point3dIdParam(), "List of Point3dId", "P", "List of Point3dId to exchange/include in main list", GH_ParamAccess.list);
            pManager.AddParameter(new RoomParam(), "All Rooms", "R", "List of all modified Rooms", GH_ParamAccess.list);
            pManager.AddTextParameter("Process", "Proc", "Description of what was changed", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region prepare_parameters
            // ------------------------------- regiester parameters ----------------------------------//
            List<Room> iRooms = new List<Room>();
            List<Point3dId> iMainPointList = new List<Point3dId>();

            StringBuilder outMessage = new StringBuilder();

            Point3dId iAorg = new Point3dId();
            Point3dId iAmov = new Point3dId();
            Point3dId iBorg = new Point3dId();
            Point3dId iBmov = new Point3dId();

            double angle_tolerance = 0.001;

            // ------------------------------- get parameters ----------------------------------//

            if (!DA.GetDataList("All Rooms", iRooms)) return;
            if (!DA.GetDataList("Main point list", iMainPointList)) return;

            if (!DA.GetData("Original point A", ref iAorg)) return;
            if (!DA.GetData("Moved point A", ref iAmov)) return;
            if (!DA.GetData("Original point B", ref iBorg)) return;
            if (!DA.GetData("Moved point B", ref iBmov)) return;

            // deep copy of points
            HashSet<Point3dId> oMainPointHashSet = new HashSet<Point3dId>();
            for (int i = 0; i < iMainPointList.Count; i++) { oMainPointHashSet.Add(iMainPointList[i].Duplicate()); } //deep copy just in case 

            #region snap
            // ------------------------------- snap ----------------------------------//
            double minDistAmov = double.MaxValue;
            double minDistBmov = double.MaxValue;

            foreach (var point in oMainPointHashSet)
            {
                double distAmov = iAmov.Point.DistanceTo(point.Point);
                double distBmov = iBmov.Point.DistanceTo(point.Point);

                if (distAmov < angle_tolerance && distAmov < minDistAmov)
                {
                    iAmov = point;
                    minDistAmov = distAmov;
                }

                if (distBmov < angle_tolerance && distBmov < minDistBmov)
                {
                    iBmov = point;
                    minDistBmov = distBmov;
                }
            }

            if (minDistAmov < angle_tolerance)
            {
                outMessage.Append("\nPoint iAmov replaced by point");
                outMessage.Append(iAmov.Id);
            }

            if (minDistBmov < angle_tolerance)
            {
                outMessage.Append("\nPoint iBmov replaced by point");
                outMessage.Append(iBmov.Id);
            }
            #endregion snap

            // ------------------------------- look for common errors in inputs ----------------------------------//
            //TODO2: Check for errors in inputs
            if (iAorg.Id == iAmov.Id && iBorg.Id == iBmov.Id)
            {
                outMessage.Append("\nFOUND NO MOVEMENT");
                DA.SetData("Process", outMessage.ToString());
                return;
            }

            #endregion prepare_parameters

            #region solve

            #region filer_data
            // ------------------------------- look for rooms that will be affected ----------------------------------//
            //prepare variables
            List<Room> roomsAffectedByBoth = new List<Room>();
            List<Room> roomsAffectedByA = new List<Room>();
            List<Room> roomsAffectedByB = new List<Room>();
            List<Room> roomsNotAffected = new List<Room>();

            //check all rooms.
            foreach (var room in iRooms)
            {
                outMessage.Append("\nRoom ");
                outMessage.Append(room.Id.ToString());
                outMessage.Append(" is affected by ");
                int indexA = room.IndexOfCornerByID(iAorg.Id);
                int indexB = room.IndexOfCornerByID(iBorg.Id);

                //add to adequate list as a deep copy
                if (indexA >= 0 && indexB >= 0) { roomsAffectedByBoth.Add(room.Duplicate()); outMessage.Append("AB."); }
                else if (indexA >= 0) { roomsAffectedByA.Add(room.Duplicate()); outMessage.Append("A."); }
                else if (indexB >= 0) { roomsAffectedByB.Add(room.Duplicate()); outMessage.Append("B."); }
                else { roomsNotAffected.Add(room.Duplicate()); outMessage.Append("none."); }
            }
            #endregion filter_data

            #region add_corners

            foreach (var room in roomsAffectedByBoth)
            {
                outMessage.Append("\n\nAnalyse room: ");
                outMessage.Append(room.Id);
                outMessage.Append("\n   Analyze before movement.");
                //Aorg relations
                List<Point3dId> adjecentCornersAOrg = room.GetAdjacentCornersById(iAorg);
                string linesRelationshipA = GeneralMethods.ThreePointsRelationship(adjecentCornersAOrg[0], iAorg, adjecentCornersAOrg[1], angle_tolerance);
                outMessage.Append("\n   Lines to Aorg are: ");
                outMessage.Append(linesRelationshipA);
                //Borg relations
                List<Point3dId> adjecentCornersBorg = room.GetAdjacentCornersById(iBorg);
                string linesRelationshipB = GeneralMethods.ThreePointsRelationship(adjecentCornersBorg[0], iBorg, adjecentCornersBorg[1], angle_tolerance);
                outMessage.Append("\n   Lines to Borg are: ");
                outMessage.Append(linesRelationshipB);

                outMessage.Append("\n   Add moved corners.");

                //Add new moved points
                int indexA = room.IndexOfCornerByID(iAorg.Id);
                int indexB = room.IndexOfCornerByID(iBorg.Id);
                room.AddCornerBetweenIndices(iAmov, indexA, indexB);

                int indexNewA = room.IndexOfCornerByID(iAmov.Id);
                int indexNewB = room.IndexOfCornerByID(iBorg.Id);
                room.AddCornerBetweenIndices(iBmov, indexNewA, indexNewB);

                Point3d centroid = room.CalculateCentroid();

                //Aorg
                if (centroid.DistanceTo(iAmov.Point) < centroid.DistanceTo(iAorg.Point) && linesRelationshipA != "parallel")
                {
                    room.RemoveCorner(iAorg.Id);

                    outMessage.Append("\n   Deleted org A point from this room: ");
                    outMessage.Append(room.Id);
                }

                //Borg
                if (centroid.DistanceTo(iBmov.Point) < centroid.DistanceTo(iBorg.Point) && linesRelationshipB != "parallel")
                {
                    room.RemoveCorner(iBorg.Id);

                    outMessage.Append("\n   Deleted org B point from this room: ");
                    outMessage.Append(room.Id);
                }

                // check for overlapping lines (= other points outside of the room)
                List<Point3dId> cornersToCheck = new List<Point3dId>();
                foreach (Point3dId corner in room.CornerPoints)
                {
                    cornersToCheck.Add(corner);
                }
                foreach (var cornerToCheck in cornersToCheck)
                {
                    string linesRelationshipmov = "none";
                    List<Point3dId> adjecentCornersmov = room.GetAdjacentCornersById(cornerToCheck);
                    if (adjecentCornersmov.Count > 0)
                    {
                        linesRelationshipmov = GeneralMethods.ThreePointsRelationship(adjecentCornersmov[0], cornerToCheck, adjecentCornersmov[1], angle_tolerance);

                        if (linesRelationshipmov == "overlapping")
                        {
                            room.RemoveCorner(cornerToCheck);
                            outMessage.Append("\n   Removed corner because it was out of the room: ");
                            outMessage.Append(cornerToCheck.Id);
                        }
                    }
                }
            }
            foreach (var room in roomsAffectedByA)
            {
                HandleRoomCorners(room, iAorg, iAmov, angle_tolerance);
            }

            foreach (var room in roomsAffectedByB)
            {
                HandleRoomCorners(room, iBorg, iBmov, angle_tolerance);
            }
            #endregion add_corners

            List<Room> roomsAffected = new List<Room>();
            roomsAffected = roomsAffectedByBoth.Concat(roomsAffectedByA).Concat(roomsAffectedByB).ToList();

            outMessage.Append("\n\nThere are ");
            outMessage.Append(roomsAffected.Count);
            outMessage.Append(" affected rooms.");

            // ------------------------------- merging all room lists ----------------------------------//
            List<Room> oRooms = new List<Room>();
            oRooms = roomsAffected.Concat(roomsNotAffected).ToList();

            oMainPointHashSet.Add(iAmov);
            oMainPointHashSet.Add(iBmov);

            #region check_for_new_corners
            //if the wall moved "over" snapping points, because there was no collision, there is a chance, that they should be now included in the room or excluded from it
            foreach (var roomToCheck in oRooms)
            {
                List<Point3dId>cornersToCheck = new List<Point3dId>();
                foreach (var corner in roomToCheck.CornerPoints) cornersToCheck.Add(corner);

                foreach (var cornerToCheck in cornersToCheck)
                {
                    //check if any point is laying on the lines of this room
                    //if yes, include it
                    foreach (var point in oMainPointHashSet)
                    {
                        //find its neighbours
                        List<Point3dId> adjecentCorners = roomToCheck.GetAdjacentCornersById(cornerToCheck);
                        Line line0 = new Line(adjecentCorners[0].Point, cornerToCheck.Point);
                        Line line1 = new Line(adjecentCorners[1].Point, cornerToCheck.Point);

                        if (roomToCheck.IndexOfCornerByID(point.Id) == -1)
                        {
                            if (line0.MinimumDistanceTo(point.Point) < angle_tolerance)
                            {
                                roomToCheck.AddCornerBetweenIndices(point,
                                    roomToCheck.IndexOfCornerByID(adjecentCorners[0].Id),
                                    roomToCheck.IndexOfCornerByID(cornerToCheck.Id));

                                #region message
                                outMessage.Append("\n\nIn room ");
                                outMessage.Append(roomToCheck.Id);
                                outMessage.Append(" added point ");
                                outMessage.Append(point.Id);
                                outMessage.Append(" between points ");
                                outMessage.Append(adjecentCorners[0].Id);
                                outMessage.Append(" , ");
                                outMessage.Append(cornerToCheck.Id);
                                outMessage.Append(" because it laid on the line. ");
                                #endregion message
                            }
                            if (line1.MinimumDistanceTo(point.Point) < angle_tolerance)
                            {
                                roomToCheck.AddCornerBetweenIndices(point,
                                    roomToCheck.IndexOfCornerByID(adjecentCorners[1].Id),
                                    roomToCheck.IndexOfCornerByID(cornerToCheck.Id));

                                #region message
                                outMessage.Append("\n\nIn room ");
                                outMessage.Append(roomToCheck.Id);
                                outMessage.Append(" added point ");
                                outMessage.Append(point.Id);
                                outMessage.Append(" between points ");
                                outMessage.Append(adjecentCorners[1].Id);
                                outMessage.Append(" , ");
                                outMessage.Append(cornerToCheck.Id);
                                outMessage.Append(" because it laid on the line. ");
                                #endregion message
                            }
                        }
                    }
                }
            }

            #endregion check_for_new_corners

            #region check_for_left_lonely_points
            outMessage.Append("\n\nCheck for lonely points");
            // by lonely i mean that it is not connected to any room or it is in a parallel lines in ALL of them (is not a corner)
            bool ifAorgLonely = true;
            bool ifBorgLonely = true;

            //check all rooms.
            foreach (var room in oRooms)
            {
                outMessage.Append("\n   Room ");
                outMessage.Append(room.Id);

                // A and B are the same but with a different vairables. Lookout for misspellings A-B !!!
                # region A
                List<Point3dId> adjecentCornersAorg = room.GetAdjacentCornersById(iAorg);
                outMessage.Append("\n      connects Aorg to points: ");
                if (adjecentCornersAorg.Count > 0)
                {
                    //define for easier copying between A and B
                    Point3dId point = iAorg;
                    List<Point3dId> adjecentCorners = adjecentCornersAorg;

                    outMessage.Append(adjecentCorners[0].Id);
                    outMessage.Append(adjecentCorners[1].Id);

                    string relation = GeneralMethods.ThreePointsRelationship(adjecentCorners[0], point, adjecentCorners[1], angle_tolerance);

                    outMessage.Append("\n      their relation: ");
                    outMessage.Append(relation);

                    if (relation == "perpendicular")
                    {
                        ifAorgLonely = false;
                        outMessage.Append("\n      so from now on point Aorg is safe.");
                    }
                }
                else outMessage.Append("none");
                # endregion A

                # region B
                List<Point3dId> adjecentCornersBorg = room.GetAdjacentCornersById(iBorg);
                outMessage.Append("\n      connects Borg to points: ");
                if (adjecentCornersBorg.Count > 0)
                {
                    //define for easier copying between A and B
                    Point3dId point = iBorg;
                    List<Point3dId> adjecentCorners = adjecentCornersBorg;

                    outMessage.Append(adjecentCorners[0].Id);
                    outMessage.Append(adjecentCorners[1].Id);

                    string relation = GeneralMethods.ThreePointsRelationship(adjecentCorners[0], point, adjecentCorners[1], angle_tolerance);

                    outMessage.Append("\n      their relation: ");
                    outMessage.Append(relation);

                    if (relation == "perpendicular")
                    {
                        ifBorgLonely = false;
                        outMessage.Append("\n      so from now on point Borg is safe.");
                    }
                }
                else outMessage.Append("none");
                # endregion B
            }

            //// ------------------------------- delete points from rooms and add to out list ----------------------------------//

            if (ifAorgLonely) { foreach (Room room in oRooms) { room.RemoveCorner(iAorg); oMainPointHashSet.Remove(iAorg); } outMessage.Append("\n\nAorg is lonely so deleted all."); }
            else { oMainPointHashSet.Add(iAorg); }

            if (ifBorgLonely) { foreach (Room room in oRooms) { room.RemoveCorner(iBorg); oMainPointHashSet.Remove(iBorg); } outMessage.Append("\n\nBorg is lonely so deleted all."); }
            else { oMainPointHashSet.Add(iBorg); }
            #endregion check_for_left_lonely_points

            #endregion solve

            #region out
            // ------------------------------- out data ----------------------------------//
            DA.SetDataList("List of Point3dId", oMainPointHashSet.Select(point => new Point3dIdGoo(point)));
            DA.SetDataList("All Rooms", oRooms.Select(room => new RoomGoo(room)));
            DA.SetData("Process", outMessage.ToString());
            #endregion out
        }

        private void HandleRoomCorners(Room room, Point3dId org, Point3dId mov, double angle_tolerance)
        {
            int indexOrg = room.IndexOfCornerByID(org.Id);
            int indexMov = room.IndexOfCornerByID(mov.Id);
            int indexToMoveBetween = room.IndexOfCornerByID(GetAdjacentCornerThatIsGettingCloser(org, mov, room.GetAdjacentCornersById(org)));

            //if they are the same, it means that there is a snap and the corner is already there= nothing more to be done
            if (indexMov != indexToMoveBetween)
            {
                room.AddCornerBetweenIndices(mov, indexOrg, indexToMoveBetween);

                // check for angled lines
                string linesRelationshipmov = "none";
                List<Point3dId> adjecentCornersmov = room.GetAdjacentCornersById(mov);
                if (adjecentCornersmov.Count > 0)
                {
                    linesRelationshipmov = GeneralMethods.ThreePointsRelationship(adjecentCornersmov[0], mov, adjecentCornersmov[1], angle_tolerance);

                    if (linesRelationshipmov == "angled" || linesRelationshipmov == "overlapping")
                    {
                        room.RemoveCorner(mov);
                    }
                }
            }
        }

        private int GetAdjacentCornerThatIsGettingCloser(Point3dId org, Point3dId mov, List<Point3dId> adjecentCornersorg)
        {
            //input bugs
            if (adjecentCornersorg == null || adjecentCornersorg.Count == 0) throw new ArgumentException("Adjacent corners list is null or empty.");
            if (adjecentCornersorg.Count != 2) throw new ArgumentException("Adjacent corners list is not 2.");

            //if one of the adjecent corners is a moved corner (=there is a snap with an existing point), choose it?
            if (adjecentCornersorg[0].Id == org.Id || adjecentCornersorg[1].Id == org.Id) throw new ArgumentException("Found adjecent corner equal to itself");
            if (adjecentCornersorg[0].Id == mov.Id)
            { 
                return adjecentCornersorg[0].Id; 
            }
            if (adjecentCornersorg[1].Id == mov.Id)
            { 
                return adjecentCornersorg[1].Id; 
            }

            double[] distanceDifferences = new double[adjecentCornersorg.Count];

            for (int i = 0; i < adjecentCornersorg.Count; i++)
            {
                distanceDifferences[i] = org.Point.DistanceTo(adjecentCornersorg[i].Point) - mov.Point.DistanceTo(adjecentCornersorg[i].Point);
            }

            int closerPointId = -1;

            //decide what is going on
            if (Math.Abs(distanceDifferences[0]) == Math.Abs(distanceDifferences[1]))
            {
                if (distanceDifferences[0] > 0 && distanceDifferences[1] > 0)
                {
                    throw new ArgumentOutOfRangeException("Can't recognise to which corners A is getting closer.[equal plus]");
                    //NOTES: found this problem when point org and mov are moving in the same line, and move is already existing. Added this case
                }
                else if (distanceDifferences[0] < 0 && distanceDifferences[1] < 0)
                {
                    throw new ArgumentOutOfRangeException("Can't recognise to which corners A is getting closer.[equal minus]");
                }
                else if (distanceDifferences[0] > 0)
                {
                    //getting closer to point [0]
                    closerPointId = adjecentCornersorg[0].Id;
                }
                else if (distanceDifferences[1] > 0)
                {
                    //getting closer to point [1]
                    closerPointId = adjecentCornersorg[1].Id;
                }
                else { throw new ArgumentOutOfRangeException("Can't recognise to which corners A is getting closer.[equal unknown]"); }
            }
            else if (Math.Abs(distanceDifferences[0]) < Math.Abs(distanceDifferences[1]))
            {
                //getting closer to point [1]
                closerPointId = adjecentCornersorg[1].Id;
            }
            else if (Math.Abs(distanceDifferences[0]) > Math.Abs(distanceDifferences[1]))
            {
                //getting closer to point [0]
                closerPointId = adjecentCornersorg[0].Id;
            }
            else { throw new ArgumentOutOfRangeException("Can't recognise to which corners A is getting closer.[unknown]"); }

            return closerPointId;
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Solvers1;

        public override Guid ComponentGuid => new Guid("8EBDD790-7E0B-498D-A891-6EA1EFD601DB");
    }
}