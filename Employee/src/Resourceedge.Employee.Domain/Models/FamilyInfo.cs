using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Models
{
    class FamilyInfo
    {
    }

    public class Spouse
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public int ChildrenBelow18 { get; set; }
        public int ChildrenAbove18 { get; set; }
    }

    public class NextofKin
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
