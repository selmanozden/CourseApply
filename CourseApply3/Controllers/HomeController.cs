using CourseApply3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Diagnostics;
using X.PagedList;
using PagedList;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static CourseApply3.Models.CourseProperty;
using System.Drawing;




namespace CourseApply3.Controllers
{
    public class HomeController : Controller
    {
        private readonly CourseDbContext _context;


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, CourseDbContext courseDbContext)
        {
            _logger = logger;
            _context = courseDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "UserPolicy")]
        public IActionResult Index2()
        {
            return View("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize(Policy = "UserPolicy")]
        public IActionResult Exam()
        {
            var exams = _context.Exams
                                .Include(e => e.Questions)
                                .ToList();
            return View(exams);
        }
        public IActionResult UserApply()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserApply(CourseProperty.UserProperty model, IFormFile profilePicture)
        {
            if (ModelState.IsValid)
            {
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await profilePicture.CopyToAsync(memoryStream);
                        model.ProfilePictureData = memoryStream.ToArray();
                        model.ProfilePictureMimeType = profilePicture.ContentType;
                    }
                }

                _context.Users.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            if ((bool)User?.Identity?.IsAuthenticated!)
            {

                return RedirectToAction("Index");
            }


            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.UsersId.ToString()),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "UserScheme");
                    var principal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync("UserScheme", principal);

                    return RedirectToAction("Index2");
                }
                ViewBag.Message = "Geçersiz Giriþ";
            }

            return View();
        }
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("UserScheme");
            return RedirectToAction("Login");
        }
        [HttpGet]
        [Authorize(Policy = "UserPoLicy")]
        public IActionResult Questions([FromQuery] int examId, [FromQuery] int questionsId, [FromQuery] string answer, [FromQuery] int page = 1)
        {
            var questions = _context.Exams.Include(x => x.Questions).FirstOrDefault(a => a.ExamId == examId);

            int questionCount = questions.Questions.Count();

            if (questionsId != 0) 
            {
                var answerProperty = new AnswerProperty
                {
                    QuestionId = questionsId,
                    Answer = answer
                };
                _context.Answers.Add(answerProperty);
                _context.SaveChanges();
            }
            if (questions == null)
                {
                return Ok(examId);
                }
            if (questionCount < page) 
            {
               return RedirectToAction("FinishExam");  
            }
            var pagedquestion = questions.Questions.ToPagedList(page, 1);
            var model = Tuple.Create(questions, pagedquestion);
            return View(model);

        }

        [Authorize(Policy = "UserPolicy")]
        public IActionResult FinishExam()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            int userIdInt = Convert.ToInt32(userId);

            // Kullanýcýnýn cevaplarýný al
            var allAnswers = _context.Answers.ToList();

            if (!allAnswers.Any())
            {
                return NotFound("Cevap bulunamadý.");
            }

            // Her bir sorunun doðru cevabýný al
            var questionIds = allAnswers.Select(a => a.QuestionId).Distinct().ToList();
            var questions = _context.Questions
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToList();

            // Puan hesaplama
            int score = 0;
            int totalQuestions = questions.Count();
            int point = 100 / totalQuestions;

            foreach (var answer in allAnswers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question != null && answer.Answer == question.CorrectAnswer)
                {
                    score += point;
                }
            }

            var userInformation = new UserInformation
            {
                Score = score,
                ExamId = questions.First().ExamId, 
                UserId = userIdInt
            };

            _context.Answers.RemoveRange(allAnswers);
            _context.UserInformations.Add(userInformation);
            _context.SaveChanges();

            return View("Congratulations", userInformation);
        }




        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); 
            }

            var user = await _context.Users.FindAsync(userId);
            var userScores = await _context.UserInformations
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Exam)
                .ToListAsync();

            ViewData["User"] = user;
            ViewData["UserScores"] = userScores;

            return View();
        }

    }
}
