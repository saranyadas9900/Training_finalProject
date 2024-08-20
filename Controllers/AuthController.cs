using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebApplication4.Models;
using WebApplication4.Repositories;

namespace WebApplication4.Controllers
{
    [NoCache]
    public class AuthController : Controller

    {

        private readonly UserRepository _userRepository = new UserRepository();
       
        public ActionResult SignUp()
        {
            return View();
        }
       
        // POST: SignUp
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   
                    _userRepository.RegisterUser(user);
                    ViewBag.Message = "Registration successful!";
                    return RedirectToAction("SignUpSuccess");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"An error occurred: {ex.Message}";
                }
            }
            return View(user);
        }
      
        // GET: SignUpSuccess
        public ActionResult SignUpSuccess()
        {
            return View();
        }

        // GET: SignIn
        public ActionResult SignIn()
        {

            return View();
        }
       
        [HttpPost]
        public ActionResult SignIn(SignInModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isValidUser = _userRepository.ValidateUser(model);
                    if (isValidUser)
                    {
                        // Retrieve the username and user ID
                        string username = model.Username;
                        int userId = _userRepository.GetUserIdByUsername(model.Username);

                        // Store the username and user ID in session
                        Session["Username"] = username;
                        Session["UserId"] = userId;


                        ViewBag.Message = "Login successful!";
                        return RedirectToAction("Index", "User");
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

        public ActionResult Logout()
        {


            if (Session["Username"] != null || Session["UserId"] != null)
            {
                Session.Clear();
                Session.Abandon(); // Clears all session data
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
            }
            return RedirectToAction("Signin", "Auth");
        
        }



    }
}