using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Models;
using WebApplication4.Repositories;
using WebApplication4.Repository;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;


namespace WebApplication4.Controllers
{
    [NoCache]
    public class AgentController : Controller
    {


        private readonly AgentRepository _agentRepository = new AgentRepository();
        public ActionResult SignIn()
        {
           
            return View();
        }

        // POST: /Agent/SignIn
        [HttpPost]
        public ActionResult SignIn(SignInModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isValidAgent = _agentRepository.ValidateAgent(model);
                    if (isValidAgent)
                    {
                        // Retrieve the username and agent ID
                        string username = model.Username;
                        int agentId = _agentRepository.GetAgentIdByUsername(username);
                        TempData["AgentID"]=agentId;

                        // Store the username and agent ID in session
                        Session["Username"] = username;
                        Session["AgentID"] = agentId;

                        ViewBag.Message = "Login successful!";
                        return RedirectToAction("Index","Agent");
                    }
                    else
                    {
                        // Invalid credentials
                        ViewBag.Message = "Invalid username or password.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(model);
        }

        // GET: /Agent/Index
        public ActionResult Index()
        {
      
            try
            {
                int agentId = GetCurrentAgentId();
                var agentModel = _agentRepository.GetAgentDetails(agentId);
                return View(agentModel);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("SignIn");
            }
        }

        // GET: Agent/CreatePropertyListing
        public ActionResult CreatePropertyListing()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePropertyListing(PropertyListing model)
        {
            if (ModelState.IsValid)
            {
                // Check if Photo is null, if so, set PhotoBase64 to null
                if (model.Photo == null)
                {
                    model.PhotoBase64 = null;
                }
                else if (model.Photo.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(model.Photo.InputStream))
                    {
                        byte[] fileBytes = binaryReader.ReadBytes(model.Photo.ContentLength);
                        model.PhotoBase64 = Convert.ToBase64String(fileBytes);
                    }
                }

                try
                {
                    // Assume GetCurrentAgentID() is a method that retrieves the logged-in agent's ID
                    int agentID = GetCurrentAgentId();
                    _agentRepository.CreatePropertyListing(model, agentID);

                    ViewBag.Message = "Property listing created successfully!";
                    return RedirectToAction("CreatePropertyListingSuccess");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }

            return View(model);
        }

        public ActionResult CreatePropertyListingSuccess()
        {
            return View();
        }
        public ActionResult ViewOwnPropertyListings()
        {
            // Assume the agent ID is retrieved from the session or authentication context
            int agentID = GetCurrentAgentId();

            var properties = _agentRepository.GetPropertyListingsByAgent(agentID);
            return View(properties);
        }
        public ActionResult Edit(int id)
        {
            try
            {
                PropertyListing listing = _agentRepository.GetPropertyListingById(id);
                if (listing == null)
                {
                    return HttpNotFound();
                }
                return View(listing);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View(new PropertyListing());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PropertyListing model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the PhotoBase64 property with the new photo data
                    if (model.Photo != null && model.Photo.ContentLength > 0)
                    {
                        using (var binaryReader = new BinaryReader(model.Photo.InputStream))
                        {
                            byte[] fileBytes = binaryReader.ReadBytes(model.Photo.ContentLength);
                            model.PhotoBase64 = Convert.ToBase64String(fileBytes);
                        }
                    }

                    _agentRepository.UpdatePropertyListing(model);
                    return RedirectToAction("ViewOwnPropertyListings");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(model);
        }

        //get
        // GET: Delete
        public ActionResult Delete(int id)
        {
            try
            {
                PropertyListing listing = _agentRepository.GetPropertyListingById(id);
                if (listing == null)
                {
                    return HttpNotFound();
                }

                // Debug or log the details of the retrieved listing
                System.Diagnostics.Debug.WriteLine($"Retrieved Listing: {listing.Title}");

                return View(listing);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View(new PropertyListing()); // Return a new instance with default values
            }
        }



        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                bool isDeleted = _agentRepository.DeletePropertyListing(id);
                if (isDeleted)
                {
                    return RedirectToAction("ViewOwnPropertyListings");
                }
                else
                {
                    ViewBag.Message = "An error occurred while deleting the property.";
                    return View("Delete");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Delete");
            }
        }

        public ActionResult ViewVisitSchedules()
        {
            try
            {
                int agentId = GetCurrentAgentId(); // Assume this method gets the logged-in agent's ID
                var visitSchedules = _agentRepository.GetVisitSchedulesForAgent(agentId);
                return View(visitSchedules);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        [HttpGet]
      
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangeAgentPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session["AgentID"] != null)
                    {
                        int agentId = (int)Session["AgentID"];
                        string hashedOldPassword = PasswordHasher.HashPassword(model.OldPassword);
                        string hashedNewPassword = PasswordHasher.HashPassword(model.NewPassword);

                        Debug.WriteLine($"Hashed Old Password: {hashedOldPassword}");
                        Debug.WriteLine($"Hashed New Password: {hashedNewPassword}");

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            using (SqlCommand command = new SqlCommand("sp_ChangeAgentPassword", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@AgentID", agentId);
                                command.Parameters.AddWithValue("@OldPassword", hashedOldPassword);
                                command.Parameters.AddWithValue("@NewPassword", hashedNewPassword);

                                connection.Open();

                                // Log query execution details
                                Debug.WriteLine($"Executing stored procedure with AgentID: {agentId}");

                                int result = (int)command.ExecuteScalar();

                                // Log result of the stored procedure
                                Debug.WriteLine($"Stored Procedure Result: {result}");

                                if (result == 1)
                                {
                                    ViewBag.Message = "Password changed successfully!";
                                    return RedirectToAction("Signin");
                                }
                                else
                                {
                                    ViewBag.Message = "Password change failed. Please ensure the old password is correct.";
                                }
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Message = "User is not authenticated.";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred: {ex.Message}");
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(model);
        }



        public ActionResult Logout()
        {
            if (Session["Username"] != null || Session["AgentID"] != null)
            {
                Session.Clear();
                Session.Abandon(); // Clears all session data
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
            }
            return RedirectToAction("SignIn","Agent");
        }

        public static class PasswordHasher
        {
            public static string HashPassword(string password)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }

        public int GetCurrentAgentId()
        {
            if (Session["AgentId"] != null)
            {
                return (int)Session["AgentId"];
            }
            else
            {
                throw new InvalidOperationException("Agent is not logged in.");


            }
        }
    }
    }