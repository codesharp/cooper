//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />

var identity = 0;
//描述任务缓存项
var Task = function () { this._init.apply(this, arguments); }
Task.prototype = {
    $el_row: null,
    $el_detail: null,
    editable: true,
    _init: function () {
        var t = arguments.length > 0 ? arguments[0] : {};
        this.editable = t['Editable'] == undefined ? true : t['Editable'];
        //数据格式适配
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

        if (debuger.isDebugEnable)
            this._getRowEl('subject').attr('placeholder', '#' + this.id());
    },
    _generateItem: function (d) { return $(render($('#tmp_region tbody').html(), d)); },
    _generateDetail: function (d) { return $(render($('#tmp_detail').html(), d)); },
    _parseDate: function (t) { return typeof (t) == 'string' ? $.datepicker.parseDate('yy-mm-dd', t) : t; },
    _parseDateString: function (t) { return (t.getMonth() + 1) + '-' + t.getDate(); },
    _parseFullDateString: function (t) { return t.getFullYear() + '-' + (t.getMonth() + 1) + '-' + t.getDate(); },
    _getRowEl: function (p) {
        var k = '$' + p;
        if (!this.$el_row[k])
            this.$el_row[k] = this.$el_row.find('#' + p);
        return this.$el_row[k];
    },
    _getDetailEl: function (p) {
        var k = '$' + p;
        if (!this.$el_detail[k])
            this.$el_detail[k] = this.$el_detail.find('#' + p);
        return this.$el_detail[k];
    },
    _setClass: function ($e, b, c) { $e[b ? 'addClass' : 'removeClass'](c); },
    ///////////////////////////////////////////////////////////////////////////////
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
        //TODO:详情插件扩展在此追加
        return this.$el_detail;
    },
    //额外修正一些显示问题 由于未呈现(append)导致的UI问题
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
    ///////////////////////////////////////////////////////////////////////////////
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
        if (debuger.isDebugEnable)
            this._getRowEl('subject').attr('placeholder', '#' + this.id());
    },
    setIndex: function (i) { this._getRowEl('index').html(i); },
    setSubject: function (s, f) {
        var k = 'subject';
        this.update(k, s);
        //为双向同步而设置的f标识
        if (f == undefined || !f)
            this.setDetail_Subject(s);
        else if (f)
            this._getRowEl(k).val(s);
    },
    setBody: function (b) {
        this.update('body', b);
        this.setDetail_Body(b);
    },
    setCompleted: function (b) {
        var k = 'isCompleted';
        this.update(k, b);
        this._getRowEl(k).css('display', b ? 'block' : 'none');
        this._setClass(this.$el_row, b, 'row_completed');
        this.setDetail_Completed(b);
    },
    setPriority: function (p) {
        var k = 'priority';
        this.update(k, p.toString());
        //设置priority图标显示与否 避免出现inline
        this._getRowEl('priority').css('display', p == 0 ? 'block' : 'none');
        this.setDetail_Priority(p);
    },
    setDueTime: function (t) {
        var k = 'duetime';
        var $e = this._getRowEl(k);
        if (t == undefined || t == null) {
            this.update(k, null);
            $e.html('&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;');
            return;
        }
        this.update(k, t);
        var today = $.datepicker.parseDate('yy-mm-dd', this._parseFullDateString(new Date())).getTime();
        var date = $.datepicker.parseDate('yy-mm-dd', this._parseFullDateString(t)).getTime();
        //优化用户化文本显示
        $e.html(today == date ? lang.today : this._parseDateString(t));
        //过期标记
        this._setClass($e, today >= date, 'cell_duetime_expired');
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
        this._getDetailEl('subject').val(s);
    },
    setDetail_Body: function (b, f) {
        if (!this.$el_detail) return;
        var $el = this._getDetailEl('body');
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
        this._getDetailEl('priority')
            .find('button')
            .removeClass('active')
            .eq(parseInt(p))
            .addClass('active');
    },
    setDetail_Completed: function (b) {
        if (!this.$el_detail) return;
        this._setClass(this._getDetailEl('isCompleted'), b, 'active');
        this._setClass(this._getDetailEl('isCompleted'), b, 'btn-success');
    },
    setDetail_DueTime: function (t) {
        if (!this.$el_detail) return;
        if (t != null)
            this._getDetailEl('dueTime').val(this._parseFullDateString(t));
    }
}
/////////////////////////////////////////////////////////////////////////////////////////
//描述任务排序数据缓存
var Sort = function () { this._init.apply(this, arguments); }
Sort.prototype = {
    $el_region: null,
    _init: function () {
        this['by'] = arguments[0];
        this['key'] = arguments[1]; //0,1,2,prj1,team1
        this['name'] = arguments[2]; //今天、稍后、迟些、项目1、团队1
        this['idx'] = arguments[3]; //[0,1,2,4]
        this.$el_region = $(render($('#tmp_region').html(), this));
        this._clearRegion();
    },
    _getTask: null,
    _getRows: function () { return this.el().find('tbody').find('tr') },
    _append: function (e) { this.el().find('tbody').append(e); },
    _prepend: function (e) { this.el().find('tbody').prepend(e); },
    _clearRegion: function () { this.el().find('tbody').empty() },
    dispose: function () {
        if (!this.$el_region) return;
        this.$el_region.remove();
        this.$el_region = null;
    },
    el: function () { return this.$el_region; },
    indexs: function (idx) { if (idx) this['idx'] = idx; return this['idx']; },
    render: function () {
        this._clearRegion();
        var idx = this.indexs();
        for (var i = 0; i < idx.length; i++) {
            var id = idx[i].toString();
            var task = this._getTask(id);
            if (!task) {
                debuger.error('wrong id ' + id, idx);
                continue;
            }
            task.renderRow();
            task.setIndex(i + 1); //设置索引显示
            this._append(task.el());
        }
        //总数
        this.el().find('#region_total').html(idx.length);
    },
    //根据el刷新索引数据以及对应索引显示
    flush: function (b) {
        var base = this;
        var $els = this._getRows();
        var ary = new Array($els.length);
        $els.each(function (i, n) {
            var id = $(n).attr('id');
            ary[i] = id;
            base._getTask(id).setIndex(i + 1); //设置索引显示
            //依据idx数据额外修正task数据
            if (b)
                base._getTask(id).set(base['by'], base['key']);
        });
        this['idx'] = ary;
        this.el().find('#region_total').html(ary.length);
    },
    //实时刷新并获取索引
    getIndexs: function () {
        var $els = this._getRows();
        var ary = new Array($els.length);
        $els.each(function (i, n) {
            var id = $(n).attr('id');
            ary[i] = id;
        });
        this['idx'] = ary;
        return this['idx'];
    },
    append: function (t, b) { this._append(t.el()); this.flush(b); }, //不可this.flush(true)
    prepend: function (t, b) { this._prepend(t.el()); this.flush(b); }
}