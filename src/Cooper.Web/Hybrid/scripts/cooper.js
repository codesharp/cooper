//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="../jquery/jquery-1.7.2.min.js" />
///<reference path="../jquery/jquery.mobile-1.1.0.min.js" />
///<reference path="../jquery/jquery.json-2.3.min.js" />
///<reference path="lang.js" />
///<reference path="common.js" />

(function () {

    //与native交互

    //获取未提供的变更记录
    function getChanges() { }
    //增加变更记录
    function addChange() { }
    //创建任务表
    function createTasklist() { }
    //刷新/同步tasklist列表
    function syncTasklist() { }
    //刷新/同步任务
    function syncTasks(tasklistId, tasklistChanges, changes, by, sorts) { }

    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------

    //TODO：
    //1. 目前还没用异步方式实现与数据库的交互，待会儿回家再处理
    //2. 另外，通过#taskDetail?id=1234的方式经过萧玄提醒，应该是可以的，我回去也试一下
    //   参考URL：http://jquerymobile.com/test/docs/api/methods.html
    //3. 页面导航时，如果是退回，需要按相反的方向退回，晚上回去也一起弄

    //当前选择的任务列表ID
    var currentTaskListId = "";
    //存放任务列表的数组
    var taskListArray = [];

    //内存中增加或更新一个Task
    function addOrUpdateTask(id, subject, body, priority, dueTime, isCompleted, fn) {

        if (currentTaskListId == null || currentTaskListId == "") {
            return;
        }

        //首先获取当前任务列表
        var taskArray = [];
        for (var index = 0; index < taskListArray.length; index++) {
            var entry = taskListArray[index];
            if (entry.id == currentTaskListId) {
                taskArray = entry.tasks;
                break;
            }
        }

        if (id == "") {
            var task = new Object();
            var id = taskArray.length + 1;
            task.id = id;
            task.subject = subject;
            task.body = body;
            task.priority = priority; //0=today 1=upcoming 2=later priority 总是以string使用
            task.dueTime = dueTime;
            task.isCompleted = isCompleted;
            task.tags = [];
            taskArray[taskArray.length] = task;
        }
        else {
            //获取当前正在编辑的任务
            var task = null;
            for (var index = 0; index < taskArray.length; index++) {
                var entry = taskArray[index];
                if (entry.id == id) {
                    task = entry;
                    break;
                }
            }

            task.subject = subject;
            task.body = body;
            task.priority = priority; //0=today 1=upcoming 2=later priority 总是以string使用
            task.dueTime = dueTime;
            task.isCompleted = isCompleted;
            task.tags = [];
        }

        if (fn) fn();

        showTaskPanel(currentTaskListId);
    }

    //获取所有任务列表
    function loadAllTaskList() {
        taskListArray = [];

        //模拟生成5个任务列表
        for (var index = 0; index < 5; index++) {
            var taskList = new Object();
            var displayNumber = index + 1;
            taskList.id = "TaskList" + displayNumber;
            taskList.name = "任务列表" + displayNumber;
            taskList.tasks = [];
            taskListArray[taskListArray.length] = taskList;
        }
    }
    //设置当前选中的任务列表
    function setCurrentTaskList(taskListId) {
        currentTaskListId = taskListId;
    }
    //获取当前任务列表的所有任务，以数组的方式返回
    function loadTasksFromCurrentTaskList() {
        var taskArray = null;
        for (var index = 0; index < taskListArray.length; index++) {
            var entry = taskListArray[index];
            if (entry.id == currentTaskListId) {
                taskArray = entry.tasks;
                break;
            }
        }
        return taskArray;
    }
    //获取指定ID的任务
    function loadTask(taskId) {
        var taskArray = loadTasksFromCurrentTaskList();
        var task = null;
        for (var index = 0; index < taskArray.length; index++) {
            var entry = taskArray[index];
            if (entry.id == taskId) {
                task = entry;
                break;
            }
        }
        return task;
    }

    //显示所有任务列表
    function showAllTasklist() {
        var ul = $('#taskListUl');

        //清空任务列表
        $('#taskListUl > li').remove();

        //填充任务列表
        for (var index = 0; index < taskListArray.length; index++) {
            var taskList = taskListArray[index];
            var li = '<li id="' + taskList.id + '"><a>' + taskList.name + '<span class="ui-li-count">' + taskList.tasks.length + '</span></a></li>';
            ul.append(li);
        }

        //刷新列表
        ul.listview('refresh');

        //设置列表每行的Click响应函数
        $('#taskListUl li').click(function () {
            showTaskPanel($(this).attr("id"));
        });
    }
    //显示指定任务列表中的所有任务
    function showTasks() {
        var ul = $('#taskUl');

        //清空任务
        $('#taskUl > li').remove();

        //获取当前任务列表的所有任务
        var taskArray = loadTasksFromCurrentTaskList();

        //填充任务
        for (var index = 0; index < taskArray.length; index++) {
            var task = taskArray[index];
            var img = null;
            if (task.isCompleted == "true") {
                img = "complete-small.png";
            }
            else {
                img = "incomplete-small.png";
            }
            var li = '<li id="' + task.id + '"><a><h3><img src="images/' + img + '"><span>' + task.subject + '</span></h3><p>' + task.body + '</p><p><strong>' + task.dueTime + '</strong></p></a></li>';
            ul.append(li);
        }

        //刷新ul
        ul.listview('refresh');

        //设置li的click响应函数
        $('#taskUl li').click(function () {
            showTaskEditPanel($(this).attr("id"));
        });
    }

    //显示指定面板
    function showPanel(panelId, params) {
        if (params != null && params != undefined && params != "") {
            var targetPage = '#' + panelId + "?" + params;
            $.mobile.changePage(targetPage, { transition: "slide", direction: 'reverse' });
        }
        else {
            var targetPage = '#' + panelId;
            $.mobile.changePage(targetPage, { transition: "slide", direction: 'reverse' });
        }
    }
    //显示任务面板
    function showTaskPanel(taskListId) {
        setCurrentTaskList(taskListId);
        showPanel("taskPanel");
        showTasks();
    }
    //显示任务详情面板
    function showTaskDetailPanel(taskId) {
        showPanel("taskDetailPanel", "taskId=" + taskId);
        //获取数据，TODO
    }
    //显示任务新增编辑面板
    function showTaskEditPanel(taskId) {
        showPanel("taskEditPanel", "taskId=" + taskId);
    }
    //根据指定的任务ID初始化任务编辑面板
    function initializeTaskEditPanel(taskId) {
        //先清空边界面板
        clearTaskEditPanel();

        //如果当前存在任务ID，则加载并显示在任务编辑面板
        if (taskId != null && taskId != "") {
            var task = loadTask(taskId);
            if (task != null) {
                $("#taskId").val(task.id);
                $("#subject").val(task.subject);
                $("#body").val(task.body);
                //set priority, TODO
                $("#duetimeForEdit").val(task.dueTime);
                $("#isCompleted").val(task.isCompleted);
            }
        }
    }
    //刷新任务面板
    function refreshTaskPanel() {
        showTaskPanel(currentTaskListId);
    }
    //清空任务编辑面板
    function clearTaskEditPanel() {
        $("#subject").val("");
        $("#body").val("");
        //clear priority, TODO
        $("#duetimeForEdit").val("");
        $("#isCompleted").val("");
    }

    //验证用户有效性
    function validateUser(userName, password) {
        //TODO，在这里验证用户名和密码
        return true;
    }
    //用户登录处理
    function login(userName, password) {
        if (validateUser(userName, password)) {
            showPanel("taskListPanel");
            loadAllTaskList();
            showAllTasklist();
        }
        else {
            alert(lang.loginFailed);
        }
    }

    //从当前url地址栏获取任务ID
    //地址：http://localhost:54185/Hybrid/index.htm#taskEditPanel?taskId=1
    //通过将参数存放在url，即便我们采用刷新页面，也不会丢失当前的任务ID
    //我觉得任务列表的ID也可以采用这种方式，即通过保存在url中，从而即便我们
    //刷新页面也能知道当前的任务列表ID或任务ID
    function getTaskId() {
        var regex = /^#taskEditPanel/;
        var url = $.mobile.path.parseUrl(window.location.href);
        if (url.hash.search(regex) !== -1) {
            var taskId = url.hash.replace(/.*taskId=/, "");
            return taskId;
        }
    }
    //切换页面前执行的函数
    function onbeforepagechange(e, data) {
        if (typeof data.toPage === "string") {
            var url = $.mobile.path.parseUrl(data.toPage);

            //以下判断当前要切换到的页面是否是任务编辑页面，
            //如果是则从URL取出任务ID然后初始化任务编辑页面
            var regex = /^#taskEditPanel/;
            if (url.hash.search(regex) !== -1) {
                var taskId = url.hash.replace(/.*taskId=/, "");
                initializeTaskEditPanel(taskId);
                //以下这一句必须，因为发现如果不设置则jquery mobile不会在url地址栏显示taskId
                data.options.dataUrl = url.href;
            }

            //将来也可以考虑将TaskList的ID也用上面类似方式实现，
            //这样可以不需要定义内存变量保存当前的任务列表ID
        }
    }

    window.showPanel = showPanel;
    window.login = login;
    window.addOrUpdateTask = addOrUpdateTask;
    window.showTaskEditPanel = showTaskEditPanel;
    window.refreshTaskPanel = refreshTaskPanel;
    window.onbeforepagechange = onbeforepagechange;
})();


