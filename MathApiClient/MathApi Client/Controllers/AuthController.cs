using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MathApi_Client.Models;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MathApi_Client.Controllers;

public class AuthController : Controller
{
    // Docker service name and internal port
    private static HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://math-api:8080/"), 
    };

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(LoginModel login)
    {
        StringContent jsonContent = new(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json"); 
        HttpResponseMessage response = await httpClient.PostAsync("api/Auth/Register", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            AuthResponse? deserialisedResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
            
            // Save to Session so the MathController can access them
            HttpContext.Session.SetString("currentUser", deserialisedResponse.UserId);
            HttpContext.Session.SetString("MathJWT", deserialisedResponse.Token);
            
            return RedirectToAction("Calculate", "Math");                
        }
        
        ViewBag.Result = "Registration failed. Please try again.";
        return View();
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel login)
    {
        StringContent jsonContent = new(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json"); 
        HttpResponseMessage response = await httpClient.PostAsync("api/Auth/Login", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            AuthResponse? deserialisedResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
            
            HttpContext.Session.SetString("currentUser", deserialisedResponse.UserId);
            HttpContext.Session.SetString("MathJWT", deserialisedResponse.Token);
            
            return RedirectToAction("Calculate", "Math");                
        }

        ViewBag.Result = "Invalid Login Details";
        return View();
    }

    [HttpGet]
    public IActionResult LogOut()
    {
        HttpContext.Session.Remove("currentUser");
        HttpContext.Session.Remove("MathJWT"); 
        return RedirectToAction("Login");
    }
}