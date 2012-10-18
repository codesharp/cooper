using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CooperDemo.Infrastructure;
using CooperDemo.Model;
using System.ComponentModel;
using System.Json;

namespace CooperDemo
{
    [Activity(Label = "Cooper", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ILogger _logger;
        private ITaskFolderService _taskFolderService;
        private string _domain = "taobao-hz";
        private string _userName = "hewang.txh";
        private string _pasword = "$$88yyQK,,";
        private ListView _listView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var assemblies = new Assembly[] {
                Assembly.Load("CooperDemo.Model"),
                Assembly.Load("CooperDemo.Repositories"),
                Assembly.Load("CooperDemo")
            };

            Configuration.Config()
                .SetResolver(new TinyIoCDependencyResolver())
                .RegisterDefaultComponents(assemblies);

            _logger = DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(MainActivity));
            _taskFolderService = DependencyResolver.Resolve<ITaskFolderService>();

            SetContentView(Resource.Layout.Main);

            _listView = FindViewById<ListView>(Resource.Id.list);
            _listView.SetOnCreateContextMenuListener(this);

            Button button1 = FindViewById<Button>(Resource.Id.GetAllTaskFoldersButton);
            button1.Click += (sender, e) =>
            {
                SendGetAllTaskFoldersRequest();
            };

            SendLoginRequest();

            _logger.InfoFormat("Application '{0}' Started.", "Cooper");
        }

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            menu.Add("删除");
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var taskFolder = this._listView.Adapter.GetItem(info.Position).Cast<TaskFolder>();

            switch (item.ItemId)
            {
                case 0:
                    {
                        _taskFolderService.Delete(taskFolder);
                        var folders = _taskFolderService.GetAllTaskFolders();
                        _listView.Adapter = new TaskFolderAdapter(this, Resource.Layout.TaskFolderListItem, folders.ToArray());
                        return true;
                    }
            }

            return false;
        }

        /// <summary>
        /// 发送登陆请求
        /// </summary>
        private void SendLoginRequest()
        {
            var url = Constants.LOGIN_URL;
            var postData = string.Format("state=login&cbDomain={0}&tbLoginName={1}&tbPassword={2}", _domain, _userName, _pasword);
            HttpWebRequestHelper.SendHttpPostRequest(null, url, postData,
            response =>
            {
                var result = HttpWebRequestHelper.GetTextFromResponse(response);

                _logger.InfoFormat("Request URL:{0}, StatusCode:{1}", url, response.StatusCode);
                _logger.InfoFormat("Result:{0}", result);

                if (response.Cookies.Count > 0)
                {
                    var cookie = response.Cookies.Cast<Cookie>().First();
                    var sharedPreferences = GetSharedPreferences("CooperPreferences", FileCreationMode.Private);
                    CookieManager.SaveCookie(sharedPreferences, "CooperCookie", cookie);
                }
            });
        }

        /// <summary>
        /// 发送获取所有任务表的请求
        /// </summary>
        private void SendGetAllTaskFoldersRequest()
        {
            var sharedPreferences = GetSharedPreferences("CooperPreferences", FileCreationMode.Private);
            var cookie = CookieManager.GetCookie(sharedPreferences, "CooperCookie");
            var url = Constants.GET_TASKFOLDER_LIST_URL;

            HttpWebRequestHelper.SendHttpPostRequest(cookie, url, null,
            response =>
            {
                var result = HttpWebRequestHelper.GetTextFromResponse(response);

                _logger.InfoFormat("Request URL:{0}, StatusCode:{1}", url, response.StatusCode);
                _logger.InfoFormat("Result:{0}", result);

                var jsonValue = JsonObject.Parse(result);
                var taskFolders = new List<TaskFolder>();
                var obj = jsonValue as JsonObject;

                foreach (var pair in obj)
                {
                    if (Utils.IsNumber(pair.Key))
                    {
                        var taskFolder = new TaskFolder();
                        taskFolder.ID = Utils.ConvertType<int>(pair.Key);
                        taskFolder.Name = pair.Value;
                        taskFolders.Add(taskFolder);
                        SaveTaskFolder(taskFolder);
                    }
                    else
                    {
                        _logger.InfoFormat("Fetched TaskFolder:{0}", pair.Key);
                    }
                }

                RunOnUiThread(() =>
                {
                    var folders = _taskFolderService.GetAllTaskFolders();
                    _listView.Adapter = new TaskFolderAdapter(this, Resource.Layout.TaskFolderListItem, folders.ToArray());
                });
            });
        }
        /// <summary>
        /// 将一个任务表保存或更新到本地Sqlite数据库
        /// </summary>
        /// <param name="taskFolder"></param>
        private void SaveTaskFolder(TaskFolder taskFolder)
        {
            var folder = _taskFolderService.GetTaskFolder(taskFolder.ID);
            if (folder == null)
            {
                _taskFolderService.Create(taskFolder);
            }
            else
            {
                _taskFolderService.Update(taskFolder);
            }
        }
    }
}