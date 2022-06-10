using MyEvernote.Common.Helpers;
using MyEvernote.DataAccessLayer.EntityFramework;
using MyEvernote.Entities;
using MyEvernote.Entities.Messages;
using MyEvernote.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.BusinessLayer
{
    public class EvernoteUserManager
    {
        private Repository<EvernoteUser> repo_user = new Repository<EvernoteUser>();
        public BusinessLayerResult<EvernoteUser> RegisterUser(RegisterViewModel data)

        {

            EvernoteUser user = repo_user.Find(x => x.Username == data.Username || x.Email == data.EMail);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            if (user != null)
            {
                if (user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı.");
                }
                if (user.Email == data.EMail)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı.");
                }
            }
            else
            {
                int dbResult = repo_user.Insert(new EvernoteUser()
                {
                    Username = data.Username,
                    Email = data.EMail,
                    ProfileImageFilename = "user_boy.jpg",
                    Password = data.Password,
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = false,
                    IsAdmin = false



                });
                if (dbResult > 0)
                {
                    res.Result = repo_user.Find(x => x.Email == data.EMail && x.Username == data.Username);


                    string siteUri = ConfigHelper.Get<string>("SiteRootUri");
                    string activateUri = $"{siteUri}/Home/UserActivate/{res.Result.ActivateGuid}";
                    string body = $"Merhaba {res.Result.Username};<br><br>Hesabınızı aktifleştirmek için <a href='{activateUri}' target='_blank'> tıklayınız.</a>";
                    MailHelper.SendMail(body, res.Result.Email, "MyEvernote hesap Aktifleştirme");


                }

            }
            return res;

        }

        public BusinessLayerResult<EvernoteUser> GetUserById(int id)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = repo_user.Find(x => x.Id == id);
            if (res.Result == null)
            {
                res.AddError(ErrorMessageCode.UserNotFound, "Kullanıcı bulunamadı.");
            }

            return res;
        }

        public BusinessLayerResult<EvernoteUser> LoginUser(LoginViewModel data)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = repo_user.Find(x => x.Username == data.Username && x.Password == data.Password);

            if (res.Result != null)
            {
                if (!res.Result.IsActive)
                {
                    res.AddError(ErrorMessageCode.UserIsNotActive, "Kullanıcı aktifleştirilmemiştir.");
                    res.AddError(ErrorMessageCode.CheckYourEmail, " Lütfen e - posta adresinizi kontrol ediniz.");

                }
            }
            else
            {
                res.AddError(ErrorMessageCode.UsernameOrPassWrong, "Kullanıcı adı yada şifre uyuşmuyor");
            }
            return res;
        }
        public BusinessLayerResult<EvernoteUser> UpdateProfile(EvernoteUser data)
        {
            EvernoteUser db_user = repo_user.Find(x => x.Id != data.Id && (x.Username == data.Username || x.Email == data.Email));
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();


            if (db_user != null && db_user.Id != data.Id)
            {
                if (db_user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (db_user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı.");
                }

                return res;
            }

            res.Result = repo_user.Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;

            if (string.IsNullOrEmpty(data.ProfileImageFilename) == false)
            {
                res.Result.ProfileImageFilename = data.ProfileImageFilename;
            }

            if (repo_user.Update(res.Result) == 0)
            {
                res.AddError(ErrorMessageCode.ProfileCouldNotUpdated, "Profil güncellenemedi.");
            }

            return res;
        }

        public BusinessLayerResult<EvernoteUser> RemoveUserById(int id)
        {

            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            EvernoteUser user = repo_user.Find(x => x.Id == id);

            if (user != null)
            {
                if (repo_user.Delete(user) == 0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotRemove, "Kullanıcı silinemedi.");
                    return res;
                }
            }
            else
            {
                res.AddError(ErrorMessageCode.UserCouldNotFind, "Kullanıcı bulunamadı.");
            }

            return res;
        }
    

        public BusinessLayerResult<EvernoteUser> ActivateUser(Guid activateId)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = repo_user.Find(x => x.ActivateGuid == activateId);
            if (res.Result != null)
            {
                if (res.Result.IsActive)
                {
                    res.AddError(ErrorMessageCode.UserAlreadyActivate, "Kullanıcı zaten aktif edilmiştir.");
                    return res;
                }
                res.Result.IsActive = true;
                repo_user.Update(res.Result);
            }
            else
            {
                res.AddError(ErrorMessageCode.ActivateIdDoesNotExists, "Aktifleştirilecek kullanıcı bulunamadı.");
            }
            return res;
        }


    }
}
