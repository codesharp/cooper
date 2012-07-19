//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

///<reference path="lang.js" />
///<reference path="common.js" />
///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_priority.js" />
///<reference path="task_due.js" />
///<reference path="../Content/jquery/qunit-1.9.0.js" />
///<reference path="../Content/jquery/jquery-1.7.2.min.js" />
//cooper web ui auto tests
//amazing?
//TODO:refact for ui variables like css names or html tags
function runTests() {
    var row_task = 'row_task';
    var class_row_task = '.row_task';
    var row_active = 'row_active';
    var class_row_active = '.row_active';

    ///////////////////////////////////////////////////////////////////////////////
    //初始化
    test('init', function () {
        ok(cached_tasks, 'cached_tasks init');
        ok(cached_idxs, 'cached_idxs init');
    });
    ///////////////////////////////////////////////////////////////////////////////
    //按钮区域
    //新建
    test('append', function () {
        var c = taskCount();
        appendTask();
        equal(taskCount(), c + 1, 'add 1 task');
    });
    //删除
    test('delete', function () {
        appendTask();
        focusRow(0);
        var c = taskCount();
        deleteTask();
        equal(taskCount(), c - 1, 'delete 1 task');
        //撤销删除区域显示
        equal($el_cancel_delete.css('display'), 'block', 'cancel delete region show');
    });
    ///////////////////////////////////////////////////////////////////////////////
    //列表区域
    //鼠标操作
    //click
    test('click_row_task', function () {
        var e = jQuery.Event('click');
        var $r = $el_wrapper_region.find(class_row_task).eq(0);
        $r.find('input').trigger(e);
        ok($r.hasClass(row_active) && $el_wrapper_region.find(class_row_active).length == 1, 'row 1 click and focus via input');

        //event不可重用
        var e2 = jQuery.Event('click');
        $r = $el_wrapper_region.find(class_row_task).eq(1);
        
        $r.find('.cell_bool').trigger(e2);
        ok($r.hasClass(row_active) && $el_wrapper_region.find(class_row_active).length == 1, 'row 2 click and focus via other element');
    });
    //hover
    test('hover_row_task', function () {
        ok(true, 'hard to mock hover');
        return;
        var e = jQuery.Event('mouseover');
        var $r = $el_wrapper_region.find(class_row_task).eq(0);
        $r.trigger(e);
        equal($r.find('td.cell_bool ul.nav').eq(1).find('i.icon-check').css('display'), 'block', 'icon complete show');
        equal($r.find('td.cell_bool ul.nav').eq(1).find('i.icon-inbox').css('display'), 'block', 'icon priority show');
    });
    //complete
    test('click_complete', function () {
        var $r = focusRow(0);
        var $icon = $r.find('td.cell_bool ul.nav').eq(1).find('i.icon-check');
        $icon.click();
        ok($r.hasClass('row_completed'), 'mark task as completed');
        ok(cached_tasks[$r.attr('id')].isCompleted(), 'task completed');
        $icon.click();
        ok(!$r.hasClass('row_completed'), 'mark task as incompleted');
        ok(!cached_tasks[$r.attr('id')].isCompleted(), 'task incompleted');
    });
    //ctrl+click
    test('ctrl+click', function () {
        appendTask();
        appendTask();
        appendTask();
        var $r = focusRow(0);
        var e = jQuery.Event('click'); e.ctrlKey = true;
        $r.find('input').trigger(e);
        //反选
        ok(!$r.hasClass(row_active), 'row ctrl+click and inactive');
        var e2 = jQuery.Event('click'); e2.ctrlKey = true;
        $r.find('input').trigger(e2);
        //正选
        ok($r.hasClass(row_active), 'row ctrl+click and active');
        //跨行多选
        focusRow(0);
        var e3 = jQuery.Event('click'); e3.ctrlKey = true;
        $r = $el_wrapper_region.find(class_row_task).eq(2);
        $r.find('input').trigger(e3);
        ok($r.hasClass(row_active) && $el_wrapper_region.find(class_row_active).length == 2, 'row ctrl+click and 2 actives');
    });
    //shift+click
    test('shift+click', function () {
        var $r = focusRow(0);
        var e = jQuery.Event('click'); e.shiftKey = true;
        $r.find('input').trigger(e);
        //正选
        ok($r.hasClass(row_active), 'row shift+click and active');
        //批量
        focusRow(0);
        var e2 = jQuery.Event('click'); e2.shiftKey = true;
        $r = $el_wrapper_region.find(class_row_task).eq(2);
        $r.find('input').trigger(e2);
        ok($r.hasClass(row_active) && $el_wrapper_region.find(class_row_active).length == 3, 'row shift+click and 3 actives');
    });
    //快捷键操作
    //up/down
    //ctrl up/down
    //shift up/down
    //backspace
    //enter
    //ctrl+enter
    ///////////////////////////////////////////////////////////////////////////////
    //详情区域
    //complete
    //subject
    //body
    //priority
    //duetime

    function taskCount() {
        var i = 0;
        for (var k in cached_tasks)
            if (cached_tasks[k])
                i++;
        return i;
    }
    function focusRow(i) {
        var e = jQuery.Event('click');
        var $r = $el_wrapper_region.find(class_row_task).eq(i);
        $r.find('input').trigger(e);
        return $r;
    }
}