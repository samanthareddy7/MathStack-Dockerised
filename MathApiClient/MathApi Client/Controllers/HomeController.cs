using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MathApi_Client.Models;

namespace MathApi_Client.Controllers;

public class HomeController : Controller
{
  public IActionResult Index()
{
    // If they aren't logged in, send them straight to the Auth controller
    if (string.IsNullOrEmpty(HttpContext.Session.GetString("MathJWT")))
    {
        return RedirectToAction("Login", "Auth");
    }
    return View();
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
