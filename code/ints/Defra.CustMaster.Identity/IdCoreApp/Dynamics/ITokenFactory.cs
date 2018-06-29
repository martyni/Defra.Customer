namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{
    public interface ITokenFactory
    {
        string GetToken();

        void InvalidateToken();
    }
}
