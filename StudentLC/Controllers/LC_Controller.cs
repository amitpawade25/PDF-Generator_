using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using StudentInfoApi.Models;

namespace StudentLC.Controllers
{
    public class LcController : Controller
    {
        // GET: student
        public ActionResult Index()
        {
            IEnumerable<Student> student = null;
            StudentDbContext db = new StudentDbContext();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:51096/api/");

                var responseTask = client.GetAsync("Students");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var taskresult = result.Content.ReadAsAsync<IList<Student>>();
                    responseTask.Wait();

                    student = taskresult.Result;
                }
                else
                {
                    student = Enumerable.Empty<Student>();
                    ModelState.AddModelError(string.Empty, "Server Error Please Contact to Admin");
                }
            }
            return View(student);
        }

        [HttpGet]
        public ActionResult create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult create(Student student)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:51096/api/Students");

                var postTask = client.PostAsJsonAsync("students", student);
                postTask.Wait();
                student.DOB.ToString();
                student.AdmissionDate.ToString();
                student.DateOfSchoolLeaving.ToString();
                student.CertificateRecivedDate.ToString();
                var result = postTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError(string.Empty, "Server Error Please contact to admin");

            return View(student);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            Student student = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:51096/api/Students");
                var responseTask = client.GetAsync("Students?Id=" + id.ToString());
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var resultTask = result.Content.ReadAsAsync<Student>();
                    responseTask.Wait();

                    student = resultTask.Result;
                }
            }
            return View(student);
        }
        //-------------------------------------------------------------------------------------------

        public ActionResult Reports(string ReportType,int? id)
        {
           List< Student> student = new List<Student>();
            Student s = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:51096/api/Students");
                var responseTask = client.GetAsync("Students?Id=" + id.ToString());
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var resultTask = result.Content.ReadAsAsync<Student>();
                    responseTask.Wait();

                    s = resultTask.Result;
                    student.Add(s);     
                }
            }
            
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = Server.MapPath("~/Reports/StudentReport.rdlc");
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Name = "StudentInfoDataset";
            reportDataSource.Value = student;
            localReport.DataSources.Add(reportDataSource);
            string reportType = ReportType;
            string mimeType;
            string encoding;
            string fileNameExtension;
            if(reportType == "PDF")
            {
                fileNameExtension = "pdf";
            }
           else  if (reportType == "Word")
            {
                fileNameExtension = "docx";
            }
          else
            {
                fileNameExtension = "jpg";
            }
            string[] streams;
            Warning[] warnings;
            byte[] renderByte;
            renderByte = localReport.Render(reportType, "", out mimeType, out encoding,
                       out fileNameExtension, out streams, out warnings);
            Response.AddHeader("comntent-disposition", "attachment ;filename=student_report." + fileNameExtension);
            return File(renderByte, fileNameExtension);
           
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
    }
}