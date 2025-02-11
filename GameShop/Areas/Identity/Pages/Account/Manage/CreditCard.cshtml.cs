// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Shop.DataAccess.Migrations;
using Shop.DataAccess.Repository;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameShop.Areas.Identity.Pages.Account.Manage
{
    public class CreditCardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

       

        public CreditCardModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUnitOfWork unitOfWork)

        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;

    }


        /////
        public static byte[] GenerateRandomKey(int keySizeInBytes) // פונקציה לבחירת מפתח רנדומלית
        {
            byte[] key = new byte[keySizeInBytes];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        // Generate random IV
        public static byte[] GenerateRandomIV(int blockSizeInBytes) // פונקציה לבחירה של וקטור רנדומלי
        {
            byte[] iv = new byte[blockSizeInBytes];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        /////

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
            
            [Display(Name = "card Number")]

            public string cardNumber { get; set; }
            [Display(Name = "Month")]

            public string month { get; set; }
            [Display(Name = "Year")]

            public string year { get; set; }
            [Display(Name = "CVC")]

            public string CVC { get; set; }

            public byte[] aesKey { get; set; }
            public byte[] aesIV { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;



            CreditCard card= _unitOfWork.CreditCard.Get(u=>u.ApplicationUserId == user.Id);
            if(card != null)
            {
                if(card.aesKey.IsNullOrEmpty())
                {

                    card.aesKey = GenerateRandomKey(32);
                    _unitOfWork.CreditCard.Update(card);
                    _unitOfWork.Save();
                }
                if (card.aesIV.IsNullOrEmpty())
                {

                    card.aesIV = GenerateRandomIV(16);
                    _unitOfWork.CreditCard.Update(card);
                    _unitOfWork.Save();
                }


                Input = new InputModel
                {

                    cardNumber = card.cardNumber,
                    month = card.month,
                    year = card.year,
                    CVC = card.CVC,
                    aesKey=card.aesKey,
                    aesIV=card.aesIV

                };
            }
            else
            {
               

                Input = new InputModel
                {
                    cardNumber = "",
                    month = "",
                    year = "",
                    CVC = ""
                };
            }
           



          
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

            CreditCard card = _unitOfWork.CreditCard.Get(u => u.ApplicationUserId == user.Id);

            if (card != null)
            {

                double number;
                int monthNumber;
                string startedCardNumber = "";
                if (!Input.cardNumber.IsNullOrEmpty()&& Input.cardNumber.Length >= 16)
                {
                    startedCardNumber = $"**** **** **** {Input.cardNumber.Substring(15)}";
                }
                if (card.cardNumber.IsNullOrEmpty() || Input.cardNumber!= startedCardNumber)
                {
                    //var updateResult = await _userManager.UpdateAsync(user); // Update the user in the database
                    if (double.TryParse(Input.cardNumber, out number)&& Input.cardNumber.Length==16)
                    {
                        card.cardNumber = EncryptString(Input.cardNumber, card.aesKey, card.aesIV);
                        _unitOfWork.CreditCard.Update(card);
                        _unitOfWork.Save();

                    }
                    else
                    {
                        StatusMessage = "Error when trying to set card Number. Please enter a valid card Number";
                        return RedirectToPage();
                    }
                }

                if (Input.month != card.month)
                {
                    if (int.TryParse(Input.month, out monthNumber) )
                    {
                        if (monthNumber >= 1 && monthNumber <= 12)
                        {
                            card.month = Input.month;
                            _unitOfWork.CreditCard.Update(card);
                            _unitOfWork.Save();

                        }
                        else
                        {
                            StatusMessage = "Error when trying to set Month. Please enter a standard month.";
                            return RedirectToPage();
                        }


                    }
                    else
                    {
                        StatusMessage = "Unexpected error when trying to set Month.";
                        return RedirectToPage();
                    }
                }

                if (Input.year != card.year)
                {
                    if (double.TryParse(Input.year, out number)&& Input.year.Length<3)
                    {
                        card.year = Input.year;
                        _unitOfWork.CreditCard.Update(card);
                        _unitOfWork.Save();

                    }
                    else
                    {
                        StatusMessage = "Unexpected error when trying to set Year.";
                        return RedirectToPage();
                    }
                }

                if (Input.CVC !="***")
                {
                    if (double.TryParse(Input.CVC, out number)&& Input.CVC.Length== 3)
                    {
                        card.CVC = EncryptString(Input.CVC, card.aesKey, card.aesIV);
                        _unitOfWork.CreditCard.Update(card);
                        _unitOfWork.Save();

                    }
                    else
                    {

                        StatusMessage = "Error when trying to set CVC. Please enter a valid CVC Number";
                        return RedirectToPage();
                    }
                }
            }
            else
            {
                byte[] aesKeytemp = GenerateRandomKey(32);
                byte[] aesIVtemp = GenerateRandomIV(16);

                CreditCard newcard = new()
                {

                    aesKey = aesKeytemp,
                    aesIV = aesIVtemp,
                    ApplicationUserId = user.Id,
                    cardNumber = EncryptString(Input.cardNumber, aesKeytemp, aesIVtemp),
                    month = Input.month,
                    year = Input.year,
                    CVC = EncryptString(Input.CVC, aesKeytemp, aesIVtemp)

                };
                _unitOfWork.CreditCard.Add(newcard);
                _unitOfWork.Save();
            }


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }


        /////// AVITAL
        ///
        public string EncryptString(string plainText,byte[] aesKey, byte[] aesIV)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // פענוח! מקבלת את המפתח והוקטור ואת הטקסט שמוצפן ואמורה להחזיר את הטקסט המקורי
        public string DecryptString(string cipherText,byte[] aesKey, byte[] aesIV)
        {


            // משתנה ריק לבצע עליו את ההמרות
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

       

    }
}
