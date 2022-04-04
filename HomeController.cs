using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebbiSkools_Ltd.Models;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace WebbiSkools.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        const string SessionUsername = "_Username";
        const string SessionPassword = "_Password";
        const string SessionAuth = "_Auth";
        const string SessionQuizID = "_QuizID";
        const string SessionQuizName = "_QuizName";

        public IActionResult Index()
        {
            HttpContext.Session.SetInt32(SessionAuth, 0);
            return View();
        }

        public IActionResult Home()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) > 0)
            {
                ViewBag.Username = HttpContext.Session.GetString(SessionUsername);
                return View();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login(IFormCollection formCollection)
        {
            string Username = formCollection["Username"];
            string Password = formCollection["Password"];

            var queryParameters = new DynamicParameters();
            queryParameters.Add("@Username", Username);
            queryParameters.Add("@Password", Password);
            List<User> user = new List<User>();
            using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
            {
                string StoredProc = "dbo.GetUser";
                user = db.Query<User>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
            }

            foreach (var u in user)
            {
                if (u.ResponseMessage == "User successfully logged in")
                {
                    HttpContext.Session.SetString(SessionUsername, Username);
                    HttpContext.Session.SetString(SessionPassword, Password);
                    HttpContext.Session.SetInt32(SessionAuth, u.RoleID);
                    return RedirectToAction("Home");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult CreateQuiz()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateNewQuiz(IFormCollection formCollection)
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                string quizName = formCollection["QuizName"];

                var queryParameters = new DynamicParameters();
                queryParameters.Add("@QuizName", quizName);
                List<Quiz> response = new List<Quiz>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.createQuiz";
                    response = db.Query<Quiz>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
                }

                bool quizCreatedSuccessfully = (response.Count > 0);

                if (quizCreatedSuccessfully == true)
                {
                    foreach (Quiz quizAttribute in response)
                    {
                        int quizID = quizAttribute.QuizID;
                        HttpContext.Session.SetInt32(SessionQuizID, quizID);
                        HttpContext.Session.SetString(SessionQuizName, quizName);

                        return RedirectToAction("QuestionOne");
                    }
                }
                return RedirectToAction("Error");
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateQuestionSet(IFormCollection formCollection)
        {
            string stringQuestionID = formCollection["QuestionID"];
            int questionID = int.Parse(stringQuestionID);
            string questionText = formCollection["QuestionText"];
            string answerOne = formCollection["AnswerOne"];
            string answerTwo = formCollection["AnswerTwo"];
            string answerThree = formCollection["AnswerThree"];
            string answerFour = formCollection["AnswerFour"];
            string answerFive = formCollection["AnswerFive"];

            bool questionTextExistsUnder50Char = (questionText.Count() > 0 && questionText.Count() < 50);
            bool answerOneExistsUnder250Char = (answerOne.Count() > 0 && answerOne.Count() < 250);
            bool answerTwoExistsUnder250Char = (answerTwo.Count() > 0 && answerTwo.Count() < 250);
            bool answerThreeExistsUnder250Char = (answerThree.Count() > 0 && answerThree.Count() < 250);

            if (questionTextExistsUnder50Char == true && answerOneExistsUnder250Char == true && answerTwoExistsUnder250Char == true && answerThreeExistsUnder250Char == true)
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@QuizID", HttpContext.Session.GetInt32(SessionQuizID));
                queryParameters.Add("@QuestionText", questionText);
                queryParameters.Add("@QuestionID", questionID);
                queryParameters.Add("@AnswerOne", answerOne);
                queryParameters.Add("@AnswerTwo", answerTwo);
                queryParameters.Add("@AnswerThree", answerThree);
                queryParameters.Add("@AnswerFour", answerFour);
                queryParameters.Add("@AnswerFive", answerFive);
                List<QuestionSetCreated> response = new List<QuestionSetCreated>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.updQuestionSet";
                    response = db.Query<QuestionSetCreated>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
                }

                foreach (QuestionSetCreated id in response)
                {
                    int responseQuizID = id.QuestionID;

                    return responseQuizID switch
                    {
                        1 => RedirectToAction("QuestionTwo"),
                        2 => RedirectToAction("QuestionThree"),
                        3 => RedirectToAction("QuestionFour"),
                        4 => RedirectToAction("QuestionFive"),
                        5 => RedirectToAction("QuestionSix"),
                        6 => RedirectToAction("QuestionSeven"),
                        7 => RedirectToAction("QuestionEight"),
                        8 => RedirectToAction("QuestionNine"),
                        9 => RedirectToAction("QuestionTen"),
                        _ => RedirectToAction("QuizComplete")
                    };
                }
            }

            return RedirectToAction("ErrorQuestionCreation");
        }

        public ActionResult QuestionOne()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionTwo()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionThree()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionFour()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionFive()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionSix()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionSeven()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionEight()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionNine()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult QuestionTen()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        [HttpPatch]
        public ActionResult QuizCompleted()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                ViewBag.Username = HttpContext.Session.GetString(SessionUsername);
                ViewBag.QuizName = HttpContext.Session.GetString(SessionQuizName);

                var queryParameters = new DynamicParameters();
                queryParameters.Add("@QuizID", HttpContext.Session.GetInt32(SessionQuizID));
                List<QuizCompleted> response = new List<QuizCompleted>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.updQuizCompleted";
                    response = db.Query<QuizCompleted>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
                }

                bool quizCompletedSuccessfully = (response.Count() > 0);

                if (quizCompletedSuccessfully == true)
                {
                    return View();
                }
                return RedirectToAction("ErrorQuestionCreation");
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult EditQuizzes()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                HttpContext.Session.SetString(SessionQuizID, "_QuizID");
                HttpContext.Session.SetString(SessionQuizName, "_QuizName");

                List<QuizView> response = new List<QuizView>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.GetQuizView";
                    response = db.Query<QuizView>(StoredProc, commandType: CommandType.StoredProcedure).ToList();
                }
                var quizModel = response.GroupBy(item => item.QuizName).ToArray();
                return View(quizModel);
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 2)
            {
                return RedirectToAction("ViewQuizzes");
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 1)
            {
                return RedirectToAction("RestrictedQuizzes");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ViewQuizzes()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 2)
            {
                HttpContext.Session.SetString(SessionQuizID, "_QuizID");
                HttpContext.Session.SetString(SessionQuizName, "_QuizName");

                List<QuizView> response = new List<QuizView>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.GetQuizView";
                    response = db.Query<QuizView>(StoredProc, commandType: CommandType.StoredProcedure).ToList();
                }
                var quizModel = response.GroupBy(item => item.QuizName).ToArray();
                return View(quizModel);
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return RedirectToAction("EditQuizzes");
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 1)
            {
                return RedirectToAction("RestrictedQuizzes");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult RestrictedQuizzes()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 1)
            {
                HttpContext.Session.SetString(SessionQuizID, "_QuizID");
                HttpContext.Session.SetString(SessionQuizName, "_QuizName");

                List<QuizView> response = new List<QuizView>();
                using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
                {
                    string StoredProc = "dbo.GetQuizView";
                    response = db.Query<QuizView>(StoredProc, commandType: CommandType.StoredProcedure).ToList();
                }
                var quizModel = response.GroupBy(item => item.QuizName).ToArray();
                return View(quizModel);
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return RedirectToAction("EditQuizzes");
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) == 2)
            {
                return RedirectToAction("ViewQuizzes");
            }

            return RedirectToAction("Index");
        }

        [HttpDelete]
        public IActionResult RemoveQuiz(QuizView quizView)
        {
            int quizID = quizView.QuizID;
            var queryParameters = new DynamicParameters();
            queryParameters.Add("@QuizID", quizID);
            List<RemovedItem> response = new List<RemovedItem>();
            using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
            {
                string StoredProc = "dbo.delQuiz";
                response = db.Query<RemovedItem>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
            }
            foreach (var removedItem in response)
            {
                bool removedItemSuccessfully = (removedItem.ItemRemoved == 1);
                if (removedItemSuccessfully == true)
                {
                    return View(response);
                }

            }

            return View(response);
        }

        [HttpDelete]
        public IActionResult RemoveQuestion(QuizView quizView)
        {
            int uniqueSetID = quizView.UniqueSetID;
            int itemRemoved = 0;
            var queryParameters = new DynamicParameters();
            queryParameters.Add("@UniqueSetID", uniqueSetID);
            queryParameters.Add("@ItemRemoved", itemRemoved);
            List<RemovedItem> response = new List<RemovedItem>();
            using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
            {
                string StoredProc = "dbo.delQuestion";
                response = db.Query<RemovedItem>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
            }

            return RedirectToAction("EditQuizzes");
        }

        [HttpPatch]
        public IActionResult RemoveAnswer(QuizView quizView)
        {
            int uniqueSetID = quizView.UniqueSetID;
            string answerFour = quizView.AnswerFour;
            string answerFive = quizView.AnswerFive;

            var queryParameters = new DynamicParameters();
            queryParameters.Add("@UniqueSetID", uniqueSetID);
            queryParameters.Add("@AnswerFour", answerFour);
            queryParameters.Add("@AnswerFive", answerFive);
            List<RemovedItem> response = new List<RemovedItem>();
            using (IDbConnection db = new SqlConnection(@"Data Source = (LocalDb)\AdamsQuiz; Initial Catalog = QuizDB; Integrated Security = True"))
            {
                string StoredProc = "dbo.updAnswerToNull";
                response = db.Query<RemovedItem>(StoredProc, queryParameters, commandType: CommandType.StoredProcedure).ToList();
            }

            return RedirectToAction("EditQuizzes");
        }

        public ActionResult ErrorQuestionCreation()
        {
            if (HttpContext.Session.GetInt32(SessionAuth) == 3)
            {
                return View();
            }
            else if (HttpContext.Session.GetInt32(SessionAuth) > 0 && HttpContext.Session.GetInt32(SessionAuth) < 3)
            {
                return RedirectToAction("Home");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
