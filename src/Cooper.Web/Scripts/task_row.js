//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />

//处理row_task相关

//批量选中 配合shift全选
UI_List_Common.prototype._batchRowActive = function ($r1, $r2) {
    var $end = null;
    var flag = false;
    var base = this;
    this.removeActive(this.getTasks()).each(function (i, n) {
        if (flag) return;
        var $r = $(n);
        if ($end == null && base.getTaskId($r) == base.getTaskId($r1))
            $end = $r2;
        else if ($end == null && base.getTaskId($r) == base.getTaskId($r2))
            $end = $r1;
        if ($end != null) {
            base.setActive($r);
            flag = base.getTaskId($r) == base.getTaskId($end);
        }
    });
}
//模拟行点击，置为激活
UI_List_Common.prototype._fireRowClick = function ($r, c, s) {
    this.setActive($r);
    this.focusRow($r);
    this.$focusRow = $r;
    //详情呈现
    var $actives = this.getActives();
    if ($actives.length == 1)
        this._renderDetail(this.$focusRow);
    if ($actives.length > 1)
        this._renderBatchDetail($actives);
}
UI_List_Common.prototype._fireRowSingleClick = function ($r) {
    this.removeActive(this.getActives());
    this._fireRowClick($r);
}
//以焦点行为依据
UI_List_Common.prototype._findFocusPrev = function () { return this._findNextAndPrev(this.$focusRow)[0]; }
UI_List_Common.prototype._findFocusNext = function () { return this._findNextAndPrev(this.$focusRow)[1]; }
//与选中行为依据
UI_List_Common.prototype._findActivePrev = function () {
    var $actives = this.getActives();
    return this._findNextAndPrev($actives.length == 1 ? this.$focusRow : $actives.first())[0];
}
UI_List_Common.prototype._findActiveNext = function () {
    var $actives = this.getActives();
    return this._findNextAndPrev($actives.length == 1 ? this.$focusRow : $actives.last())[1];
}
//返回row_task的有效前后行
UI_List_Common.prototype._findNextAndPrev = function ($r) {
    if ($r == null)
        return [null, null];
    var base = this;
    var id = this.getTaskId($r);
    var $prev = null;
    var $next = null;
    var $all = this.getTasks();
    var flag = false;
    $all.each(function (i, n) {
        if (flag) return;
        if (base.getTaskId($(n)) != id)
            $prev = $(n);
        else {
            if (i + 1 < $all.length)
                $next = $all.eq(i + 1);
            flag = true;
        }
    });
    if (debuger.isDebugEnable)
        debuger.debug([
            $prev == null ? null : this.getTaskId($prev),
            this.getTaskId(this.$focusRow),
            $next == null ? null : this.getTaskId($next)
        ]);
    return [$prev, $next];
}

//返回row_task的有效前后row或前后region
UI_List_Common.prototype._findAnyPrev = function ($r) {
    var $prev = null;
    if ($r.prev().length != 0)
        $prev = $r.prev();
    else {
        $prev = this.getRegionOfRow($r).prev();
        $prev = $prev.length == 0 ? null : $prev;
    }
    return $prev;
}
UI_List_Common.prototype._findAnyNext = function ($r) {
    var $next = null;
    if ($r.next().length != 0)
        $next = $r.next();
    else {
        $next = this.getRegionOfRow($r).next();
        $next = $next.length == 0 ? null : $next;
    }
    return $next;
}