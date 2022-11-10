using System;
using System.Collections.Generic;

namespace web.Models
{
    public class Skupina
    {
        public Skupina(){
            
        }
        public Skupina(int uciteljID, int bazenID, int progaID, int ura)
        {
            UciteljID = uciteljID;
            BazenID = bazenID;
            ProgaID = progaID;
            Ura = ura;
        }

        public int SkupinaID { get; set; }
        public int UciteljID { get; set; }
        public int BazenID { get; set; }
        public int ProgaID { get; set; }
        public int Ura { get; set; }
        public ICollection<Izvedba> Izvedbe { get; set; }
        public ICollection<Plavalec> Plavalci { get; set; }

        public Ucitelj Ucitelj { get; set; }
        public Bazen Bazen { get; set; }
    }
}