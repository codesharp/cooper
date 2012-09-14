//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />
///<reference path="task.js" />
///<reference path="task_common.js" />

//根据分配人Assignee列表模式
var UI_List_Assignee = function () { }
UI_List_Assignee.prototype = new UI_List_Common();
UI_List_Assignee.prototype.mode = 'ByAssignee';
UI_List_Assignee.prototype.modeArgs = {
    //快捷键开关
    shortcuts_move: true,
    //是否允许对invalid区域的任务变更完成状态非
    shortcuts_canSetCompleted_RowOfInValidRegion: false,
    //是否可编辑
    editable: true
}
UI_List_Assignee.prototype.noAssignee = 0;
////////////////////////////////////////////////////////////////////////////////////////
//额外实现父级定义的扩展
//刷新索引时
UI_List_Assignee.prototype.onFlushSorts = function () {
    
}
//binds时
UI_List_Assignee.prototype.onPrepareBinds = function () {
}
//优先级调整时
UI_List_Assignee.prototype.onPriorityChange = function ($r, task, old, p) {
}
//是否完成调整时
UI_List_Assignee.prototype.onCompletedChange = function (task, old, b) {
}
////////////////////////////////////////////////////////////////////////////////////////
//覆盖父级实现
//UI_List_Assignee.prototype._isValidRegion = function ($r) { return !this._isArchivedRegion($r); }
//UI_List_Assignee.prototype._isRowOfValidRegion = function ($r) { return !this._isArchivedRow($r); }
UI_List_Assignee.prototype.render = function () {
    this.$wrapper.empty();
    //未分配区域
    this._renderBySort(this.noAssignee);
    //遍历所有分配者
    for(var k in this.getSorts())
        if(k!=this.noAssignee)
            this._renderBySort(k);
    //默认追加一条
    if (this.getTasks().length == 0)
        this.appendTask(0);
    //绑定
    this._prepareBinds();
    //显示在其他模式下隐藏的元素
    this.removeHidenInMode();
}
UI_List_Assignee.prototype.appendTask = function () {
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
        t['data']['assignee'] = active.assignee();
        t.setAssignee(active.assignee());
        this._appendTaskToRow($row, t, active);
        //设置assignee为当前区域的assignee
        this.getSortByKey(t.assignee() == null ? 0 : t.assignee().id).flush();
    }
    //默认追加到NoAssignee
    else {
        t['data']['assignee'] = null;
        t.setAssignee(null);
        this.getSortByKey(0).append(t);
    }

    //子类逻辑修正
    //由于新增需要重新hover
    this._prepareBinds();
    //总是展开
    this._expandRowRegion(t.el());
    //焦点
    this._fireRowSingleClick(t.el());
    //TODO:重构appendTask,逻辑可复用
}
////////////////////////////////////////////////////////////////////////////////////////
//对象创建
function create_UI_List_Assignee() {
    var ui = new UI_List_Assignee();
    return ui;
}