using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bridge.Web.Validators
{
    public class StringCurrencyValidator : ValidationAttribute
    {
        public StringCurrencyValidator(string message)
            : base(message)
        {
        }

        public override bool IsValid(object value)
        {
            return isValid(value);
        }

        public static bool isValid(object value)
        {
            if (value == null || (value is string) == false)
            {
                return false;
            }

            decimal relAmount;
            return Decimal.TryParse(Convert.ToString(value), out relAmount);
        }
    }
}