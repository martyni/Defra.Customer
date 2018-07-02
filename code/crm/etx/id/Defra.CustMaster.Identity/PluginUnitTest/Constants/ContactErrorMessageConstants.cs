using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defra.PluginUnitTest.Constants
{
   public class ContactErrorMessageConstants
    {
       public static String ContactIdMaxLenErrorMsg = "Contact ID is invalid/exceed the max length(50);";
       public static  String FirstNameMaxLengthErrorMsg = "First Name cannot be greater than 50;";
       public static  String MiddlenameMaxLengthErrorMsg = "Middle Name cannot be greater than 50;";
       public static  String LastNameMaxLengthErrorMsg = "Last Name cannot be greater than 50";
       public static  String EmailMaxLengthErrorMsg = "Email cannot be greater than 100;";
       public static  String TelephoneMaxLengthErrorMsg = "Telephone cannot be greater than 50;";
       public static String TermsAndConditionMaxLengthErrorMsg = "T&C Accepted Version cannot be greater than 5;";
    }
}
