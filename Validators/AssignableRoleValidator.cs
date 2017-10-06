using Bridge.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bridge.Web.Validators
{
    public class AssignableRoleAttribute : ValidationAttribute
    {
        public override bool IsValid( object value ) {
            return RegisterModel.AssignableRoles.Keys.Contains( value.ToString() );
        }
    }
}