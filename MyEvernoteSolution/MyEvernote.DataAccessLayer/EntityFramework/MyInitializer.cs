using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MyEvernote.Entities;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
    public class MyInitializer : CreateDatabaseIfNotExists<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context)
        {
            EvernoteUser admin = new EvernoteUser()
            {
                //admin kullanıcısı ekleme

                Name = "furkan",
                Surname = "öztürk",
                Email = "furkanozturrk@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = true,
                Username = "furkanozturk",
                ProfileImageFilename="user_boy.jpg",
                Password = "12345",           
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now.AddMinutes(5),
                ModifiedUsername = "furkanozturk",


            };
            EvernoteUser standartUser = new EvernoteUser()
            {
                //standart kullanıcı ekleme

                Name = "mustafa",
                Surname = "öztürk",
                Email = "mustafaozturrk@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = false,
                Username = "mustafaozturk",
                ProfileImageFilename = "user_boy.jpg",
                Password = "4321",
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now.AddMinutes(5),
                ModifiedUsername = "mustafaozturk"
            };

            context.EvernoteUsers.Add(admin);
            context.EvernoteUsers.Add(standartUser);

            for (int i = 0; i < 8; i++)
            {
                EvernoteUser user = new EvernoteUser()
                {
                    //standart kullanıcı ekleme

                    Name = FakeData.NameData.GetFemaleFirstName(),
                    Surname = FakeData.NameData.GetSurname(),
                    Email = FakeData.NetworkData.GetEmail(),
                    ProfileImageFilename = "user_boy.jpg",
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = true,
                    IsAdmin = false,
                    Username = $"user {i} ",
                    Password = "123",
                    CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                    ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                    ModifiedUsername = $"user {i} "
                };
                context.EvernoteUsers.Add(user);

            }
           

            context.SaveChanges();

            List<EvernoteUser> userlist = context.EvernoteUsers.ToList();


            //fake category olusturma

            for (int i = 0; i < 10; i++)
            {
                Category cat = new Category()
                {
                    
                    Title = FakeData.PlaceData.GetStreetName(),
                    Description = FakeData.PlaceData.GetAddress(),
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    ModifiedUsername = "furkanozturk"
                };

                context.Categories.Add(cat);

                //fake note ekleme
                for (int k = 0; k < FakeData.NumberData.GetNumber(5, 9); k++)
                {
                    Note note = new Note()
                    {
                        Title = FakeData.TextData.GetAlphabetical(FakeData.NumberData.GetNumber(5, 25)), //fake baslık alfabetik 5 25 arası
                        Text = FakeData.TextData.GetSentences(FakeData.NumberData.GetNumber(1, 3)), //fake cümle 1 3 arası

                        IsDraft = false,
                        LikeCount = FakeData.NumberData.GetNumber(1, 9),
                        Owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)],
                        CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedUsername = (k % 2 == 0) ? admin.Username : standartUser.Username,

                    };
                    cat.Notes.Add(note);

                    //fake yorum ekleme
                    for (int j = 0; j < FakeData.NumberData.GetNumber(3, 5); j++)
                    {
                        Comment comment = new Comment()
                        {
                            Text = FakeData.TextData.GetSentence(),
                            Owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)],
                            CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedUsername = (j % 2 == 0) ? admin.Username : standartUser.Username,
                        };
                        note.Comments.Add(comment);
                    }
                    // fake begenme ekleme

                    for (int l = 0; l < note.LikeCount; l++)
                    {
                        Liked liked = new Liked()
                        {
                            LikedUser = userlist[l]
                        };
                        note.Likes.Add(liked);
                    }

                }
            }

            context.SaveChanges();

        }
    }
}
