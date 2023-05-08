using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using System.Text;

namespace GeneratePlan.Ghc.GhcMovement
{
    public class GhcFindPossibleMovement : GH_Component
    {
        public GhcFindPossibleMovement()
          : base("Find Possible Movement", "MV",
              "Find possible movement vector based on the chosen endpoints and list of all iRooms. ",
              "DplMgr", "Process movement")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RoomParam(), "Rooms", "R", "List of all iRooms in the program", GH_ParamAccess.list);
            pManager.AddParameter(new Point3dIdParam(), "LineEndpoints", "LE", "List of two Point3dIds that are the endpoints of the line that needs moving", GH_ParamAccess.list);
            pManager.AddNumberParameter("Maximal movement", "Max", "Maximal distance movement. Multiplication of step!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Step", "St", "Movement step", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Movement Vectors", "MV", "All possible movement vectors", GH_ParamAccess.list);
            pManager.AddTextParameter("Process", "Proc", "Description of what was changed", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region prepareVariables
            //------------------------------------------------------ initialize variables --------------------------------------------------//
            List<Room> iRooms = new List<Room>();
            List<Point3dId> iLineEndpoints = new List<Point3dId>();
            double maxMovement = double.MaxValue;
            double step = double.MaxValue;

            StringBuilder outMessage = new StringBuilder();
            double tolerance = 0.001;

            // List to store movement vectors
            List<Vector3d> possibleMovements = new List<Vector3d>();
            List<Vector3d> oValidMovements = new List<Vector3d>();

            //------------------------------------------------------ get variables --------------------------------------------------//
            if (!DA.GetDataList("Rooms", iRooms) || !DA.GetDataList("LineEndpoints", iLineEndpoints)) return;
            if (!DA.GetData("Maximal movement", ref maxMovement)) return;
            if (!DA.GetData("Step", ref step)) return;

            //------------------------------------------------------ common bugs --------------------------------------------------//
            if (iLineEndpoints.Count != 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "There should be exactly two endpoints in the list.");
                return;
            }

            if (step <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Step has to be positive");
                return;
            }

            //------------------------------------------------------ deep copy --------------------------------------------------//
            List<Room> rooms = new List<Room>();
            foreach (var room in iRooms) { rooms.Add(room.Duplicate()); }

            List<Point3dId> lineEndpoints = new List<Point3dId>();
            foreach (var endpoint in iLineEndpoints) { lineEndpoints.Add(endpoint.Duplicate()); }

            //------------------------------------------------------ rooms with both corners --------------------------------------------------//
            List<Room> roomsWithBothCorners = new List<Room>();
            foreach (Room room in rooms)
            {
                bool containsFirstCorner = room.CornerPoints.Exists(corner => corner.Id == lineEndpoints[0].Id);
                bool containsSecondCorner = room.CornerPoints.Exists(corner => corner.Id == lineEndpoints[1].Id);

                if (containsFirstCorner && containsSecondCorner)
                {
                    roomsWithBothCorners.Add(room);
                }
            }
            //Room chosenRoom = iChosenRoom.Duplicate();
            #endregion prepareVariables

            #region solve

            #region direction_possible_movements
            //------------------------------------------------------ decide LineToMoveDirection of line --------------------------------------------------//
            string LineToMoveDirection = "none";
            Point3d max_point = new Point3d(0, 0, 0);
            Point3d min_point = new Point3d(0, 0, 0);

            if (Math.Abs(lineEndpoints[0].Point.X - lineEndpoints[1].Point.X) < tolerance)
            {
                LineToMoveDirection = "vertical";
                if (lineEndpoints[0].Point.Y > lineEndpoints[1].Point.Y)
                {
                    max_point = lineEndpoints[0].Point;
                    min_point = lineEndpoints[1].Point;
                }
                else
                {
                    max_point = lineEndpoints[1].Point;
                    min_point = lineEndpoints[0].Point;
                }

                //all possible movements
                int numberOfSteps = (int)Math.Floor(maxMovement / step);
                for (int i = -numberOfSteps; i <= numberOfSteps; i++)
                {
                    double x = i * step;
                    possibleMovements.Add(new Vector3d(x, 0, 0));
                }
            }
            else if (Math.Abs(lineEndpoints[0].Point.Y - lineEndpoints[1].Point.Y) < 0.001)
            {
                LineToMoveDirection = "horizontal";
                if (lineEndpoints[0].Point.X > lineEndpoints[1].Point.X)
                {
                    max_point = lineEndpoints[0].Point;
                    min_point = lineEndpoints[1].Point;
                }
                else
                {
                    max_point = lineEndpoints[1].Point;
                    min_point = lineEndpoints[0].Point;
                }

                //all possible movements
                int numberOfSteps = (int)Math.Floor(maxMovement / step);
                for (int i = -numberOfSteps; i <= numberOfSteps; i++)
                {
                    double y = i * step;
                    possibleMovements.Add(new Vector3d(0, y, 0));
                }

            }
            outMessage.Append("The chosen line LineToMoveDirection: ");
            outMessage.Append(LineToMoveDirection);
            #endregion direction_possible_movements

            var (directCollisionDistances, indirectCollistionDistances, snapDistances) = FindCollisionAndSnapLocations(LineToMoveDirection, min_point, max_point, rooms, roomsWithBothCorners, tolerance);

            #region analyze_locations

            #region message
            outMessage.Append("\nThe number of direct collision distances: ");
            outMessage.Append(directCollisionDistances.Count);
            if (directCollisionDistances.Count > 0)
            {
                outMessage.Append("\n   Max ");
                outMessage.Append(directCollisionDistances.Max());
                outMessage.Append("\n   Min ");
                outMessage.Append(directCollisionDistances.Min());
            }

            outMessage.Append("\nThe number of indirect collision distances: ");
            outMessage.Append(indirectCollistionDistances.Count);
            if (indirectCollistionDistances.Count > 0)
            {
                outMessage.Append("\n   Max ");
                outMessage.Append(indirectCollistionDistances.Max());
                outMessage.Append("\n   Min ");
                outMessage.Append(indirectCollistionDistances.Min());
            }

            outMessage.Append("\nThe number of snap distances: ");
            outMessage.Append(snapDistances.Count);
            #endregion message

            HashSet<double> collisionDistances = new HashSet<double>();
            collisionDistances.UnionWith(directCollisionDistances);
            collisionDistances.UnionWith(indirectCollistionDistances);

            // Find the closest minus number to zero in CollisionDistances
            double closestMinus = collisionDistances.Where(x => x < 0).DefaultIfEmpty(-maxMovement).Max();
            closestMinus = closestMinus > -step ? 0 : closestMinus+step;
            // Find the closest plus number to zero in CollisionDistances
            double closestPlus = collisionDistances.Where(x => x > 0).DefaultIfEmpty(maxMovement).Min();
            closestPlus = closestPlus < step ? 0 : closestPlus - step;

            // Filter snapDistances that are inside the range
            HashSet<double> filteredSnapDistances = new HashSet<double>(snapDistances.Where(snapDistance => snapDistance > closestMinus && snapDistance < closestPlus));
            #endregion analyze_locations

            #region filter_locations_to_valid_movements
            // Filter oValidMovements based on the range
            foreach (var movement in possibleMovements)
            {
                if (LineToMoveDirection == "horizontal" && movement.Y > closestMinus && movement.Y < closestPlus)
                {
                    oValidMovements.Add(movement);
                }
                else if (LineToMoveDirection == "vertical" && movement.X > closestMinus && movement.X < closestPlus)
                {
                    oValidMovements.Add(movement);
                }
            }

            // Add filteredSnapDistances as movement vectors to oValidMovements
            int snap_probability = 3; // if 1, that it is as possible as every movement. Every larger number is multiplication of probability
            for (int i = 0; i < snap_probability; i++)
            {
                foreach (var snapDistance in filteredSnapDistances)
                {
                    if (LineToMoveDirection == "horizontal")
                    {
                        oValidMovements.Add(new Vector3d(0, snapDistance, 0));
                    }
                    else if (LineToMoveDirection == "vertical")
                    {
                        oValidMovements.Add(new Vector3d(snapDistance, 0, 0));
                    }
                }
            }

            if (oValidMovements.Count == 0) oValidMovements.Add(new Vector3d(0, 0, 0));
            #endregion filter_locations_to_valid_movements


            #endregion solve

            #region out
            DA.SetDataList("Movement Vectors", oValidMovements);
            DA.SetData("Process", outMessage.ToString());
            #endregion out
        }

        private (HashSet<double> directCollisionDistances, HashSet<double> indirectCollisionDistances, HashSet<double> snapDistances) FindCollisionAndSnapLocations(string direction, Point3d min_point, Point3d max_point, List<Room> rooms, List<Room> roomsWithBothCorners, double tolerance)
        {
            // HashSet to store collision locations referencing Y or X values of the Point3d (depending if line is vertical or horizontal)
            HashSet<double> directCollisionDistances = new HashSet<double>();
            HashSet<double> indirectCollisionDistances = new HashSet<double>();
            HashSet<double> snapDistances = new HashSet<double>();

            if (direction != "horizontal" && direction != "vertical") return (null, null, null);

            foreach (Room room in rooms)
            {
                foreach (Point3dId corner in room.CornerPoints)
                {
                    var adjecentCorners = room.GetAdjacentCornersById(corner);
                    if (adjecentCorners == null) return (null, null, null);

                    #region define_direction_sensitive_variables
                    //define locations in each important point in such a way, so the function works for both horizontal and vertical line
                    double cornerPrimaryCoord = direction == "horizontal" ? corner.Point.X : corner.Point.Y;
                    double cornerSecondaryCoord = direction == "horizontal" ? corner.Point.Y : corner.Point.X;

                    double point0PrimaryCoord = direction == "horizontal" ? adjecentCorners[0].Point.X : adjecentCorners[0].Point.Y;
                    double point0SecondaryCoord = direction == "horizontal" ? adjecentCorners[0].Point.Y : adjecentCorners[0].Point.X;

                    double point1PrimaryCoord = direction == "horizontal" ? adjecentCorners[1].Point.X : adjecentCorners[1].Point.Y;
                    double point1SecondaryCoord = direction == "horizontal" ? adjecentCorners[1].Point.Y : adjecentCorners[1].Point.X;

                    double minPrimaryCoord = direction == "horizontal" ? min_point.X : min_point.Y;
                    double maxPrimaryCoord = direction == "horizontal" ? max_point.X : max_point.Y;
                    double mainSecondaryCoord = direction == "horizontal" ? min_point.Y : min_point.X; // the same for min and max
                    #endregion define_direction_sensitive_variables

                    #region collisions
                    double distance = cornerSecondaryCoord - mainSecondaryCoord;
                    //direct collision
                    if (cornerPrimaryCoord < maxPrimaryCoord && cornerPrimaryCoord > minPrimaryCoord)// if the corner lies between max and min => in a possible collision
                    {
                        directCollisionDistances.Add(distance);
                    }
                    //indirect collision - the corner and its adjecent line wraps around the moving line
                    //it means that corner and one of its adjecent lines need to be above and below the moving line in the same time
                    else if (
                        (cornerPrimaryCoord <= minPrimaryCoord && 
                            (point0PrimaryCoord >= maxPrimaryCoord 
                            || 
                            point1PrimaryCoord >= maxPrimaryCoord)) 
                        ||
                        (cornerPrimaryCoord >= maxPrimaryCoord && 
                            (point0PrimaryCoord <= minPrimaryCoord 
                            || 
                            point1PrimaryCoord <= minPrimaryCoord))
                    )
                    {
                        //it also catches itself, so the check to delete the zero distance
                        if(distance != 0) indirectCollisionDistances.Add(distance);
                    }
                    //snap possition
                    else if ((Math.Abs(cornerPrimaryCoord - maxPrimaryCoord) < tolerance || Math.Abs(cornerPrimaryCoord - minPrimaryCoord) < tolerance))
                    {
                        if (distance != 0)
                        {
                            if (roomsWithBothCorners.Exists(r => r.Id == room.Id)) indirectCollisionDistances.Add(distance);
                            else snapDistances.Add(distance);
                        }
                    }

                    //in other cases the point lies away from the moving line thus is not added to any list
                    #endregion collisions
                }
            }
            return (directCollisionDistances, indirectCollisionDistances, snapDistances);
        }



        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Solvers1;

        public override Guid ComponentGuid => new Guid("B5CA4419-C19B-4D75-8D51-8EEAB7AC4791");
    }
}