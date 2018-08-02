namespace Defra.CustMaster.Identity.CoreApp.Model
{
    using System;
    public class WebFaultException:Exception
    {        
        public string ErrorMsg;

        public int HttpStatusCode;

        public WebFaultException(string errorMsg, int httpStatusCode)
        {
            ErrorMsg = errorMsg;
            HttpStatusCode = httpStatusCode;
        }
    }
}