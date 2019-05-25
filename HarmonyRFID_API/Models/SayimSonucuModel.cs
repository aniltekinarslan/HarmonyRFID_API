using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HarmonyRFID_API.Models
{
    public class SayimSonucuModel
    {
        public long BARKOD_SERINO { get; set; }
        public string EPC { get; set; }
        public int SAYIM_NO { get; set; }
        public DateTime SAYIM_TARIH { get; set; }
        public Byte SAYIM_DURUM { get; set; }
    }
}