using HarmonyRFID_API.Database;
using HarmonyRFID_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace HarmonyRFID_API.Controllers
{
    public class ValuesController : BaseController
    {
        [HttpPost]
        [Route("api/Kimliklendirme")]
        public object Kimliklendirme([FromBody]List<KimliklendirmeModel> model)
        {
            if (!ModelState.IsValid)
                return ShowMessage(-1, "Hatalı veri girişi. Lütfen veriyi JSON Array olarak gönderiniz.");

            if (model.Count < 1)
                return ShowMessage(-2, "Veri gönderilmedi.");

            var errorModelList = "";
            foreach (var m in model)
            {
                bool durum = db.Execute("INSERT INTO RFIDBarkod (BarkodNo, EPC, TID) VALUES (@BarkodNo, @EPC, @TID)", db.BuildDbParamList(
                                                                new DBParameter("BarkodNo", DBTypes.BigInt, m.BARKOD_SERINO),
                                                                new DBParameter("EPC", DBTypes.VarChar, m.EPC),
                                                                new DBParameter("TID", DBTypes.VarChar, m.TID)
                                                               ), false, false);
                if (!durum)
                    errorModelList += m.BARKOD_SERINO + ",";
            }

            if (errorModelList == "")
                return ShowMessage(0, "OK");

            return ShowMessage(-3, "Barkodlar eklenemedi: " + errorModelList);
        }


        [HttpGet]
        [Route("api/EnvanterGuncelleme")]
        public object EnvanterGuncelleme()
        {

            var dt = db.GetTable(@"with ctebarkod as
                                       (select BarkodNo, StokKod, VariantKod, GDepoKod DepoKod, round(sum(Gmiktar-CMiktar), 6) Bakiye
                                        from StokHareketBarkod sh
                                        group by BarkodNo, StokKod, VariantKod, GDepoKod
                                        having round(sum(Gmiktar-CMiktar), 6)<>0
                                       )
                                    Select * from ctebarkod cd inner join RFIDBarkod RF on RF.BarkodNo = cd.BarkodNo
                                    where Bakiye > 0", false);

            if (dt == null)
                return ShowMessage(-41, "Gönderilecek kayıt bulunamadı.");
            else if (dt.Rows == null)
                return ShowMessage(-42, "Gönderilecek kayıt bulunamadı.");
            else if (dt.Rows.Count < 1)
                return ShowMessage(-43, "Gönderilecek kayıt bulunamadı.");

            var list = new List<EnvanterGuncellemeModel>();
            foreach (DataRow r in dt.Rows)
            {
                var obj = new EnvanterGuncellemeModel();
                obj.BARKOD_SERINO = Convert.ToInt64(r["BarkodNo"]);
                obj.EPC = r["EPC"].ToString();
                list.Add(obj);
            }

            if (list.Count < 1)
                return ShowMessage(-5, "Gönderilecek kayıt bulunamadı.");

            return list;
        }


        [HttpPost]
        [Route("api/SayimSonuclari")]
        public object SayimSonuclari([FromBody]List<SayimSonucuModel> model)
        {
            if (!ModelState.IsValid)
                return ShowMessage(-1, "Hatalı veri girişi. Lütfen veriyi JSON Array olarak gönderiniz.");

            if (model.Count < 1)
                return ShowMessage(-2, "Veri gönderilmedi.");

            var errorModelList = "";
            foreach (var m in model)
            {
                bool durum = db.Execute("INSERT INTO EnvanterDetayRFID (SayimNo, SayimTarihi, BarkodNo, EPC, SayimDurum) VALUES (@SayimNo, @SayimTarihi, @BarkodNo, @EPC, @SayimDurum)", db.BuildDbParamList(
                                                                new DBParameter("SayimNo", DBTypes.Int, m.SAYIM_NO),
                                                                new DBParameter("SayimTarihi", DBTypes.SmallDateTime, m.SAYIM_TARIH),
                                                                new DBParameter("BarkodNo", DBTypes.BigInt, m.BARKOD_SERINO),
                                                                new DBParameter("EPC", DBTypes.VarChar, m.EPC),
                                                                new DBParameter("SayimDurum", DBTypes.TinyInt, m.SAYIM_DURUM)
                                                               ), false, false);
                if (!durum)
                    errorModelList += m.BARKOD_SERINO + ",";
            }

            if (errorModelList == "")
                return ShowMessage(0, "OK");

            return ShowMessage(-3, "Sayım Sonucu eklenemedi: " + errorModelList);
        }



        [HttpPost]
        [Route("api/BarkodListesi")]
        public object BarkodListesi([FromBody]BarkodListesiModel model)
        {
            var list = new List<BarkodListesiSonucModel>();

            var fnTarih = DateTime.Today.ToShortDateString();
            var girisDepo = "MAMÜL DEPO";
            var cikisDepo = "MONTAJ";

            if (model != null)
            {
                if (model.Tarih != null && model.Tarih != DateTime.MinValue)
                    fnTarih = model.Tarih.ToShortDateString();

                if (!string.IsNullOrWhiteSpace(model.GirisDepo))
                    girisDepo = model.GirisDepo;

                if (!string.IsNullOrWhiteSpace(model.CikisDepo))
                    cikisDepo = model.CikisDepo;
            }

            var dt = db.GetTable(@"select b.BarkodNo, isnull((select 1 from RFIDBarkod rf where rf.BarkodNo = b.BarkodNo), 0) Islendi
                                    from StokHareketleri s inner join StokHareketBarkod b on b.SHid = s.id
                                    where s.Tarih = @fnTarih And 
                                       s.HareketTipi = 'T' And 
                                       s.GirisDepoKod = @GirisDepoKod and 
                                       s.CikisDepoKod = @CikisDepoKod
                                    Group by b.BarkodNo, b.VariantKod order by b.BarkodNo", db.BuildDbParamList(
                                        new DBParameter("@fnTarih", DBTypes.SmallDateTime, fnTarih),
                                        new DBParameter("@GirisDepoKod", DBTypes.VarChar, girisDepo),
                                        new DBParameter("@CikisDepoKod", DBTypes.VarChar, cikisDepo)
                                        ), false, false);

            if (dt == null || dt.Rows == null || dt.Rows.Count < 1)
                return ShowMessage(-4, "Gösterilecek kayıt bulunamadı. BarkodListesi");

            foreach (DataRow r in dt.Rows)
            {
                var obj = new BarkodListesiSonucModel();
                obj.BarkodNo = Convert.ToInt64(r["BarkodNo"]);
                obj.Islendi = Convert.ToBoolean(r["Islendi"]);
                list.Add(obj);
            }

            if (list.Count < 1)
                return ShowMessage(-5, "Gösterilecek kayıt bulunamadı.");

            return list;
        }

        [HttpPost]
        [Route("api/BarkodSilme")]
        public object BarkodSilme([FromBody]List<long> model)
        {
            if (!ModelState.IsValid)
                return ShowMessage(-1, "Hatalı veri girişi. Lütfen veriyi JSON Array olarak gönderiniz.");

            if (model.Count < 1)
                return ShowMessage(-2, "Veri gönderilmedi.");

            var whereSql = string.Join(",", model.ToArray());

            var sql = "DELETE FROM RFIDBarkod WHERE BarkodNo IN (" + whereSql + ")";
            bool res = db.Execute(sql, false);
            //if (!res)
            //    return ShowMessage(-3, "Gönderdiğiniz barkodlar arasında, RFIDBarkod tablosunda silinecek barkod bulunamadı.");

            sql = "DELETE FROM EnvanterDetayRFID WHERE BarkodNo IN (" + whereSql + ")";
            res = db.Execute(sql, false);
            //if (!res)
            //    return ShowMessage(-4, "Gönderdiğiniz barkodlar arasında, EnvanterDetayRFID tablosunda silinecek barkod bulunamadı.");

            return ShowMessage(0, "OK"); 
        }


        [HttpPost]
        [Route("api/BarkodListesiV2")]
        public object BarkodListesiV2([FromBody]BarkodListesiV2Model model)
        {
            var list = new List<BarkodListesiV2SonucModel>();

            var fnTarih = DateTime.Today.ToShortDateString();
            var girisDepo = "MAMÜL DEPO";
            var cikisDepo = "MONTAJ";

            if (model != null)
            {
                if (model.Tarih != null && model.Tarih != DateTime.MinValue)
                    fnTarih = model.Tarih.ToShortDateString();

                if (!string.IsNullOrWhiteSpace(model.GirisDepo))
                    girisDepo = model.GirisDepo;

                if (!string.IsNullOrWhiteSpace(model.CikisDepo))
                    cikisDepo = model.CikisDepo;
            }

            var dt = db.GetTable(@"select b.BarkodNo, isnull((select 1 from RFIDBarkod rf where rf.BarkodNo = b.BarkodNo), 0) Islendi, s.VariantKod, s.Tarih
                                    from StokHareketleri s inner join StokHareketBarkod b on b.SHid = s.id
                                    where s.Tarih = @fnTarih And 
                                       s.HareketTipi = 'T' And 
                                       s.GirisDepoKod = @GirisDepoKod and 
                                       s.CikisDepoKod = @CikisDepoKod
                                    Group by b.BarkodNo, b.VariantKod, s.VariantKod, s.Tarih order by b.BarkodNo", db.BuildDbParamList(
                                        new DBParameter("@fnTarih", DBTypes.SmallDateTime, fnTarih),
                                        new DBParameter("@GirisDepoKod", DBTypes.VarChar, girisDepo),
                                        new DBParameter("@CikisDepoKod", DBTypes.VarChar, cikisDepo)
                                        ), false, false);

            if (dt == null || dt.Rows == null || dt.Rows.Count < 1)
                return ShowMessage(-4, "Gösterilecek kayıt bulunamadı. BarkodListesiV2");

            foreach (DataRow r in dt.Rows)
            {
                var obj = new BarkodListesiV2SonucModel();
                obj.BarkodNo = Convert.ToInt64(r["BarkodNo"]);
                obj.Islendi = Convert.ToBoolean(r["Islendi"]);
                obj.VariantKod = r["VariantKod"].ToString();
                obj.Tarih = Convert.ToDateTime(r["Tarih"]);
                list.Add(obj);
            }

            if (list.Count < 1)
                return ShowMessage(-5, "Gösterilecek kayıt bulunamadı.");

            return list;
        }
    }
}
