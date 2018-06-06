using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace HelloSpiral
{
    public class HelloSpiralInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "HelloSpiral";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("ff1397e5-186d-44e4-adfe-fa46c827603b");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
