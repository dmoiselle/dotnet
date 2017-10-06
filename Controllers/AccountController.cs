using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Bridge.Web.Models;
using Bridge.Services;
using Bridge.Services.Membership;

namespace Bridge.Web.Controllers
{
    public class AccountController : ApplicationControllerBase
    {
        public AccountController( 
            IFormsAuthenticationService formsService, 
            IAccountMembershipService membershipService,
            IRolesService rolesService ) {

            FormsService = formsService;
            MembershipService = membershipService;
            RolesService = rolesService;
        }

        [HttpGet]
        public ActionResult LogOn() {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn( LogOnModel model, string returnUrl ) {
            if ( ModelState.IsValid ) {
                if ( MembershipService.ValidateUser( model.UserName, model.Password ) ) {
                    FormsService.SignIn( model.UserName, model.RememberMe );
                    if ( Url.IsLocalUrl( returnUrl ) ) {
                        return Redirect( returnUrl );
                    }
                    else {
                        return RedirectToAction( "Index", "Home" );
                    }
                }
                else {
                    ModelState.AddModelError( "", "The user name or password provided is incorrect." );
                }
            }

            // If we got this far, something failed, redisplay form
            return View( model );
        }

        [HttpGet]
        public ActionResult LogOff() {
            FormsService.SignOut();

            return RedirectToAction( "Index", "Home" );
        }

        [HttpGet]
        [Authorize( Roles = AccountMembershipService.ADMINISTRATOR_ROLE )]
        public ActionResult Register() {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        [Authorize( Roles = AccountMembershipService.ADMINISTRATOR_ROLE )]
        public ActionResult Register( RegisterModel model ) {
            if ( ModelState.IsValid ) {
                // Attempt to register the user
                var createStatus = MembershipService.CreateUser( model.UserName, model.Password, model.Email );

                if ( createStatus == MembershipCreateStatus.Success ) {
                    RolesService.AddUserToRole( model.UserName, model.Role );
                    FormsService.SignIn( model.UserName, createPersistentCookie: false );
                    return RedirectToAction( "Index", "Home" );
                }
                else {
                    ModelState.AddModelError( "", AccountValidation.ErrorCodeToString( createStatus ) );
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View( model );
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword() {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword( ChangePasswordModel model ) {
            if ( ModelState.IsValid ) {
                if ( MembershipService.ChangePassword( User.Identity.Name, model.OldPassword, model.NewPassword ) ) {
                    return RedirectToAction( "ChangePasswordSuccess" );
                }
                else {
                    ModelState.AddModelError( "", "The current password is incorrect or the new password is invalid." );
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View( model );
        }

        [HttpGet]
        public ActionResult ChangePasswordSuccess() {
            return View();
        }

        private readonly IFormsAuthenticationService FormsService;
        private readonly IAccountMembershipService MembershipService;
        private readonly IRolesService RolesService;

    }
}