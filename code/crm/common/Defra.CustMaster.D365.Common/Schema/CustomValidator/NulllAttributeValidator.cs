using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.schema.CustomValidator
{
    [AttributeUsage(AttributeTargets.Property |
      AttributeTargets.Field, AllowMultiple = false)]
    sealed public class NulllAttributeValidator: ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            return value == null ? false : true;
        }


        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
              ErrorMessageString, name);
        }
    }
}
