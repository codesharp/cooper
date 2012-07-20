//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using CodeSharp.Core;
using CodeSharp.Core.Services;
using Cooper.Model.Accounts;

namespace AliCooper.Model.Accounts
{
    [CodeSharp.Core.Component]
    [Transactional]
    public class AccountHelper : Cooper.Model.Accounts.AccountHelper
    {
        public AccountHelper(ILoggerFactory factory
            , IAccountService accountService
            , IAccountConnectionService connectionService)
            : base(factory
            , accountService
            , connectionService) { }

        protected override AccountConnection GenerateConnection(Account account, Type connectionType, string connectionAccountName, string token)
        {
            //增加Ark连接
            if (connectionType == typeof(ArkConnection))
                return new ArkConnection(connectionAccountName, token, account);

            return base.GenerateConnection(account, connectionType, connectionAccountName, token);
        }
    }
}