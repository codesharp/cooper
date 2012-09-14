//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_row.js" />
///<reference path="task_bind.js" />

//team模块相关功能事件绑定
UI_List_Common.prototype._bindTeam = function () {
    var base = this;

    ////////////////////////////////////////////////////////////////////////////////////////
    //详情区域
    ////////////////////////////////////////////////////////////////////////////////////////
    /*this.$wrapper_detail.click(function (e) {
        var $el = $(e.target);
        var ids = getIds($el);
        if (!ids) return;
    });*/

    //与team相关功能的UI行为在此设置，以便于阅读
    Task.prototype.render_detail_comments = function ($c, cs) {
        $c.empty();
        $.each(cs, function (i, n) {
            $c.append('\
                <tr>\
                    <td class="comment-icon"><i class="icon-comment"></i></td>\
                    <td class="comment-content">\
                        <span class="comment-creator">' + (n['creator'] ? n['creator']['name'] : '') + '</span>\
                        <span class="comment-text">' + n['body'].replace(/\n/g, '<br/>') + '</span>\
                        <div class="comment-time">' + moment(n['createTime'], "YYYY-MM-DD HH:mm:ss").fromNow() + '</div>\
                    </td>\
                </tr>\ '
            );
        });
    }
    Task.prototype.bind_detail_team = function () { base.bind_detail_team.apply(base, arguments); };
}

UI_List_Common.prototype.bind_detail_team = function ($el_detail, task) {
    var base = this;
    var batch = $.isArray(task);

    //执行人设置区域初始化
    var $assignee = $el_detail.find('#assignee');
    var $assignee_input = $el_detail.find('#assignee_input');
    var $assignee_btn = $el_detail.find('#assignee_btn');
    base.detail_array_control_bind(task,
        null,
        null,
        true,
        $assignee,
        $assignee_input,
        $assignee_btn,
        base.getTeamMembers(),
        matcher,
        sorter,
        function (val) {
            debuger.debug('updater-assignee-val', val);
            var item = getItem(val);
            debuger.debug('updater-assignee-item', item);
            if (batch) {
                for (var i = 0; i < task.length; i++)
                    if (task[i].editable) {
                        var old = task[i].assignee();
                        task[i].setAssignee(item);
                        if (base.onAssigneeChange)
                            base.onAssigneeChange(task[i], old, item);
                    }
            }
            else {
                var old = task.assignee();
                task.setAssignee(item);
                if (base.onAssigneeChange)
                    base.onAssigneeChange(task, old, item);
            }
            return item['name'];
        },
        highlighter,
        function () {
            if (!batch)
                //blur事件内容清空时移除assignee
                if ($assignee_input.val() == '') {
                    var old = task.assignee();
                    task.setAssignee(null);
                    if (base.onAssigneeChange)
                        base.onAssigneeChange(task, old, null);
                }
        }
    );

    //项目设置区域初始化
    var $projects = $el_detail.find('#projects');
    var $projects_btn = $el_detail.find('#projects_btn');
    var $projects_input = $el_detail.find('#projects_input');
    base.detail_array_control_bind(task,
        'render_detail_projects',
        'removeProject',
        false,
        $projects,
        $projects_input,
        $projects_btn,
        base.getProjects(),
        matcher,
        sorter,
        function (val) {
            debuger.debug('updater-projects-val', val);
            var item = getItem(val);
            debuger.debug('updater-projects-item', item);
            if (batch) {
                for (var i = 0; i < task.length; i++)
                    if (task[i].editable)
                        task[i].addProject(item);
            }
            else
                task.addProject(item);
            return item['name'];
        },
        highlighter
    );

    //评论区域
    var $comments = $el_detail.find('#comments');
    var $comment_btn = $el_detail.find('#comment_btn');
    var $comment_input = $el_detail.find('#comment_input');
    $comment_btn.unbind('click').click(function () {
        var body = $.trim($comment_input.val());
        if (body == '') return;
        if (body.length < 5) return alert(lang.comment_must_more_than_5);
        if (body.length > 500) return alert(lang.comment_must_less_than_500);
        task.addComment(body);
        $comment_input.val('');
    });


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
            //下拉格式：name(email)
            item = item['id'] + '#' + n + (item['email'] ? '(' + item['email'] + ')' : '');
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
        });
    }
    function getItem(val) {
        var a = val.split('#');
        return {
            'id': a[0],
            'name': a[1].split('(')[0]//name(email)
        };
    }
}