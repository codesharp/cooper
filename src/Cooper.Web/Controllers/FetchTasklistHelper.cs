//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cooper.Model.Accounts;
using System.Net;
using CodeSharp.Core.Utils;
using System.Text;
using Cooper.Model.Tasks;

namespace Cooper.Web.Controllers
{
    /// <summary>Fetch默认实现
    /// </summary>
    [CodeSharp.Core.Component]
    public class FetchTasklistHelper
    {
        private static readonly Serializer _serializer = new Serializer();
        private IAccountConnectionService _connectionService;
        private string _git_api_issues;
        public FetchTasklistHelper(IAccountConnectionService connectionService, string git_api_issues)
        {
            this._connectionService = connectionService;
            this._git_api_issues = git_api_issues;
        }

        public virtual bool IsFetchTasklist(string tasklistId)
        {
            return tasklistId == "github";
        }
        public virtual TaskInfo[] FetchTasks(Account account, string tasklistId)
        {
            //集成github issues
            if (tasklistId == "github")
            {
                var git = this._connectionService.GetConnections(account).FirstOrDefault(o => o is GitHubConnection);
                if (git == null) return null;

                var access_token = _serializer.JsonDeserialize<IDictionary<string, string>>(git.Token)["access_token"];

                using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    var all = _serializer.JsonDeserialize<IDictionary<string, object>[]>(wc.DownloadString(string.Format("{0}?access_token={1}"
                        , this._git_api_issues
                        , access_token)));

                    if (all == null) return null;

                    return all.Select(o => new TaskInfo()
                    {
                        ID = o["id"].ToString(),
                        Body = string.Format("{0}\n\n{1}", o["title"], o["html_url"]),
                        DueTime = null,
                        IsCompleted = false,
                        Priority = 0,
                        Subject = string.Format("#{0} {1}", o["number"], o["title"]),
                        Editable = false
                    }).ToArray();
                }
            }
            return null;
        }
    }
}
