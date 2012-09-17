//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_row.js" />

//键盘快捷键
UI_List_Common.prototype._bindShortcuts = function () {
    var base = this;

    //可连续触发
    this.$wrapper.keydown(function (e) {
        //debuger.debug(e.keyCode);
        //debuger.debug(e.shiftKey);
        //是否是输入法事件
        var ime = e.keyCode == 229;
        //首次shift时
        if (e.keyCode == 16 || (e.shiftKey && ime)) {
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
        //此处集中忽略导致系统快捷键功能失效
        //if (!ctrl && !shift && !up && !down && !enter && !backspace)
        //return;

        var $focus = base.$focusRow;
        var $actives = base.getActives();
        //过滤不在合法区域的行
        var $actives2 = $actives.filter(function () { return base._isRowOfValidRegion($(this)); });
        //以及非编辑状态的任务
        var $actives3 = $actives2.filter(function () { return base.getTask($(this)).editable; });

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
        //仅当有焦点行时才有效的行为
        if ($focus == null) return;
        ////////////////////////////////////////////////////////////////////////////////////////
        //新建Enter
        //由于输入法问题，不可放置在keyup事件中
        //https://github.com/codesharp/cooper/issues/98
        if (!ctrl && enter && base.modeArgs.editable) {
            base.appendTask();
            return false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////
        //删除backspace PS:若使用keyup会与正常删除冲突导致只剩一个字符时触发删除
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
            if (base.deleteTask(b) != 0) {
                if ($p != null)
                    base._fireRowClick($p);
                else if ($n != null)
                    base._fireRowClick($n);
            }
            return false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////
        //上下移动Ctrl+↓ 需处理跨空region的移动
        if (ctrl) {
            if (!base.modeArgs.editable || !base.modeArgs.shortcuts_move) return;
            if (up) {
                var $prev = base._findAnyPrev($actives2.first());
                if (!$prev) return;
                if (base.isTask($prev))
                    $actives2.insertBefore($prev);//TODO:增加判断目标区域的sortkey是否变更
                else if (base._isValidRegion($prev))
                    $prev.append($actives3);
                //UNDONE:不可编辑的任务不应被跨分组移动，当前只处理了基本情况
                //https://github.com/codesharp/cooper/issues/105
                //简单处理：不可编辑的直接忽略，使用$active3
            }
            else if (down) {
                var $next = base._findAnyNext($actives2.last());
                if (!$next) return;
                if (base.isTask($next))
                    $actives2.insertAfter($next);
                else if (base._isValidRegion($next))
                    $next.prepend($actives3);
            }
            //避免阻断其他快捷键
            //issue:https://github.com/codesharp/cooper/issues/87
            if (up || down) {
                //移动过程中会丢失焦点，在此修正
                base.focusRow($focus);
                //由于顺序变更需要刷新排序等
                base._flushSorts();
                //取消事件冒泡避免滚动条意外滚动
                e.preventDefault();
                e.stopPropagation();
            }
            return;
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
            base._batchRowActive(base._$shiftBegin, base.$focusRow);
            return;
        }
    });


    //非连续触发，安全操作
    this.$wrapper.keyup(function (e) {
        //debuger.debug('keyup=' + e.keyCode);
        if (e.keyCode == 17) return;
        var ctrl = e.ctrlKey;
        var shift = e.shiftKey;
        var up = e.keyCode == 38;
        var down = e.keyCode == 40;
        var enter = e.keyCode == 13;
        var backspace = e.keyCode == 8;
        if (!ctrl && !shift && !up && !down && !enter && !backspace)
            return;

        var $focus = base.$focusRow;
        if ($focus == null) return;

        var $actives = base.getActives();
        //过滤不在合法区域的行
        var $actives2 = $actives.filter(function () { return base._isRowOfValidRegion($(this)) });
        //以及非编辑状态的任务
        var $actives3 = $actives2.filter(function () { return base.getTask($(this)).editable; });

        ////////////////////////////////////////////////////////////////////////////////////////
        //完成Ctrl+Enter
        if (ctrl && enter) {
            var $rows = base.modeArgs.shortcuts_canSetCompleted_RowOfInValidRegion ? $actives : $actives3;
            if ($rows.length == 0) return;
            var i = base.getTask($rows.first()).isCompleted();
            $rows.each(function () {
                var task = base.getTask($(this));
                task.setCompleted(i); //先统一修正
                task.setCompleted(!task.isCompleted());
            });
            //批量详情处理
            if ($actives.length > 1)
                base._renderBatchDetail($actives);
            return;
        }
    });
}