using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ConsumerApplication.Models;

namespace ConsumerApplication.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> Logger;

	public HomeController(ILogger<HomeController> logger)
	{
		Logger = logger;
	}

	public IActionResult Index()
	{
		Logger.LogInformation("Index");
		return View();
	}

	public IActionResult Privacy()
	{
		Logger.LogInformation("Privacy");
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}