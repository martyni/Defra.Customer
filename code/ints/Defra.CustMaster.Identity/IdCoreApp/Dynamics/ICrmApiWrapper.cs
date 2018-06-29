using Defra.CustomerMaster.Identity.Api.Model;

namespace Defra.CustomerMaster.Identity.Api.Dynamics
{

    public interface ICrmApiWrapper
    {
        ContactModel CreateContact(ContactModel contact);
        ServiceObject InitialMatch(string b2cObjectId);
        //ServiceObject UserInfo(ContactModel contact);
        ServiceUserLinks Authz(string serviceID, string b2cObjectId);
    }
}
