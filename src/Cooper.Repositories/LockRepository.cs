//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Castles;
using Cooper.Model;
using NHibernate;
using NHibernate.Criterion;

namespace Cooper.Repositories
{
    //基于DB的同步锁实现
    [Transactional]
    public class LockRepository : NHibernateRepositoryBase<string, Lock>, ILockHelper
    {
        #region ILockHelper Members
        [Transaction(TransactionMode.Requires)]
        public void Init<T>()
        {
            if (this.Require<T>() == null)
                this.Add(new Lock(typeof(T).Name));
        }

        public Lock Require<T>()
        {
            return this.Require(typeof(T));
        }

        public Lock Require(Type type)
        {
            return this.GetSession()
                .CreateCriteria<Lock>()
                .SetLockMode(LockMode.Upgrade)
                .Add(Expression.IdEq(type.Name))
                .UniqueResult<Lock>();
        }

        #endregion
    }
}