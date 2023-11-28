using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Test;
using IdentityModel;
using ids.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityServerHost.Pages.Admin;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    //private readonly TestUserStore _users;
    private readonly IIdentityServerInteractionService _interaction;

    [BindProperty]
    public InputModel Input { get; set; }

    public Index(
        IIdentityServerInteractionService interaction, UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        //// this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
        //_users = users ?? throw new Exception("Please call 'AddTestUsers(TestUsers.Users)' on the IIdentityServerBuilder in Startup or remove the TestUserStore from the AccountController.");

        _interaction = interaction;
    }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = System.Web.HttpUtility.UrlDecode(returnUrl) };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "create")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl);
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        if (await _userManager.FindByNameAsync(Input.Username) != null)
        {
            ModelState.AddModelError("Input.Username", "Invalid username");
        }

        if (ModelState.IsValid)
        {
            var iduser = new IdentityUser() { UserName = Input.Username, NormalizedUserName = Input.Name, Email = Input.Email };
            var createdAsync = await _userManager.CreateAsync(iduser, Input.Password);

            List<Claim> claims = new List<Claim>();
            await _userManager.Users
                .ForEachAsync(
                    async (u) => claims.AddRange(await _userManager.GetClaimsAsync(u)));
            var max = claims.Where(c => c.Type == CustomJwtClaimTypes.CustomerNumber).Max(cl => cl.Value);
            int res = max == null ? 0 : Int32.Parse(max);
            

            var addClaimsResult = await _userManager.AddClaimsAsync(iduser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, Input.Name),
                new Claim(JwtClaimTypes.GivenName, Input.Name),
                new Claim(JwtClaimTypes.Email, Input.Email),
                new Claim(CustomJwtClaimTypes.CustomerNumber, (res++).ToString()),
                new Claim(JwtClaimTypes.Role, "Standard")
            });

            if (createdAsync.Succeeded && addClaimsResult.Succeeded)
            {
                // issue authentication cookie with subject ID and username
                var isuser = new IdentityServerUser(iduser.Id)
                {
                    DisplayName = iduser.UserName
                };

                await HttpContext.SignInAsync(isuser);

                if (context != null)
                {
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(Input.ReturnUrl);
                    }

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(Input.ReturnUrl);
                }

                // request for a local page
                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }

            }
            else
            {
                foreach (var error in createdAsync.Errors)
                {
                    ModelState.AddModelError("Input.Password", error.Description);
                }
            }
        }

        return Page();
    }
}