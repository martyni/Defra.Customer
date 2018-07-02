namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{
    using System.Net.Http;

    public interface IClientFactory
    {
        HttpClient GetHttpClient();
    }
}