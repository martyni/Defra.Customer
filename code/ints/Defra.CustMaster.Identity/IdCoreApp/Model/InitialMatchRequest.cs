namespace Defra.CustMaster.Identity.CoreApp.Model
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
  
    public class InitialMatchRequest
    {
        [JsonProperty("b2cobjectid")]
        [Required(ErrorMessage = "defra_b2cobjectid can not be empty or null")]
        public string B2cObjectId;
    }

    public class AuthzRequest
    {
        [JsonProperty("b2cobjectid")]
        [Required(ErrorMessage = "B2cObjectId can not be empty or null")]
        public string B2cObjectId;

        [JsonProperty("serviceid")]
        [Required(ErrorMessage = "ServiceId can not be empty or null")]
        public string ServiceId;
    }
}
