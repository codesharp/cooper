//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_row.js" />

//wrapper主体事件绑定，处理全局事件
UI_List_Common.prototype._bind = function () {
    var base = this;

    // *****************************************************
    // 部分全局绑定
    // *****************************************************
    $(document).keyup(function (e) {
        var backspace = e.keyCode == 8;
        if (backspace) return false;
    });
    // *****************************************************
    // 列表区域
    // task.editable决定部分编辑功能启用与否，其余则主要在task.js中自行控制
    // *****************************************************
    //hover行工具菜单显示处理
    this.$wrapper.find('tr.row_task').hover(function (e) {
        var $el = $(e.target);
        var $row = base.getRow($el);
        var task = base.getTask($row);
        if (!task.editable) return;
        $row.find('td.cell_num span').hide(); //TODO:要改善的菜单功能
        $row.find('td.cell_num i.icon-th').show();
        $row.find('td.cell_bool ul.nav').eq(0).hide();
        $row.find('td.cell_bool ul.nav').eq(1).find('i').show();
    }, function (e) {
        var $el = $(e.target);
        var $row = base.getRow($el);
        var task = base.getTask($row);
        if (!task.editable) return;
        $row.find('td.cell_num span').show();
        $row.find('td.cell_num i.icon-th').hide();
        $row.find('td.cell_bool ul.nav').eq(0).show();
        $row.find('td.cell_bool ul.nav').eq(1).find('i').hide();
    });
    ////////////////////////////////////////////////////////////////////////////////////////
    //input变更同步
    this.$wrapper.find('input').keyup(function () {
        var task = base.getTask(base.getRow($(this)));
        if (task.editable)
            task.setSubject($(this).val());
        else
            $(this).val(task.subject());
    });
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
            debuger.profile('row_task_click');
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
            debuger.profileEnd('row_task_click');
            return;
        }
        var task = base.getTask($row);
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
    //详情区域 支持批量处理
    ////////////////////////////////////////////////////////////////////////////////////////
    //subject、body
    this.$wrapper_detail.keyup(function (e) {
        var $el = $(e.target);
        var isSubject = $el.is('#subject');
        var isBody = $el.is('#body');
        //if (!isSubject && !isBody) return;
        var task = base.getTaskById($el.parents('.region_detail').eq(0).attr('id'));
        if (isSubject)
            task.setSubject($el.val(), true);
        else if (isBody)
            task.setBody($el.val());
    });
    this.$wrapper_detail.click(function (e) {
        var $el = $(e.target);
        var ids = $el.parents('.region_detail');
        if (ids.length == 0) return;
        ids = ids.eq(0).attr('id').split(',');
        //优先级 需处理批量场景
        if ($el.parent().is('#priority') || ($el.is('i') && $el.parent().parent().is('#priority'))) {
            for (var i = 0; i < ids.length; i++) {
                var task = base.getTaskById(ids[i]);
                if (!task.editable) continue;
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
        if ($el.is('#isCompleted') || ($el.parent().is('#isCompleted') && ($el = $el.parent()))) {
            var isCompleted = !$el.hasClass('active');
            for (var i = 0; i < ids.length; i++) {
                var task = base.getTaskById(ids[i]);
                if (!task.editable) continue;
                var old = task.isCompleted();
                task.setCompleted(isCompleted);
                //额外逻辑
                if (base.onCompletedChange)
                    base.onCompletedChange(task, old, isCompleted);
            }
            //批量情况的修正
            if (ids.length > 1)
                $el
                    [isCompleted ? 'addClass' : 'removeClass']('active')
                    [isCompleted ? 'addClass' : 'removeClass']('btn-success');
            return;
        }
    });
    this.$wrapper_detail.change(function (e) {
        var $el = $(e.target);
        //dueTime调整 需处理批量场景
        //注意对batchduetime的识别
        if ($el.is('#dueTime') || $el.is('#' + base.batch_id_dueTime)) {
            var ids = $el.parents('.region_detail');
            if (ids.length == 0) return;
            ids = ids.eq(0).attr('id').split(',');
            var tasks = [ids.length];
            var t = $.datepicker.parseDate('mm/dd/yy', $el.val());
            for (var i = 0; i < ids.length; i++) {
                var task = base.getTaskById(ids[i]);
                //若此时编辑了新增的任务，由于临时id被修正，将无法找到新的id，因此外围需调用repairBatchdetailId
                debuger.assert(task != null);
                if (!task.editable) continue;
                tasks[i] = task;
                task.setDueTime(t); //TODO:调整格式yy-mm-dd
            }
            if (ids.length > 0)
                //额外逻辑
                if (base.onDueTimeBatchChange)
                    base.onDueTimeBatchChange(tasks, t);
        }
    });

    //部分Task事件，不适合全局绑定
    Task.prototype.bind_detail = function ($el_detail, task) {
        //datepicker重复初始化问题
        $el_detail.find('#dueTime').removeClass('hasDatepicker');
        if (task.editable)
            $el_detail.find('#dueTime').datepicker();

        //标签设置区域初始化
        var $tags = $el_detail.find('#tags');
        var $tags_input = $el_detail.find('#tags_input');
        var $tags_btn = $el_detail.find('#tags_btn');
        var tags = base.getTags();
        base.detail_array_control_bind(task,
            'render_detail_tags',
            'removeTag',
            false,
            $tags,
            $tags_input,
            $tags_btn,
            tags.length > 0 ? tags : [''],//至少需要一个以供搜索时能够有机会新增
            function (item) {
                var q = this.query;
                var r = ~item.toLowerCase().indexOf(this.query.toLowerCase());
                //支持在搜索到不存在项时自动插入新项
                if (!r)
                    this.source = $.merge($.grep(this.source, function (n) { return n != q }), [q]);
                return r;
            },
            function (items) {
                var that = this;
                //重新match
                var all = $.grep(this.source, function (item) {
                    return that.matcher(item)
                })
                //HACK:重载sorter使其支持搜索时新增
                //依赖于bootstrap当前实现
                return $.fn.typeahead.Constructor.prototype.sorter.apply(this, [all]);
            },
            function (val) {
                debuger.debug('add-tags-val', val);
                task.addTag(val);
                //同时添加进全局
                base.addTag(val);
                $tags_input.blur();
                return val;
            }
        );
    }
}
//类似标签设置区域控件行为绑定，仅仅是简易复用，未控件化
//包括：数组显示，文本渲染，typeahead等
UI_List_Common.prototype.detail_array_control_bind = function (task,
    fn_render,
    fn_remove,
    single,//是否只允许单独项
    $text, $input, $btn,
    source, matcher, sorter, updater, highlighter,
    blur) {
    //设置指定区域的渲染方式
    if (fn_render) {
        task[fn_render] = function ($e, data) {
            $e.empty();
            $.each(data, function (i, n) {
                //简单处理tag呈现
                var $i = fn_render.indexOf('tag') >= 0
                    ? $('<span class="label" style="font-weight:normal;"></span>')
                    : $('<span></span>');
                //防止html/script注入
                $i.text((n['name'] || n));
                $i.append('&nbsp;<a style="color:#000;" class="flag_remove" val="'
                    + (n['id'] || n)
                    + '" title="'
                    + lang.remove_from_task
                    + '">x</a>');
                $e.append($i).append('&nbsp;');
            });
        }
    }
    //显示编辑区域
    $btn.unbind('click').click(function () {
        $btn.hide();
        if (single)
            $text.hide();
        $input.val(single ? $text.html() : '').show().focus();
    });
    //删除项
    if (fn_remove) {
        $text.unbind('click').click(function (e) {
            var $el = $(e.target);
            if ($el.hasClass('flag_remove')) {
                var i = $el.attr('val');
                //TODO:获取详情区域容器
                //TODO:支持批量设置
                /*for (var i = 0; i < ids.length; i++) {
                    base.getTaskById(ids[i]).removeTag(tag);
                }*/
                task[fn_remove](i);
            }
        });
    }
    //搜索
    var option = {
        source: source,
        matcher: matcher,
        sorter: sorter,
        updater: updater
    };
    if (highlighter)
        option.highlighter = highlighter;
    $input.typeahead(option);
    $input.unbind('blur').blur(function () {
        setTimeout(function () {
            $text.show();
            $btn.show();
            $input.hide().data('typeahead').hide();
        }, 100);//为了让typehead click有效 时间间隔至少100ms以上
        if (blur)
            blur();
    });
}