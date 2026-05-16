using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MathApi_Client.Models; 
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace MathApi_Client.Controllers;

public class MathController : Controller 
{
    private static HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://math-api:8080/"), 
    };

    private void PopulateOperations()
    {
        ViewBag.Operations = new List<SelectListItem> {
            new SelectListItem { Value = "1", Text = "+" },
            new SelectListItem { Value = "2", Text = "-" },
            new SelectListItem { Value = "3", Text = "*" },
            new SelectListItem { Value = "4", Text = "/" },
        };
    }

    public IActionResult Calculate()
    {
        var token = HttpContext.Session.GetString("MathJWT");
        if (token == null) return RedirectToAction("Login", "Auth");

        PopulateOperations(); 
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(decimal? FirstNumber, decimal? SecondNumber, int Operation)
    {
        var token = HttpContext.Session.GetString("MathJWT");
        var currentUser = HttpContext.Session.GetString("currentUser");

        if (token == null) return RedirectToAction("Login", "Auth");

        // IMPORTANT: Attach the JWT to the Authorization Header
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        decimal? Result = 0;
        MathCalculation mathCalculation;

        try {
            mathCalculation = MathCalculation.Create(FirstNumber, SecondNumber, Operation, Result, currentUser);
        }
        catch (Exception ex) {
            ViewBag.Error = ex.Message;
            PopulateOperations();
            return View();
        }
        
        StringContent jsonContent = new(JsonConvert.SerializeObject(mathCalculation), Encoding.UTF8, "application/json"); 
        HttpResponseMessage response = await httpClient.PostAsync("api/Math/PostCalculate", jsonContent);

        if (response.IsSuccessStatusCode) {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            MathCalculation? deserialisedResponse = JsonConvert.DeserializeObject<MathCalculation>(jsonResponse);
            ViewBag.Result = deserialisedResponse?.Result;
        } 
        else {
            ViewBag.Result = "An error has occurred with the API";
        }

        PopulateOperations(); 
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var token = HttpContext.Session.GetString("MathJWT");
        if (token == null) return RedirectToAction("Login", "Auth");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await httpClient.GetAsync("api/Math/GetHistory");

        if (response.IsSuccessStatusCode) {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var history = JsonConvert.DeserializeObject<List<MathCalculation>>(jsonResponse);
            return View(history);
        }

        ViewBag.HistoryMessage = "No history to show";
        return View(new List<MathCalculation>());
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var token = HttpContext.Session.GetString("MathJWT");
        if (token == null) return RedirectToAction("Login", "Auth");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await httpClient.DeleteAsync("api/Math/DeleteHistory");

        return RedirectToAction("History");
    }
}