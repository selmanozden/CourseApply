using CourseApply3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Layout.Properties;
using System.IO;
using System.Threading.Tasks;
using iText.IO.Font;
using iText.Kernel.Font;
using System.Reflection.Metadata;
using iText.Commons.Actions.Contexts;

[Authorize(Policy = "AdminPolicy")]
public class AdminController : Controller
{
    private readonly CourseDbContext _accountDbContext;

    public AdminController(CourseDbContext accountDbContext)
    {
        _accountDbContext = accountDbContext;
    }
    public async Task<IActionResult> DownloadUserPdf(int id)
    {
        var user = await _accountDbContext.Users.FirstOrDefaultAsync(x => x.UsersId == id);
        if (user == null)
        {
            return NotFound();
        }


        MemoryStream ms = new MemoryStream();
        PdfWriter writer = new PdfWriter(ms);
        PdfDocument pdf = new PdfDocument(writer);
        iText.Layout.Document document = new iText.Layout.Document(pdf);

        var fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\freesans\\FreeSans.ttf");
        var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
        document.SetFont(font);

        document.Add(new Paragraph("Kullanıcı Bilgileri").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"ID: {user.UsersId}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"Ad: {user.FirstName}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"Soyadı: {user.LastName}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"Email: {user.Email}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"Yaş: {user.Age}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
        document.Add(new Paragraph($"Giriş Tarihi: {user.AccessDate}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));

        document.Close();

        return File(ms.ToArray(), "application/pdf", $"{user.FirstName} {user.LastName} bilgileri.pdf");
    }


    public IActionResult Index()
    {
      
        if (User.FindFirstValue(ClaimTypes.Name) == null)
        {
            return RedirectToAction("AdminLogin");
        }
        return View("AdminHome");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AdminLogin()
    {
        return View("AdminLogin");
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> AdminLogin(string adminEmail, string adminPassword)
    {
        if (!ModelState.IsValid)
        {
            return View("AdminLogin");
        }

        var user = await _accountDbContext.Admins
            .FirstOrDefaultAsync(u => u.AdminEmail == adminEmail && u.AdminPassword == adminPassword);

        if (user != null)
        {
            var adminclaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.AdminEmail),
            new Claim(ClaimTypes.NameIdentifier, user.AdminId.ToString()),
        };

            var adminClaimsIdentity = new ClaimsIdentity(adminclaims, "AdminScheme");


            var principal = new ClaimsPrincipal(adminClaimsIdentity);


            await HttpContext.SignInAsync("AdminScheme", principal);



            return RedirectToAction("Index");
        }
        else
        {
            ViewBag.Message = "Geçersiz giriş denemesi.";
        }

        return View("AdminLogin");
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync("AdminScheme");
        return RedirectToAction("AdminLogin");
    }

    public async Task<IActionResult> AdminUserList()
    {
        var users = await _accountDbContext.Users.ToListAsync();
        return View("AdminUserList", users);
    }

    [HttpPost]
    public IActionResult DeleteRecord(int id)
    {
        var record = _accountDbContext.Users.FirstOrDefault(x => x.UsersId == id);
        if (record == null)
        {
            return NotFound();
        }
        _accountDbContext.Users.Remove(record);
        _accountDbContext.SaveChanges();

        return RedirectToAction("AdminUserList");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var model = _accountDbContext.Users.FirstOrDefault(x => x.UsersId == id);
        return View("AdminEdit", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CourseProperty.UserProperty model, IFormFile profilePicture)
    {
        var existingUser = _accountDbContext.Users.AsNoTracking().FirstOrDefault(x => x.UsersId == model.UsersId);
        if (existingUser == null)
        {
            return NotFound();
        }

        if (profilePicture != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await profilePicture.CopyToAsync(memoryStream);
                model.ProfilePictureData = memoryStream.ToArray();
                model.ProfilePictureMimeType = profilePicture.ContentType;
            }
        }
        else
        {
            model.ProfilePictureData = existingUser.ProfilePictureData;
            model.ProfilePictureMimeType = existingUser.ProfilePictureMimeType;
        }

        _accountDbContext.Users.Update(model);
        _accountDbContext.SaveChanges();
        return RedirectToAction("AdminUserList");
    }
    public IActionResult AdminExams()
    {
        var exams = _accountDbContext.Exams.Include(e => e.Questions).ToList();
        return View(exams);
    }
    [HttpGet]
    public IActionResult AdminAddExam() 
    { 
        return View(); 
    }
    [HttpPost]
    public IActionResult AdminAddExam(CourseProperty.ExamProperty examProperty)
    {
        _accountDbContext.Exams.Update(examProperty);
        _accountDbContext.SaveChanges();
        return RedirectToAction("AdminExams");
    }
    [HttpPost]
    public IActionResult AdminDeleteExam(int examId)
    {
        var exam = _accountDbContext.Exams.FirstOrDefault(b => b.ExamId == examId);
        if (exam != null)
        {
            _accountDbContext.Exams.Remove(exam);
            _accountDbContext.SaveChanges();
            return RedirectToAction("AdminExams");
        }
        return NotFound();
    }
    [HttpGet]
    public IActionResult AdminEditExam(int id) 
    { 
        var exam = _accountDbContext.Exams.Include(e => e.Questions).FirstOrDefault(e => e.ExamId == id);
        if (exam != null) 
        {
            return View(exam);
        }
        return View("AdminExams");
    }
    [HttpPost]
    public IActionResult AdminEditExam(string ExamName, int examId) 
    {
        if (ModelState.IsValid)
        {
            var exam = _accountDbContext.Exams.FirstOrDefault(b => b.ExamId == examId);
            if (exam != null)
            {
                exam.ExamName = ExamName;
                _accountDbContext.Update(exam);
                _accountDbContext.SaveChanges();
                return RedirectToAction("AdminExams");
            }
        }
        return View("AdminEditExam");
    }
    public IActionResult AdminDeleteQuestion(int questionId) 
    {
        var question = _accountDbContext.Questions.FirstOrDefault(a=> a.QuestionId == questionId);
        if (question != null)
        {
            _accountDbContext.Questions.Remove(question);
            _accountDbContext.SaveChanges();
            return RedirectToAction("AdminEditExam", new { id = question.ExamId });

        }
        return NotFound();
    }
    [HttpGet]
    public IActionResult AdminEditQuestion(int questionId) 
    {
        var model = _accountDbContext.Questions.FirstOrDefault(a => a.QuestionId == questionId);
        return View("AdminEditQuestion",model);
    }
    [HttpPost]
    public IActionResult AdminEditQuestion(CourseProperty.QuestionProperty questionProperty) 
    {
        if (ModelState.IsValid)
        {
            _accountDbContext.Update(questionProperty);
            _accountDbContext.SaveChanges();
        }
        return RedirectToAction("AdminEditExam", new { id = questionProperty.ExamId });
    }
    [HttpGet]
    public IActionResult AdminAddQuestion(int examId)
    {
        return View(new CourseProperty.QuestionProperty { ExamId = examId});
    }
    [HttpPost]
    public IActionResult AdminAddQuestion(CourseProperty.QuestionProperty questionProperty)
    {
        if (ModelState.IsValid)
        {
            _accountDbContext.Update(questionProperty);
            _accountDbContext.SaveChanges();
            return RedirectToAction("AdminEditExam", new { id = questionProperty.ExamId });
        }
        return View(questionProperty);
    }
}
