//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using Castle.Facilities.NHibernateIntegration;
using Castle.Services.Transaction;
using NHibernate.Criterion;

namespace Cooper.TextSnippetService
{
    [Transactional]
    public class NHibernateSnippetTextRepository : ISnippetTextRepository
    {
        private const string GetSnippetTextSQL = "select Id, Type, Key, SnippetText from Cooper_SnippetText where Type=:Type and Key=:key";
        private const string AddSnippetTextSQL = "insert into Cooper_SnippetText values (:Type, :Key, :SnippetText)";
        private const string RemoveSnippetTextSQL = "delete from Cooper_SnippetText where Id=:Id";

        private ISessionManager _sessionManager;

        public NHibernateSnippetTextRepository(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [Transaction]
        public SnippetText GetSnippetText(SnippetTextType type, string key)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(GetSnippetTextSQL);
            query.SetEnum("Type", type);
            query.SetString("Key", key);
            var result = query.UniqueResult() as object[];

            if (result != null)
            {
                return new SnippetText
                {
                    Id = long.Parse(result[0].ToString()),
                    Type = (SnippetTextType)Enum.Parse(typeof(SnippetTextType), result[1].ToString()),
                    Key = result[2].ToString(),
                    Text = result[3] as string
                };
            }

            return null;
        }
        [Transaction]
        public void AddSnippetText(SnippetTextType type, string key, string snippetText)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(AddSnippetTextSQL);
            query.SetEnum("Type", type);
            query.SetString("Key", key);
            query.SetString("SnippetText", snippetText);
            query.ExecuteUpdate();
        }
        [Transaction]
        public void RemoveSnippetText(long id)
        {
            var query = _sessionManager.OpenSession().CreateSQLQuery(RemoveSnippetTextSQL);
            query.SetInt64("Id", id);
            query.ExecuteUpdate();
        }
    }
}
