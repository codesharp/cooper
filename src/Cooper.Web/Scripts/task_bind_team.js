//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="task.js" />
///<reference path="task_common.js" />
///<reference path="task_row.js" />

//team模块相关功能事件绑定
UI_List_Common.prototype._bindTeam = function () {
    var base = this;
    ////////////////////////////////////////////////////////////////////////////////////////
    //详情区域
    ////////////////////////////////////////////////////////////////////////////////////////
    if (this.modeArgs.editable) {
        this.$wrapper_detail.click(function (e) {
            var $el = $(e.target);
            var ids = $el.parents('.region_detail');
            if (ids.length == 0) return;
            ids = ids.eq(0).attr('id').split(',');

            //assignee设置
            if ($el.is('#assignee_btn') || $el.parent().is('#assignee_btn')) {
                $el = $el.parent();
                var $p = $el.parent();
                $p.find('#assignee,#assignee_btn').hide();
                $p.find('#assignee_input').show().focus();
                return;
            }
            //project设置
            if ($el.is('#projects_btn') || $el.parent().is('#projects_btn')) {
                $el = $el.parent();
                var $p = $el.parent();
                $p.find('#projects_btn').hide();
                $p.find('#projects_input').show().focus();
                return;
            }
            if ($el.hasClass('flag_removeProject')) {
                alert('TODO');
            }
        });
    }
}