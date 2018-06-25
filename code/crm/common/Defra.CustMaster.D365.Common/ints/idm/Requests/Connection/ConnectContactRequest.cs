using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class ConnectContactRequest
    {
        [DataType(DataType.Text)]
        [MaxLength(36, ErrorMessage = "Contact id cannont be more than 36 chacters.")]
        [Required(ErrorMessage = "Contact id is mandatory.")]
        public string contactid { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "To record type required.")]
        public string torecordtype { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "From record type required.")]
        public string  fromrecordtype { get; set; }

        [MaxLength(36, ErrorMessage = "Account id cannont be more than 36 chacters.")]
        [Required(ErrorMessage = "Account id is mandatory.")]
        [DataType(DataType.Text)]
        public string organisationid { get; set; }
        public RelationsDetails relations;
    }

    public partial class RelationsDetails
    {
        [MaxLength(25, ErrorMessage = "From role name lenght cannot be more than 25.")]
        public string fromrole { get; set; }
        [MaxLength(25, ErrorMessage = "To role name lenght cannot be more than 25.")]
        [Required(ErrorMessage = "To role is mandatory.")]
        public string torole { get; set; }
    }
}
