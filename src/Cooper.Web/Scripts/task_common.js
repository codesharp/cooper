//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />

//任务列表UI模式 通用
var UI_List_Common = function () { }
UI_List_Common.prototype = {
    //标识列表模式
    mode: 'common',
    //UI模式定制参数
    modeArgs: {
        //快捷键开关
        shortcuts_move: true,
        //是否允许对invalid区域的任务变更完成状态非
        shortcuts_canSetCompleted_RowOfInValidRegion: false,
        //是否可编辑
        editable: true
    },
    $wrapper: null, //列表容器
    $wrapper_detail: null, //详情容器
    $focusRow: null, //记录焦点行元素
    $batchDetail: null, //批量详情
    $cancel_delete: null, //撤销删除区域元素
    deletes: null, //上一次任务删除记录
    deletes_timer: null, //删除恢复期间的定时器
    deletes_timer2: null, //删除恢复期间的定时器
    //常用任务属性
    today: '0',
    upcoming: '1',
    later: '2',
    ////////////////////////////////////////////////////////////////////////////////////////
    //关键样式名：row_task,row_active,todolist
    //屏蔽常用部分的UI影响
    getRow: function ($e) { return $e.parents('tr').eq(0); },
    getTaskId: function ($r) { return $r.attr('id'); },
    getRegionOfRow: function ($r) { return $r.parents('table.todolist').eq(0); },
    getTasks: function ($e) { if ($e) return $e.find('tr.row_task'); else return this.$wrapper.find('tr.row_task'); },
    getTaskVal: function ($r) { return $r.find('input').val(); },
    isTask: function ($r) { return $r.hasClass('row_task'); },
    getActives: function () { return this.$wrapper.find('tr.row_active'); },
    isActive: function ($r) { return $r.hasClass('row_active'); },
    setActive: function ($r) { return $r.addClass('row_active'); },
    removeActive: function ($r) { return $r.removeClass('row_active'); },
    focusRow: function ($r) { this._expandRowRegion($r); return $r.find('input').focus(); },
    setHidenInMode: function ($e) { return $e.addClass('disabled_in_this_mode').hide(); },
    removeHidenInMode: function () { return this.$wrapper.find('.disabled_in_this_mode').removeClass('disabled_in_this_mode').show(); },
    //缓存引用部分
    getTask: function ($r) { return cached_tasks[this.getTaskId($r)]; },
    //修正批量详情中的临时id
    repairBatchDetailId: function (old, id) {
        if (!this.$batchDetail) return;
        var prev = this.$batchDetail.attr('id').split(',');
        debuger.debug('old batch ids', prev);
        var arr = $.merge($.grep(prev, function (n) { return n != old }), [id]);
        debuger.debug('new batch ids', arr);
        this.$batchDetail.attr('id', arr.join(','));
    },
    ////////////////////////////////////////////////////////////////////////////////////////
    //主体渲染
    _renderByIdx: function (k) {
        cached_idxs[k].render();
        this.$wrapper.append(cached_idxs[k].el());
    },
    //详情区域渲染
    _renderDetail: function ($r) {
        var t = this.getTask($r);
        this.$wrapper_detail.empty().append(t.renderDetail());
        //额外修正一些由于未append导致的显示问题
        t.fixDetail();
    },
    _renderBatchDetail: function ($rows) {
        if (!this.$batchDetail)
            this.$batchDetail = $(tmp_detail_batch);
        var base = this;
        var ids = [$rows.length];
        var isCompleted, priority;
        $rows.each(function (i, n) {
            var id = base.getTaskId($(n));
            ids[i] = id;
            var task = cached_tasks[id];
            if (i == 0) {
                isCompleted = task.isCompleted();
                priority = task.priority();
                return;
            }
            if (isCompleted != task.isCompleted()) { isCompleted = null }
            if (priority != task.priority()) { priority = null }
        });
        //批量id设置
        this.$batchDetail.attr('id', ids.join(','));
        //批量标题
        this.$batchDetail.find('#subject').html($rows.length + lang.batch_task);
        //批量是否完成
        if (isCompleted != null)
            this.$batchDetail.find('#isCompleted')
            [isCompleted ? 'addClass' : 'removeClass']('active')
            [isCompleted ? 'addClass' : 'removeClass']('btn-success');
        else
            this.$batchDetail.find('#isCompleted').removeClass('active').removeClass('btn-success');
        //批量优先级
        if (priority != null)
            this.$batchDetail.find('#priority button')
                .removeClass('active')
                .eq(parseInt(priority))
                .addClass('active');
        else
            this.$batchDetail.find('#priority button').removeClass('active');
        this.$wrapper_detail.empty().append(this.$batchDetail);
        //datepicker重复初始化问题 应先append再初始化
        if (this.modeArgs.editable)
            this.$batchDetail.find('#dueTime').removeClass('hasDatepicker').datepicker();
    },
    _isBatchDetailValid: function () { return this.$batchDetail && this.$batchDetail.css('display') == 'block'; },
    ////////////////////////////////////////////////////////////////////////////////////////
    //批量对wrapper中的当前region执行flush操作
    _flushIdxs: function () {
        debuger.profile('ui_flushIdxs');
        var base = this;
        this.$wrapper.find('.todolist:visible').each(function () {
            if (cached_idxs[$(this).attr('key')] != undefined)
                cached_idxs[$(this).attr('key')].flush($(this).attr('by'));
        });
        //允许额外的实现
        if (this.onFlushIdxs)
            this.onFlushIdxs();
        debuger.profileEnd('ui_flushIdxs');
    },
    ////////////////////////////////////////////////////////////////////////////////////////
    //wrapper主体事件绑定，处理全局事件
    _prepareBinds: function () {
        var base = this;

        //取消所有已有bind
        this.$wrapper.unbind();
        this.$wrapper.find('tr.row_task').unbind();
        this.$wrapper.find('input').unbind();
        this.$wrapper_detail.unbind();
        ////////////////////////////////////////////////////////////////////////////////////////
        //列表区域
        ////////////////////////////////////////////////////////////////////////////////////////
        //hover行工具菜单显示处理
        if (this.modeArgs.editable) {
            this.$wrapper.find('tr.row_task').hover(function (e) {
                var $el = $(e.target);
                var $row = base.getRow($el);
                $row.find('td.cell_num span').hide();
                $row.find('td.cell_num i.icon-th').show();
                $row.find('td.cell_bool ul.nav').eq(0).hide();
                $row.find('td.cell_bool ul.nav').eq(1).find('i').show();
            }, function (e) {
                var $el = $(e.target);
                var $row = base.getRow($el);
                $row.find('td.cell_num span').show();
                $row.find('td.cell_num i.icon-th').hide();
                $row.find('td.cell_bool ul.nav').eq(0).show();
                $row.find('td.cell_bool ul.nav').eq(1).find('i').hide();
            });
        }
        ////////////////////////////////////////////////////////////////////////////////////////
        this.$wrapper.find('input').keyup(function () { base.getTask(base.getRow($(this))).setSubject($(this).val()); });
        //input焦点 目前需要在特定情况下对其进行修正
        //this.$wrapper.find('input').focus(function () { debuger.debug('focus'); }); //base.$focusRow = $el.parents('tr').eq(0); });
        //this.$wrapper.find('input').blur(function () { debuger.debug('blur'); }); //base.$focusRow = null; });
        ////////////////////////////////////////////////////////////////////////////////////////
        //click以及ctrlKey+click
        this.$wrapper.click(function (e) {
            var ctrl = e.ctrlKey;
            var shift = e.shiftKey;
            var $el = $(e.target);
            var $row = base.getRow($el);
            //只处理任务行
            if (!base.isTask($row)) return;
            //总是设置input焦点
            base.focusRow($row);
            var $focus = base.$focusRow = $row;
            ////////////////////////////////////////////////////////////////////////////////////////
            //row_task选中处理
            if (!$el.is('i') && !$el.is('a') && base.isTask($row)) {
                var $avtives = base.getActives();
                //多选支持
                if (!ctrl && !shift)
                    base.removeActive($avtives);
                //ctrl反选支持
                base[ctrl && base.isActive($row) ? 'removeActive' : 'setActive']($row);
                $avtives = base.getActives();
                if (shift)
                    base._batchRowActive($avtives.first(), $avtives.last());
                //此时选中情况已经变化
                $avtives = base.getActives();
                //呈现详情
                if ($avtives.length == 1)
                    base._renderDetail($focus);
                //批量详情
                else if ($avtives.length > 1)
                    base._renderBatchDetail($avtives);
                //设置是否可编辑
                base._setEditable(base.$wrapper_detail);
                return;
            }
            var task = cached_tasks[base.getTaskId($row)];
            ////////////////////////////////////////////////////////////////////////////////////////
            //是否完成
            if ($el.is('i') && $el.hasClass('icon-check')) {
                var old = task.isCompleted();
                task.setCompleted(!old);
                if (base.onCompletedChange)
                    base.onCompletedChange(task, old, !old);
                return;
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //priority选择
            if ($el.is('a') && $el.parent().is('li')) {
                var old = task.priority();
                var p = base[$el.attr('priority')];
                if (old == p) return;
                task.setPriority(p);
                debuger.assert(task.priority() == p);
                if (base.onPriorityChange)
                    base.onPriorityChange($row, task, old, p);
                return;
            }
        });
        ////////////////////////////////////////////////////////////////////////////////////////
        //其他快捷键
        this.$wrapper.keydown(function (e) {//TODO:调整keydown和keyup事件，避免持续keydown导致的损耗
            //debuger.debug(e.keyCode);
            //首次shift时
            if (e.keyCode == 16) {
                base._$shiftBegin = base.$focusRow;
                return;
            }
            //首次ctrl时
            else if (e.keyCode == 17) return;

            var ctrl = e.ctrlKey;
            var shift = e.shiftKey;
            var up = e.keyCode == 38;
            var down = e.keyCode == 40;
            var enter = e.keyCode == 13;
            var backspace = e.keyCode == 8;

            //集中忽略非快捷键处理
            if (!ctrl && !shift && !up && !down && !enter && !backspace)
                return;

            var $focus = base.$focusRow;
            var $actives = base.getActives();
            //过滤不在合法区域的行
            var $actives2 = $actives.filter(function () { return base._isRowOfValidRegion($(this)); });

            ////////////////////////////////////////////////////////////////////////////////////////
            //非编辑模式下的快捷键处理
            //上下切换↓
            if (!ctrl && !shift) {
                var $prev = base._findFocusPrev();
                var $next = base._findFocusNext();
                if (up && $prev)
                    base._fireRowSingleClick($prev);
                else if (down && $next)
                    base._fireRowSingleClick($next);
                //修正编辑状态
                base._setEditable(base.$wrapper_detail);
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //非编辑模式则忽略快捷键
            if (!base.modeArgs.editable) return;
            ////////////////////////////////////////////////////////////////////////////////////////
            //编辑模式下的快捷键处理
            //删除backspace 
            if (backspace) {
                var txt = base.getTaskVal($focus);
                var $p, $n;
                var ary;
                var b = false;
                if ($actives.length > 1) { //多行删除
                    $p = base._findActivePrev();
                    $n = base._findActiveNext();
                } else if (txt == '') {//焦点行删除
                    var ary = base._findNextAndPrev($focus);
                    $p = ary[0];
                    $n = ary[1];
                } else if ($actives.length == 1 && txt != '') {//带内容的焦点行删除
                    var $p = base._findNextAndPrev($focus)[0];
                    if ($p == null) return;
                    var input = $focus.find('input')[0];
                    if (input.selectionStart > 0 || input.selectionEnd > 0) return;
                    var prev = base.getTask($p);
                    //合并到上一行
                    prev.setSubject(prev.subject() + txt, true);
                    b = true; //合并操作不需要出现撤销删除
                }
                base.deleteTask(b);
                if ($p != null)
                    base._fireRowClick($p);
                else if ($n != null)
                    base._fireRowClick($n);
                return false;
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //仅当有焦点行时才有效的行为
            if ($focus == null) return;
            //新建Enter
            if (!ctrl && enter) {
                base.appendTask();
                return;
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //完成Ctrl+Enter
            if (ctrl && enter) {
                var $rows = base.modeArgs.shortcuts_canSetCompleted_RowOfInValidRegion ? $actives : $actives2;
                if ($rows.length == 0) return;
                var i = cached_tasks[base.getTaskId($rows.first())].isCompleted();
                $rows.each(function () {
                    var task = cached_tasks[base.getTaskId($(this))];
                    task.setCompleted(i); //先统一修正
                    task.setCompleted(!task.isCompleted());
                });
                //批量详情处理
                if (base._isBatchDetailValid())
                    base._renderBatchDetail($rows);
                return;
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //上下移动Ctrl+↓ 需处理跨空region的移动
            if (ctrl) {
                if (!base.modeArgs.shortcuts_move) return;
                if (up) {
                    var $prev = base._findAnyPrev($actives2.first());
                    if (!$prev) return;
                    if (base.isTask($prev))
                        $actives2.insertBefore($prev);
                    else if (base._isValidRegion($prev))
                        $prev.append($actives2);
                }
                else if (down) {
                    var $next = base._findAnyNext($actives2.last());
                    if (!$next) return;
                    if (base.isTask($next))
                        $actives2.insertAfter($next);
                    else if (base._isValidRegion($next))
                        $next.prepend($actives2);
                }
                //移动过程中会丢失焦点，在此修正
                base.focusRow($focus);
                //由于顺序变更需要刷新排序等
                base._flushIdxs();
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            //批量选择Shift+↓
            else if (shift) {
                var $prev = base._findFocusPrev();
                var $next = base._findFocusNext();
                if (up && $prev)
                    base._fireRowClick($prev);
                if (down && $next)
                    base._fireRowClick($next);
                base._batchRowActive(base._$shiftBegin, base.$focusRow)
            }
        });
        ////////////////////////////////////////////////////////////////////////////////////////
        //详情区域
        ////////////////////////////////////////////////////////////////////////////////////////
        if (this.modeArgs.editable) {
            this.$wrapper_detail.click(function (e) {
                var $el = $(e.target);
                var ids = $el.parents('.region_detail');
                if (ids.length == 0) return;
                ids = ids.eq(0).attr('id').split(',');
                //优先级 需处理批量场景
                if ($el.parent().is('#priority') || ($el.is('i') && $el.parent().parent().is('#priority'))) {
                    for (var i = 0; i < ids.length; i++) {
                        var task = cached_tasks[ids[i]];
                        var old = task.priority();
                        var p = $el.attr('priority');
                        task.setPriority(p);
                        //额外逻辑
                        if (base.onPriorityChange)
                            base.onPriorityChange(task.el(), task, old, p);
                    }
                    return;
                }
                //是否完成 需处理批量场景
                if ($el.is('#isCompleted') || ($el = $el.parent()).is('#isCompleted')) {
                    var isCompleted = !$el.hasClass('active');
                    for (var i = 0; i < ids.length; i++) {
                        var task = cached_tasks[ids[i]];
                        var old = task.isCompleted();
                        task.setCompleted(isCompleted);
                        //额外逻辑
                        if (base.onCompletedChange)
                            base.onCompletedChange(task, old, isCompleted);
                    }
                    //批量情况的修正 TODO:与task内的setdetailcompleted逻辑调整进行复用
                    if (ids.length > 1)
                        $el
                        [isCompleted ? 'addClass' : 'removeClass']('active')
                        [isCompleted ? 'addClass' : 'removeClass']('btn-success');
                    return;
                }
            });
            this.$wrapper_detail.keyup(function (e) {
                var $el = $(e.target);
                var isSubject = $el.is('#subject');
                var isBody = $el.is('#body');
                if (!isSubject && !isBody) return;
                var task = cached_tasks[$el.parents('.region_detail').eq(0).attr('id')];
                if (isSubject)
                    task.setSubject($el.val(), true);
                else if (isBody)
                    task.setBody($el.val());
            });
            this.$wrapper_detail.change(function (e) {
                var $el = $(e.target);
                //dueTime调整 需处理批量场景
                if ($el.is('#dueTime')) {
                    var ids = $el.parents('.region_detail');
                    if (ids.length == 0) return;
                    ids = ids.eq(0).attr('id').split(',');
                    var tasks = [ids.length];
                    var t = $.datepicker.parseDate('mm/dd/yy', $el.val());
                    for (var i = 0; i < ids.length; i++) {
                        var task = cached_tasks[ids[i]];
                        //若此时编辑了新增的任务，由于临时id被修正，将无法找到新的id，因此外围需调用repairBatchdetailId
                        debuger.assert(task != null);
                        tasks[i] = task;
                        task.setDueTime(t); //TODO:调整格式yy-mm-dd
                    }
                    if (ids.length > 0)
                    //额外逻辑
                        if (base.onDueTimeBatchChange)
                            base.onDueTimeBatchChange(tasks, t);
                }
            });
        }
        //允许额外的实现
        if (this.onPrepareBinds)
            this.onPrepareBinds();
    },
    //批量选中 配合shift全选
    _batchRowActive: function ($r1, $r2) {
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
    },
    //模拟行点击，置为激活
    _fireRowClick: function ($r, c, s) {
        //模拟event
        //var event = jQuery.Event("click");
        //event.ctrlKey = c == undefined ? false : c;
        //event.shiftKey = s == undefined ? false : s;
        //UNDONE:由于事件之间影响，此处直接完成ui逻辑
        //$r.find('input').focus().trigger(event);
        this.setActive($r);
        this.focusRow($r);
        this.$focusRow = $r;

        //详情呈现
        var $actives = this.getActives();
        if ($actives.length == 1)
            this._renderDetail(this.$focusRow);
        if ($actives.length > 1)
            this._renderBatchDetail($actives);
    },
    _fireRowSingleClick: function ($r) {
        this.removeActive(this.getActives());
        this._fireRowClick($r);
    },
    //以焦点行为依据
    _findFocusPrev: function () { return this._findNextAndPrev(this.$focusRow)[0]; },
    _findFocusNext: function () { return this._findNextAndPrev(this.$focusRow)[1]; },
    //与选中行为依据
    _findActivePrev: function () {
        var $actives = this.getActives();
        return this._findNextAndPrev($actives.length == 1 ? this.$focusRow : $actives.first())[0];
    },
    _findActiveNext: function () {
        var $actives = this.getActives();
        return this._findNextAndPrev($actives.length == 1 ? this.$focusRow : $actives.last())[1];
    },
    //返回row_task的有效前后行
    _findNextAndPrev: function ($r) {
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
        debuger.debug([
            $prev == null ? null : this.getTaskId($prev),
            this.getTaskId(this.$focusRow),
            $next == null ? null : this.getTaskId($next)
        ]);
        return [$prev, $next];
    },
    //返回row_task的有效前后row或前后region
    _findAnyPrev: function ($r) {
        var $prev = null;
        if ($r.prev().length != 0)
            $prev = $r.prev();
        else {
            $prev = this.getRegionOfRow($r).prev();
            $prev = $prev.length == 0 ? null : $prev;
        }
        return $prev;
    },
    _findAnyNext: function ($r) {
        var $next = null;
        if ($r.next().length != 0)
            $next = $r.next();
        else {
            $next = this.getRegionOfRow($r).next();
            $next = $next.length == 0 ? null : $next;
        }
        return $next;
    },
    _expandRegion: function ($r) { $r.show(); },
    _expandRowRegion: function ($row) {
        $row.parents('tbody').eq(0).show();
    },
    _setEditable: function ($el) {
        $el.find('input').attr('readonly', !this.modeArgs.editable);
        $el.find('textarea').attr('readonly', !this.modeArgs.editable);
    },
    _appendTaskToRow: function ($row, t, a) {
        var active = a == undefined ? cached_tasks[this.getTaskId($row)] : a;
        var $input = $row.find('input');
        t.el()[$input[0].selectionStart == 0 && $input.val() != '' ? 'insertBefore' : 'insertAfter']($row);
        if ($input[0].selectionStart > 0) {
            //若出现截断则拆分任务
            var prevVal = $input.val().substring(0, $input[0].selectionStart);
            var nextVal = $input.val().substring($input[0].selectionStart);
            debuger.debug('prevVal=' + prevVal);
            debuger.debug('nextVal=' + nextVal);
            active.setSubject(prevVal, true);
            t.setSubject(nextVal, true);
        }
    },
    ////////////////////////////////////////////////////////////////////////////////////////
    //行为和主要差异部分 不做实现
    //判断区域合法性
    _isValidRegion: function ($r) { return true; },
    _isRowOfValidRegion: function ($r) { return true; },
    //完整渲染
    render: function () { },
    //新建
    appendTask: function () { },
    //归档
    archiveTasks: function () { },
    ////////////////////////////////////////////////////////////////////////////////////////
    //行为和主要差异部分 部分实现
    //删除
    deleteTask: function (b) {
        var $actives = this.getActives();
        var l = $actives.length;
        if (l == 0)
            return;
        var base = this;

        //先清空上一次的删除缓冲
        this.continueDelete();
        this.deletes = [];
        //批量删除
        $actives.each(function () {
            var id = base.getTaskId($(this));
            var active = cached_tasks[id];
            cached_tasks[id] = null;
            //追加删除变更
            base.deletes = $.merge(base.deletes, [{ 'Type': 1, 'ID': id}]);
            active.el().remove();
        });

        if (b) {
            this.continueDelete();
            return;
        }
        //给予一定时间的撤销机会 
        //注意：由于定时原因，会导致重新加载操作时删除记录未被提交
        var i = 10;
        this.$cancel_delete.show().find('span').eq(0).html(l);
        this.deletes_timer = setTimeout(function () { base.continueDelete(); }, i * 1000);
        //优化体验 给出倒计时
        clearInterval(this.deletes_timer2);
        var $temp = this.$cancel_delete.show().find('span').eq(1).html(i);
        this.deletes_timer2 = setInterval(function () { if (i-- > 0) $temp.html(i) }, 1000);

        this._flushIdxs();
        this.$wrapper_detail.empty();
    },
    cancelDelete: function () {
        debuger.info('cancel deletes', this.deletes);
        clearTimeout(this.deletes_timer);
        this.deletes_timer = null;
        this.deletes = null;
        this.$cancel_delete.hide();
    },
    continueDelete: function () {
        debuger.info('continue deletes', this.deletes);
        clearTimeout(this.deletes_timer);
        //填充进全局删除记录
        if (this.deletes)
            changes_delete = $.merge(changes_delete, this.deletes);
        this.deletes_timer = null;
        this.deletes = null;
        this.$cancel_delete.hide();
    },
    toggleTasks: function () {
        var display = this.$wrapper.find('tbody').first().css('display');
        this.$wrapper.find('tbody')[display == 'none' ? 'show' : 'hide']();
    },
    setEditable: function (b) {
        debuger.debug('setEditable=' + b);
        this.modeArgs.editable = b;
        this.modeArgs.shortcuts_move = this.modeArgs.editable && this.modeArgs.shortcuts_move;
        this.modeArgs.shortcuts_canSetCompleted_RowOfInValidRegion = this.modeArgs.editable && this.modeArgs.shortcuts_canSetCompleted_RowOfInValidRegion;
        this._prepareBinds();
        this._setEditable(this.$wrapper);
    }
}
//父子辅助-多态模拟
function call_child(o, n, args) {
    if (o.child)
        return o.child[n].apply(o.child, args);
    else
        return o[n].apply(o, args);
}