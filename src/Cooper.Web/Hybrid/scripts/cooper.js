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
    //用于过滤显示任务状态
    var taskStatusFilter = "";

    //设置当前选中的任务列表
    function setCurrentTaskList(taskListId) {
        currentTaskListId = taskListId;
    }
    //返回当前选中的任务列表
    function getCurrentTaskList() {
        return currentTaskListId;
    }

    //判断当前网络是否可用
    function isNetworkAvailable() {
        //之后需要借助于native接口实现，目前先mock实现
        return true;
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
            task.isEditable = true;
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
            taskStatusFilter = "all";
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

        if (taskArray == null) {
            return;
        }

        var li1 = '<li style="background-color: #ebebeb">马上完成 <span class="ui-li-count">{0}</span></li>';
        var li2 = '<li style="background-color: #ebebeb">稍后完成 <span class="ui-li-count">{0}</span></li>';
        var li3 = '<li style="background-color: #ebebeb">迟些再说 <span class="ui-li-count">{0}</span></li>';

        var items1 = [];
        var items2 = [];
        var items3 = [];

        //填充任务
        for (var index = 0; index < taskArray.length; index++) {
            var task = taskArray[index];

            //如果设置了是否完成的状态过滤条件，则需要进行判断
            if (taskStatusFilter != undefined && taskStatusFilter != null && taskStatusFilter != "all") {
                if (taskStatusFilter != task.isCompleted) {
                    continue;
                }
            }

            var img = null;
            if (task.isCompleted == "true") {
                img = "complete-small.png";
            }
            else {
                img = "incomplete-small.png";
            }
            var li = '<li id="' + task.id + '"><a href=#taskDetailPage?taskId=' + task.id + ' data-transition="slide"><h3><img src="images/' + img + '"><span>' + task.subject + '</span></h3><p>' + task.body + '</p><p><strong>' + task.dueTime + '</strong></p></a></li>';

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

        if (items1.length > 0 || items2.length > 0 || items3.length > 0) {
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
        }

        //刷新ul
        ul.listview('refresh');
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

                if (task.priority == "0" || task.priority == "1" || task.priority == "2") {
                    $("#radio-taskPriority-" + task.priority).attr('checked', true);
                    $("input[name='taskPriority']").checkboxradio("refresh");
                }

                $("#taskDueTime").val(task.dueTime);

                $("#isTaskCompleted").val(task.isCompleted);
                $("#isTaskCompleted").slider('refresh');
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

                if (task.priority == "0" || task.priority == "1" || task.priority == "2") {
                    $("#radio-priority-" + task.priority).attr('checked', true);
                    $("input[name='priority']").checkboxradio("refresh");
                }

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
        $("#radio-taskPriority-0").attr('checked', true);
        $("#radio-taskPriority-1").attr('checked', false);
        $("#radio-taskPriority-2").attr('checked', false);
        $("input[name='taskPriority']").checkboxradio("refresh");
        $("#taskDueTime").val("");
        $("#isTaskCompleted").val("false");
        $("#isTaskCompleted").slider('refresh');
    }
    //清空任务编辑页面
    function clearTaskEditPage() {
        $("#subject").val("");
        $("#body").val("");
        $("#radio-priority-0").attr('checked', true);
        $("#radio-priority-1").attr('checked', false);
        $("#radio-priority-2").attr('checked', false);
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

        if (direction != undefined && direction != null && direction != "") {
            $.mobile.changePage(targetPage, { transition: transition, direction: direction });
        }
        else {
            $.mobile.changePage(targetPage, { transition: transition });
        }
    }

    //----------------------------------------------------------------
    //Event Binding
    //----------------------------------------------------------------

    //刷新任务列表
    $(document).delegate("#refreshTaskListsImg", "click", function () {
        $.mobile.showPageLoadingMsg();
        showAllTasklist();
        setTimeout(function () { $.mobile.hidePageLoadingMsg(); }, 1000);
    });
    //新增编辑任务确定按钮
    $(document).delegate("#addOrUpdateTaskButton", "click", function () {
        var id = "";
        if (pageData != undefined && pageData.taskId != undefined && pageData.taskId != null) {
            id = pageData.taskId;
        }

        var subject = $("#subject").val();
        var body = $("#body").val();
        var selectedPriority = $("input[name='priority']:checked");
        var priority = selectedPriority != null ? selectedPriority.val() : 0;
        var dueTime = $("#duetime").val();
        var isCompleted = $("#isCompleted").val();

        addOrUpdateTask(id, subject, body, priority, dueTime, isCompleted, function (result) {
            if (!result.success) {
                alert(result.message);
            }
            else {
                showPage("taskPage", "listId=" + getCurrentTaskList(), "slidedown", "reverse");
            }
        });
    });
    //登录页面确定按钮
    $(document).delegate("#loginButton", "click", function () {
        validateUser($("#username").val(), $("#password").val(), function (result) {
            if (!result.success) {
                alert(result.message);
            }
            else {
                showPage("taskListPage");
            }
        });
    });
    //转到到任务编辑页面
    $(document).delegate("#gotoTaskEditPage", "click", function () {
        showPage("taskEditPage", "taskId=" + pageData.taskId, "slideup");
    });
    //刷新任务页面
    $(document).delegate("#refreshTasksImg", "click", function () {
        $.mobile.showPageLoadingMsg();
        showTasks(getCurrentTaskList());
        setTimeout(function () { $.mobile.hidePageLoadingMsg(); }, 1000);
    });

    //显示个人任务，包含已完成和未完成的所有任务
    $(document).delegate("#showAllTasksImg", "click", function () {
        taskStatusFilter = "all";
        showTasks(pageData.listId);
    });
    //显示已完成任务
    $(document).delegate("#showCompletedTasksImg", "click", function () {
        taskStatusFilter = "true";
        showTasks(pageData.listId);
    });
    //显示未完成任务
    $(document).delegate("#showUnCompletedTasksImg", "click", function () {
        taskStatusFilter = "false";
        showTasks(pageData.listId);
    });

    //新增任务列表确定按钮
    $(document).delegate("#saveNewTaskList", "click", function () {
        addTaskList($("#tasklistName").val(), function (result) {
            if (!result.success) {
                alert(result.message);
                return false;
            }
        });
    });

    //以下三个事件响应函数用户在任务详情页面自动更新用户修改
    //优先级
    $(document).delegate("[name=taskPriority]", "change", function () {
        var priority = $('input[name=taskPriority]:checked').val();
        autoUpdateTaskPriority(pageData.taskId, priority, function (result) {
            if (!result.success) {
                alert(result.message);
            }
        });
    });
    //是否完成
    $(document).delegate("#isTaskCompleted", "change", function () {
        var isCompleted = $(this).val();
        autoUpdateTaskStatus(pageData.taskId, isCompleted, function (result) {
            if (!result.success) {
                alert(result.message);
            }
        });
    });
    //截止时间
    $(document).delegate("#taskDueTime", "blur", function () {
        var dueTime = $(this).attr("value");
        autoUpdateTaskDueTime(pageData.taskId, dueTime, function (result) {
            if (!result.success) {
                alert(result.message);
            }
        });
    });

    //tasklist page event handlers
    $(document).delegate("#taskListPage", "pageinit", function () {
        loadAllTaskList(function (result) {
            if (!result.success) {
                alert(result.message);
            }
        });
    });
    $(document).delegate("#taskListPage", "pagebeforeshow", function () {
        showAllTasklist();
    });
    //task edit page event handlers
    $(document).delegate("#addTaskListPage", "pagebeforeshow", function () {
        $("#tasklistName").val("");
    });
    //task page event handlers
    $(document).delegate("#taskPage", "pagebeforeshow", function () {
        setCurrentTaskList(pageData.listId);
        var taskArray = loadTasksFromCurrentTaskList();
        if (taskArray == null || taskArray.length == 0) {
            $("#addFirstTaskButton").show();
        }
        else {
            $("#addFirstTaskButton").hide();
        }
        $('#showAllTasksImg').click();
    });
    //task detail page event handlers
    $(document).delegate("#taskDetailPage", "pagebeforeshow", function () {
        showTaskOnDetailPage(pageData.taskId);
    });
    //task edit page event handlers
    $(document).delegate("#taskEditPage", "pagebeforeshow", function () {
        clearTaskEditPage();
        if (pageData != undefined && pageData.taskId != undefined && pageData.taskId != null && pageData.taskId != "") {
            showTaskOnEditPage(pageData.taskId);
        }
    });
})();