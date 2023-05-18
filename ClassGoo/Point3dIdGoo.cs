using System;
using System.Collections.Generic;
using GeneratePlan.Class;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GeneratePlan.ClassGoo //NOTES: Klasa Goo = pozwalajaca GH na manipulowanie klasa glowna
    //NOTES: w Goo objektem jest Goo, a do klasy do której się odnosi mamy dostęp pod nazwą Value
{
    public class Point3dIdGoo : GH_GeometricGoo<Point3dId>,IGH_PreviewData
    {
        //-------------------------------------------------CONSTRUCTORS-------------------------------------------//
        //NOTES: konstruktory Goo uruchamiają się przy KAŻDYM komponencie. W każdym jest nowa deep copy tego samego obietku.
        public Point3dIdGoo()
        {
            this.Value = null;
        }

        public Point3dIdGoo(Point3dId point3dID)
        {
            this.Value = point3dID.Duplicate() as Point3dId; //deep copy
        }

        //-------------------------------------------------BASIC METHODS FOR VALIDITY-------------------------------------------//
        public override bool IsValid
        {
            get { return this.Value != null; }
        }

        public override string TypeName
        {
            get { return "Point3dId"; }
        }

        public override string TypeDescription
        {
            get { return "A Point3d object with an Id."; }
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            if (this.Value == null)
                return null;
            else
                return new Point3dIdGoo(this.Value);
        }

        //-------------------------------------------------TRANSITIONS-------------------------------------------//

        public override string ToString() 
        {
            if (this.Value == null)
                return "Null Point3dId";
            else
                return this.Value.Point.ToString() + ", Id: " + this.Value.Id.ToString();
        }

        public override bool CastFrom(object source)
        {
            if (source == null) return false;

            if (source is Point3dId)
            {
                Value = source as Point3dId;
                return true;
            }
            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null) return false;
            if (typeof(Q).IsAssignableFrom(typeof(Point3dId)))
            {
                target = (Q)(object)Value;
                return true;
            }
            return false;
        }

        //-------------------------------------------------GEOMETRY METHODS-------------------------------------------//

        public override BoundingBox Boundingbox => throw new NotImplementedException(); //TODO2: boundingbox

        public BoundingBox ClippingBox
        {
            get
            {
                Point3d minPoint = new Point3d(this.Value.Point.X - 0.001, this.Value.Point.Y - 0.001, this.Value.Point.Z - 0.001);
                Point3d maxPoint = new Point3d(this.Value.Point.X + 0.001, this.Value.Point.Y + 0.001, this.Value.Point.Z + 0.001);
                return new BoundingBox(minPoint, maxPoint);
            }
        }//TODO2: clippingbox

        public override BoundingBox GetBoundingBox(Transform xform) => throw new NotImplementedException(); //TODO2: boundingbox

        public override IGH_GeometricGoo Transform(Transform xform) //TODO2: nie działa? move sie nie przesuwa
        {
            if (Value == null) { return null; }; 
            Value.Point.Transform(xform);
            return this;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph) //TOLEARN: obczaić co robi morph
        {
            Point3d pt = xmorph.MorphPoint(Value.Point);
            Value.Point = pt;

            return this;
        }

        //-------------------------------------------------PREVIEW METHODS-------------------------------------------//

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if(Value == null) { return; };
            args.Pipeline.DrawPoint(Value.Point); //TOLEARN: czym sa w tym wypadku args?
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            return; //there is no meshes to draw because this is a point
        }
    }
}