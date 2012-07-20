using FluentNHibernate.Mapping;
using AliCooper.Model.Accounts;

namespace AliCooper.Model.Mappings
{
    //映射覆盖
    public class AccountConnectionMap : Cooper.Model.Mappings.AccountConnectionMap { }
    public class GoogleConnectionMap : Cooper.Model.Mappings.GoogleConnectionMap { }
    public class GitHubConnectionMap : Cooper.Model.Mappings.GitHubConnectionMap { }

    public class ArkConnectionMap : SubclassMap<ArkConnection>
    {
        public ArkConnectionMap()
        {
            Table("Cooper_AccountConnection");
            DiscriminatorValue("ark");
        }
    }
}