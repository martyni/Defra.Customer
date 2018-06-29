namespace Defra.CustomerMaster.Identity.Api.Dynamics
{
    using System.Net.Http;

    public interface IClientFactory
    {
        HttpClient GetHttpClient();
    }
}