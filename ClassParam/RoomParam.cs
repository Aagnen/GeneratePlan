using GeneratePlan.ClassGoo;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratePlan.ClassParam
{

    public class RoomParam : GH_PersistentParam<RoomGoo>
    {
        public RoomParam()
            : base(new GH_InstanceDescription("Room", "Rm", "A Room object containing corner points, function, area, aspect ratio, and adjacent rooms.", "DplMgr", "Rooms"))
        {
        }

        public override Guid ComponentGuid => new Guid("0E84D6C7-9811-4320-B283-0CD289203EA2");

        protected override System.Drawing.Bitmap Icon => Properties.Resources.ikonka_room_param;

        protected override GH_GetterResult Prompt_Plural(ref List<RoomGoo> values)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Singular(ref RoomGoo value)
        {
            return GH_GetterResult.cancel;
        }


    }

}
