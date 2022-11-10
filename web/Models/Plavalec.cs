using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web.Models
{
    public class Plavalec
    {
        public Plavalec(){
            
        }
        public Plavalec(string ime, string priimek, DateTime datumRojstva, int skupinaID)
        {
            Ime = ime;
            Priimek = priimek;
            DatumRojstva = datumRojstva;
            SkupinaID = skupinaID;
        }

        public int PlavalecID { get; set; }
        public string Ime { get; set; }
        public string Priimek { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DatumRojstva { get; set; }
        public int SkupinaID { get; set; }
        public ApplicationUser Uporabnik { get; set; }

        public Skupina Skupina {get; set;}
    }
}