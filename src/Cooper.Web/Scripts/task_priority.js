//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />
///<reference path="task.js" />
///<reference path="task_common.js" />

//根据优先级Priority列表模式 0=today 1=upcoming 2=later
//主要差异点：无需显示due状态，集中归档
var UI_List_Priority = function () { }
UI_List_Priority.prototype = new UI_List_Common();
UI_List_Priority.prototype.mode = 'ByPriority';
UI_List_Priority.prototype.modeArgs = {
    //快捷键开关
    shortcuts_move: true,
    //是否允许对invalid区域的任务变更完成状态非
    shortcuts_canSetCompleted_RowOfInValidRegion: false,
    //是否可编辑
    editable: true
}
////////////////////////////////////////////////////////////////////////////////////////
//归档
UI_List_Priority.prototype.todolist_archived = 'todolist_archived';
UI_List_Priority.prototype.archive = new Sort('', 'archived', lang.archive, [], function (i) { return cached_tasks[i]; });
UI_List_Priority.prototype._isArchivedRow = function ($r) { return this.getRegionOfRow($r).hasClass(this.todolist_archived); }
UI_List_Priority.prototype._isArchivedRegion = function ($r) { return $r.hasClass(this.todolist_archived); }
UI_List_Priority.prototype.showArchive = function () { this.archive.el().show(); }
UI_List_Priority.prototype.hideArchive = function () { this.archive.el().hide(); }
//隐藏优先级相关图标
UI_List_Priority.prototype._hidePriority = function () { this.setHidenInMode(this.$wrapper.find('td.cell_bool ul.nav:first-child').find('.priority')); }
////////////////////////////////////////////////////////////////////////////////////////
//额外实现父级定义的扩展
//刷新索引时
UI_List_Priority.prototype.onFlushIdxs = function () {
    this.archive.flush();
    this._hidePriority();
}
//binds时
UI_List_Priority.prototype.onPrepareBinds = function () {
    var base = this;
}
//优先级调整时
UI_List_Priority.prototype.onPriorityChange = function ($r, task, old, p) {
    //排除已归档
    if (this._isArchivedRow($r)) return;
    UI_List_Priority.onPriorityChange(this, $r, task, old, p);
    //修正显示
    this._hidePriority();
}
//是否完成调整时
UI_List_Priority.prototype.onCompletedChange = function (task, old, b) {
    //仅针对已归档行
    if (!this._isArchivedRow(task.el())) return;
    if (old == b) return;
    this.getSortByKey(task.priority()).append(task);
    if (!b) this.archive.flush();
    this._hidePriority();
}
////////////////////////////////////////////////////////////////////////////////////////
//覆盖父级实现
UI_List_Priority.prototype._isValidRegion = function ($r) { return !this._isArchivedRegion($r); }
UI_List_Priority.prototype._isRowOfValidRegion = function ($r) { return !this._isArchivedRow($r); }
UI_List_Priority.prototype.render = function (b) {//b=是否显示归档区域
    this.$wrapper.empty();
    //优先级区域
    this._renderByIdx(this.today);
    this._renderByIdx(this.upcoming);
    this._renderByIdx(this.later);
    //默认追加一条
    if (this.getTasks().length == 0)
        this.appendTask(0);
    //主动做一次归档
    this.archiveTasks();
    //归档区域
    if (this.getTasks(this.archive.el()).length > 0 || b) {
        this.archive.render();
        this.archive.el().addClass(this.todolist_archived);
        this.$wrapper.prepend(this.archive.el());
    }
    //绑定
    this._prepareBinds();
    //显示在其他模式下隐藏的元素
    this.removeHidenInMode();
    //修正
    this._hidePriority();
}
UI_List_Priority.prototype.archiveTasks = function () {
    var base = this;
    //TODO:归档时按完成时间排序？
    this.eachTask(function (t) {
        if (t.isCompleted())
            base.archive.el().append(t.el());
    });
    //刷新索引
    this._flushIdxs();
    //单独刷新archive
    this.archive.flush();
}
UI_List_Priority.prototype.appendTask = function (p) {
    debuger.profile('ui_appendTask');
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
    //仅在非归档行追加
    if ($row != null && !this._isArchivedRow($row)) {
        var active = this.getTask($row);
        this._appendTaskToRow($row, t, active);
        t.setPriority(active.priority());
        this.getSortByKey(t.priority()).flush();
    }
    //默认追加到later
    else {
        t.setPriority(p == undefined ? 2 : p);
        this.getSortByKey(t.priority()).append(t);
    }

    //子类逻辑修正
    //优先级图标显示修正
    this._hidePriority();
    //由于新增需要重新hover
    this._prepareBinds();
    //总是展开
    this._expandRowRegion(t.el());
    //焦点
    this._fireRowSingleClick(t.el());
    debuger.profileEnd('ui_appendTask');
}
////////////////////////////////////////////////////////////////////////////////////////
//可复用行为
UI_List_Priority.onPriorityChange = function (base, $r, task, old, p) {
    base.getSortByKey(p).append(task, true);
    base.getSortByKey(old).flush();
}
////////////////////////////////////////////////////////////////////////////////////////
//对象创建
function create_UI_List_Priority() {
    var ui = new UI_List_Priority();
    return ui;
}