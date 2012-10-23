//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core.DomainBase;

namespace Cooper.Model.Accounts
{
    /// <summary>evernote帐号连接
    /// </summary>
    public class EverNoteConnection : AccountConnection
    {
        protected EverNoteConnection() : base() { }//由于NH
        public EverNoteConnection(string name, string token, Account account) : base(name, token, account) { }
    }
}