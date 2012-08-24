//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="../Content/jquery/qunit-1.9.0.js" />
///<reference path="../Content/jquery/jquery-1.7.2.min.js" />
///<reference path="lang.js" />
///<reference path="common.js" />
///<reference path="task.js" />
///<reference path="task_common.js" />

//cooper web ui auto tests
//amazing?

var row_task = 'row_task';
var class_row_task = '.row_task';
var row_active = 'row_active';
var class_row_active = '.row_active';
var row_completed = 'row_completed';

var $el_wrapper_region = $('#todolist_wrapper');
var $el_wrapper_detail = $('#detail_wrapper');
var $el_cancel_delete = $('#cancel_delete');

var up = 38;
var down = 40;

function runAllPersonal() {
    test('init', function () {
        ok(taskCount() > 0, 'cached_tasks init');
    });

    runMouseTests();
    runShortcutsTests();
    runDetailTests();
    runTaskFolderTests();
}

function runAllTeam() {
    test('init', function () {
        ok(taskCount() > 0, 'cached_tasks init');
    });

    runMouseTests();
    runShortcutsTests();
    runDetailTests();
}

function runMouseTests() {
    //预置一些参数适合同步测试
    UI_List_Common.prototype.detail_timer_enable = false;

    test('append', function () {
        var c = taskCount();
        appendTask();
        assertAppend(c);
    });
    test('delete', function () {
        appendTask();
        focusRow(0);
        var c = taskCount();
        deleteTask();
        assertDelete(c);
    });
    ///////////////////////////////////////////////////////////////////////////////
    //任务列表区域
    //鼠标操作
    //click
    test('click_row_task', function () {
        var e = jQuery.Event('click');
        var $r = $el_wrapper_region.find(class_row_task).eq(0);
        $r.find('input').trigger(e);
        ok($r.hasClass(row_active) && getActives().length == 1, 'row 1 click and focus via input');

        //event不可重用
        var e2 = jQuery.Event('click');
        $r = $el_wrapper_region.find(class_row_task).eq(1);

        $r.find('.cell_bool').trigger(e2);
        ok($r.hasClass(row_active) && getActives().length == 1, 'row 2 click and focus via other element');
    });
    //hover
    test('hover_row_task', function () {
        ok(true, 'hard to mock hover');
        return;
        var e = jQuery.Event('mouseover');
        var $r = getTasks().eq(0);
        $r.trigger(e);
        equal($r.find('#isCompleted').css('display'), 'block', 'icon complete show');
        equal($r.find('#priority').css('display'), 'block', 'icon priority show');
    });
    //complete
    test('click_complete', function () {
        var $r = focusRow(0);
        var $icon = $r.find('i.icon-check');
        $icon.click();
        assertIsCompleted($r, true);
        $icon.click();
        assertIsCompleted($r, false);
    });
    //ctrl+click
    test('ctrl+click', function () {
        appendTask();
        appendTask();
        appendTask();
        var $r = focusRow(0);
        var e = jQuery.Event('click'); e.ctrlKey = true;
        $r.find('#subject').trigger(e);
        //反选
        ok(!$r.hasClass(row_active), 'row ctrl+click and inactive');
        var e2 = jQuery.Event('click'); e2.ctrlKey = true;
        $r.find('#subject').trigger(e2);
        //正选
        ok($r.hasClass(row_active), 'row ctrl+click and active');
        //跨行多选
        focusRow(0);
        var e3 = jQuery.Event('click'); e3.ctrlKey = true;
        $r = getTasks().eq(2);
        $r.find('#subject').trigger(e3);
        ok($r.hasClass(row_active) && getActives().length == 2, 'row ctrl+click and 2 actives');
    });
    //shift+click
    test('shift+click', function () {
        var $r = focusRow(0);
        var e = jQuery.Event('click'); e.shiftKey = true;
        $r.find('#subject').trigger(e);
        //正选
        ok($r.hasClass(row_active), 'row shift+click and active');
        //批量
        focusRow(0);
        var e2 = jQuery.Event('click'); e2.shiftKey = true;
        $r = getTasks().eq(2);
        $r.find('#subject').trigger(e2);
        ok($r.hasClass(row_active) && getActives().length == 3, 'row shift+click and 3 actives');
    });
        
    UI_List_Common.prototype.detail_timer_enable = true;
}

//快捷键操作
function runShortcutsTests() {
    //预置一些参数适合同步测试
    UI_List_Common.prototype.detail_timer_enable = false;

    test('up/down', function () {
        focusRow(0);
        var e = jQuery.Event('keydown'); e.keyCode = down;
        $el_wrapper_region.trigger(e);
        equal(getActives().length, 1, 'only 1 active after move down');
        equal(getTask(getActives().first()), getTask(getRow(1)), 'focus move down to second row');

        var e2 = jQuery.Event('keydown'); e2.keyCode = up;
        $el_wrapper_region.trigger(e2);
        equal(getActives().length, 1, 'only 1 active after move up');
        deepEqual(getTask(getActives().first()), getTask(getRow(0)), 'focus move up to first row');
    });
    test('ctrl+up/down', function () {
        var $r = focusRow(0);

        var e = jQuery.Event('keydown'); e.ctrlKey = true; e.keyCode = down;
        $el_wrapper_region.trigger(e);
        equal(getActives().length, 1, 'only 1 active after move down');
        equal(getTask($r), getTask(getRow(1)), 'original row move down to second');

        var e2 = jQuery.Event('keydown'); e2.ctrlKey = true; e2.keyCode = up;
        $el_wrapper_region.trigger(e2);
        equal(getActives().length, 1, 'only 1 active after move up');
        deepEqual(getTask($r), getTask(getRow(0)), 'original row move up to first');
    });
    test('shift+up/down', function () {
        focusRow(0);

        //shift begin
        var e1 = jQuery.Event('keydown'); e1.keyCode = 16;
        $el_wrapper_region.trigger(e1);

        var e2 = jQuery.Event('keydown'); e2.shiftKey = true; e2.keyCode = down;
        $el_wrapper_region.trigger(e2);
        equal(getActives().length, 2, '2 actives');

        var e3 = jQuery.Event('keydown'); e3.shiftKey = true; e3.keyCode = up;
        $el_wrapper_region.trigger(e3);
        equal(getActives().length, 1, '1 actives');
    });
    test('backspace', function () {
        focusRow(0);
        var c = taskCount();
        var e = jQuery.Event('keydown'); e.keyCode = 8;
        $el_wrapper_region.trigger(e);
        assertDelete(c);
    });
    test('enter', function () {
        var c = taskCount();
        focusRow(0);
        var e = jQuery.Event('keyup'); e.keyCode = 13;
        $el_wrapper_region.trigger(e);
        assertAppend(c);
    });
    //ctrl+enter
    test('ctrl+enter', function () {
        var $r = focusRow(0);
        var e = jQuery.Event('keyup'); e.ctrlKey = true; e.keyCode = 13;
        $el_wrapper_region.trigger(e);
        assertIsCompleted($r, true);
        $el_wrapper_region.trigger(e);
        assertIsCompleted($r, false);
    });

    UI_List_Common.prototype.detail_timer_enable = true;
}

function runDetailTests() {
    UI_List_Common.prototype.detail_timer_enable = false;
    ///////////////////////////////////////////////////////////////////////////////
    //详情区域
    //complete
    test('detail_complete', function () {
        var $r = focusRow(0);
        var $e = $el_wrapper_detail.find('#isCompleted');
        $e.click();
        assertIsCompleted($r, true);
        $e.click();
        assertIsCompleted($r, false);
    });
    //subject
    test('detail_change_subjct', function () {
        var $r = focusRow(0);
        var $e = $el_wrapper_detail.find('#subject');
        var e = jQuery.Event('keyup');
        var v = 'abc';
        $e.val(v).trigger(e);
        assertSubjectChange($r, v);
    });
    //body
    test('detail_change_body', function () {
        var $r = focusRow(0);
        var $e = $el_wrapper_detail.find('#body');
        var e = jQuery.Event('keyup');
        var v = 'abc';
        $e.val(v).trigger(e);
        equal(getTask($r).body(), v, 'body change');
    });
    //priority
    test('detail_change_priority', function () {
        var $r = focusRow(0);
        var $e = $el_wrapper_detail.find('#priority');
        $e.find('button:eq(1)').click();
        assertPriorityChange($r, 1);
        $e.find('button:eq(2)').click();
        assertPriorityChange($r, 2);
    });
    //dueTime
    test('detail_change_dueTime', function () {
        var $r = focusRow(0);
        var $e = $el_wrapper_detail.find('#dueTime').val('8/7/12').change();
        assertDueTimeChange($r, '2012-8-7');
    });
    UI_List_Common.prototype.detail_timer_enable = true;
}

function runTaskFolderTests() {
    ///////////////////////////////////////////////////////////////////////////////
    //任务表操作
    test('add_taskFolder', function () {
        $('.flag_openTaskFolder').click();
        equal($('#taskFolderModal').css('display'), 'block', 'task folder modal show');
        var folder = 'test';
        $('#taskFolderModal input').val(folder);
        $('.flag_addTaskFolder').click();

        //TODO:引入一个async框架
        setTimeout(function () {
            test('remove_taskFolder', function () {
                equal($('#taskFolder_title').html(), folder, 'task folder created');
                equal(taskCount(), 1, 'switch to new task folder');
                $('.flag_removeTaskFolder').click();
                ok(true);
            });
        }, 1000);
    });
}




function assertAppend(c) {
    equal(taskCount(), c + 1, 'add 1 task');
    equal(getActives().length, 1, 'only the new task is focus');
    equal(getTask(getActives().first()).id().indexOf('temp'), 0, 'new task id is "temp_*"');
}
function assertDelete(c) {
    equal(taskCount(), c - 1, 'delete 1 task when backspace');
    equal($el_cancel_delete.css('display'), 'block', 'cancel delete region show');
}
function assertIsCompleted($r, b) {
    var task = getTask($r);
    equal($r.hasClass(row_completed), b, 'mark taskrow iscompleted change');
    equal(task.isCompleted(), b, 'task iscompleted change');
    //detail
    equal(task.$el_detail.find('#isCompleted').hasClass('active'), b);
}
function assertSubjectChange($r, v) {
    var task = getTask($r);
    equal(task.subject(), v, 'subject change');
    equal($r.find('#subject').val(), v, 'subject change in row');
    //detail
    equal(task.$el_detail.find('#subject').val(), v, 'subject change in detail');
}
function assertPriorityChange($r, p) {
    var task = getTask($r);
    equal(task.priority(), p);
    //detail
    equal(task.$el_detail.find('#priority button.active').attr('priority'), p, 'priority change in detail');
    //TODO:断言位置切换
}
function assertDueTimeChange($r, t) {
    var task = getTask($r);
    //date assert需要改进
    this[!t ? 'equal' : 'notEqual']($.trim($r.find('#dueTimeLabel').html()), '', 'dueTime change in row');
    this[!t ? 'equal' : 'notEqual'](task.due(), null);
    equal(task.$el_detail.find('#dueTime').val(), t, 'dueTime change in detail');
}
function appendTask() { $('.flag_appendTask:first').click(); }
function deleteTask() { $('.flag_deleteTask:first').click(); }
function getTask($r) { return UI_List_Common.prototype.getTask($r); }
function taskCount() { var i = 0; UI_List_Common.prototype.eachTask(function (t) { i++; }); return i; }
function getActives() { return $el_wrapper_region.find(class_row_active); }
function getTasks() { return $el_wrapper_region.find(class_row_task); }
function getRow(i) { return $el_wrapper_region.find(class_row_task).eq(i); }
function focusRow(i) {
    var e = jQuery.Event('click');
    var $r = $el_wrapper_region.find(class_row_task).eq(i);
    $r.find('input').trigger(e);
    return $r;
}