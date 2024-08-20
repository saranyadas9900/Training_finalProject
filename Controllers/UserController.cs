using System;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Web.Mvc;
using WebApplication4.Models;
using System.Web;

using WebApplication4.Repositories;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Configuration;

using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;


namespace WebApplication4.Controllers
{
    [NoCache]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();



       

        public ActionResult Index()
        {
            try
            {


                var userId = GetCurrentUserId();
                var userModel = _userRepository.GetUserDetails(userId);
                return View(userModel);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }

        
        public ActionResult ViewPropertyListings()
        {
            try
            {
                var properties = _userRepository.GetPropertyListings();
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                return View(properties);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }
        
        public ActionResult ScheduleVisit(int id)
        {
            // Store the PropertyId in TempData for later use
            TempData["PropertyId"] = id;
            // Create and pass an empty VisitSchedule model to the view
            return View("ScheduleVisit", new VisitSchedule());
        }
        public ActionResult VisitSuccess()
        {
            return View();
        }

       
        [HttpPost]
        public ActionResult ScheduleVisit(VisitSchedule model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int propertyId = (int)TempData["PropertyId"];
                    int userId = GetCurrentUserId();
                    var property = _userRepository.GetPropertyListingById(propertyId);

                    // Check if the visit time is already taken for any property managed by the same agent
                    if (_userRepository.IsVisitTimeTaken(property.AgentID.GetValueOrDefault(), model.VisitDate))
                    {
                        ViewBag.Message = "The selected time slot is already taken . Please choose a different time.";
                        return View(model);
                    }

                    _userRepository.RemoveExistingVisit(propertyId, userId);

                    _userRepository.ScheduleVisit(propertyId, userId, model.VisitDate);
                    return RedirectToAction("VisitSuccess");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(model);
        }


       
        [HttpGet]
        public ActionResult SearchProperties()
        {
            return View();
        }
     
        [HttpPost]
        public ActionResult SearchProperties(string state, string city, int? minBedrooms, int? minBathrooms, decimal? minPrice, decimal? maxPrice)
        {
            var properties = _userRepository.SearchProperties(state, city, minBedrooms, minBathrooms, minPrice, maxPrice);
            return View("SearchResults", properties);
        }
     
        [HttpPost]
        public ActionResult AddToFavorites(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                _userRepository.AddToFavorites(userId, id);

                // Use TempData to pass the success message
                TempData["SuccessMessage"] = "Item added to your favorites list!";
                return RedirectToAction("ViewPropertyListings");
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }


    
        public ActionResult ViewFavoriteProperties()
        {
            try
            {
                int userId = GetCurrentUserId();
                List<PropertyListing> favoriteProperties = _userRepository.GetFavoriteProperties(userId);
                return View(favoriteProperties);
            }
            catch (Exception ex)
            {
                // Log the exception details (consider using a logging framework like NLog, Serilog, etc.)
                // For now, you can use a simple approach to display the message
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }
     
        [HttpPost]
        public ActionResult RemoveFromFavorites(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                _userRepository.RemoveFromFavorites(userId, id);
                TempData["SuccessMessage"] = "Property removed from favorites.";
                return RedirectToAction("ViewFavoriteProperties");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ViewFavoriteProperties");
            }
        }



       

        // GET: User/Profile
        public ActionResult ProfileUser()
        {
            // Get the currently logged-in user's ID
            int userId = GetCurrentUserId();
            User user = _userRepository.GetUserDetails(userId);
            return View(user);
        }
       
        public ActionResult EditProfile(int id)
        {
            var user = _userRepository.GetUserDetails(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);  
        }
     
        // Action to handle the form submission for updating the profile
        [HttpPost]
        public ActionResult UpdateProfile(User model)
        {
            if (ModelState.IsValid)
            {
                bool updateSuccess =  _userRepository.UpdateUser(model);  // Call the method you provided
                if (updateSuccess)
                {
                    return RedirectToAction("ProfileUser", new { id = model.UserID });  // Redirect to profile view or another appropriate action
                }
                else
                {
                    ModelState.AddModelError("", "Error updating user details. Please try again.");
                }
            }
            return View("EditProfile", model);  // Return to the edit view with validation errors if any
        }


        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
       
        // GET: User/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View(new ChangeUserPasswordViewModel()); // Ensure this matches the view's expected model type
        }
        
        public ActionResult ChangePassword(ChangeUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session["Username"] != null && Session["UserId"] != null)
                    {
                        string username = (string)Session["Username"];
                        int userId = (int)Session["UserId"];

                        User user = _userRepository.GetUserById(userId);

                        if (user != null)
                        {
                            string hashedOldPassword = PasswordHasher.HashPassword(model.OldPassword);
                            Debug.WriteLine($"Hashed Old Password: {hashedOldPassword}");
                            Debug.WriteLine($"Stored Password: {user.Password}");

                            Console.WriteLine($"Hashed Old Password: {hashedOldPassword}");
                            Console.WriteLine($"Stored Password: {user.Password}");

                            if (hashedOldPassword == user.Password)
                            {
                                string newHashedPassword = PasswordHasher.HashPassword(model.NewPassword);

                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    using (SqlCommand command = new SqlCommand("sp_ChangeUserPassword", connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("@Username", username);
                                        command.Parameters.AddWithValue("@NewPassword", newHashedPassword);
                                        connection.Open();
                                        var result = (int)command.ExecuteScalar();
                                        if (result == 1)
                                        {
                                            ViewBag.Message = "Password changed successfully!";
                                            return RedirectToAction("SignIn", "Auth");
                                            
                                        }
                                        else
                                        {
                                            ViewBag.Message = "Password change failed. Please try again.";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Old password is incorrect.";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "User not found.";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "User is not authenticated.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(model);
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
