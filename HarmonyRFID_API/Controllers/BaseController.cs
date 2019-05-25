using HarmonyRFID_API.Database;
using HarmonyRFID_API.Models;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;

namespace HarmonyRFID_API.Controllers
{
    public class BaseController : ApiController
    {
        protected DBHelper db = null;
        public BaseController()
        {
            var conn = WebConfigurationManager.ConnectionStrings["HarmonyConnection"].ConnectionString;
            db = new DBHelper(conn);
        }

        protected MessageModel ShowMessage(int errCode, string errMsg)
        {
            return new MessageModel() { ErrorCode = errCode, ErrorMessage = errMsg };
        }
    }
}