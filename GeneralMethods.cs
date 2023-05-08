using GeneratePlan.Class;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratePlan
{
    public static class GeneralMethods
    {
        public static bool AreLinesParallel(Line line1, Line line2, double tolerance = 0.01)
        {
            Vector3d direction1 = line1.Direction;
            Vector3d direction2 = line2.Direction;

            direction1.Unitize();
            direction2.Unitize();

            double dotProduct = direction1 * direction2;
            double angle = Math.Acos(dotProduct);

            return (Math.Abs(angle) < tolerance) || (Math.Abs(angle - Math.PI) < tolerance);
        }

        public static string ThreePointsRelationship(Point3dId point1, Point3dId mainPoint, Point3dId point2, double tolerance) 
        {
            List<Point3d> adjecentCornersLocations = new List<Point3d> {
                        point1.Point,
                        mainPoint.Point,
                        point2.Point };
            Polyline adjecentLines = new Polyline(adjecentCornersLocations);
            string linesRelationship = GeneralMethods.LineRelationship(new Line(adjecentLines[0], adjecentLines[1]), new Line(adjecentLines[2], adjecentLines[1]), tolerance);
            return linesRelationship;
        }

        /// <summary>
        /// BOTH LINES NEED TO END IN THE SAME POINT
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static string LineRelationship(Line line1, Line line2, double tolerance = 0.001)
        {
            Vector3d vector1 = line1.To - line1.From;
            Vector3d vector2 = line2.To - line2.From;
            vector1.Unitize();
            vector2.Unitize();

            double dotProduct = vector1 * vector2;
            double cosine = Math.Abs(dotProduct);

            if (cosine < tolerance)
            {
                return "perpendicular";
            }
            else if (Math.Abs(cosine - 1) < tolerance)
            {
                // Check if lines share a common point
                if (line1.From.DistanceTo(line2.From) < tolerance ||
                    line1.From.DistanceTo(line2.To) < tolerance ||
                    line1.To.DistanceTo(line2.From) < tolerance ||
                    line1.To.DistanceTo(line2.To) < tolerance)
                {
                    // Check if lines have the same direction
                    if (dotProduct > 0)
                    {
                        return "overlapping";
                    }
                    else
                    {
                        return "parallel";
                    }
                }
                else
                {
                    return "parallel";
                }
            }
            else
            {
                return "angled";
            }

        }

        public static int ChooseRandomIndex(List<object> list, int seed) 
        {
            int count = list.Count;
            Random r = new Random(seed);
            int index = r.Next(0, count);

            return index;
        }

    }
}
