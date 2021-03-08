using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPD.Controllers
{
    public class HelpController : Controller
    {
        
        public ActionResult Index()
        {
            return File(Server.MapPath("/Views/Help/index.html"), "text/html");
        }
    }
}