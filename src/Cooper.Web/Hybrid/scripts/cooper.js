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

    //显示所有任务列表
    function showAllTasklist() {
        var ul = $('#taskListUl');
        ul.append('<li id="1"><a>Task List1<span class="ui-li-count">12</span></a></li>');
        ul.append('<li id="2"><a>Task List2<span class="ui-li-count">12</span></a></li>');
        ul.append('<li id="3"><a>Task List3<span class="ui-li-count">12</span></a></li>');
        ul.append('<li id="4"><a>Task List4<span class="ui-li-count">12</span></a></li>');
        ul.listview('refresh');

        $('#taskListUl li').click(function () {
            showTaskPanel($(this).attr("id"));
        });
    }
    //显示指定任务列表中的所有任务
    function showTasks(taskListId) {
        var ul = $('#taskUl');

        ul.append('<li style="background-color: #ebebeb">马上完成 <span class="ui-li-count">2</span></li>');
        ul.append('<li id="1"><a><h3><img src="images/complete-small.png"><span>task1</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');
        ul.append('<li id="2"><a><h3><img src="images/complete-small.png"><span>task2</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');

        ul.append('<li style="background-color: #ebebeb">稍后完成 <span class="ui-li-count">2</span></li>');
        ul.append('<li id="3"><a><h3><img src="images/complete-small.png"><span>task3</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');
        ul.append('<li id="4"><a><h3><img src="images/complete-small.png"><span>task4</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');

        ul.append('<li style="background-color: #ebebeb">迟些再说 <span class="ui-li-count">2</span></li>');
        ul.append('<li id="5"><a><h3><img src="images/complete-small.png"><span>task5</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');
        ul.append('<li id="6"><a><h3><img src="images/complete-small.png"><span>task6</span></h3><p>content</p><p><strong>duetime</strong></p></a></li>');

        ul.listview('refresh');

        $('#taskUl li').click(function () {
            showTaskEditPanel($(this).attr("id"));
        });
    }

    //显示指定面板
    function showPanel(panelId) {
        $.mobile.changePage('#' + panelId, { transition: "slide", direction: 'reverse' });
    }
    //显示任务面板
    function showTaskPanel(taskListId) {
        showPanel("taskPanel");
        showTasks(taskListId);
    }
    //显示任务详情面板
    function showTaskDetailPanel(taskId) {
        showPanel("taskDetailPanel");
        //获取数据，TODO
    }
    //显示任务新增编辑面板
    function showTaskEditPanel(taskId) {
        showPanel("taskEditPanel");
        //获取数据，TODO
    }

    function validateUser(userName, password) {
        //TODO，在这里验证用户名和密码
        return true;
    }
    function login(userName, password) {
        if (validateUser(userName, password)) {
            showPanel("taskListPanel");
            showAllTasklist();
        }
        else {
            alert(lang.loginFailed);
        }
    }

    window.showPanel = showPanel;
    window.login = login;
})();


