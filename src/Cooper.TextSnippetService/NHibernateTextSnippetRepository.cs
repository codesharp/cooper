using System;
using System.Linq;
using Castle.Facilities.NHibernateIntegration;
using Castle.Services.Transaction;
using NHibernate.Criterion;

namespace Cooper.TextSnippetService
{
    [Transactional]
    public class NHibernateTextSnippetRepository : ITextSnippetRepository
    {
        private const string GetTextSnippetSQL = "select Id, Type, Key, SnippetText from Cooper_TextSnippet where Type=:Type and Key=:key";
        private const string AddTextSnippetSQL = "insert into Cooper_TextSnippet values (:Type, :Key, :SnippetText)";
        private const string RemoveTextSnippetSQL = "delete from Cooper_TextSnippet where Id=:Id";

        private ISessionManager _sessionManager;

        public NHibernateTextSnippetRepository(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [Transaction]
        public TextSnippet GetTextSnippet(TextSnippetType type, string key)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(GetTextSnippetSQL);
            query.SetEnum("Type", type);
            query.SetString("Key", key);
            var result = query.UniqueResult() as object[];

            if (result != null)
            {
                return new TextSnippet
                {
                    Id = long.Parse(result[0].ToString()),
                    Type = (TextSnippetType)Enum.Parse(typeof(TextSnippetType), result[1].ToString()),
                    Key = result[2].ToString(),
                    SnippetText = result[3] as string
                };
            }

            return null;
        }
        [Transaction]
        public void AddTextSnippet(TextSnippetType type, string key, string snippetText)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(AddTextSnippetSQL);
            query.SetEnum("Type", type);
            query.SetString("Key", key);
            query.SetString("SnippetText", snippetText);
            query.ExecuteUpdate();
        }
        [Transaction]
        public void RemoveTextSnippet(long id)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(RemoveTextSnippetSQL);
            query.SetInt64("Id", id);
            query.ExecuteUpdate();
        }
    }
}
