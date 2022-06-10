using MyEvernote.BusinessLayer;
using MyEvernote.Entities;
using MyEvernote.Entities.Messages;
using MyEvernote.Entities.ValueObjects;
using MyEvernote.WebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyEvernote.WebApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            
            NoteManager nm = new NoteManager();

            return View(nm.GetAllNote().OrderByDescending(x => x.ModifiedOn).ToList());
           
        }
        public ActionResult ByCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CategoryManager cm = new CategoryManager();
            Category cat = cm.GetCategoryById(id.Value);
            if (cat == null)
            {
                return HttpNotFound();
                //return RedirectToAction("Index", "Home");
            }

            return View("Index", cat.Notes.OrderByDescending(x => x.ModifiedOn).ToList());
        }
        public ActionResult MostLiked()
        {
            NoteManager nm = new NoteManager();

            return View("Index", nm.GetAllNote().OrderByDescending(x => x.LikeCount).ToList());
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult ShowProfile()
        {
            EvernoteUser currentUser = Session["login"] as EvernoteUser;
            EvernoteUserManager eum = new EvernoteUserManager();
            BusinessLayerResult<EvernoteUser> res = eum.GetUserById(currentUser.Id);

            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Hata Oluştu",
                    Items = res.Errors
                };

                return View("Error", errorNotifyObj);
            }

            return View(res.Result);
        }
        public ActionResult EditProfile()
        {
            EvernoteUser currentUser = Session["login"] as EvernoteUser;

            EvernoteUserManager eum = new EvernoteUserManager();
            BusinessLayerResult<EvernoteUser> res = eum.GetUserById(currentUser.Id);

            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Hata Oluştu",
                    Items = res.Errors
                };

                return View("Error", errorNotifyObj);
            }

            return View(res.Result);
        }
        [HttpPost]
        public ActionResult EditProfile(EvernoteUser model, HttpPostedFileBase ProfileImage)
        {
            ModelState.Remove("ModifiedUsername");

            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    if (ProfileImage != null &&
                        (ProfileImage.ContentType == "image/jpeg" ||
                        ProfileImage.ContentType == "image/jpg" ||
                        ProfileImage.ContentType == "image/png"))
                    {
                        string filename = $"user_{model.Id}.{ProfileImage.ContentType.Split('/')[1]}";

                        ProfileImage.SaveAs(Server.MapPath($"~/images/{filename}"));
                        model.ProfileImageFilename = filename;
                    }
                }

                EvernoteUserManager eum = new EvernoteUserManager();
                BusinessLayerResult<EvernoteUser> res = eum.UpdateProfile(model);

                if (res.Errors.Count > 0)
                {
                    ErrorViewModel errorNotifyObj = new ErrorViewModel()
                    {
                        Items = res.Errors,
                        Title = "Profil Güncellenemedi.",
                        RedirectingUrl = "/Home/EditProfile"
                    };

                    return View("Error", errorNotifyObj);
                }

               
                Session["login"] = res.Result;

                return RedirectToAction("ShowProfile");


            }
            else
            {
                return View(model);
            }
        }
    

        public ActionResult DeleteProfile()
        {
            EvernoteUser currentUser = Session["login"] as EvernoteUser;

            EvernoteUserManager eum = new EvernoteUserManager();
            BusinessLayerResult<EvernoteUser> res = eum.RemoveUserById(currentUser.Id);

            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Items = res.Errors,
                    Title = "Profil Silinemedi.",
                    RedirectingUrl = "/Home/ShowProfile"
                };

                return View("Error", errorNotifyObj);
            }

            Session.Clear();

            return RedirectToAction("Index");
        }


        public ActionResult TestNotify()
        {
            ErrorViewModel model = new ErrorViewModel()
            {
                Header = "Yönlendirme..",
                Title = "Ok Test",
                RedirectingTimeout = 10000,
                Items = new List<ErrorMessageObj>() {
                    new ErrorMessageObj() { Message = "test başarılı 1" },
                    new ErrorMessageObj() { Message = "test başarılı 2" } }
            };

            return View("Error", model);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                EvernoteUserManager eum = new EvernoteUserManager();
                BusinessLayerResult<EvernoteUser> res = eum.LoginUser(model);
                if (res.Errors.Count > 0)
                {
                    if (res.Errors.Find(x => x.Code == ErrorMessageCode.UserIsNotActive) != null)
                    {
                        ViewBag.SetLink = "http://Home/Activate/1234-4567-78980";

                    }


                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    return View(model);

                }

                Session["login"] = res.Result;      
                return RedirectToAction("Index");  

            }


            return View(model);


        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                EvernoteUserManager eum = new EvernoteUserManager();
                BusinessLayerResult<EvernoteUser> res = eum.RegisterUser(model);
                if (res.Errors.Count > 0)
                {
                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    return View(model);
                }
                
                OkViewModel notifyObj = new OkViewModel()
                {
                    Title = "Kayıt Başarılı",
                    RedirectingUrl = "/Home/Login",
                };
                notifyObj.Items.Add("Lütfen e-posta adresinize gönderdigimiz aktivasyon link'ine tıklayarak hesabınızı aktive edinizi.Hesabınızı aktive etmeden not ekleyemez ve beğenme yapamazsınız.");

                return View("Ok", notifyObj);



            }
            return View(model);
        }

        public ActionResult UserActivate(Guid id)
        {
            EvernoteUserManager eum = new EvernoteUserManager();
            BusinessLayerResult<EvernoteUser> res = eum.ActivateUser(id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Geçersiz İşlem",
                    Items = res.Errors
                };

                return View("Error", errorNotifyObj);
            }

            OkViewModel okNotifyObj = new OkViewModel()
            {
                Title = "Hesap Aktifleştirildi",
                RedirectingUrl = "/Home/Login",

            };

            okNotifyObj.Items.Add(" Hesabınız aktifleştirildi.Artık not paylaşabilir ve beğenme yapabilirsiniz.");

            return View("Ok", okNotifyObj);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
