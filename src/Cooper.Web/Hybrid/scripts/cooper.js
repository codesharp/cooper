//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="../jquery/jquery-1.7.2.min.js" />
///<reference path="../jquery/jquery.mobile-1.1.0.min.js" />
///<reference path="../jquery/jquery.json-2.3.min.js" />
///<reference path="../jquery/jquery.cookie.js" />
///<reference path="hybrid.js" />
///<reference path="lang.js" />

/////////////////////////////////////////////////////////////////////////////////////////

(function () {
    //内存存储当前账号的所有任务表
    var taskLists = null;
    //内存存储当前任务列表下的所有任务
    var tasksInCurrentList = null;
    //表示当前任务列表是否可编辑
    var isCurrentTaskListEditable = null;

    //获取当前任务列表内的所有任务，存放在本地内存
    function loadTasksInCurrentList(listId, callback) {
        getTasksByPriority(listId, "all", function (result) {
            if (result.status) {
                tasksInCurrentList = result.data.tasks;
                isCurrentTaskListEditable = result.data.isListEditable;
                if (callback != null) {
                    callback();
                }
            }
        });
    }
    //从当前的本地js内存中缓存的当前任务表的所有任务中查找指定的任务
    function loadTaskFromLocal(taskId) {
        for (var index = 0; index < tasksInCurrentList.length; index++) {
            if (tasksInCurrentList[index].id == taskId) {
                return tasksInCurrentList[index];
            }
        }
    }
    //获取指定ID的任务，从本地内存加载
    function loadTask(taskId, callback) {
        if (tasksInCurrentList == null) {
            loadTasksInCurrentList(pageData.listId, function () {
                var task = loadTaskFromLocal(taskId);
                callback(task);
            });
        }
        else {
            var task = loadTaskFromLocal(taskId);
            callback(task);
        }
    }

    //----------------------------------------------------------------
    //以下函数用于显示数据到界面或者更新界面的数据
    //----------------------------------------------------------------

    //清空taskListPage
    function clearTaskListPage() {
        $("#taskListPage #taskListUl > li").remove();
    }
    //清空taskPage
    function clearTaskPage() {
        $("#taskPage #taskPageTitle").html("");
        $("#taskPage #taskUl > li").remove();
        $("#taskPage #displayInfoWhenNoTaskExist").html("");
        $("#taskPage #addFirstTaskButton").hide();
    }
    //清空任务详情页面
    function clearTaskDetailPage() {
        $("#taskDetailPage #taskSubject").html("");
        $("#taskDetailPage #taskBody").html("");
        $("#taskDetailPage #radio-taskPriority-0").attr('checked', true);
        $("#taskDetailPage #radio-taskPriority-1").attr('checked', false);
        $("#taskDetailPage #radio-taskPriority-2").attr('checked', false);
        $("input[name='taskPriority']").checkboxradio("refresh");
        $("#taskDetailPage #taskDueTime").val("");
        $("#taskDetailPage #isTaskCompleted").val("false");
        $("#taskDetailPage #isTaskCompleted").slider('refresh');
        $("#taskDetailPage #deleteTaskButton").hide();
    }
    //清空任务编辑页面
    function clearTaskEditPage() {
        $("#taskEditPage #subject").val("");
        $("#taskEditPage #body").val("");
        $("#taskEditPage #radio-priority-0").attr('checked', true);
        $("#taskEditPage #radio-priority-1").attr('checked', false);
        $("#taskEditPage #radio-priority-2").attr('checked', false);
        $("input[name='priority']").checkboxradio("refresh");
        $("#taskEditPage #duetime").val("");
        $("#taskEditPage #isCompleted").val("false");
        $("#taskEditPage #isCompleted").slider('refresh');
    }
    //将给定的任务列表显示到界面
    function showTaskLists(taskLists) {
        //清空任务列表
        clearTaskListPage();

        //填充任务列表
        var ul = $('#taskListPage #taskListUl');
        var liArray = [];
        for (var index = 0; index < taskLists.length; index++) {
            var taskList = taskLists[index];
            var li = '<li id="' + taskList.id + '"><a data-transition="slide" href="#taskPage?listId=' + taskList.id + '&isCompleted=all">' + taskList.name + '</a></li>';
            liArray[liArray.length] = li;
        }
        ul.append(liArray.join(''));

        //刷新任务列表
        ul.listview('refresh');
    }
    //加载并显示任务列表
    function loadAndShowTaskLists() {
        showLoading();
        getTasklists(function (result) {
            if (result.status) {
                showTaskLists(result.data.taskLists);
                taskLists = result.data.taskLists;
                hideLoading();
            }
            else {
                hideLoading();
                alert(result.message);
            }
        });
    }
    //在任务页面显示当前的任务表的名称
    function showTaskListName(listId) {
        if (taskLists == null) {
            getTasklists(function (result) {
                if (result.status) {
                    taskLists = result.data.taskLists;
                    for (var index = 0; index < taskLists.length; index++) {
                        if (taskLists[index].id == listId) {
                            $("#taskPage #taskPageTitle").html(taskLists[index].name);
                            break;
                        }
                    }
                }
            });
        }
        else {
            for (var index = 0; index < taskLists.length; index++) {
                if (taskLists[index].id == listId) {
                    $("#taskPage #taskPageTitle").html(taskLists[index].name);
                    break;
                }
            }
        }
    }
    //显示指定任务列表中的任务
    function showTasks(listId, tasks, isCompleted) {
        //清空任务
        clearTaskPage();

        //显示任务列表的标题
        showTaskListName(listId);

        var li1 = '<li style="background-color: #ebebeb">尽快完成<span class="ui-li-count">{0}</span></li>';
        var li2 = '<li style="background-color: #ebebeb">稍后完成<span class="ui-li-count">{0}</span></li>';
        var li3 = '<li style="background-color: #ebebeb">迟些再说<span class="ui-li-count">{0}</span></li>';

        var items1 = [];
        var items2 = [];
        var items3 = [];

        //填充任务
        for (var index = 0; tasks != null && index < tasks.length; index++) {
            var task = tasks[index];
            var img = null;
            if (task.isCompleted.toString() == "true") {
                img = "complete-small.png";
            }
            else {
                img = "incomplete-small.png";
            }
            var li = '<li id="' + listId + '|' + task.id + '"><a><h3><img id="' + task.id + '" src="images/' + img + '"><span>' + task.subject + '</span></h3><p>' + task.body + '</p><p><strong>' + task.dueTime + '</strong></p></a></li>';

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

        var ul = $("#taskPage #taskUl");
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

            ul.append(totalItems.join(''));
        }

        //刷新ul
        ul.listview('refresh');

        if (isCurrentTaskListEditable == true) {
            //设置列表每行的表示任务是否完成状态的Click响应函数，为了实现在用户点击是否完成的图标时，能够自动切换任务的状态
            $('#taskUl img').click(function (event) {
                var taskId = $(this).attr("id");
                var imgUrl = $(this).attr("src");
                if (imgUrl.indexOf('images/complete-small.png') != -1) {
                    updateTaskProperty(taskId, 'isCompleted', 'false', function (result) {
                        if (result.status) {
                            $('#taskUl #' + taskId).attr("src", 'images/incomplete-small.png');
                        }
                        else {
                            alert(result.message);
                        }
                    });
                }
                else if (imgUrl.indexOf('images/incomplete-small.png') != -1) {
                    updateTaskProperty(taskId, 'isCompleted', 'true', function (result) {
                        if (result.status) {
                            $('#taskUl #' + taskId).attr("src", 'images/complete-small.png');
                        }
                        else {
                            alert(result.message);
                        }
                    });
                }
                //停止冒泡，确保不会跳转到任务详情页
                event.stopPropagation();
            });
        }

        //设置列表每行的Click响应函数
        $('#taskUl li').click(function (event) {
            var entry = $(this).attr("id").split('|');
            var listId = entry[0];
            var taskId = entry[1];
            showPage("taskDetailPage", 'listId=' + listId + '&taskId=' + taskId);
        });

        //根据当前任务表是否可编辑来显示或隐藏新增任务按钮
        if (isCurrentTaskListEditable == false) {
            $("#addNewTaskButton").hide();
        }
        else if (isCurrentTaskListEditable == true) {
            $("#addNewTaskButton").show();
        }

        //根据任务是否存在现实友好信息
        $("#addFirstTaskButton").hide();
        $("#displayInfoWhenNoTaskExist").html("");
        if (isCompleted == "all") {
            if (items1.length == 0 && items2.length == 0 && items3.length == 0) {
                if (isCurrentTaskListEditable == true) {
                    $("#addFirstTaskButton").show();
                }
            }
        }
        else if (isCompleted == "true") {
            if (items1.length == 0 && items2.length == 0 && items3.length == 0) {
                $("#displayInfoWhenNoTaskExist").html(lang.noCompletedTaskPromptInfo);
            }
        }
        else if (isCompleted == "false") {
            if (items1.length == 0 && items2.length == 0 && items3.length == 0) {
                $("#displayInfoWhenNoTaskExist").html(lang.noUnCompletedTaskPromptInfo);
            }
        }
    }
    //加载并显示指定任务列表中的符合条件的所有任务
    function loadAndShowTasks(listId, isCompleted) {
        $("#taskPage #addNewTaskButton").attr("href", "#taskEditPage?listId=" + listId + "&isCompleted=" + isCompleted);
        $("#taskPage #addFirstTaskButton").attr("href", "#taskEditPage?listId=" + listId + "&isCompleted=" + isCompleted);
        $("#taskPage #showAllTasksButton").attr("href", "#taskPage?listId=" + listId + "&isCompleted=all");
        $("#taskPage #showCompletedTasksButton").attr("href", "#taskPage?listId=" + listId + "&isCompleted=true");
        $("#taskPage #showUnCompletedTasksButton").attr("href", "#taskPage?listId=" + listId + "&isCompleted=false");
        showLoading();
        getTasksByPriority(listId, isCompleted, function (result) {
            if (result.status) {
                tasksInCurrentList = result.data.tasks;
                isCurrentTaskListEditable = result.data.isListEditable;
                showTasks(listId, result.data.tasks, isCompleted);
                hideLoading();
            }
            else {
                hideLoading();
                alert(result.message);
            }
        });
    }
    //根据指定的任务ID将任务数据显示到任务详情页面
    function showTaskOnDetailPage(taskId) {
        //先清空页面
        clearTaskDetailPage();

        //如果当前存在任务ID，则加载并显示在任务详情页面
        if (taskId != null && taskId != "") {
            loadTask(taskId, function (task) {
                if (task != null) {
                    $("#taskDetailPage #taskSubject").html(task.subject);
                    $("#taskDetailPage #taskBody").html(task.body);

                    if (task.priority == "0" || task.priority == "1" || task.priority == "2") {
                        $("#taskDetailPage #radio-taskPriority-" + task.priority).attr('checked', true);
                    }

                    $("#taskDetailPage #taskDueTime").val(task.dueTime);
                    if (task.isCompleted != null) {
                        $("#taskDetailPage #isTaskCompleted").val(task.isCompleted.toString());
                    }

                    //设置控件是否可编辑
                    if (isCurrentTaskListEditable == true) {
                        $("#taskDetailPage #taskDueTime").removeAttr('disabled');
                        $("#taskDetailPage #isTaskCompleted").slider('enable');
                        $("input[name='taskPriority']").checkboxradio('enable');
                        $("#taskDetailPage #gotoTaskEditPage").attr("href", "#taskEditPage?listId=" + pageData.listId + "&taskId=" + task.id);
                        $("#taskDetailPage #gotoTaskEditPage").show();
                        $("#taskDetailPage #deleteTaskButton").show();
                    }
                    else {
                        $("#taskDetailPage #taskDueTime").attr('disabled', true);
                        $("#taskDetailPage #isTaskCompleted").slider('disable');
                        $("input[name='taskPriority']").checkboxradio('disable');
                        $("#taskDetailPage #gotoTaskEditPage").attr("href", "#");
                        $("#taskDetailPage #gotoTaskEditPage").hide();
                        $("#taskDetailPage #deleteTaskButton").hide();
                    }

                    $("input[name='taskPriority']").checkboxradio("refresh");
                    $("#taskDetailPage #isTaskCompleted").slider('refresh');
                }
            });
        }
    }
    //根据指定的任务ID初始化任务编辑页面
    function showTaskOnEditPage(taskId) {
        //先清空页面
        clearTaskEditPage();

        //如果当前存在任务ID，则加载并显示在任务编辑页面
        if (taskId != null && taskId != "") {
            loadTask(taskId, function (task) {
                if (task != null) {
                    $("#taskEditPage #subject").val(task.subject);
                    $("#taskEditPage #body").val(task.body);

                    if (task.priority == "0" || task.priority == "1" || task.priority == "2") {
                        $("#taskEditPage #radio-priority-" + task.priority).attr('checked', true);
                    }

                    $("#taskEditPage #duetime").val(task.dueTime);
                    if (task.isCompleted != null) {
                        $("#taskEditPage #isCompleted").val(task.isCompleted.toString());
                    }

                    //设置控件是否可编辑
                    if (isCurrentTaskListEditable == true) {
                        $("#taskEditPage #subject").removeAttr('disabled');
                        $("#taskEditPage #body").removeAttr('disabled');
                        $("#taskEditPage #duetime").removeAttr('disabled');
                        $("#taskEditPage #isCompleted").slider('enable');
                        $("input[name='priority']").checkboxradio('enable');
                        $("#taskEditPage #saveTaskButton").show();
                    }
                    else {
                        $("#taskEditPage #subject").attr('disabled', true);
                        $("#taskEditPage #body").attr('disabled', true);
                        $("#taskEditPage #duetime").attr('disabled', true);
                        $("#taskEditPage #isCompleted").slider('disable');
                        $("input[name='priority']").checkboxradio('disable');
                        $("#taskEditPage #saveTaskButton").hide();
                    }

                    $("input[name='priority']").checkboxradio("refresh");
                    $("#taskEditPage #isCompleted").slider('refresh');
                }
            });
        }
    }
    //更新一个Task的单个属性，任务详情页面会用到此函数
    function updateTaskProperty(taskId, propertyName, propertyValue, callback) {
        loadTask(taskId, function (task) {
            //如果不存在则直接退出
            if (task == null) {
                return;
            }

            //根据判断更新相应属性
            if (propertyName == "priority") {
                task.priority = propertyValue;
                task.isDirty = true;
            }
            else if (propertyName == "dueTime") {
                task.dueTime = propertyValue;
                task.isDirty = true;
            }
            else if (propertyName == "isCompleted") {
                task.isCompleted = propertyValue;
                task.isDirty = true;
            }

            updateTask(pageData.listId, task, getTaskChanges(task), callback);
        });
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
    //显示Loading效果
    function showLoading(loadingMessage) {
        $.mobile.loadingMessageTextVisible = true;
        if (loadingMessage == undefined || loadingMessage == null) {
            $.mobile.loadingMessage = "页面加载中...";
        }
        else {
            $.mobile.loadingMessage = loadingMessage;
        }
        $.mobile.showPageLoadingMsg();
    }
    //隐藏Loading效果
    function hideLoading() {
        $.mobile.hidePageLoadingMsg();
    }
    //判断当前访问页面是否在移动设备
    function isMobileDevice() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent);
    }
    //判断是否是空字符串
    function isNullOrEmpty(value) {
        if (value == null || $.trim(value) == "") {
            return true;
        }
        return false;
    }

    //----------------------------------------------------------------
    //按钮事件响应绑定
    //----------------------------------------------------------------

    //登录页面:“确定”按钮事件响应
    $(document).delegate("#loginPage #loginButton", "click", function () {
        var userName = $("#username").val();
        var password = $("#password").val();
        if (isNullOrEmpty(userName)) {
            alert(lang.usernameCannotEmpty);
            return;
        }
        if (isNullOrEmpty(password)) {
            alert(lang.passwordCannotEmpty);
            return;
        }
		showLoading("登录中...");
        login(userName, password, "normal", function (result) {
            if (!result.status) {
                alert(result.message);
            }
            else {
                if (isMobileDevice()) {
                    syncTaskLists(null, function (result) {
                        if (result.status) {
							hideLoading();
                            showPage("taskListPage");
                        }
                        else {
							hideLoading();
                            alert(result.message);
                        }
                    });
                }
                else {
					hideLoading();
                    $.cookie('cooper_web_loginUser', userName);
                    showPage("taskListPage");
                }
            }
        });
    });
    //登录页面:“跳过”按钮事件响应
    $(document).delegate("#loginPage #skipLoginButton", "click", function () {
        if (isMobileDevice()) {
            login("", "", "anonymous", function (result) {
                if (!result.status) {
                    alert(result.message);
                }
                else {
                    showPage("taskListPage");
                }
            });
        }
    });
    //Setting中的更换账号页面:“注销”按钮事件响应
    $(document).delegate("#setCurrentAccountPage #logoutButton", "click", function () {
		showLoading("注销中...");
        logout(function (result) {
            if (!result.status) {
				hideLoading();
                alert(result.message);
            }
            else {
				hideLoading();
                $('#backToLoginPageButton').click();
            }
        });
    });
    //新增任务列表页面:“确定”按钮事件响应
    $(document).delegate("#addTaskListPage #saveNewTaskListButton", "click", function () {
        var taskListName = $("#tasklistName").val();
        if (isNullOrEmpty(taskListName)) {
            alert(lang.taskListNameCannotEmpty);
            return;
        }

        var taskListId = generateNewTaskListId();
        createTasklist(taskListId, taskListName, function (result) {
            if (!result.status) {
                alert(result.message);
            }
            else {
                showPage("taskListPage");
            }
        });
    });
    //任务列表页面:“刷新”按钮事件响应
    $(document).delegate("#taskListPage #refreshTaskListsButton", "click", function () {
        if (isMobileDevice()) {
            showLoading();
            syncTaskLists(null, function (result) {
                if (result.status) {
                    getTasklists(function (result) {
                        if (result.status) {
                            showTaskLists(result.data.taskLists);
                            taskLists = result.data.taskLists;
                            hideLoading();
                        }
                        else {
                            hideLoading();
                            alert(result.message);
                        }
                    });
                }
                else {
                    hideLoading();
                    alert(result.message);
                }
            });
        }
        else {
            loadAndShowTaskLists();
        }
    });
    //任务编辑页面:“确定”按钮事件响应
    $(document).delegate("#taskEditPage #saveTaskButton", "click", function () {
        var taskId = "";
        if (pageData != null && pageData.taskId != null && pageData.taskId != "") {
            taskId = pageData.taskId;
        }

        var subject = $("#taskEditPage #subject").val();
        var body = $("#taskEditPage #body").val();
        var selectedPriority = $("input[name='priority']:checked");
        var priority = selectedPriority != null ? selectedPriority.val() : "0";
        var dueTime = $("#taskEditPage #duetime").val();
        var isCompleted = $("#taskEditPage #isCompleted").val();

        //Mobile模式下，任务标题不允许为空，因为显示效果不好
        if (isMobileDevice()) {
            if (isNullOrEmpty(subject)) {
                alert(lang.taskSubjectCannotEmpty);
                return;
            }
        }

        if (taskId == "") {
            //新增任务
            var task = new Task();
            task.subject = subject;
            task.body = body;
            task.priority = priority;
            task.dueTime = dueTime;
            task.isCompleted = isCompleted;
            task.isNew = true;

            createTask(pageData.listId, task, getTaskChanges(task), function (result) {
                if (result.status) {
                    history.back();
                }
                else {
                    alert(result.message);
                }
            });
        }
        else {
            //编辑任务
            var task = new Task();
            task.id = taskId;
            task.subject = subject;
            task.body = body;
            task.priority = priority;
            task.dueTime = dueTime;
            task.isCompleted = isCompleted;
            task.isDirty = true;

            updateTask(pageData.listId, task, getTaskChanges(task), function (result) {
                if (result.status) {
                    loadTask(taskId, function (taskInMemory) {
                        taskInMemory.subject = task.subject;
                        taskInMemory.body = task.body;
                        taskInMemory.priority = task.priority;
                        taskInMemory.dueTime = task.dueTime;
                        taskInMemory.isCompleted = task.isCompleted;
                        taskInMemory.isDirty = false;
                        history.back();
                    });
                }
                else {
                    alert(result.message);
                }
            });
        }
    });
    //任务详情页面:“删除”按钮事件响应
    $(document).delegate("#taskDetailPage #deleteTaskButton", "click", function () {
        deleteTask(pageData.listId, pageData.taskId, function (result) {
            if (result.status) {
                history.back();
            }
            else {
                alert(result.message);
            }
        });
    });
    //任务页面:“刷新”按钮事件响应
    $(document).delegate("#taskPage #refreshTasksButton", "click", function () {
        if (isMobileDevice()) {
			if (isCurrentTaskListEditable == false) {
				loadAndShowTasks(pageData.listId, pageData.isCompleted);
			}
			else if (isCurrentTaskListEditable == true) {
				showLoading();
				syncTaskLists(pageData.listId, function (result) {
					if (result.status) {
						getTasksByPriority(pageData.listId, pageData.isCompleted, function (result) {
							if (result.status) {
								tasksInCurrentList = result.data.tasks;
								isCurrentTaskListEditable = result.data.isListEditable;
								showTasks(pageData.listId, result.data.tasks, pageData.isCompleted);
								hideLoading();
							}
							else {
								hideLoading();
								alert(result.message);
							}
						});
					}
					else {
						hideLoading();
						alert(result.message);
					}
				});
			}
        }
        else {
            loadAndShowTasks(pageData.listId, pageData.isCompleted);
        }
    });
    //任务页面:“个人任务”Tab事件响应
    $(document).delegate("#taskPage #showAllTasksButton", "click", function () {
        if (!isMobileDevice()) {
            window.location = this.href;
        }
        loadAndShowTasks(pageData.listId, "all");
    });
    //任务页面:“已完成”Tab事件响应
    $(document).delegate("#taskPage #showCompletedTasksButton", "click", function () {
        if (!isMobileDevice()) {
            window.location = this.href;
        }
        loadAndShowTasks(pageData.listId, "true");
    });
    //任务页面:“未完成”Tab事件响应
    $(document).delegate("#taskPage #showUnCompletedTasksButton", "click", function () {
        if (!isMobileDevice()) {
            window.location = this.href;
        }
        loadAndShowTasks(pageData.listId, "false");
    });

    //以下三个事件响应函数用户在任务详情页面自动更新用户修改
    //优先级
    $(document).delegate("[name=taskPriority]", "change", function () {
        var priority = $('input[name=taskPriority]:checked').val();
        updateTaskProperty(pageData.taskId, "priority", priority, function (result) {
            if (!result.status) {
                alert(result.message);
            }
        });
    });
    //是否完成
    $(document).delegate("#taskDetailPage #isTaskCompleted", "change", function () {
        var isCompleted = $(this).val();
        updateTaskProperty(pageData.taskId, "isCompleted", isCompleted, function (result) {
            if (!result.status) {
                alert(result.message);
            }
        });
    });
    //截止时间
    $(document).delegate("#taskDetailPage #taskDueTime", "blur", function () {
        var dueTime = $(this).attr("value");
        updateTaskProperty(pageData.taskId, "dueTime", dueTime, function (result) {
            if (!result.status) {
                alert(result.message);
            }
        });
    });

    //----------------------------------------------------------------
    //Jquery Mobile Event Binding
    //----------------------------------------------------------------

    $(document).delegate("#loginPage", "pagebeforeshow", function (e, data) {
        if (isMobileDevice()) {
            $('#skipLoginButton').show();
        }
        else {
            $('#skipLoginButton').hide();
        }
    });
    $(document).delegate("#taskListPage", "pagebeforeshow", function (e, data) {
        clearTaskListPage();
    });
    $(document).delegate("#taskListPage", "pageshow", function (e, data) {
        loadAndShowTaskLists();
    });
    $(document).delegate("#addTaskListPage", "pagebeforeshow", function (e, data) {
        $("#tasklistName").val("");
    });
    $(document).delegate("#taskPage", "pagebeforeshow", function (e, data) {
        clearTaskPage();
    });
    $(document).delegate("#taskPage", "pageshow", function (e, data) {
        loadAndShowTasks(pageData.listId, pageData.isCompleted);

        if (pageData.isCompleted == null || pageData.isCompleted == "all") {
            $("#taskPage #showAllTasksButton").addClass("ui-btn-active");
        }
        else if (pageData.isCompleted == "true") {
            $("#taskPage #showCompletedTasksButton").addClass("ui-btn-active");
        }
        else if (pageData.isCompleted == "false") {
            $("#taskPage #showUnCompletedTasksButton").addClass("ui-btn-active");
        }
    });
    $(document).delegate("#taskDetailPage", "pagebeforeshow", function (e, data) {
        clearTaskDetailPage();
    });
    $(document).delegate("#taskDetailPage", "pageshow", function (e, data) {
        showTaskOnDetailPage(pageData.taskId);
    });
    $(document).delegate("#taskEditPage", "pagebeforeshow", function (e, data) {
        clearTaskEditPage();
    });
    $(document).delegate("#taskEditPage", "pageshow", function (e, data) {
        showTaskOnEditPage(pageData.taskId);
    });
    $(document).delegate("#settingPage", "pagebeforeshow", function (e, data) {
        var ul = $('#settingPage #settingListItems');
        $("#settingPage #settingListItems > li").remove();
        var liArray = [];

        if (isMobileDevice()) {
            liArray.push('<li><a href="#setCurrentAccountPage" data-transition="slide">账号设置</a></li>');
            liArray.push('<li><a href="#aboutPage" data-transition="slide">版本信息</a></li>');
        }
        else {
            liArray.push('<li><a href="#aboutPage" data-transition="slide">版本信息</a></li>');
        }

        ul.append(liArray.join(''));
        ul.listview('refresh');
    });
    $(document).delegate("#setCurrentAccountPage", "pagebeforeshow", function (e, data) {
        if (isMobileDevice()) {
            getCurrentUser(function (result) {
                if (result.status) {
                    if (result.data.username != null && result.data.username != "") {
                        $('#currentUserName').html(lang.currentLoginUsernameLabel + result.data.username);
                        $('#logoutButton').show();
                        $('#backToLoginPageButton').hide();
                    }
                    else {
                        $('#currentUserName').html(lang.currentAnonymousUsernameLabel);
                        $('#logoutButton').hide();
                        $('#backToLoginPageButton').show();
                    }
                }
            });
        }
        else {
            $('#currentUserName').html('');
            $('#logoutButton').hide();
            $('#backToLoginPageButton').hide();
        }
    });
})();