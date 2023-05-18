using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeneratePlan.Class;
using GeneratePlan.ClassParam;
using GeneratePlan.ClassGoo;
using Eto.Forms;

namespace GeneratePlan.ClassGoo
{

    public class RoomGoo : GH_GeometricGoo<Room>, IGH_PreviewData
    {
        //-------------------------------------------------CONSTRUCTORS-------------------------------------------//
        public RoomGoo() { Value = null; }
        public RoomGoo(Room room) //NOTES w konstruktorze Goo musi być deep copy!
        {
            Value = room.Duplicate();
        }
        //public RoomGoo(RoomGoo goo) : base(goo.Value) { }

        //-------------------------------------------------BASIC METHODS FOR VALIDITY-------------------------------------------//
        public override bool IsValid
        {
            get { return Value != null; }
        }

        public override string TypeName
        {
            get { return "Room"; }
        }

        public override string TypeDescription
        {
            get { return "A Room object containing corner points, function, area, aspect ratio, and adjacent rooms."; }
        }

        public override IGH_GeometricGoo DuplicateGeometry() //deep copy! //NOTES: zmienienie tutaj na deep copy naprawiło błąd aktualizacji wstecznej 
        {
            if (this.Value == null)
                return null;
            else
                return new RoomGoo(Value.Duplicate());
        }

        //-------------------------------------------------TRANSITIONS-------------------------------------------//

        public override bool CastFrom(object source)
        {
            if (source is Room room)
            {
                Value = room;
                return true;
            }
            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(Room)))
            {
                target = (Q)(object)Value;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return Value != null ? $"Room Id: {Value.Id}, Function: {Value.Function}" : "null"; //TOLEARN: co to za zapis?
        }

        //-------------------------------------------------GEOMETRY METHODS-------------------------------------------//

        public override BoundingBox Boundingbox => throw new NotImplementedException(); //TODO2: boundingbox

        public BoundingBox ClippingBox
        {
            get
            {
                Point3d minPoint = new Point3d(this.Value.CornerPoints[0].Point.X - 0.001, this.Value.CornerPoints[0].Point.Y - 0.001, this.Value.CornerPoints[0].Point.Z - 0.001);
                Point3d maxPoint = new Point3d(this.Value.CornerPoints[3].Point.X + 0.001, this.Value.CornerPoints[3].Point.Y + 0.001, this.Value.CornerPoints[3].Point.Z + 0.001);
                return new BoundingBox(minPoint, maxPoint);
            }
        }//TODO2: clippingbox

        public override BoundingBox GetBoundingBox(Transform xform) => throw new NotImplementedException(); //TODO2: boundingbox

        public override IGH_GeometricGoo Transform(Transform xform) //TODO2: nie działa? move sie nie przesuwa
        {
            if (Value == null) { return null; };
            foreach (var corner in Value.CornerPoints) 
            {
                corner.Point.Transform(xform);
            }
            return this;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph) //TOLEARN: obczaić co robi morph
            //TODO2: napisac Morph room
        {
            throw new NotImplementedException();
        }

        #region PreviewMethods
        //-------------------------------------------------PREVIEW METHODS-------------------------------------------//
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null) { return; };

            Polyline polyline = new Polyline();
            foreach (Point3dId point in Value.CornerPoints)
            {
                polyline.Add(point.Point);
            }
            polyline.Add(Value.CornerPoints[0].Point);

            System.Drawing.Color linesColor = System.Drawing.Color.FromName("Bittersweet");

            args.Pipeline.DrawPolyline(polyline, linesColor);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null) { return; }; //TODO3: Room mesh vis
        }
        #endregion PreviewMethods
    }

}
