using KVB.Models;
using KVB.Models.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;



namespace KVB.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }


  
    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    public IActionResult login()
    {
        return View();
    }

    [HttpPost]
   
    public IActionResult login(login model)

    {

        if (ModelState.IsValid)

        {
            if (!string.IsNullOrEmpty({ model.name) && string.IsNullOrEmpty.( model.password)) {

                    if (IsValidUser(model.name, model.password))
                    {
                        HttpContext.Session.SetString("loggedIn","true");
                        return RedirectToAction("dashboard");

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid username or password.");
                        return RedirectToAction("login");
                    }
                }
            } }

        return View();
    }
    private bool IsValidUser(string name, string password)

    {
        string query = $"SELECT * FROM Kvblogin where name =  '{name}' and Password = '{password}'";
        DataTable dataTable = dbhelper.ExecuteQuery(query);
        if (dataTable.Rows.Count > 0)
        {
            return true;

        }

        return false;

    }

    [HttpPost]
    public IActionResult Register(credentials cr)
    {
        if (ModelState.IsValid)
        {
            string query = $"insert into [Kvb].[dbo].[Kvblogin] (name,password,emailid,phone ) values('{cr.name}','{cr.password}','{cr.emailid}','{cr.phone}') ";
            DataTable dataTable = dbhelper.ExecuteQuery(query);
            return RedirectToAction("login");
        }
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }
   
    public IActionResult Dashboard()
    {
        string query = "SELECT SUM(Enrollment) FROM DataReport WHERE Enrollment > 0";
        DataTable d = dbhelper.ExecuteQuery(query);
        string query1 = "SELECT SUM(TotalSuccess) FROM DataReport WHERE TotalSuccess > 0";
        DataTable d1 = dbhelper.ExecuteQuery(query1);
        string query2 = "SELECT Count(Verification1) FROM Verification WHERE Verification1 < 70";
        DataTable d2 = dbhelper.ExecuteQuery(query2);
        string query3 = "SELECT Sum(EnrollmentFailure) FROM DataReport WHERE EnrollmentFailure > 0";
        DataTable d3 = dbhelper.ExecuteQuery(query3);
        string query4 = "SELECT Sum(LoginFailed) FROM DataReport WHERE LoginFailed > 0";
        DataTable d4 = dbhelper.ExecuteQuery(query4);
        int enrollmentCount = Convert.ToInt32(d.Rows[0][0]);
        int verificationCount = Convert.ToInt32(d1.Rows[0][0]);
        int verificationfailure = Convert.ToInt32(d2.Rows[0][0]);
        int enrollmentfailure = Convert.ToInt32(d3.Rows[0][0]);
        int FalseAcceptance = Convert.ToInt32(d4.Rows[0][0]);
        List<DataPoint> dataPoints = new List<DataPoint>();
        dataPoints.Add(new DataPoint("Enrollment", enrollmentCount));
        dataPoints.Add(new DataPoint("Verification", verificationCount));
        dataPoints.Add(new DataPoint("Verification Failed", verificationfailure));
        dataPoints.Add(new DataPoint("False Acceptance", FalseAcceptance));
        ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
        ViewBag.Echart = enrollmentCount;
        ViewBag.Vchart = verificationCount;
        ViewBag.VFchart = verificationfailure;
        ViewBag.EFchart = enrollmentfailure;
        ViewBag.FAchart = FalseAcceptance;
        return View();
    }

    public IActionResult DataReport()
    {
        DataTable dataTable = new DataTable();
        List<DataReport> sflist = new List<DataReport>();
        string query = " Select * from  [Kvb].[dbo].[DataReport]";

        dataTable = DbHelper.ExecuteQuery(query);
        foreach (DataRow ds in dataTable.Rows)
        {
            DataReport vr = new DataReport();
            vr.Date = (string)ds["Date"];
            vr.Enrollment = (int)ds["Enrollment"];
            vr.EnrollmentFailure = (int)ds["EnrollmentFailure"];
            vr.LoginAttempt = (string)ds["LoginAttempt"];
            vr.LoginSuccessthroughDirectVoiceAuthentication = (string)ds["LoginSuccessthroughDirectVoiceAuthentication"];
            vr.LoginSuccessThroughAIModel = (string)ds["LoginSuccessThroughAIModel"];
            vr.TotalSuccess = (int)ds["TotalSuccess"];
            vr.LoginFailed = (int)ds["LoginFailed"];
            sflist.Add(vr);
        }
        return View(sflist);

    }
    public IActionResult Enrollment()
    {
        DataTable dataTable = new DataTable();
        List<Enrollment> sflist = new List<Enrollment>();
        string query = " Select * from  [Kvb].[dbo].[Enrollment]";
        dataTable = DbHelper.ExecuteQuery(query);
        foreach (DataRow ds in dataTable.Rows)
        {
            Enrollment vr = new Enrollment();
            vr.date = (string)ds["DATE"];
            vr.customerId = (string)ds["CustomerId"];
            vr.uniqueId = (string)ds["UniqueId"];
            vr.EnrollmentId = (string)ds["UniqueId"];
            sflist.Add(vr);
        }
            return View(sflist);
    }
    [HttpPost]
    public IActionResult Enrollment(string inputStartDate, string inputEndDate)
    {
        DateTime start = Convert.ToDateTime(inputStartDate);
        DateTime end = Convert.ToDateTime(inputEndDate);
        string sdate = start.ToString("dd'-'MM'-'yyyy");
        string edate = end.ToString("dd'-'MM'-'yyyy");
        DataTable dataTable = new DataTable();
        List<Enrollment> sflist = new List<Enrollment>();
        string query = $"select * from Enrollment where Date between '{sdate}'and'{edate}'";
        dataTable = DbHelper.ExecuteQuery(query);
        foreach (DataRow ds in dataTable.Rows)
        {
            Enrollment vr = new Enrollment();
            vr.date = (string)ds["DATE"];
            vr.customerId = (string)ds["CustomerId"];
            vr.uniqueId = (string)ds["UniqueId"];
            vr.EnrollmentId = (string)ds["UniqueId"];
            sflist.Add(vr);
        }
        return View(sflist);
    }

    public IActionResult Verification()
    {
        DataTable dataTable = new DataTable();
        List<verification> sflist = new List<verification>();
        string query = " Select * from  [Kvb].[dbo].[Verification]";
        dataTable = DbHelper.ExecuteQuery(query);
        foreach (DataRow ds in dataTable.Rows)
        {
            verification vr = new verification();
            vr.date = (string)ds["DATE"];
            vr.customerId = (string)ds["CustomerId"];
            vr.uniqueId = (string)ds["UniqueId"];
            vr.Verification1 = (double)ds["Verification1"];
            vr.captchaId = (string)ds["CaptchaId"];
            vr.captchaReturn = (string)ds["CaptchaReturn"];
            vr.digit = (string)ds["Digit"];
            vr.Verification2 = (string)ds["Verification2"];
            sflist.Add(vr);
        }
        return View(sflist);
}
    [HttpPost]
    public IActionResult Verification(string inputStartDate, string inputEndDate)
    { 
    DateTime start=Convert.ToDateTime(inputStartDate);
    DateTime end=Convert.ToDateTime(inputEndDate);
    string sdate = start.ToString("dd'-'MM'-'yyyy");
    string edate = end.ToString("dd'-'MM'-'yyyy");
    DataTable dt=new DataTable();
    List<verification> vl = new List<verification>();
    string query = $"select * from Verification where Date between '{sdate }'and'{edate}'";
    dt= DbHelper.ExecuteQuery(query);
    foreach(DataRow ds in dt.Rows)
        {
            verification vr = new verification();
            vr.date = (string)ds["DATE"];
            vr.customerId = (string)ds["CustomerId"];
            vr.uniqueId = (string)ds["UniqueId"];
            vr.Verification1 = (double)ds["Verification1"];
            vr.captchaId = (string)ds["CaptchaId"];
            vr.captchaReturn = (string)ds["CaptchaReturn"];
            vr.digit = (string)ds["Digit"];
            vr.Verification2 = (string)ds["Verification2"];
            vl.Add(vr);
        }
        return View(vl);
    }

        [HttpPost]
    public IActionResult DataReport(string inputStartDate,string inputEndDate) {
        DateTime StartDate=Convert.ToDateTime(inputStartDate);
        DateTime EndDate = Convert.ToDateTime(inputEndDate);
        string Sdate = StartDate.ToString("dd'-'MM'-'yyyy");
        string Edate = EndDate.ToString("dd'-'MM'-'yyyy");
        string query = $" SELECT *  from DataReport WHERE Date BETWEEN '{Sdate}' AND '{Edate}'";
        List<DataReport> sflist=new List<DataReport>();
        DataTable dt= DbHelper.ExecuteQuery(query);
        
        foreach (DataRow ds in dt.Rows)
        {
            DataReport vr = new DataReport();
            vr.Date = (string)ds["Date"];
            vr.Enrollment = (int)ds["Enrollment"];
            vr.EnrollmentFailure = (int)ds["EnrollmentFailure"];
            vr.LoginAttempt = (string)ds["LoginAttempt"];
            vr.LoginSuccessthroughDirectVoiceAuthentication = (string)ds["LoginSuccessthroughDirectVoiceAuthentication"];
            vr.LoginSuccessThroughAIModel = (string)ds["LoginSuccessThroughAIModel"];
            vr.TotalSuccess = (int)ds["TotalSuccess"];
            vr.LoginFailed = (int)ds["LoginFailed"];
            sflist.Add(vr);
        }
        
        return View(sflist);    
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
}