namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{
    using Defra.CustMaster.Identity.CoreApp.Model;

    public interface ICrmApiWrapper
    {
        ContactModel CreateContact(ContactModel contact);

        ServiceObject InitialMatch(string b2cObjectId);
    
        ServiceUserLinks Authz(string serviceID, string b2cObjectId);
    }
}
