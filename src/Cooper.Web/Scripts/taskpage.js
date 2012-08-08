//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />

//(function () {

//page内全局元素
var $el_wrapper_region = null;
var $el_wrapper_detail = null;
var $el_cancel_delete = null;

var ui_list_helper, ui_list_helper_priority, ui_list_helper_due;
var isShowArchive = false; //是否显示归档区域
var currentMode = byPriority; //当前列表模式，默认使用优先级列表模式
var currentList = null; //当前任务表标识

var timer;
var idChanges = {};
var preSorts = null;

$(function () {
    $el_wrapper_region = $('#todolist_wrapper');
    $el_wrapper_detail = $('#detail_wrapper');
    $el_cancel_delete = $('#cancel_delete');

    UI_List_Common.prototype.$wrapper = $el_wrapper_region;
    UI_List_Common.prototype.$wrapper_detail = $el_wrapper_detail;
    UI_List_Common.prototype.$cancel_delete = $el_cancel_delete;

    ui_list_helper_priority = create_UI_List_Priority();
    ui_list_helper_due = create_UI_List_Due();
    refreshTasklists();
    list(0);
});

//全局按钮函数
function appendTask() { ui_list_helper.appendTask(); }
function toggleTasks() { ui_list_helper.toggleTasks(); }
function deleteTask() { ui_list_helper.deleteTask(); }
//撤销删除
function cancelDelete() {
    ui_list_helper.cancelDelete();
    //UNDONE:重演设计未完成之前通过重新加载来解决恢复
    currentMode();
}
//继续删除
function continueDelete() { ui_list_helper.continueDelete(); }
function syncAccount() { $('#syncModal').modal('show'); }
function archiveTasks() { ui_list_helper.archiveTasks(); }
function hideArchive() {
    isShowArchive = false;
    if (ui_list_helper == ui_list_helper_priority) byPriority();
}
function showArchive() {
    isShowArchive = true;
    if (ui_list_helper == ui_list_helper_priority) byPriority();
}
function share(e) {
    $('#shareModal').modal('show');
    $('#share_title').val($(e).parents('div.region_detail').eq(0).find('input').val() + lang.share_description);
}
///////////////////////////////////////////////////////////////////////////////////////
//tasklist ui
function refreshTasklists() {
    $('#tasklists .changelist').unbind().click(function () {
        list($(this).attr('id'));
    });
}
function list(id) {
    $('#tasklist_title').html($('#tasklists a.changelist[id="' + id + '"]').find('span').html());
    currentMode(function () {
        currentList = id == 0 ? '' : id;
        $('#tasklist_delete')[isNaN(parseInt(currentList)) ? 'hide' : 'show']();
    }, function () {
        var temp = parseInt(id);
        if (isNaN(temp))
            $('.flag_by').hide();
    });
}
function addTasklist(id, name) {
    var $e = $(render($('#tmp_tasklist_item').html(), { id: id }));
    $e.find('span').text(name);
    $('#tasklists').append($e);
    refreshTasklists();
}
function removeTasklist(i) {
    $('#tasklists a[id="' + i + '"]').parent().remove();
}
function openTasklists() {
    $('#tasklistModal').modal('show');
}
function doAddTasklist(btn) {
    var $btn = $(btn);
    var val = $.trim($btn.prev().val());
    $btn.parent()[val == '' ? 'addClass' : 'removeClass']('error');
    if (val == '') return;

    $.post(url_tasklist_create, { name: val, type: 'personal' }, function (d) {
        debuger.info('new tasklist#' + d);
        $btn.prev().val('');
        $('#tasklistModal').modal('hide');
        addTasklist(d, val);
        list(d);
    });
}
function doRemoveTasklist() {
    if (!confirm(lang.confirm_delete_tasklist)) return;

    $.post(url_tasklist_delete, { id: currentList }, function () {
        debuger.info('remove tasklist#' + currentList);
        removeTasklist(currentList);
        list(0);
    });
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
        if (fn1) fn1();
        $.ajax({
            url: b ? url_task_byPriority : url_task_byPriority_incompleted,
            //任务表标识
            data: { tasklistId: currentList },
            type: 'POST',
            dataType: 'json',
            beforeSend: function () { $el_wrapper_region.empty().append($('#loading').html()); },
            success: function (data) {
                init(data.List, data.Sorts);
                (ui_list_helper = ui_list_helper_priority).render(b);
                fixAfterSyncOrReload(data.Editable);
                $('.flag_by').hide();
                $('.flag_by_priority').show();
                if (fn2) fn2();
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
        if (fn1) fn1();
        $.ajax({
            url: url_task_byDueTime,
            //任务表标识
            data: { tasklistId: currentList },
            type: 'POST',
            dataType: 'json',
            beforeSend: function () { $el_wrapper_region.empty().append($('#loading').html()); },
            success: function (data) {
                init(data.List, data.Sorts);
                (ui_list_helper = ui_list_helper_due).render();
                fixAfterSyncOrReload(data.Editable);
                $('.flag_by').hide();
                if (fn2) fn2();
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
    var idxs = [];
    for (var i in cached_sorts)
        idxs = $.merge(idxs, [{
            'By': cached_sorts[i]['by'],
            'Key': cached_sorts[i]['key'],
            'Indexs': cached_sorts[i].getIndexs()
        }]);
    return idxs;
}
////////////////////////////////////////////////////////////////////////////////////////
//TODO:增加网络异常修正逻辑
function sync(fn) {
    if (timer)
        clearTimeout(timer);
    //从未初始化时
    if (!cached_tasks) {
        if (fn) fn();
        resetTimer();
        return;
    }
    //变更记录
    var arr = [];
    for (var i in cached_tasks)
        if (cached_tasks[i])
            arr = $.merge(arr, cached_tasks[i].popChanges());
    //id修正 避免同步间隙间对新增记录的变更导致此次同步时被重复新增
    for (var i = 0; i < arr.length; i++) {
        if (idChanges[arr[i]['ID']] != undefined) {
            var old = arr[i]['ID'];
            arr[i]['ID'] = idChanges[old];
            debuger.info('客户端id修正，' + old + '->' + arr[i]['ID']);
        }
    }
    //删除记录
    arr = $.merge(arr, changes_delete);
    changes_delete = [];
    //排序索引
    var idxs = getSorts();
    var sorts = $.toJSON(getSorts());
    if (sorts != preSorts)
        debuger.info('将排序索引同步至Server', idxs);
    if (arr.length > 0)
        debuger.info('将变更同步至Server', arr);
    //没有任何变更
    if (sorts == preSorts && arr.length == 0) {
        if (fn) fn();
        resetTimer();
        return;
    }
    preSorts = sorts;
    //提交变更记录
    $.post(url_task_sync, {
        tasklistId: currentList,
        //变更列表
        changes: $.toJSON(arr),
        //排序/显示模式
        by: ui_list_helper.mode,
        //排序记录
        sorts: sorts
    }, function (data) {
        $('#error_lose_connect').fadeOut(500);
        //修正
        var corrects = data; //$.evalJSON(data);
        $.each(corrects, function (i, n) {
            //修正id
            cached_tasks[n['OldId']].setId(n['NewId']);
            cached_tasks[n['NewId']] = cached_tasks[n['OldId']];
            cached_tasks[n['OldId']] = null;
            //HACK:协助修正batchdetail中的临时标识
            ui_list_helper.repairBatchDetailId(n['OldId'], n['NewId']);
            //需要记录id变更以用于客户端修正
            idChanges[n['OldId']] = n['NewId'];
            debuger.info('来自server的id修正处理，' + n['OldId'] + '->' + n['NewId'], n);
        });

        if (fn) fn();
        resetTimer();
    });
}
function resetTimer() {
    timer = setTimeout(sync, 2000);
}
//})();

////////////////////////////////////////////////////////////////////////////////////////
//all=来自server的所有任务数组
//idx=优先级排序
function init(all, idxs) {
    //清理
    if (cached_tasks)
        for (var i in cached_tasks)
            if (cached_tasks[i])
                cached_tasks[i].dispose();
    if (cached_sorts)
        for (var i in cached_sorts)
            if (cached_sorts[i])
                cached_sorts[i].dispose();
    //构建本地缓存 cached_tasks
    cached_tasks = {};
    for (var i = 0; i < all.length; i++)
        cached_tasks[all[i]['ID'].toString()] = new Task(all[i]);
    debuger.info('original tasks', all);
    debuger.info('cached tasks', cached_tasks);
    //构建分组排序缓存 cached_idx {'0':List}
    cached_sorts = {};
    for (var i = 0; i < idxs.length; i++)
        cached_sorts[idxs[i]['Key']] = new Sort(
            idxs[i]['By'],
            idxs[i]['Key'],
            idxs[i]['Name'],
            idxs[i]['Indexs'],
            function (i) { return cached_tasks[i]; });
    debuger.info('original sorts', idxs);
    debuger.info('cached sorts', cached_sorts);
}