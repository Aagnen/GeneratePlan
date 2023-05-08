using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace GeneratePlan
{
    public class DplMgr230414Info : GH_AssemblyInfo
    {
        public override string Name => "GeneratePlan";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("10208da5-08a5-4a89-af1c-aada6b171d1e");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}