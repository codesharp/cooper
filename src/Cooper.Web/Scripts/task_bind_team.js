//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_row.js" />

//team模块相关功能事件绑定
UI_List_Common.prototype._bindTeam = function () {
    var base = this;

    if (!this.modeArgs.editable) return;
    ////////////////////////////////////////////////////////////////////////////////////////
    //详情区域
    ////////////////////////////////////////////////////////////////////////////////////////
    this.$wrapper_detail.click(function (e) {
        var $el = $(e.target);
        var ids = getIds($el);
        if (!ids) return;

        //assignee设置
        if ($el.is('#assignee_btn') || $el.parent().is('#assignee_btn')) {
            $el = $el.parent();
            var $p = $el.parent();
            $p.find('#assignee,#assignee_btn').hide();
            $p.find('#assignee_input').val($('#assignee', $p).html()).show().focus();
            return;
        }
        //project设置
        if ($el.is('#projects_btn') || $el.parent().is('#projects_btn')) {
            $el = $el.parent();
            var $p = $el.parent();
            $p.find('#projects_btn').hide();
            $p.find('#projects_input').val('').show().focus();
            return;
        }
        if ($el.hasClass('flag_removeProject')) {
            var projectId = $el.attr('id');
            debuger.debug('remove project#' + projectId, ids);
            for (var i = 0; i < ids.length; i++) {
                base.getTaskById(ids[i]).removeProject(projectId);
            }
        }
    });

    //与team相关功能的UI行为在此设置，以便于阅读
    Task.prototype.render_detail_projects = function ($p, ps) {
        $p.empty();
        $.each(ps, function (i, n) {
            $p.append('<span>'
                + n['name']
                + ' <a class="flag_removeProject" id="'
                + n['id']
                + '" title="'
                + lang.remove_from_project
                + '">x</a></span> ');
        });
    }
    Task.prototype.bind_detail_team = function ($el_detail, task) {
        var $assignee = $el_detail.find('#assignee');
        var $assignee_input = $el_detail.find('#assignee_input');
        var $projects = $el_detail.find('#projects');
        var $projects_input = $el_detail.find('#projects_input');

        //var ids = getIds($el_detail);
        //if (!ids) return;
        //TODO:支持批量设置

        //typeahead组件初始化
        $assignee_input.typeahead({
            source: base.getTeamMembers(),
            sorter: sorter,
            matcher: matcher,
            highlighter: highlighter,
            updater: function (val) {
                debuger.debug('updater-assignee-val', val);
                var item = getItem(val);
                debuger.debug('updater-assignee-item', item);
                task.setAssignee(item);
                $assignee_input.blur();
                return item['name'];
            }
        });
        $projects_input.typeahead({
            source: base.getProjects(),
            sorter: sorter,
            matcher: matcher,
            highlighter: highlighter,
            updater: function (val) {
                debuger.debug('updater-projects-val', val);
                var item = getItem(val);
                debuger.debug('updater-projects-item', item);
                task.addProject(item);
                $projects_input.blur();
                return item['name'];
            }
        });

        //blur事件无法全局绑定，但逻辑仍保持在此
        $assignee_input.unbind('blur').blur(function () {
            var $p = $(this).parent();
            $p.find('#assignee,#assignee_btn').show();
            $p.find('#assignee_input').hide().data('typeahead').hide();
        });
        $projects_input.unbind('blur').blur(function () {
            var $p = $(this).parent();
            $p.find('#projects_btn').show();
            $p.find('#projects_input').hide().data('typeahead').hide();
        });
    }

    function getIds($el) {
        var ids = $el.parents('.region_detail');
        return ids.length != 0 ? ids.eq(0).attr('id').split(',') : null;
    }
    function matcher(item) {
        return ~item.name.toLowerCase().indexOf(this.query.toLowerCase())
    }
    function sorter(items) {
        var beginswith = []
        , caseSensitive = []
        , caseInsensitive = []
        , item

        while (item = items.shift()) {
            var n = item['name'];
            item = item['id'] + '#' + n;
            if (!n.toLowerCase().indexOf(this.query.toLowerCase())) beginswith.push(item)
            else if (~n.indexOf(this.query)) caseSensitive.push(item)
            else caseInsensitive.push(item)
        }

        return beginswith.concat(caseSensitive, caseInsensitive)
    }
    function highlighter(item) {
        item = item.split('#')[1];
        var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&')
        return item.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
            return '<strong>' + match + '</strong>'
        })
    }
    function getItem(val) {
        var a = val.split('#');
        return { 'id': a[0], 'name': a[1] };
    }
}