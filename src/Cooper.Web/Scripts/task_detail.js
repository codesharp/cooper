//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_bind.js" />
///<reference path="task_bind_shortcuts.js" />
///<reference path="task_bind_team.js" />
///<reference path="task_row.js" />

//修正批量详情中的临时id
UI_List_Common.prototype.repairBatchDetailId = function (old, id) {
    if (!this.$batchDetail) return;
    var prev = this.$batchDetail.attr('id').split(',');
    debuger.debug('old batch ids', prev);
    var arr = $.merge($.grep(prev, function (n) { return n != old }), [id]);
    debuger.debug('new batch ids', arr);
    this.$batchDetail.attr('id', arr.join(','));
}
//详情区域渲染
UI_List_Common.prototype._renderDetail = function ($r) {
    if (this.detail_timer)
        clearTimeout(this.detail_timer);
    var base = this;
    var fn = function () {
        var t = base.getTask($r);
        base.$wrapper_detail.empty().append(t.renderDetail());
        //额外修正一些由于未append导致的显示问题
        t.fixDetail();
    }
    if (this.detail_timer_enable)
        this.detail_timer = setTimeout(fn, 100); //增加timer延迟优化性能
    else
        fn();
}
//批量详情区域渲染
UI_List_Common.prototype._renderBatchDetail = function ($rows) {
    if (this.detail_timer)
        clearTimeout(this.detail_timer);

    var base = this;
    var fn = function () {
        var tasks = [$rows.length];
        $rows.each(function (i, n) { tasks[i] = base.getTask($(n)); });
        base.renderBatchDetail(tasks);
    }
    if (this.detail_timer_enable)
        this.detail_timer = setTimeout(fn, 100); //增加timer延迟优化操作感观性能
    else
        fn();
}
UI_List_Common.prototype.renderBatchDetail = function (tasks) {
    if (!this.$batchDetail)
        //由于datepicker不支持id重复
        this.$batchDetail = $(render($('#tmp_detail_batch').html(), { 'dueTimeBatchId': this.batch_id_dueTime }));

    var base = this,
        ids = [tasks.length],
        editable,
        isCompleted,
        priority,
        tags = [],
        projects = [];
    $.each(tasks, function (i, task) {
        ids[i] = task.id();
        //有一个可编辑即可
        if (task.editable) editable = true;
        //合并tag
        tags = $.merge(tags, task.tags());
        //合并project
        projects = $.merge(projects, task.projects());
        //以首个任务为准
        if (i == 0) {
            isCompleted = task.isCompleted();
            priority = task.priority();
            return;
        }
        if (isCompleted != task.isCompleted()) { isCompleted = null }
        if (priority != task.priority()) { priority = null }
    });
    //批量id设置
    base.$batchDetail.attr('id', ids.join(','));
    //批量标题
    base.$batchDetail.find('#subject').html(tasks.length + lang.batch_task);
    //批量是否完成
    if (isCompleted != null)
        base.$batchDetail.find('#isCompleted')
        [isCompleted ? 'addClass' : 'removeClass']('active')
        [isCompleted ? 'addClass' : 'removeClass']('btn-success');
    else
        base.$batchDetail.find('#isCompleted').removeClass('active').removeClass('btn-success');
    //批量优先级
    if (priority != null)
        base.$batchDetail.find('#priority button')
            .removeClass('active')
            .eq(parseInt(priority))
            .addClass('active');
    else
        base.$batchDetail.find('#priority button').removeClass('active');
    //呈现
    base.$wrapper_detail.empty().append(base.$batchDetail);

    //部分未全局处理的事件绑定
    //批量详情绑定，如：标签
    base.bind_detail(base.$batchDetail, tasks);
    //批量标签
    base.detail_array_control_render(base.$batchDetail.find('#tags'), tags);

    //批量团队相关的详情，如：项目、分配
    if (base.bind_detail_team)
        base.bind_detail_team(base.$batchDetail, tasks);
    //批量项目
    base.detail_array_control_render(base.$batchDetail.find('#projects'), projects);

    //批量编辑状态
    base.$batchDetail.find('#isCompleted').attr('disabled', !editable);
    base.$batchDetail.find('#priority button').attr('disabled', !editable);
    base.$batchDetail.find('#' + base.batch_id_dueTime).attr('disabled', !editable);
    base.$batchDetail.find('#tags_btn')[editable ? 'show' : 'hide']();
    //批量团队属性编辑状态
    base.$batchDetail.find('#assignee_btn')[editable ? 'show' : 'hide']();
    base.$batchDetail.find('#projects_btn')[editable ? 'show' : 'hide']();

    //datepicker重复初始化问题 应先append再初始化
    if (editable)
        base.$batchDetail.find('#' + base.batch_id_dueTime).val('').removeClass('hasDatepicker').datepicker();
}