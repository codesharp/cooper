//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />
/////////////////////////////////////////////////////////////////////////////////////////
//global specials of tasks
var cached_tasks = null;
var cached_idxs = null;
var identity = 0;
var changes_delete = [];//用于记录删除
//template
var tmp_region = $('#tmp_region').html();
var tmp_item = $('#tmp_region tbody').html();
var tmp_item_region_name = "tbody";
var tmp_detail = $('#tmp_detail').html();
var tmp_detail_batch = $('#tmp_detail_batch').html();
//$element
var $el_wrapper_region = $('#todolist_wrapper');
var $el_wrapper_detail = $('#detail_wrapper');
var $el_cancel_delete = $('#cancel_delete');
/////////////////////////////////////////////////////////////////////////////////////////
//描述任务缓存项
var Task = function () { this._init.apply(this, arguments); }
Task.prototype = {
    $el_row: null,
    $el_detail: null,
    editable: true,
    _init: function () {
        var t = arguments.length > 0 ? arguments[0] : {};
        this.editable = t['Editable'] == undefined ? true : t['Editable'];
        this['data'] = {
            'id': t['ID'] != undefined ? t['ID'].toString() : 'temp_' + (++identity) + '_' + new Date().getTime(), //可自动构建临时id 总是以string使用
            'subject': t['Subject'] != undefined ? t['Subject'] : '',
            'body': t['Body'] != undefined ? t['Body'] : '',
            'priority': t['Priority'] != undefined ? t['Priority'] : 0, //0=today 1=upcoming 2=later priority 总是以string使用
            'dueTime': t['DueTime'] != undefined && t['DueTime'] != null && t['DueTime'] != '' ? this._parseDate(t['DueTime']) : null,
            'isCompleted': t['IsCompleted'] != undefined ? t['IsCompleted'] : false,
            'tags': []
        }
        this.$el_row = this._generateItem(this['data']);
        this.$el_detail = null;
    },
    _generateItem: function (d) { return $(render(tmp_item, d)); },
    _generateDetail: function (d) { return $(render(tmp_detail, d)); },
    _parseDate: function (t) { return typeof (t) == 'string' ? $.datepicker.parseDate('yy-mm-dd', t) : t; },
    _parseDateString: function (t) { return (t.getMonth() + 1) + '-' + t.getDate(); },
    _parseFullDateString: function (t) { return t.getFullYear() + '-' + (t.getMonth() + 1) + '-' + t.getDate(); },
    renderRow: function () {
        this.setCompleted(this.isCompleted());
        this.setPriority(this.priority());
        this.setDueTime(this.dueTime());
    },
    renderDetail: function () {
        if (!this.$el_detail)
            this.$el_detail = this._generateDetail(this['data']);
        //datepicker重复初始化问题
        this.$el_detail.find('#dueTime').removeClass('hasDatepicker');
        if (this.editable)
            this.$el_detail.find('#dueTime').datepicker();
        //设置值
        this.setDetail_Completed(this.isCompleted());
        this.setDetail_Subject(this.subject());
        this.setDetail_Priority(this.priority());
        this.setDetail_DueTime(this.dueTime());
        this.setDetail_Body(this.get('body'));
        //设置url快捷链接区域 临时方案
        var $urls = this.$el_detail.find('#urls');
        $urls.find('ul').empty();
        var i = 0;
        this.get('body').replace(/[http|https|ftp]+:\/\/\S*/ig, function (m) {
            if (i++ == 0)
                $urls.find('button:first a').attr('href', m).attr('title', m).html(m);
            else
                $urls.find('ul').append('<li><a target="_blank" href="' + m + '" title="' + m + '">' + m.substring(0, 30) + '...</a></li>');
        });
        $urls.parents('tr')[i == 0 ? 'hide' : 'show']();
        $urls.find('button:eq(1)')[i == 1 ? 'hide' : 'show']();
        $urls.find('url')[i == 1 ? 'hide' : 'show']();

        return this.$el_detail;
    },
    //额外修正一些显示问题 由于未呈现导致的
    fixDetail: function () {
        this.setDetail_Body(this.get('body'));
    },
    dispose: function () {
        if (this.$el_row) {
            this.$el_row.remove();
            this.$el_row = null;
        }
        if (this.$el_detail) {
            this.$el_detail.remove();
            this.$el_detail = null;
        }
    },
    get: function (k) { return this['data'][k]; },
    update: function (k, v) {
        if (!this.editable) return;
        if (this['data'][k] == v) return false;
        this['data'][k] = v;
        //设计变更列表，用于提交到server
        if (!this.changes)
            this.changes = {};
        //只记录最后一次
        this.changes[k] = { 'ID': this.id(), 'Name': k, 'Value': v };
        debuger.info('new changelog ' + k + ' of ' + this.id(), this.changes[k]);
        return true;
    },
    set: function (k, v) {
        if (k == 'priority')
            this.setPriority(v);
        if (k == 'dueTime')
            this.setDueTime(v);
    },
    popChanges: function () {
        if (!this.changes) return [];
        var arr = [];
        for (var k in this.changes)
            arr = $.merge(arr, [this.changes[k]]);
        this.changes = null;
        return arr;
    },
    //常用属性
    el: function () { return this.$el_row; },
    id: function () { return this.get('id'); },
    isCompleted: function () { return this.get('isCompleted') },
    priority: function () { return this.get('priority') },
    dueTime: function () { return this.get('dueTime'); },
    subject: function () { return this.get('subject'); },
    ///////////////////////////////////////////////////////////////////////////////
    //属性以及ui设置
    setId: function (i) {
        this['data']['id'] = i;
        this.$el_row.attr('id', i);
        this.setDetail_Id(i);
    },
    setIndex: function (i) {
        this.$el_row.find('.cell_num span').html(i);
    },
    setSubject: function (s, f) {
        this.update('subject', s);
        //为双向同步而设置的f标识
        if (f == undefined || !f)
            this.setDetail_Subject(s);
        else if (f)
            this.$el_row.find('input').val(s);
    },
    setBody: function (b) {
        this.update('body', b);
        this.setDetail_Body(b);
    },
    setCompleted: function (b) {
        this.update('isCompleted', b);
        this.$el_row.find('.cell_bool .nav').eq(0).find('.icon-ok').css('display', b ? 'block' : 'none');
        this.$el_row[b ? 'addClass' : 'removeClass']('row_completed');
        this.setDetail_Completed(b);
    },
    setPriority: function (p) {
        this.update('priority', p.toString());
        //设置priority图标显示与否 避免出现inline
        this.$el_row.find('.cell_bool .nav').eq(0).find('.icon-time').css('display', p == 0 ? 'block' : 'none');
        this.setDetail_Priority(p);
    },
    setDueTime: function (t) {
        if (t == undefined || t == null) {
            this.update('dueTime', null);
            this.$el_row.find('.cell_duetime').html('&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;');
            return;
        }
        this.update('dueTime', t);
        var today = $.datepicker.parseDate('yy-mm-dd', this._parseFullDateString(new Date())).getTime();
        var date = $.datepicker.parseDate('yy-mm-dd', this._parseFullDateString(t)).getTime();
        //优化用户化文本显示
        this.$el_row.find('.cell_duetime').html(today == date ? lang.today : this._parseDateString(t));
        //过期标记
        this.$el_row.find('.cell_duetime')[today >= date ? 'addClass' : 'removeClass']('cell_duetime_expired');
        this.setDetail_DueTime(t);
    },
    ///////////////////////////////////////////////////////////////////////////////
    //detail设置
    setDetail_Id: function (i) {
        if (!this.$el_detail) return;
        this.$el_detail.attr('id', i);
    },
    setDetail_Subject: function (s) {
        if (!this.$el_detail) return;
        this.$el_detail.find('#subject').val(s);
    },
    setDetail_Body: function (b, f) {
        if (!this.$el_detail) return;
        var $el = this.$el_detail.find('#body');
        //修正高度 自适应
        var base = $el[0];
        //$el.height('');//auto
        base.rows = base.value.split('\n').length + 1;
        var l = parseInt($el.css('line-height').replace('px', ''));
        //ie下不兼容
        if (isNaN(l) || base.scrollHeight == 0) return;
        base.rows = Math.floor(base.scrollHeight / l);
        //无需此步骤
        //$el.html(b);
    },
    setDetail_Priority: function (p) {
        if (!this.$el_detail) return;
        this.$el_detail.find('#priority button')
            .removeClass('active')
            .eq(parseInt(p)).addClass('active');
    },
    setDetail_Completed: function (b) {
        if (!this.$el_detail) return;
        this.$el_detail.find('#isCompleted')
            [b ? 'addClass' : 'removeClass']('active')
            [b ? 'addClass' : 'removeClass']('btn-success');
    },
    setDetail_DueTime: function (t) {
        if (!this.$el_detail) return;
        if (t != null)
            this.$el_detail.find('#dueTime').val(this._parseFullDateString(t));
    }
}
/////////////////////////////////////////////////////////////////////////////////////////
//描述任务排序索引缓存
var Idx = function () { this._init.apply(this, arguments); }
Idx.prototype = {
    $el_region: null,
    _init: function () {
        this['by'] = arguments[0];
        this['key'] = arguments[1]; //0,1,2,prj1,team1
        this['name'] = arguments[2]; //今天、稍后、迟些、项目1、团队1
        this['idx'] = arguments[3]; //[0,1,2,4]
        this.$el_region = this._generateRegion();
    },
    _generateRegion: function () {
        var $el = $(render(tmp_region, this));
        $el.find(tmp_item_region_name).empty();
        return $el;
    },
    dispose: function () {
        if (!this.$el_region) return;
        this.$el_region.remove();
        this.$el_region = null;
    },
    el: function () { return this.$el_region; },
    indexs: function (idx) { if (idx) this['idx'] = idx; return this['idx']; },
    //渲染至region内
    render: function () {
        this.$el_region.find(tmp_item_region_name).empty();
        var idx = this.indexs();
        for (var i = 0; i < idx.length; i++) {
            var id = idx[i].toString();
            var task = cached_tasks[id];
            if (!task) {
                debuger.error('wrong id ' + id, idx);
                continue;
            }
            task.renderRow();
            task.setIndex(i + 1); //设置索引显示
            this.$el_region.find(tmp_item_region_name).append(task.el());
        }
        //总数
        this.$el_region.find('.badge').html(idx.length);
    },
    //根据el刷新索引数据以及对应索引显示
    flush: function (k) {
        var base = this;
        var $els = this.$el_region.find(tmp_item_region_name).find('tr');
        var ary = new Array($els.length);
        $els.each(function (i, n) {
            var id = $(n).attr('id');
            ary[i] = id;
            cached_tasks[id].setIndex(i + 1); //设置索引显示
            //依据idx数据额外修正task数据
            if (k != undefined)
                cached_tasks[id].set(k, base['key']);
        });
        this['idx'] = ary;
        this.$el_region.find('.badge').html(ary.length);
    },
    //实时刷新并获取索引
    getIndexs: function () {
        var $els = this.$el_region.find(tmp_item_region_name).find('tr');
        var ary = new Array($els.length);
        $els.each(function (i, n) {
            var id = $(n).attr('id');
            ary[i] = id;
        });
        this['idx'] = ary;
        return this['idx'];
    },
    append: function (t) { this.el().find(tmp_item_region_name).append(t.el()); this.flush(); },
    prepend: function (t) { this.el().find(tmp_item_region_name).prepend(t.el()); this.flush(); }
}
////////////////////////////////////////////////////////////////////////////////////////
//all=来自server的所有任务数组
//idx=优先级排序
function init(all, idxs) {
    //清理
    if (cached_tasks)
        for (var i in cached_tasks)
            if (cached_tasks[i])
                cached_tasks[i].dispose();
    if (cached_idxs)
        for (var i in cached_idxs)
            if (cached_idxs[i])
                cached_idxs[i].dispose();
    //构建本地缓存 cached_tasks
    cached_tasks = {};
    for (var i = 0; i < all.length; i++)
        cached_tasks[all[i]['ID'].toString()] = new Task(all[i]);
    debuger.info('original tasks', all);
    debuger.info('cached tasks', cached_tasks);
    //构建分组排序缓存 cached_idx {'0':List}
    cached_idxs = {};
    for (var i = 0; i < idxs.length; i++)
        cached_idxs[idxs[i]['Key']] = new Idx(idxs[i]['By'], idxs[i]['Key'], idxs[i]['Name'], idxs[i]['Indexs']);
    debuger.info('original sorts', idxs);
    debuger.info('cached sorts', cached_idxs);
}