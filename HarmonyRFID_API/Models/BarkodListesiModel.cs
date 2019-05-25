using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HarmonyRFID_API.Models
{
    public class BarkodListesiModel
    {
        public DateTime Tarih { get; set; }
        public string GirisDepo { get; set; }
        public string CikisDepo { get; set; }
    }

    public class BarkodListesiSonucModel
    {
        public long BarkodNo { get; set; }
        public bool Islendi { get; set; }
    }

    public class BarkodListesiV2Model
    {
        public DateTime Tarih { get; set; }
        public string GirisDepo { get; set; }
        public string CikisDepo { get; set; }
    }

    public class BarkodListesiV2SonucModel
    {
        public long BarkodNo { get; set; }
        public bool Islendi { get; set; }
        public string VariantKod { get; set; }
        public DateTime Tarih { get; set; }
    }
}