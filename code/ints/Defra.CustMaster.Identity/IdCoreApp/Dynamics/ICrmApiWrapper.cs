using Defra.CustMaster.Identity.CoreApp.Model;

namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{

    public interface ICrmApiWrapper
    {
        ContactModel CreateContact(ContactModel contact);
        ServiceObject InitialMatch(string b2cObjectId);
        //ServiceObject UserInfo(ContactModel contact);
        ServiceUserLinks Authz(string serviceID, string b2cObjectId);
    }
}
