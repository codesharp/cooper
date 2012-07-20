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
    }
}