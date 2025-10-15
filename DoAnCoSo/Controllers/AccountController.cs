using DoAnCoSo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("GoogleResponse")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
        {
            return RedirectToAction("Login");
        }

        var claims = result.Principal.Claims;

        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Không lấy được email từ tài khoản Google.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name
            };

            var resultCreate = await _userManager.CreateAsync(user);
            if (!resultCreate.Succeeded)
            {
                return BadRequest("Không thể tạo tài khoản người dùng.");
            }

            var loginInfo = new UserLoginInfo(result.Principal.Identity.AuthenticationType, googleId, "Google");
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                return BadRequest("Không thể liên kết Google với người dùng.");
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }
}
