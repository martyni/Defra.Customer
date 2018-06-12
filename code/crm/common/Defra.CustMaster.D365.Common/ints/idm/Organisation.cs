using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class Organisation
    {
        [DataType(DataType.Text)]
        [MaxLength(160,ErrorMessage = "Organiation name is more than 160 characters.")]
        public string name { get; set; }
        [Required(ErrorMessage = "Business cannot be empty.")]
        [DataType(DataType.Text)]
        public int type { get; set; }
        [MaxLength(8,ErrorMessage = "Company House Id cannot be more than 8 characters.")]
        public string crn { get; set; }
        [MaxLength(100, ErrorMessage = "Email address cannot be more than 100 characters long.")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        [MaxLength(100, ErrorMessage = "Validated with company house should have y or n")]

        [RegularExpression(@"^y|n",
         ErrorMessage = "Validated with company house can either be y or n.")]
        public bool validatedwithcompanieshouse { get; set; }
        public Address address { get; set; }
        public string telephone { get; set; }

        
        public int hierarchylevel { get; set; }
        public ParentOrganisation parentorganisation { get; set; }
    }

}
