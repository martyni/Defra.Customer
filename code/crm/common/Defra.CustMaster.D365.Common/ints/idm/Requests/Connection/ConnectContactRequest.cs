using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Requests.Connection
{
    public partial class ConnectContactRequest
    {
        [DataType(DataType.Text)]
        [MaxLength(36, ErrorMessage = "Contact id cannont be more than 36 chacters.")]
        [Required(ErrorMessage = "Contact id is mandatory.")]
        public string contactid { get; set; }

        [MaxLength(36, ErrorMessage = "Account id cannont be more than 36 chacters.")]
        [Required(ErrorMessage = "Account id is mandatory.")]
        [DataType(DataType.Text)]
        public string accountid { get; set; }


        [MaxLength(25, ErrorMessage = "From role name lenght cannot be more than 25.")]
        [Required(ErrorMessage = "From role is mandatory.")]
        public string fromrole { get; set; }

        [MaxLength(25, ErrorMessage = "To role name lenght cannot be more than 25.")]
        public string torole { get; set; }

    }
}
