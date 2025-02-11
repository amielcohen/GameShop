// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shop.Models;

namespace GameShop.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Street Address")]
            public string StreetAddress { get; set; }

            [Display(Name = "City")]
            public string City { get; set; }

            [Display(Name = "State")]
            public string State { get; set; }

            [Display(Name = "Zip Code")]
            public string ZipCode { get; set; }

            [Display(Name = "Full Name")]

            public string name { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                City = user.City ,
                StreetAddress = user.StreetAddress,
                State=user.State,
                ZipCode=user.ZipCode,
                name=user.name
            };
          
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync((ApplicationUser)user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = (ApplicationUser)await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync((ApplicationUser)user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.City != user.City)
            {
                user.City = Input.City; // Update the City property of the user
                var updateResult = await _userManager.UpdateAsync(user); // Update the user in the database
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set city.";
                    return RedirectToPage();
                }
            }

            if (Input.StreetAddress != user.StreetAddress)
            {
                user.StreetAddress = Input.StreetAddress; 
                var updateResult = await _userManager.UpdateAsync(user); 
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set Street Address.";
                    return RedirectToPage();
                }
            }

            if (Input.State != user.State)
            {
                user.State = Input.State; 
                var updateResult = await _userManager.UpdateAsync(user); 
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set State.";
                    return RedirectToPage();
                }
            }

            if (Input.ZipCode != user.ZipCode)
            {
                user.ZipCode = Input.ZipCode; 
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set Zip Code.";
                    return RedirectToPage();
                }
            }

            if (Input.name != user.name)
            {
                user.name = Input.name;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set Zip Code.";
                    return RedirectToPage();
                }
            }







            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
