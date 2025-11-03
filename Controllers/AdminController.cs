using Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

//this controller is for admin
[Controller]
[Route("[controller]")]
public class AdminController:Controller
{
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;


    public AdminController(TelegramAuthService auth,MyContext context)
    {
        _auth = auth;
        _context = context;
    }
    
    //there should be controllers like create room, delete room, change name, delete user and atc.
}