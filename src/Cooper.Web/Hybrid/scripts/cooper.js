//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="../jquery/jquery-1.7.2.min.js" />
///<reference path="../jquery/jquery.mobile-1.1.0.min.js" />
///<reference path="../jquery/jquery.json-2.3.min.js" />
///<reference path="lang.js" />
///<reference path="common.js" />

(function () {

    //当前选择的任务列表ID
    var currentTaskListId = "";
    //存放任务列表的数组
    var taskListArray = [];

    //设置当前选中的任务列表
    function setCurrentTaskList(taskListId) {
        currentTaskListId = taskListId;
    }
    //返回当前选中的任务列表
    function getCurrentTaskList() {
        return currentTaskListId;
    }

    //----------------------------------------------------------------
    //以下函数与操作数据库相关
    //----------------------------------------------------------------

    //验证用户有效性
    function validateUser(userName, password, fn) {
        //TODO，在这里验证用户名和密码

        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //新增一个TaskList
    function addTaskList(taskListName, fn) {
        var taskList = new Object();
        var displayNumber = taskListArray.length + 1;

        taskList.id = "TaskList" + displayNumber;
        taskList.name = taskListName;
        taskList.tasks = [];
        taskListArray[taskListArray.length] = taskList;

        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //增加或更新一个Task
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

        if (priority == undefined || priority == null || priority == "") {
            priority = "0"; //如果优先级为空，则赋默认值为今天完层
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

        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //详情页面自动更新任务优先级
    function autoUpdateTaskPriority(taskId, priority, fn) {
        var task = loadTask(taskId);
        if (task != null) {
            task.priority = priority;
        }
        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //详情页面自动更新任务截止时间
    function autoUpdateTaskDueTime(taskId, dueTime, fn) {
        var task = loadTask(taskId);
        if (task != null) {
            task.dueTime = dueTime;
        }
        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //详情页面自动更新任务完成状态
    function autoUpdateTaskStatus(taskId, isCompleted, fn) {
        var task = loadTask(taskId);
        if (task != null) {
            task.isCompleted = isCompleted;
        }
        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
    }
    //获取所有任务列表
    function loadAllTaskList(fn) {
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
        if (fn != null) {
            fn({ 'success': true, 'message': 'success' });
        }
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

    //----------------------------------------------------------------
    //以下函数用于显示/更新界面
    //----------------------------------------------------------------

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
            var listId = $(this).attr("id");
            showPage("taskPage", "listId=" + listId);
        });
    }
    //显示指定任务列表中的所有任务
    function showTasks(listId) {
        var ul = $('#taskUl');

        //清空任务
        $('#taskUl > li').remove();

        if (listId == null || listId == "") {
            return;
        }

        //获取当前任务列表的所有任务
        var taskArray = loadTasksFromCurrentTaskList();

        var li1 = '<li style="background-color: #ebebeb">马上完成 <span class="ui-li-count">{0}</span></li>';
        var li2 = '<li style="background-color: #ebebeb">稍后完成 <span class="ui-li-count">{0}</span></li>';
        var li3 = '<li style="background-color: #ebebeb">迟些再说 <span class="ui-li-count">{0}</span></li>';

        var items1 = [];
        var items2 = [];
        var items3 = [];

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

            if (task.priority == "0") {
                items1[items1.length] = li;
            }
            else if (task.priority == "1") {
                items2[items2.length] = li;
            }
            else if (task.priority == "2") {
                items3[items3.length] = li;
            }
        }

        var totalItems = [];

        totalItems[totalItems.length] = li1.replace("{0}", items1.length);
        for (var index = 0; index < items1.length; index++) {
            totalItems[totalItems.length] = items1[index];
        }
        totalItems[totalItems.length] = li2.replace("{0}", items2.length);
        for (var index = 0; index < items2.length; index++) {
            totalItems[totalItems.length] = items2[index];
        }
        totalItems[totalItems.length] = li3.replace("{0}", items3.length);
        for (var index = 0; index < items3.length; index++) {
            totalItems[totalItems.length] = items3[index];
        }

        for (var index = 0; index < totalItems.length; index++) {
            ul.append(totalItems[index]);
        }

        //刷新ul
        ul.listview('refresh');

        //设置li的click响应函数
        $("#taskUl li").each(function () {
            if ($(this).attr("id") != undefined) {
                $(this).click(function () {
                    var taskId = $(this).attr("id");
                    showPage("taskDetailPage", "taskId=" + taskId);
                });
            }
        });
    }
    //根据指定的任务ID将任务数据显示到任务详情页面
    function showTaskOnDetailPage(taskId) {
        //先清空页面
        clearTaskDetailPage();

        //如果当前存在任务ID，则加载并显示在任务详情页面
        if (taskId != null && taskId != "") {
            var task = loadTask(taskId);
            if (task != null) {
                $("#taskSubject").html(task.subject);
                $("#taskBody").html(task.body);

                if (task.priority == "0") {
                    $("#radio-level-1").attr('checked', true);
                }
                else if (task.priority == "1") {
                    $("#radio-level-2").attr('checked', true);
                }
                else if (task.priority == "2") {
                    $("#radio-level-3").attr('checked', true);
                }
                $("input[type='radio']").checkboxradio("refresh");

                $("#taskDueTime").val(task.dueTime);

                $("#complete").val(task.isCompleted);
                $("#complete").slider('refresh');
            }
        }
    }
    //根据指定的任务ID初始化任务编辑页面
    function showTaskOnEditPage(taskId) {
        //先清空边界页面
        clearTaskEditPage();

        //如果当前存在任务ID，则加载并显示在任务编辑页面
        if (taskId != null && taskId != "") {
            var task = loadTask(taskId);
            if (task != null) {
                $("#subject").val(task.subject);
                $("#body").val(task.body);

                if (task.priority == "0") {
                    $("#radio-choice-1").attr('checked', true);
                }
                else if (task.priority == "1") {
                    $("#radio-choice-2").attr('checked', true);
                }
                else if (task.priority == "2") {
                    $("#radio-choice-3").attr('checked', true);
                }
                $("input[type='radio']").checkboxradio("refresh");

                $("#duetime").val(task.dueTime);

                $("#isCompleted").val(task.isCompleted);
                $("#isCompleted").slider('refresh');
            }
        }
    }
    //清空任务详情页面
    function clearTaskDetailPage() {
        $("#taskSubject").html("");
        $("#taskBody").html("");
        $("#radio-level-1").attr('checked', true);
        $("#radio-level-2").attr('checked', false);
        $("#radio-level-3").attr('checked', false);
        $("input[name='level']").checkboxradio("refresh");
        $("#taskDueTime").val("");
        $("#complete").val("false");
        $("#complete").slider('refresh');
    }
    //清空任务编辑页面
    function clearTaskEditPage() {
        $("#subject").val("");
        $("#body").val("");
        $("#radio-choice-1").attr('checked', true);
        $("#radio-choice-2").attr('checked', false);
        $("#radio-choice-3").attr('checked', false);
        $("input[name='priority']").checkboxradio("refresh");
        $("#duetime").val("");
        $("#isCompleted").val("false");
        $("#isCompleted").slider('refresh');
    }

    //显示指定页面
    function showPage(pageId, params, transition, direction) {
        if (transition == undefined || transition == null || transition == "") {
            transition = "slide";
        }

        var targetPage = "";
        if (params != null && params != undefined && params != "") {
            targetPage = '#' + pageId + "?" + params;
        }
        else {
            targetPage = '#' + pageId;
        }

        if (direction != undefined || direction != null || direction != "") {
            $.mobile.changePage(targetPage, { transition: transition, direction: direction });
        }
        else {
            $.mobile.changePage(targetPage, { transition: transition });
        }
    }

    //----------------------------------------------------------------
    //以下代码将一些需要被UI用到的函数暴露出去
    //----------------------------------------------------------------

    window.showPage = showPage;

    window.setCurrentTaskList = setCurrentTaskList;
    window.getCurrentTaskList = getCurrentTaskList;

    window.validateUser = validateUser;
    window.addTaskList = addTaskList;
    window.loadAllTaskList = loadAllTaskList;
    window.addOrUpdateTask = addOrUpdateTask;
    window.autoUpdateTaskPriority = autoUpdateTaskPriority;
    window.autoUpdateTaskDueTime = autoUpdateTaskDueTime;
    window.autoUpdateTaskStatus = autoUpdateTaskStatus;

    window.showAllTasklist = showAllTasklist;
    window.showTasks = showTasks;
    window.showTaskOnDetailPage = showTaskOnDetailPage;
    window.showTaskOnEditPage = showTaskOnEditPage;
    window.clearTaskDetailPage = clearTaskDetailPage;
    window.clearTaskEditPage = clearTaskEditPage;
})();