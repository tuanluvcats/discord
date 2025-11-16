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
        QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1(); 
        public ActionResult Index()
        {
            var sp = db.SanPhams.ToList();
            return View(sp);
        }

     
    }
}