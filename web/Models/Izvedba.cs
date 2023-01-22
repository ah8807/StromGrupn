using System;
using System.Collections.Generic;

namespace web.Models
{
    public class Izvedba
    {
        public int IzvedbaID { get; set; }
        public DateTime DatumUra { get; set; }
        public int SkupinaID { get; set; }
        public int? NadomestniUciteljID { get; set; }
        public Ucitelj? NadomestniUcitelj { get; set; }
        public Skupina Skupina { get; set; }
    }
}