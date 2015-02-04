using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net.Http;
using System.Net;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext DBCONTEXT = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(PopulateViewModel());
        }

        public PartialViewResult AddUrl(string Url)
        {
            try
            {
                var itemToAdd = new WebApplication1.Models.Uri();
                itemToAdd.UrlPart = (Url);
                itemToAdd.UrlIdentity = 1;
                DBCONTEXT.Urls.Add(itemToAdd);
                DBCONTEXT.SaveChanges();

                return PartialView("UrlListPartial", PopulateViewModel());
            }
            catch(Exception ex)
            {
                //TODOS
                return PartialView();
            }
        }


        public async Task<PartialViewResult> NextUrl(string _rowVersion)
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = DBCONTEXT.Urls.ToList();
            byte[] rowVersion = Convert.FromBase64String(_rowVersion);

            if(retModel.UrlList.Count() < 2 ) //IF LIST IS ONLY 1 ELEMENT
            {
                retModel.UrlCurrent = DBCONTEXT.Urls.First().UrlPart;
                return PartialView("IFramePartial", retModel);
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {
                string[] fieldsToBind = new string[] { "RowVersion", "UrlIdentity" };
                var itemToUpdate = await DBCONTEXT.CurrUrl.FindAsync(1);

                if (TryUpdateModel(itemToUpdate, fieldsToBind))
                {
                    var currentId = retModel.UrlList.First().CurrentUrl.UrlIdentity;
                    var index = retModel.UrlList.FindIndex(m => m.Id == currentId);

                    try
                    { 
                        //IF AT END OF LIST
                        if(index+1 >= retModel.UrlList.Count()) 
                        {
                            index = 0;
                            //UpdateDB ITEM
                            itemToUpdate.UrlIdentity = retModel.UrlList[index].Id;
                            DBCONTEXT.Entry(itemToUpdate).OriginalValues["RowVersion"] = rowVersion;
                            DBCONTEXT.Entry(itemToUpdate).State = EntityState.Modified;
                            await DBCONTEXT.SaveChangesAsync();

                            //Update Model with latest vals.
                            retModel = PopulateViewModel();
                            return PartialView("IFramePartial", retModel);
                        }
                        else
                        {
                            //UpdateDB ITEM
                            index = index+1;
                            itemToUpdate.UrlIdentity = retModel.UrlList[index].Id;
                            DBCONTEXT.Entry(itemToUpdate).OriginalValues["RowVersion"] = rowVersion;
                            DBCONTEXT.Entry(itemToUpdate).State = EntityState.Modified;
                            await DBCONTEXT.SaveChangesAsync();

                            //Update Model with latest vals.
                            retModel = PopulateViewModel();
                            return PartialView("IFramePartial", retModel);
                        }
                    }
                    catch(DbUpdateConcurrencyException)
                    {
                        //Someone else nexted before. Feed user latest Model vals
                        retModel = PopulateViewModel();
                    }
                    catch(Exception)
                    {
                        //WHy am i here ?

                        retModel = PopulateViewModel();
                    }
                }
                else
                {
                    retModel = PopulateViewModel();
                }
            }
            return PartialView("IFramePartial", retModel);
        }

        public PartialViewResult Sync()
        {
            return PartialView("IFramePartial", PopulateIFrame());
        }

        public JsonResult Syn(string rowV)
        {
            if (IsSync(rowV))
                return this.Json(new { syn = true });
            else
                return this.Json(new {syn = false});
        }








        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        #region DBHelpers
        public bool IsSync(string rowV)
        {
            var UrlList = DBCONTEXT.Urls.ToList();
            var identity = UrlList.First().CurrentUrl;
            if (Convert.ToBase64String(identity.RowVersion) == rowV)
                return true;
            else
                return false;
        }


        public HomeViewModel PopulateViewModel()
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = DBCONTEXT.Urls.ToList();
            var identity = retModel.UrlList.First().CurrentUrl; //CurrentURL row
            var CurrentUrl = retModel.UrlList.First(i => i.Id == identity.UrlIdentity);//Get URL item using currentUrlIdentifier
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = Convert.ToBase64String(identity.RowVersion);
            return retModel;
        }

        public HomeViewModel PopulateIFrame()
        {
            var retModel = new HomeViewModel();
            var UrlList = DBCONTEXT.Urls.ToList();
            var identity = UrlList.First().CurrentUrl; //CurrentURL row
            var CurrentUrl = UrlList.First(i => i.Id == identity.UrlIdentity);//Get URL item using currentUrlIdentifier
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = Convert.ToBase64String(identity.RowVersion);
            return retModel;
        }

        #endregion
    }
}