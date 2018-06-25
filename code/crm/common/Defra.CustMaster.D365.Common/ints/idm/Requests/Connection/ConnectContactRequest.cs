using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class ConnectContactRequest
    {
        [DataType(DataType.Text)]
        [MaxLength(36, ErrorMessage = "From record id cannont be more than 36 chacters.")]
       
        public string fromrecordid { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "To record type required.")]
        public RecordTypeName torecordtype { get; set; }

        [DataType(DataType.Text)]
        public RecordTypeName fromrecordtype { get; set; }

        [MaxLength(36, ErrorMessage = "To record id cannont be more than 36 chacters.")]
        [Required(ErrorMessage = "To record id is mandatory.")]
        [DataType(DataType.Text)]
        public string torecordid { get; set; }

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

    public enum RecordTypeName
    {
        organisation = 1 , contact = 2
    }
}
