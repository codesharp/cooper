//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="../jquery/jquery-1.7.2.min.js" />
///<reference path="../jquery/jquery.mobile-1.1.0.min.js" />
///<reference path="../jquery/jquery.json-2.3.min.js" />
///<reference path="cordova-1.9.0.js" />
///<reference path="lang.js" />
///<reference path="common.js" />

/////////////////////////////////////////////////////////////////////////////////////////

//NOTES:
//This file contains functions which is used to communication with ios native app api.

//Task的ChangeLog以及Sort信息的格式示例
//        var changeLogs =
//                    [
//                        {"ID":"temp_1","Name":"subject","Value":"new task 001","Type":0},
//                        {"ID":"temp_1","Name":"body","Value":"task description","Type":0}
//                    ];
//        var sorts =
//                  [
//                      {"By":"priority","Key":"0","Name":"尽快完成","Indexs":["temp_1","temp_2"]},
//                      {"By":"priority","Key":"1","Name":"稍后完成","Indexs":["temp_3","temp_4"]},
//                      {"By":"priority","Key":"2","Name":"迟些再说","Indexs":["temp_5","temp_6"]}
//                  ];

//变量定义
var newTaskTempIdPrefix = "temp_";
var loginUrl = "Account/Login";
var getTasklistsUrl = "Personal/GetTasklists";
var createTaskListUrl = "Personal/CreateTasklist";
var syncTaskUrl = "Personal/Sync";
var getTasksUrl = "Personal/GetByPriority";

//创建一个唯一标识
function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x7 | 0x8)).toString(16);
    });
    return uuid;
}
//Task修改日志信息对象
var ChangeLog = function () {
    this.Type = 0;
    this.ID = "";
    this.Name = "";
    this.Value = "";
};
//Task排序信息
var Sort = function () {
    this.By = 0;
    this.Key = "";
    this.Name = "";
    this.Indexs = [];
};
//任务对象定义
var Task = function () {
    this.id = newTaskTempIdPrefix + generateUUID();
    this.subject = "";
    this.body = "";
    this.priority = "0";
    this.dueTime = "";
    this.isCompleted = "false";
    this.tags = [];
    this.isEditable = true;
};

//判断当前网络是否可用
function isNetworkAvailable() {
    //之后需要借助于native接口实现，目前先mock实现
    return true;
}
//同步发送ajax post请求
function ajaxPost(url, data, callback) {
    
    var items = url.split("/");
    var serviceName = items[0];
    var actionName = items[1].toLowerCase();

    //因为参数必须是数组，所以把参数放在一个数组中
    var params = [];
    params.push(data);
    //alert('v' + ',' + serviceName + ',' + actionName);
    //调用Native接口
    Cordova.exec(
        function (result) {
            if (callback != null) {
                callback(result);
            }
        },
        function () { },
        serviceName,
        actionName,
        params
    );
}

//返回给定的单个Task的ChangeLog信息
function getSingleTaskChanges(task) {
    var changeLogs = [];
    var changeLog = new ChangeLog();

    changeLog.ID = task.id;
    changeLog.Name = "subject";
    changeLog.Value = task.subject;
    changeLogs.push(changeLog);

    changeLog = new ChangeLog();
    changeLog.ID = task.id;
    changeLog.Name = "body";
    changeLog.Value = task.body;
    changeLogs.push(changeLog);

    changeLog = new ChangeLog();
    changeLog.ID = task.id;
    changeLog.Name = "priority";
    changeLog.Value = task.priority;
    changeLogs.push(changeLog);

    changeLog = new ChangeLog();
    changeLog.ID = task.id;
    changeLog.Name = "dueTime";
    changeLog.Value = task.dueTime;
    changeLogs.push(changeLog);

    changeLog = new ChangeLog();
    changeLog.ID = task.id;
    changeLog.Name = "isCompleted";
    changeLog.Value = task.isCompleted;
    changeLogs.push(changeLog);

    return changeLogs;
};
//返回给定的所有Tasks的ChangeLog信息
function getChanges(tasks) {
    var totalChangeLogs = [];
    for (var index = 0; index < tasks.length; index++) {
        var changeLogs = getSingleTaskChanges(tasks[index]);
        for (var i = 0; i < changeLogs.length; i++) {
            totalChangeLogs.push(changeLogs[i]);
        }
    }
    return totalChangeLogs;
}
//返回给定Task数组的Sort信息
function getSorts(tasks) {
    var sorts = [];

    var sort1 = new Sort();  //存放所有priority="0"的Task
    sort1.By = "priority";
    sort1.Key = "0";
    sort1.Name = "尽快完成";

    var sort2 = new Sort();  //存放所有priority="1"的Task
    sort2.By = "priority";
    sort2.Key = "1";
    sort2.Name = "稍后完成";

    var sort3 = new Sort();  //存放所有priority="2"的Task
    sort3.By = "priority";
    sort3.Key = "2";
    sort3.Name = "迟些再说";

    for (var index = 0; index < tasks.length; index++) {
        if (tasks[index].priority == sort1.Key) {
            sort1.Indexs.push(tasks[index].id);
        }
        else if (tasks[index].priority == sort2.Key) {
            sort2.Indexs.push(tasks[index].id);
        }
        else if (tasks[index].priority == sort3.Key) {
            sort3.Indexs.push(tasks[index].id);
        }
    }

    sorts.push(sort1);
    sorts.push(sort2);
    sorts.push(sort3);

    return sorts;
}

//验证用户有效性
function validateUser(userName, password, callback) {
    ajaxPost(
        loginUrl,
        { userName: userName, password: password },
        function (result) {
            if (callback != null) {
                if (result.toString() == "true") {
                    callback({ 'success': true, 'message': null });
                }
                else {
                    callback({ 'success': false, 'message': lang.loginFailed });
                }
            }
        }
    );
    }
//获取所有任务列表
function loadAllTaskList(callback) {
    ajaxPost(
        getTasklistsUrl,
        null,
        function (result) {
            var taskLists = [];
            for (key in result) {
                var taskList = {};
                taskList.id = key;
                taskList.name = result[key];
                taskList.taskCount = 0;
                taskLists.push(taskList);
            }
            if (callback != null) {
                callback({ 'success': true, 'taskLists': taskLists, 'message': null });
            }
        }
    );
}
//获取指定任务表内的指定状态的所有任务
function loadTasks(listId, isCompleted, callback) {
    ajaxPost(
        getTasksUrl,
        { tasklistId: listId },
        function (result) {
//            var str = JSON.stringify(data);
//             alert(str);
            var tasks = [];
            var tasksFromServer = result != null && result.List != null ? result.List : [];
            for (var index = 0; index < tasksFromServer.length; index++) {
                var taskFromServer = tasksFromServer[index];

                //过滤出不符合是否完成条件的任务
                if (isCompleted == "true" || isCompleted == "false") {
                    if (taskFromServer["IsCompleted"] == null || taskFromServer["IsCompleted"].toString() != isCompleted) {
                        continue;
                    }
                }

                var task = new Task();
                task.id = taskFromServer["ID"];
             task.subject = taskFromServer["Subject"] == null ? "" : taskFromServer["Subject"];
                task.body = taskFromServer["Body"] == null ? "" : taskFromServer["Body"];
             task.dueTime = taskFromServer["DueTime"] == null ? "" : taskFromServer["DueTime"];
                task.priority = taskFromServer["Priority"];
                task.isCompleted = taskFromServer["IsCompleted"];
                task.isEditable = taskFromServer["Editable"];
                tasks.push(task);
            }
            if (callback != null) {
                callback({ 'success': true, 'tasks': tasks, 'message': 'success' });
            }
        }
    );
}
//新增一个TaskList
function addTaskList(taskListName, callback) {
    ajaxPost(
        createTaskListUrl,
        { name: taskListName, type: "personal" },
        function (result) {
            if (callback != null) {
                if (!isNaN(result)) {
                    callback({ 'success': true, 'message': null });
                }
                else {
                    callback({ 'success': false, 'message': lang.addTaskListFailed });
                }
            }
        }
    );
}
//同步指定任务表中的最新任务信息
function syncTasks(listId, tasks, callback) {
    //获取修改信息
    var changes = getChanges(tasks);
    //获取排序信息
    var sorts = getSorts(tasks);

    //发送ajax请求
    ajaxPost(
        syncTaskUrl,
        { tasklistId: listId, tasklistChanges: null, changes: $.toJSON(changes), by: "ByPriority", sorts: $.toJSON(sorts) },
        function (result) {
            //修正新增Task的ID
            for (var index = 0; index < result.length; index++) {
                var correction = result[index];
                for (var index = 0; index < tasks.length; index++) {
                    if (correction.OldId == tasks[index].id) {
                        tasks[index].id = correction.NewId;
                        break;
                    }
                }
            }
        }
    );

    //调用回调函数
    if (callback != null) {
        callback({ 'success': true, 'message': null });
    }
}