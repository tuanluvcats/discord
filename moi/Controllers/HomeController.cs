using moi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace moi.Controllers
{
    public class HomeController : Controller
    {
        QL_BanGaRanEntities db = new QL_BanGaRanEntities(); 
        public ActionResult Index()
        {
            var sp = db.SanPhams.ToList();
            return View(sp);
        }

     
    }
}