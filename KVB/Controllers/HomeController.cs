using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using KVB.Models;
using KVB.Models.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Org.BouncyCastle.Tls.Crypto;
using System.Data;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

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
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost]
    
    public IActionResult login(login model)

    {

        if (ModelState.IsValid)

        {
            if (!string.IsNullOrEmpty(model.name) && !string.IsNullOrEmpty(model.password))
            {

                if (IsValidUser(model.name, model.password))
                {
                    HttpContext.Session.SetString("LoggedIn", "true");
                    return RedirectToAction("dashboard","home");

                }
                else
                {
                    ModelState.AddModelError(string.Empty,"Invalid username or password");
                    //return RedirectToAction("login");
                }

            }
        }
        

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
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("LoggedIn") == "true")
        {
            string[] queries = new string[]
 {
    "SELECT COUNT(*) AS TotalEnrollment FROM Enrollment",
    "SELECT count(Verification1) AS TotalSuccess FROM Verification WHERE Verification1 > 70",
    "SELECT COUNT(Verification1) AS VerificationFailure FROM Verification WHERE Verification1 < 70",
    "SELECT SUM(EnrollmentFailure) AS TotalEnrollmentFailure FROM DataReport WHERE EnrollmentFailure > 0",
    "SELECT SUM(LoginFailed) AS TotalLoginFailed FROM DataReport WHERE LoginFailed > 0"
 };

            object[] results = new object[5]; // Array to store the results

            for (int i = 0; i < queries.Length; i++)
            {
                DataTable dataTable = dbhelper.ExecuteQuery(queries[i]);
                if (dataTable.Rows.Count > 0 && dataTable.Rows[0][0] != DBNull.Value)
                {
                    results[i] = dataTable.Rows[0][0]; // Store the result in the array
                }
                else
                {
                    results[i] = null; // Handle null case if needed
                }
            }

            // Retrieve results from the array
            object totalEnrollment = results[0];
            object totalSuccess = results[1];
            object verificationfail= results[2];
            object totalEnrollmentFailure = results[3];
            object totalLoginFailed = results[4];

            // You can then use these variables as needed

            //        string query = @"
            //SELECT 
            //    (SELECT SUM(Enrollment) FROM DataReport WHERE Enrollment > 0) AS TotalEnrollment,
            //    (SELECT SUM(TotalSuccess) FROM DataReport WHERE TotalSuccess > 0) AS TotalSuccess,
            //    (SELECT COUNT(Verification1) FROM Verification WHERE Verification1 < 70) AS VerificationCount,
            //    (SELECT SUM(EnrollmentFailure) FROM DataReport WHERE EnrollmentFailure > 0) AS TotalEnrollmentFailure,
            //    (SELECT SUM(LoginFailed) FROM DataReport WHERE LoginFailed > 0) AS TotalLoginFailed";
           
            //string query1 = "SELECT SUM(TotalSuccess) FROM DataReport WHERE TotalSuccess > 0";
            //DataTable d1 = dbhelper.ExecuteQuery(query1);
            //string query2 = "SELECT Count(Verification1) FROM Verification WHERE Verification1 < 70";
            //DataTable d2 = dbhelper.ExecuteQuery(query2);
            //string query3 = "SELECT Sum(EnrollmentFailure) FROM DataReport WHERE EnrollmentFailure > 0";
            //DataTable d3 = dbhelper.ExecuteQuery(query3);
            //string query4 = "SELECT Sum(LoginFailed) FROM DataReport WHERE LoginFailed > 0";
            //DataTable d4 = dbhelper.ExecuteQuery(query4);
                 List<DataPoint> dataPoints = new List<DataPoint>();
                dataPoints.Add(new DataPoint("Enrollment", totalEnrollment));
                dataPoints.Add(new DataPoint("Verification", totalSuccess));
                dataPoints.Add(new DataPoint("Verification Failed", verificationfail));
                dataPoints.Add(new DataPoint("False Acceptance", totalLoginFailed));
                ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
                ViewBag.Echart = totalEnrollment;
                ViewBag.Vchart = totalSuccess;
                ViewBag.VFchart = verificationfail;
                ViewBag.EFchart = totalEnrollmentFailure;
                ViewBag.FAchart = totalLoginFailed;

          
            
        }
        else
        {
          return RedirectToAction("login"); 
        }
        return View();
    }

    public IActionResult DataReport()
    {
        if (HttpContext.Session.GetString("LoggedIn") == "true")
        {
            DataTable DataTable = new DataTable();
            List<DataReport> Relist = new List<DataReport>();
            string query1 = "SELECT SUBSTRING(COALESCE(e.DATE, v.DATE), 1, 10) AS UniqueDate,"+" "+
            "COUNT(DISTINCT e.CustomerId) AS EnrollmentCount," +" "+
            "COUNT(DISTINCT v.CustomerId) AS EnrollmentFailure,"+" "+
            "COUNT(DISTINCT CASE WHEN v.Verification1 > 70 THEN 1 ELSE NULL END) AS TotalSuccess,"+" "+
            "COUNT( DISTINCT CASE WHEN v.Verification1 > 70 THEN 1 ELSE NULL END) AS LoginFailed" +" "+
            "FROM Enrollment e" +" "+
            "FULL OUTER JOIN Verification v ON SUBSTRING(e.Date, 1, 10) = SUBSTRING(v.Date, 1, 10)"+" "+
            "GROUP BY SUBSTRING(COALESCE(e.DATE, v.DATE), 1, 10);";


            //string query1 = "merge into datareport as target using (select substring(e.date,1, 10) as date,count(e.customerid) as enrollment," +
            //"count(v.customerid) as enrollmentfailure," + " " +
            //"count(*) as loginattempt," + " " +
            //"count(*) as loginsuccessthroughdirectvoiceauthentication," + " " +
            //"count(*) as loginsuccessthroughaimodel," + " " +
            //"count(*) as totalsuccess," + " " +
            //"count(*) as loginfailed from verification v join enrollment e on e.customerid = v.customerid group by substring(e.date,1, 10), e.customerid, v.customerid ) as source on target.date = source.date" + " " +
            //"when matched then update set" + " " +
            //"target.enrollment = target.enrollment + source.enrollment," + " " +
            //"target.enrollmentfailure = target.enrollmentfailure + source.enrollmentfailure," + " " +
            //"target.loginattempt = target.loginattempt + source.loginattempt," + " " +
            //"target.loginsuccessthroughdirectvoiceauthentication = target.loginsuccessthroughdirectvoiceauthentication + source.loginsuccessthroughdirectvoiceauthentication," + " " +
            //"target.loginsuccessthroughaimodel = target.loginsuccessthroughaimodel + source.loginsuccessthroughaimodel," + " " +
            //"target.totalsuccess = target.totalsuccess + source.totalsuccess," + " " +
            //"target.loginfailed = target.loginfailed + source.loginfailed" + " " +
            //"when not matched then" + " " +
            //"insert(date, enrollment, enrollmentfailure, loginattempt, loginsuccessthroughdirectvoiceauthentication, loginsuccessthroughaimodel, totalsuccess, loginfailed)" + " " +
            //"values( source.date, source.enrollment, source.enrollmentfailure, source.loginattempt, source.loginsuccessthroughdirectvoiceauthentication, source.loginsuccessthroughaimodel, source.totalsuccess, source.loginfailed);";
            DbHelper.ExecuteQuery(query1);
            DataTable = DbHelper.ExecuteQuery(query1);
            //foreach (DataRow ds in dataTable.Rows)
            //{
            //    DataReport vr = new DataReport();
            //    vr.Date = (string)ds["Date"];
            //    vr.Enrollment = (int)ds["Enrollment"];
            //    vr.EnrollmentFailure = (int)ds["EnrollmentFailure"];
            //    vr.TotalSuccess = (int)ds["TotalSuccess"];
            //    vr.LoginFailed = (int)ds["LoginFailed"];
            //    Relist.Add(ds);
            //}
          return View(DataTable);
        }
        else
        {
        return RedirectToAction("login");
        }
    }
    public IActionResult Enrollment()
    {
        if (HttpContext.Session.GetString("LoggedIn") == "true")
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
        else
        {
            return RedirectToAction("login");
        }
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
        if (HttpContext.Session.GetString("LoggedIn") == "true")
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
        else
        {
        return RedirectToAction("login");
        }
        
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
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("login");
    }

    //private void PdfGen()
    //{
    //    Document doc = new Document(PageSize.A4);

    //    try
    //    {

    //        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(
    //          Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Report.pdf", FileMode.Create));
    //        doc.Open();
    //        PdfPTable tbl = new PdfPTable(4);

    //        DataTable dt = DbHelper.ExecuteQuery("select * from Employee");
    //        foreach (DataColumn c in dt.Columns)
    //        {
    //            tbl.AddCell(new Phrase(c.Caption));
    //        }
    //        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
    //        var fnt = new Font(bf, 13.0f, 1, BaseColor.BLUE);
    //        foreach (DataRow row in dt.Rows)
    //        {

    //            tbl.AddCell(new Phrase(row[0].ToString()));
    //            tbl.AddCell(new Phrase(row[1].ToString()));
    //            tbl.AddCell(new Phrase(row[2].ToString()));
    //            tbl.AddCell(new Phrase(row[3].ToString(), fnt));
    //        }
    //        doc.Add(tbl);
    //        doc.Close();
    //        System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Report.pdf");
    //    }
    //    catch (Exception ae)
    //    {
    //        throw ae;
    //    }
    //}
    public IActionResult toggler()
    {
        return View();
    }
    public IActionResult ssx()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult processform()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}