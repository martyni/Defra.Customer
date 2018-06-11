namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class Organisation
    {
        public string name { get; set; }
        public int type { get; set; }
        public string crn { get; set; }
        public string email { get; set; }
        public bool validatedwithcompanieshouse { get; set; }
        public Address address { get; set; }
        public string telephone { get; set; }
        public int hierarchylevel { get; set; }
        public ParentOrganisation parentorganisation { get; set; }
    }

}
