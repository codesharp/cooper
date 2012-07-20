using Cooper.Model.Accounts;
namespace AliCooper.Model.Accounts
{
    /// <summary>Ark帐号连接
    /// </summary>
    public class ArkConnection : AccountConnection
    {
        protected ArkConnection() : base() { }//由于NH
        public ArkConnection(string name, string token, Account account) : base(name, token, account) { }
    }
}