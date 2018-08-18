using System.Collections.Generic;

namespace GXCalcTool.Models
{
    public class GXCalcMainModel
    {
        public List<Country> Countries { get; set; }
        public List<Area> Areas { get; set; }

    }

    public class Country
    {
        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }

    public class Area
    {
        public string AreaID { get; set; }
        public string AreaName { get; set; }
    }
}
