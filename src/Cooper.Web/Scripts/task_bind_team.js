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
            $p.find('#projects_input').show().focus();
            return;
        }
        if ($el.hasClass('flag_removeProject')) {
            alert('TODO');
        }
    });

    Task.prototype.bind_detail_team = function ($el_detail) {
        //TODO:typehead组件初始化

        //blur事件无法全局绑定，但逻辑仍保持在此
        $el_detail.find('#assignee_input').unbind('blur').blur(function () {
            var $p = $(this).parent();
            $p.find('#assignee,#assignee_btn').show();
            $p.find('#assignee_input').hide();
        });
        $el_detail.find('#projects_input').unbind('blur').blur(function () {
            var $p = $(this).parent();
            $p.find('#projects_btn').show();
            $p.find('#projects_input').hide();
        });
    }

    function getIds($el) {
        var ids = $el.parents('.region_detail');
        return ids.length != 0 ? ids.eq(0).attr('id').split(',') : null;
    }
}