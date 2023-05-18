using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using Rhino.Collections;

namespace GeneratePlan.Class
{
    public class Point3dId //TOLEARN: powinno być structem a nie klasą
        //NOTES: Struct - mala, niereferencyjna jednostka
    {
        //------------------------------------------properties------------------------------------//
        public Point3d Point { get; set; }
        public int Id { get; private set; }

        //------------------------------------------constructors------------------------------------//
        public Point3dId(int id, Point3d point)
        {
            Id = id;
            Point = point;
        }

        public Point3dId(Point3dId other) //NOTES: kazda klasa musi miec deep copy. Jesli jest structem / niereferencyjna -> wystarczy zwykla kopia wartosci
        {
            Id = other.Id;
            Point = other.Point;
        }

        public Point3dId()
        {
            Id = 99999;
            Point = new Point3d(0,0,0);
        }

        //------------------------------------------basic methods------------------------------------//

        public void Transform(Transform xForm) //TODO22: move nie dziala
        {
            Point.Transform(xForm);
        }

        public Point3dId Duplicate() //this is a deep copy
        {
            return new Point3dId(this);
        }

        //------------------------------------------other methods------------------------------------//
        public void ReplaceOrAddInListById(List<Point3dId> pointList)
        {
            // Find the index of the point with the same Id as this point.
            int index = pointList.FindIndex(point => point.Id == this.Id);

            // If the point is found, replace it with this point.
            if (index >= 0)
            {
                pointList[index] = this;
            }
            else
            {
                // If the point with the specified Id is not found, add this point to the list.
                pointList.Add(this);
            }
        }

        /// <summary>
        /// Replace this in a list be Id->returns true. If this is not in the list -> returns false.
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public bool ReplaceInListById(List<Point3dId> pointList)
        {
            // Find the index of the point with the same Id as this point.
            int index = pointList.FindIndex(point => point.Id == this.Id);

            // If the point is found, replace it with this point.
            if (index >= 0)
            {
                pointList[index] = this;
                return true;
            }
            else
            {
                // If the point with the specified Id is not found, add this point to the list.
                return false;
            }
        }

        //------------------------------------------equality and hashing------------------------------------//
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Point3dId other = (Point3dId)obj;
            return Id == other.Id && Point == other.Point;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Id.GetHashCode();
            hash = hash * 23 + Point.GetHashCode();
            return hash;
        }
    }


}