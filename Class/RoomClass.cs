using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using System.Linq;

namespace GeneratePlan.Class
{
    public class Room
    {
        //---------------------------------PROPERTIES--------------------------------//
        public int Id { get; private set; }
        public List<Point3dId> CornerPoints { get; set; }
        public string Function { get; set; }
        public double Area { get; private set; }
        public double AspectRatio { get; private set; }
        public Dictionary<int, Room> AdjacentRooms { get; private set; }


        //---------------------------------CONSTRUCTORS--------------------------------//
        //NOTES: Konstruktory powinny być zawsze: duplikujący to samo, pusty, zwykły
        public Room(List<Point3dId> cornerPoints, string function = "default function", int id = 999)
        {
            Id = id;
            CornerPoints = cornerPoints;
            Function = function;

            UpdateArea();
            UpdateAspectRatio();
            AdjacentRooms = new Dictionary<int, Room>(); //TODO2: dodać przeliczanie pokoi
        }
        public Room(Room other)
        {
            other.Duplicate();
        }
        //---------------------------------BASE METHODS--------------------------------//

        public Room Duplicate()
        {
            Room newRoom = new Room(new List<Point3dId>(this.CornerPoints), this.Function, this.Id); //NOTES: czy to na pewno deep copy? -> chyba tak. działa
            return newRoom;
        }

        //---------------------------------GEOMETRIC METHODS--------------------------------//

        public Polyline ToPolyline()
        {
            Polyline polyline = new Polyline();
            foreach (Point3dId point in CornerPoints)
            {
                polyline.Add(point.Point);
            }
            polyline.Add(CornerPoints[0].Point); // Close the polyline by adding the first point again.
            return polyline;
        }

        public static double ComputeArea(List<Point3dId> cornerPoints)
        {
            if (cornerPoints == null || cornerPoints.Count < 3)
            {
                return 0.0;
            }

            List<Point3d> points = new List<Point3d>();
            foreach (Point3dId point3dID in cornerPoints)
            {
                points.Add(point3dID.Point);
            }

            Polyline polyline = new Polyline(points);
            if (!polyline.IsClosed)
                polyline.Add(points[0]); // Close the polyline
            polyline.SetAllZ(0.0);
            Curve polyline_curve = polyline.ToPolylineCurve();

            return AreaMassProperties.Compute(polyline_curve).Area;
        }

        private double ComputeAspectRatio(List<Point3dId> cornerPoints)
        {
            // implementation to compute the aspect ratio based on the corner points
            return 0.0;
        }

        public Point3d CalculateCentroid()
        {
            Point3d centroid = Point3d.Origin;
            int pointCount = CornerPoints.Count;

            for (int i = 0; i < pointCount; i++)
            {
                centroid += CornerPoints[i].Point;
            }

            if (pointCount > 0)
            {
                centroid /= pointCount;
            }

            return centroid;
        }

        public List<Point3dId> GetAdjacentCornersById(Point3dId corner)
        {
            List<Point3dId> adjacentCorners = new List<Point3dId>();

            // Find the index of the corner in the CornerPoints list
            int cornerIndex = this.IndexOfCornerByID(corner.Id);

            // If the corner is not found, return an empty list
            if (cornerIndex == -1)
            {
                return adjacentCorners;
            }

            // Get the index of the previous corner
            int previousIndex = cornerIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = CornerPoints.Count - 1;
            }

            // Get the index of the next corner
            int nextIndex = (cornerIndex + 1) % CornerPoints.Count;

            // Add the previous and next corners to the output list
            adjacentCorners.Add(CornerPoints[previousIndex]);
            adjacentCorners.Add(CornerPoints[nextIndex]);

            return adjacentCorners;
        }

        public Polyline GetAdjecentPolylineById(Point3dId corner)
        {
            List<Point3dId> adjecentCornerns = GetAdjacentCornersById(corner);
            Polyline polyline = new Polyline();
            polyline.Add(adjecentCornerns[0].Point);
            polyline.Add(corner.Point);
            polyline.Add(adjecentCornerns[1].Point);

            return polyline;
        }

        public int NumberOfPerpendicular(List<Room> affectedRooms, Point3d pointlocation, int pointID)
        {
            Point3dId point = new Point3dId(pointID, pointlocation);
            List<Point3dId> adjacentCornersInMainRoom = this.GetAdjacentCornersById(point);
            if (adjacentCornersInMainRoom.Count == 0) { return 0; }
            List<Point3d> adjacentLinesPointsInMainRoom = new List<Point3d> { adjacentCornersInMainRoom[0].Point, point.Point, adjacentCornersInMainRoom[1].Point };
            Polyline adjacentLinesInMainRoom = new Polyline(adjacentLinesPointsInMainRoom);
            int Perpendicular = 0;

            foreach (var affectedRoom in affectedRooms)
            {
                Line line1 = new Line(adjacentLinesInMainRoom.PointAt(0), adjacentLinesInMainRoom.PointAt(1));
                Line line2 = new Line(adjacentLinesInMainRoom.PointAt(1), adjacentLinesInMainRoom.PointAt(2));
                bool areParallel = GeneralMethods.AreLinesParallel(line1, line2);

                if (!areParallel)
                {
                    Perpendicular += 1;
                }
            }
            return Perpendicular;
        }


        //---------------------------------update methods--------------------------------//
        //----------------------------update corners---------------------------//

        /// <summary>
        /// updates corners. If there already is a corner with this Id -> it will be exchanged. 
        /// If there is none with this Id -> it will be added in the adequate place.
        /// If there is no new corner with the Id of an old one, the old one stays unchanged. 
        /// Upadates area and aspect ratio.
        /// Returns true if success, false if there is something wrong - for example duplicates of Point3dId.Id
        /// </summary>
        public bool UpdateAddCornersByID(List<Point3dId> newCorners)
        {
            bool ifSuccess = true;
            // Create a dictionary for faster lookups
            Dictionary<int, Point3dId> newCornersDict = new Dictionary<int, Point3dId>();

            // Add new corners to the dictionary, avoiding duplicates
            foreach (Point3dId newCorner in newCorners)
            {
                if (!newCornersDict.ContainsKey(newCorner.Id))
                {
                    newCornersDict.Add(newCorner.Id, newCorner);
                }
                else { ifSuccess = false; }
            }
            if (newCornersDict.Count == 0) { ifSuccess = false; }

            // Replace the existing corner points
            for (int i = 0; i < CornerPoints.Count; i++)
            {
                if (newCornersDict.TryGetValue(CornerPoints[i].Id, out Point3dId replacement))
                {
                    CornerPoints[i] = replacement;
                    newCornersDict.Remove(CornerPoints[i].Id);
                }
            }

            // Add the remaining new corners
            if (newCornersDict.Count > 0)
            {
                CornerPoints.AddRange(newCornersDict.Values);
                //this.SortCorners();
            }
            this.UpdateArea();
            this.UpdateAspectRatio();

            return ifSuccess;
        }


        /// <summary>
            /// updates corners. If there already is a corner with this Id -> it will be exchanged. 
            /// If there is none with this Id -> IGNORED.
            /// If there is no new corner with the Id of an old one, the old one stays unchanged. 
            /// Upadates area and aspect ratio.
            /// Returns true if success, false if there is something wrong - for example duplicates of Point3dId.Id
        /// </summary>
        public bool UpdateCornersByID(List<Point3dId> newCorners)
        {
            bool ifSuccess = true;
            // Create a dictionary for faster lookups
            Dictionary<int, Point3dId> newCornersDict = new Dictionary<int, Point3dId>();

            // Add new corners to the dictionary, avoiding duplicates
            foreach (Point3dId newCorner in newCorners)
            {
                if (!newCornersDict.ContainsKey(newCorner.Id))
                {
                    newCornersDict.Add(newCorner.Id, newCorner);
                }
                else { ifSuccess = false; }
            }
            if (newCornersDict.Count == 0) { ifSuccess = false; }

            // Replace the existing corner points
            for (int i = 0; i < CornerPoints.Count; i++)
            {
                if (newCornersDict.TryGetValue(CornerPoints[i].Id, out Point3dId replacement))
                {
                    CornerPoints[i] = replacement;
                    newCornersDict.Remove(CornerPoints[i].Id);
                }
            }
            this.UpdateArea();
            this.UpdateAspectRatio();

            return ifSuccess;
        }

        /// <summary>
        /// Replaces a corner point in the iRoom with a new point
        /// </summary>
        public bool ReplaceCorners(Point3dId cornersToReplace, Point3dId newCorners)
        {
            bool ifSuccess = false;

            for (int i = 0; i < this.CornerPoints.Count; i++)
            {
                if (this.CornerPoints[i].Id == cornersToReplace.Id)
                {
                    this.CornerPoints[i] = newCorners;
                    ifSuccess = true;
                    break;
                }
            }
            return ifSuccess;
        }


        //----------------------------modify corners---------------------------//

        public void AddCornerBetweenIndices(Point3dId point, int index1, int index2)
        {
            if (index1 < 0 || index1 >= CornerPoints.Count || index2 < 0 || index2 >= CornerPoints.Count)
            {
                throw new ArgumentOutOfRangeException("Indices must be within the range of the corner points list.");
            }

            int insertIndex = -1;

            // Check if the point is already one of the corners
            int existingIndex = CornerPoints.FindIndex(p => p.Id == point.Id);
            if (existingIndex != -1)
            {
                if (point.Id == CornerPoints[index1].Id || point.Id == CornerPoints[index2].Id)
                {
                    //it means that there is no need to add it as it is in the right place
                    return;
                }

                // If the point is already a corner, change its index to be between index1 and index2 => delete it to later add it
                Point3dId existingPoint = CornerPoints[existingIndex];
                CornerPoints.RemoveAt(existingIndex);
                if (index1 > existingIndex) index1 -= 1;
                else if (index1 == CornerPoints.Count) index1 -= 1;

                if (index2 > existingIndex) index2 -= 1;
                else if (index2 == CornerPoints.Count) index2 -= 1;
            }

            double absDifference = Math.Abs(index1 - index2);
            double differenceIfLastFirst = CornerPoints.Count - 1;

            //add corner
            if (absDifference != 1 && absDifference != differenceIfLastFirst)
            {
                throw new ArgumentException("Indices must be adjacent.");
            }
            else if (absDifference == differenceIfLastFirst) 
            {
                insertIndex = Math.Max(index1, index2) + 1;
            }
            else 
            {
                insertIndex = Math.Max(index1, index2);
            }
            CornerPoints.Insert(insertIndex, point);

            UpdateArea();
            UpdateAspectRatio();
        }

        public void RemoveCorner(Point3dId point)
        {
            CornerPoints.Remove(point);
            //this.SortCorners();
            UpdateArea();
            UpdateAspectRatio();
        }
        public void RemoveCorner(int pointId)
        {
            // Find the index of the point with the specified Id.
            int index = CornerPoints.FindIndex(point => point.Id == pointId);

            // If the point is found, remove it from the list.
            if (index >= 0)
            {
                CornerPoints.RemoveAt(index);
                //this.SortCorners();
                UpdateArea();
                UpdateAspectRatio();
            }
        }

        //----------------------------update params---------------------------//
        public void UpdateArea()
            {
                // Compute the area of the room
                Area = ComputeArea(CornerPoints);
            }

        public void UpdateAspectRatio()
            {
                // Compute the aspect ratio of the room
                AspectRatio = ComputeAspectRatio(CornerPoints);
            }
        public void UpdateAdjacentRooms(Dictionary<int, Room> allRooms)
    {
        // implementation to update the AdjacentRooms dictionary based on the CornerPoints and other rooms
    }

        //---------------------------------OTHER METHODS--------------------------------//

        /// <summary>
        /// Look for corner based on the Id of a Point3dId. If there is none - returns -1
        /// </summary>
        public int IndexOfCornerByID (int idToSearch)
        {
            int index = this.CornerPoints.FindIndex(point => point.Id == idToSearch);
            return index;
        }
    }



}