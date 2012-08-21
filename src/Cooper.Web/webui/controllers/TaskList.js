//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />

//取代taskpage.js
function TaskListCtrl($scope, $rootScope, $element, $routeParams) {
    //page内全局元素
    var $el_wrapper_region = null;
    var $el_wrapper_detail = null;
    var $el_cancel_delete = null;

    var cached_tasks = null;
    var cached_sorts = null;
    var changes = []; //用于记录未提交至server的变更

    var ui_list_helper, ui_list_helper_priority, ui_list_helper_due;
    var isShowArchive = false; //是否显示归档区域
    var currentMode = byPriority; //当前列表模式，默认使用优先级列表模式
    var currentList = null; //当前任务表标识

    //team相关
    var currentTeam = '', //当前团队标识
        currentProject = '', //当前项目标识
        currentTeamMembers = [], //当前团队所有成员
        currentTeamProjects = []; //当前团队所有项目

    var timer;
    var idChanges = {};
    var preSorts = null;

    var tryfail = false; //异常流模拟标识

    // *****************************************************
    // taskpage.js main changes here
    // *****************************************************
    //为团队功能填充全局变量
    if ($scope.team) {
        currentTeam = $scope.team.id;
        currentTeamProjects = currentTeam.projects;
        currentTeamMembers = currentTeam.members;
    }
    if ($scope.project)
        currentProject = $scope.project.id;
    ////////////////////////////////////////////////////////////////////////////////////////
    //all=来自server的所有任务数组
    //sorts=分组排序
    function init(all, sorts) {
        //清理
        if (cached_tasks)
            for (var i in cached_tasks)
                if (cached_tasks[i])
                    cached_tasks[i].dispose();
        if (cached_sorts)
            for (var i in cached_sorts)
                if (cached_sorts[i])
                    cached_sorts[i].dispose();
        //构建本地缓存
        cached_tasks = {};
        for (var i = 0; i < all.length; i++)
            cached_tasks[all[i]['ID'].toString()] = new Task(all[i]);
        debuger.info('original tasks', all);
        debuger.info('cached tasks', cached_tasks);
        //构建分组排序缓存 {'0':List}
        cached_sorts = {};
        for (var i = 0; i < sorts.length; i++)
            cached_sorts[sorts[i]['Key']] = new Sort(
            sorts[i]['By'],
            sorts[i]['Key'],
            sorts[i]['Name'],
            sorts[i]['Indexs']);
        debuger.info('original sorts', sorts);
        debuger.info('cached sorts', cached_sorts);
    }

    //全局按钮函数
    function appendTask() { ui_list_helper.appendTask(); }
    function toggleTasks() { ui_list_helper.toggleTasks(); }
    function deleteTask() { ui_list_helper.deleteTask(); }
    //撤销删除
    //UNDONE:重演设计未完成之前通过重新加载来解决恢复
    function cancelDelete() { ui_list_helper.cancelDelete(); currentMode(); }
    //继续删除
    function continueDelete() { ui_list_helper.continueDelete(); }
    function syncAccount() { $('#syncModal').modal('show'); }
    function archiveTasks() { ui_list_helper.archiveTasks(); }
    function hideArchive() { isShowArchive = false; if (ui_list_helper == ui_list_helper_priority) byPriority(); }
    function showArchive() { isShowArchive = true; if (ui_list_helper == ui_list_helper_priority) byPriority(); }
    //taskfolder ui
    function folder(id) {
        $('#taskFolder .flag_changeFolder').unbind().click(function () { folder($(this).attr('id')); });
        $('#taskFolder_title').html($('#taskFolder .flag_changeFolder[id="' + id + '"]').find('span').html());
        currentMode(function () {
            currentList = id == 0 ? '' : id;
            $('.flag_removeTaskFolder')[isNaN(parseInt(currentList)) ? 'hide' : 'show']();
        }, function () {
            var temp = parseInt(id);
            if (isNaN(temp))
                $('.flag_by').hide();
        });
    }
    function addTaskFolder(id, name) {
        var $e = $(render($('#tmp_taskFolder_item').html(), { id: id }));
        $e.find('span').text(name);
        $('#taskFolder').append($e);
    }
    function doAddTaskFolder(btn) {
        var $btn = $(btn);
        var val = $.trim($btn.prev().val());
        $btn.parent()[val == '' ? 'addClass' : 'removeClass']('error');
        if (val == '') return;

        $.post(url_taskFolder_create, { tryfail: tryfail, name: val, type: 'personal' }, function (d) {
            endRequest();
            debuger.info('new taskFolder#' + d);
            $btn.prev().val('');
            $('#taskFolderModal').modal('hide');
            addTaskFolder(d, val);
            folder(d);
        });
    }
    function doRemoveTaskFolder() {
        if (!confirm(lang.confirm_delete_taskFolder)) return;

        $.post(url_taskFolder_delete, { tryfail: tryfail, id: currentList }, function () {
            endRequest();
            debuger.info('remove taskFolder#' + currentList);
            $('#taskFolder a[id="' + currentList + '"]').parent().remove();
            folder(0);
        });
    }
    //team
    function team(id) {
        currentMode(function () { currentTeam = id; }, function () { });
    }
    function project(id) {
        currentMode(function () { currentProject = id; }, function () { });
    }
    function doRemoveProject() {
        alert('todo');
    }
    ////////////////////////////////////////////////////////////////////////////////////////
    //切换tab时将总是从server读取最新数据以及正确的索引信息
    //切换之间需要内存清理
    //b=是否加载完整列表（包含已完成的）
    function byPriority(fn1, fn2) {
        var b = isShowArchive;
        currentMode = byPriority;
        $el_wrapper_region.empty().append($('#loading').html());
        $el_wrapper_detail.empty();
        fixBeforeSyncOrReload();
        sync(function () {
            if (fn1 && typeof (fn1) == 'function') fn1();
            $.ajax({
                url: b ? url_task_byPriority : url_task_byPriority_incompleted,
                data: getPostData(),
                type: 'POST',
                dataType: 'json',
                beforeSend: function () { $el_wrapper_region.empty().append($('#loading').html()); },
                success: function (data) {
                    endRequest();
                    init(data.List, data.Sorts);
                    (ui_list_helper = ui_list_helper_priority).render(b);
                    fixAfterSyncOrReload(data.Editable);
                    $('.flag_by').hide();
                    $('.flag_by_priority').show();
                    if (fn2 && typeof (fn2) == 'function') fn2();
                }
            });
        });
    }
    function byDueTime(fn1, fn2) {
        currentMode = byDueTime;
        $el_wrapper_region.empty().append($('#loading').html());
        $el_wrapper_detail.empty();
        fixBeforeSyncOrReload();
        sync(function () {
            if (fn1 && typeof (fn1) == 'function') fn1();
            $.ajax({
                url: url_task_byDueTime,
                data: getPostData(),
                type: 'POST',
                dataType: 'json',
                beforeSend: function () { $el_wrapper_region.empty().append($('#loading').html()); },
                success: function (data) {
                    endRequest();
                    init(data.List, data.Sorts);
                    (ui_list_helper = ui_list_helper_due).render();
                    fixAfterSyncOrReload(data.Editable);
                    $('.flag_by').hide();
                    if (fn2 && typeof (fn2) == 'function') fn2();
                }
            });
        });
    }
    //同步执行一些额外的修正工作，主要针对缓冲数据以及模式切换时的全局变量
    function fixBeforeSyncOrReload() {
        if (!ui_list_helper) return;
        //由于此时缓冲的删除记录可能由于delete_timer而未被提交
        ui_list_helper.continueDelete();
    }
    function fixAfterSyncOrReload(e) {
        //刷新全局变量
        //重设排序记录
        preSorts = $.toJSON(getSorts());
        debuger.debug('set preSorts', preSorts);
        //设置是否编辑状态
        setEditable(e);
        //自动选中首行
        firstClick();
    }
    function setEditable(b) {
        ui_list_helper.setEditable(b);
        $('.flag_editable')[b ? 'show' : 'hide']();
    }
    function firstClick() {
        $el_wrapper_region.find('tr.row_task input').first().click();
    }
    function getSorts() {
        var sorts = [];
        for (var i in cached_sorts)
            sorts = $.merge(sorts, [{
                'By': cached_sorts[i]['by'],
                'Key': cached_sorts[i]['key'],
                'Indexs': cached_sorts[i].getIndexs()
            }]);
        return sorts;
    }
    ////////////////////////////////////////////////////////////////////////////////////////
    function sync(fn) {
        if (timer)
            clearTimeout(timer);
        //从未初始化时
        if (!cached_tasks) {
            if (fn) fn();
            resetTimer();
            return;
        }
        //合并最新的变更记录
        for (var i in cached_tasks)
            if (cached_tasks[i])
                changes = $.merge(changes, cached_tasks[i].popChanges());
        //id修正 避免同步间隙间对新增记录的变更导致此次同步时被重复新增
        for (var i = 0; i < changes.length; i++) {
            if (idChanges[changes[i]['ID']] != undefined) {
                var old = changes[i]['ID'];
                changes[i]['ID'] = idChanges[old];
                debuger.info('id fix at client，' + old + '->' + changes[i]['ID']);
            }
        }
        //排序索引
        var sorts = $.toJSON(getSorts());
        var isSortsChange = sorts != preSorts;
        if (isSortsChange)
            debuger.info('sync sorts to server', sorts);
        if (changes.length > 0)
            debuger.info('sync changes to server', changes);
        //没有任何变更
        if (!isSortsChange && changes.length == 0) {
            if (fn) fn();
            resetTimer();
            return;
        }
        preSorts = sorts;
        //提交变更记录
        $.post(url_task_sync, getPostData({
            //变更列表
            changes: $.toJSON(changes),
            //排序/显示模式
            by: ui_list_helper.mode,
            //排序记录
            sorts: isSortsChange ? sorts : null
        }), function (data) {
            changes = []; //成功提交变更后清空变更记录
            endRequest();
            //修正
            var corrects = data; //$.evalJSON(data);
            $.each(corrects, function (i, n) {
                if (!cached_tasks[n['OldId']]) return;
                //修正id
                cached_tasks[n['OldId']].setId(n['NewId']);
                cached_tasks[n['NewId']] = cached_tasks[n['OldId']];
                cached_tasks[n['OldId']] = null;
                //HACK:协助修正batchdetail中的临时标识
                ui_list_helper.repairBatchDetailId(n['OldId'], n['NewId']);
                //需要记录id变更以用于客户端修正
                idChanges[n['OldId']] = n['NewId'];
                debuger.info('id fix from server，' + n['OldId'] + '->' + n['NewId'], n);
            });

            if (fn) fn();
            resetTimer();
        });
    }
    function resetTimer() {
        timer = setTimeout(sync, 2000);
    }
    function getPostData(d) {
        if (!d) d = {};
        d.tryfail = tryfail;
        d.taskFolderId = currentList;
        d.teamId = currentTeam;
        d.projectId = currentProject;
        return d;
    }
    function endRequest() {
        $('#error_lose_connect').fadeOut(500);
        $('#region_error').hide();
    }
    function globalBinds() {
        $('.flag_continueDelete').click(continueDelete);
        $('.flag_cancelDelete').click(cancelDelete);

        $('.flag_byPriority').click(byPriority);
        $('.flag_byDueTime').click(byDueTime);

        $('.flag_openTaskFolder').click(openTaskFolder);
        $('.flag_addTaskFolder').click(function () { doAddTaskFolder(this); });
        $('.flag_removeTaskFolder').click(doRemoveTaskFolder);

        $('.flag_archiveTasks').click(archiveTasks);
        $('.flag_hideArchive').click(hideArchive);
        $('.flag_showArchive').click(showArchive);

        $('.flag_toggleTasks').click(toggleTasks);
        $('.flag_appendTask').click(appendTask);
        $('.flag_deleteTask').click(deleteTask);

        $('.flag_removeProject').click(doRemoveProject);

        $('.flag_tryfail').click(function () { tryfail = !tryfail; $(this).html('tryfail=' + tryfail); });
    }
    function ajaxSetup() {
        $.ajaxSetup({
            cache: false,
            error: function (x, e) {
                $('#error_lose_connect').fadeIn(500);
                //临时处理modal内异常
                if ($('div.modal:visible').length > 0) {
                    if (!window.$error)
                        window.$error = $('<div id="region_error" class="alert alert-error">' + lang.sorry_error_occur_retry_later + '</div>');
                    //处理弹窗内的error显示
                    $('div.modal:visible .modal-body').prepend(window.$error);
                    window.$error.fadeIn(500);
                }
                resetTimer();
            }
        });
    }
    $(function () {
        resize();

        $el_wrapper_region = $('#todolist_wrapper');
        $el_wrapper_detail = $('#detail_wrapper');
        $el_cancel_delete = $('#cancel_delete');

        Sort.prototype._getTask = function (i) { return cached_tasks[i]; };
        //为ui设置全局变量
        UI_List_Common.prototype.$wrapper = $el_wrapper_region;
        UI_List_Common.prototype.$wrapper_detail = $el_wrapper_detail;
        UI_List_Common.prototype.$cancel_delete = $el_cancel_delete;
        //为ui设置全局函数
        UI_List_Common.prototype.commitDeletes = function (d) { changes = $.merge(changes, d); };
        UI_List_Common.prototype.eachTask = function (fn) { for (var id in cached_tasks) if (cached_tasks[id]) fn(cached_tasks[id]); };
        UI_List_Common.prototype.getTaskById = function (i) { return cached_tasks[i]; };
        UI_List_Common.prototype.getSortByKey = function (k) { return cached_sorts[k]; };
        UI_List_Common.prototype.setTask = function (i, t) { cached_tasks[i] = t; };
        //团队相关
        // *****************************************************
        // taskpage.js main changes here
        // *****************************************************
        UI_List_Common.prototype.getProjects = function () { return $scope.team.projects; }
        UI_List_Common.prototype.getTeamMembers = function () { return $scope.team.members; }

        ui_list_helper_priority = create_UI_List_Priority();
        ui_list_helper_due = create_UI_List_Due();

        globalBinds();
        ajaxSetup();


        if (currentProject)
            project(currentProject);
        else if (currentTeam)
            team(currentTeam);
        else
            folder(0);
    });
}