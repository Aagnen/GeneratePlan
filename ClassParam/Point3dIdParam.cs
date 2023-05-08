using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using GeneratePlan.ClassGoo;

namespace GeneratePlan.ClassParam
{
    public class Point3dIdParam : GH_PersistentParam<Point3dIdGoo>
    {
        public Point3dIdParam()
            : base(new GH_InstanceDescription("Point3dId Param", "PtID", "A Point3dId parameter.", "DplMgr", "Point3dId"))
        {
        }

        public override Guid ComponentGuid => new Guid("0E84D6C7-9822-4320-B283-0CD289203EA2");

        protected override GH_GetterResult Prompt_Plural(ref List<Point3dIdGoo> values) //TOLEARN: Prompt plural i singular w params? 
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Singular(ref Point3dIdGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_Point3dID_param;
    }
}