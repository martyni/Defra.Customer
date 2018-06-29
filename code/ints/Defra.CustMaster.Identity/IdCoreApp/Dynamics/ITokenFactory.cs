namespace Defra.CustomerMaster.Identity.Api.Dynamics
{
    public interface ITokenFactory
    {
        string GetToken();

        void InvalidateToken();
    }
}
