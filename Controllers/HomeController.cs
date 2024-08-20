using System.Data.SqlClient;
using System.Data;
using System;
using System.Web.Mvc;
using WebApplication4.Models;
using WebApplication4.Repositories;


namespace WebApplication4.Controllers
{
    [NoCache]
    public class HomeController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();

        public ActionResult Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userModel = _userRepository.GetUserDetails(userId);
                // User is authenticated, render user-specific navbar
               
            }
            catch (InvalidOperationException)
            {
                // User is not authenticated, render default navbar
                
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Learn more about our Real Estate Portal.";
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
           
            return View();
        }

        [HttpPost]
        public ActionResult Contact(Contact model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand("sp_InsertContactMessage", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@Name", model.Name);
                            command.Parameters.AddWithValue("@Email", model.Email);
                            command.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                            command.Parameters.AddWithValue("@Message", model.Message);

                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }

                    ViewBag.Message = "Thank you for contacting us. We will get back to you shortly.";
                    model = new Contact();  // Clear form data
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }

            return View(new Contact());
        }
        public int GetCurrentUserId()
        {
            if (Session["UserId"] != null)
            {
                return (int)Session["UserId"];
            }
            else
            {
                throw new InvalidOperationException("User is not logged in.");


            }
        }
    }
}