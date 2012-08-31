//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />
///<reference path="task.js" />
///<reference path="task_common.js" />

//根据dueTime排序任务列表UI模式
var UI_List_Due = function () { }
UI_List_Due.prototype = new UI_List_Common();
UI_List_Due.prototype.mode = 'ByDueTime';
UI_List_Due.prototype.modeArgs = {
    //快捷键开关
    shortcuts_move: true,
    //是否允许对invalid区域的任务变更完成状态非
    shortcuts_canSetCompleted_RowOfInValidRegion: true,
    //是否可编辑
    editable: true
}
////////////////////////////////////////////////////////////////////////////////////////
//按dueTime排序区域
UI_List_Due.prototype.byDueTime = null;
UI_List_Due.prototype._isDueTimeRow = function ($r) { return this.getRegionOfRow($r).hasClass('todolist_dueTime'); }
UI_List_Due.prototype._isDueTimeRegion = function ($r) { return $r.hasClass('todolist_dueTime'); }
UI_List_Due.prototype.renderByDueTime = function (b) {
    //按dueTime升序排列
    var idx = this.byDueTime.indexs();
    for (var i = 0; i < idx.length; i++) {
        for (var j = idx.length - 1; j > 0; j--) {
            var d = this.getTaskById(idx[j]).due();
            var r = d ? d.getTime() : 9346303971569;
            d = this.getTaskById(idx[j - 1]).due();
            var l = d ? d.getTime() : 9346303971569;
            if (r < l) {
                var temp = idx[j - 1];
                idx[j - 1] = idx[j];
                idx[j] = temp;
            }
        }
    }
    this.byDueTime.render();
    if (!b)
        this._prepareBinds();
}
////////////////////////////////////////////////////////////////////////////////////////
//额外实现父级定义的扩展
//优先级调整时
UI_List_Due.prototype.onPriorityChange = function ($r, task, old, p) {
    //排除时间排序区域
    if (this._isDueTimeRow($r)) return;
    UI_List_Priority.onPriorityChange(this, $r, task, old, p);
}
//绑定时
UI_List_Due.prototype.onPrepareBinds = function () {
    var base = this;
    this.$wrapper.keydown(function (e) {
        var ctrl = e.ctrlKey;
        var shift = e.shiftKey;
        var up = e.keyCode == 38;
        var down = e.keyCode == 40;
        var $focus = base.$focusRow;
        var $actives = base.getActives();

        if (ctrl) {
            //针对dueTime排序区域将上下移动行为调整为dueTime变更
            //仅支持单个
            if ((!up && !down)
                || $actives.length != 1
                || $focus == null
                || !base._isDueTimeRow($focus)) return;

            var task = base.getTask($focus);
            var dueTime = task.due();
            var day = 86400000;
            if (up)
                dueTime = addDay(dueTime, -1);
            else if (down)
                dueTime = addDay(dueTime, 1);
            task.setDueTime(dueTime);
            //优化 避免重新排序
            var arr = base._findNextAndPrev($focus);
            var prev = arr[0] == null ? null : base.getTask(arr[0]).due();
            var next = arr[1] == null ? null : base.getTask(arr[1]).due();
            if ((prev == null || prev.getTime() <= dueTime) && (next == null || next.getTime() >= dueTime))
                return false;
            //重排序
            base.renderByDueTime();
            //移动过程中会丢失焦点，在此修正
            base.focusRow($focus);
            return false; //避免滚动条移动
        }
    });
}
//批量变更dueTime时
UI_List_Due.prototype.onDueTimeBatchChange = function (tasks, t) {
    for (var i = 0; i < tasks.length; i++)
        if (this._isDueTimeRow(tasks[i].el())) {
            this.renderByDueTime();
            return;
        }
}
////////////////////////////////////////////////////////////////////////////////////////
//覆盖父级实现
UI_List_Due.prototype._isValidRegion = function ($r) { return !this._isDueTimeRegion($r); }
UI_List_Due.prototype._isRowOfValidRegion = function ($r) { return !this._isDueTimeRow($r); }
UI_List_Due.prototype.render = function () {
    this.$wrapper.empty();
    //dueTime排序区域
    this.byDueTime = this.getSortByKey('dueTime');
    this.renderByDueTime(true);
    this.$wrapper.append(this.byDueTime.el().addClass('todolist_dueTime'));
    //优先级区域
    this._renderBySort(this.today);
    this._renderBySort(this.upcoming);
    this._renderBySort(this.later);
    //默认追加一条
    if (this.getTasks().length == 0)
        this.appendTask(0);
    //绑定
    this._prepareBinds();
    //显示在其他模式下隐藏的元素
    this.removeHidenInMode();
}
UI_List_Due.prototype.appendTask = function (p) {
    var t = new Task();
    this.setTask(t.id(), t);
    //基本渲染
    t.renderRow();

    var $row = null;
    var $actives;
    //追加到焦点行之后
    if (this._$focusRow != null)
        $row = this._$focusRow;
    //追加到选中行之后
    else if (($actives = this.getActives()).length == 1)
        $row = $actives;
    if ($row != null) {
        var active = this.getTask($row);
        this._appendTaskToRow($row, t, active);

        var dueTime = active.due();
        if (dueTime != null) {
            t['data']['dueTime'] = dueTime;
            t.setDueTime(dueTime);
            this.byDueTime.flush();
        } else {
            t['data']['priority'] = active.priority();
            t.setPriority(active.priority());
            this.getSortByKey(t.priority()).flush();
        }
    }
    //默认追加到later
    else {
        p = p == undefined ? 2 : p;
        t['data']['priority'] = p;
        t.setPriority(p);
        this.getSortByKey(t.priority()).append(t);
    }

    //由于新增需要重新hover
    this._prepareBinds();
    //总是展开
    this._expandRowRegion(t.el());
    //焦点
    this._fireRowSingleClick(t.el());
}
function addDay(t, d) {
    var day = 86400000;
    return new Date(t.getTime() + (d * day));
}
//对象创建
function create_UI_List_Due() {
    var ui = new UI_List_Due();
    return ui;
}