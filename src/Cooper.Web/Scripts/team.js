//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />

//team相关模型，设计意图同task

var Project = function () { this._init.apply(this, arguments); }
Project.prototype = {
    $scope:null,//便于绑定处理
    $el_detail: null,
    _init: function () {
        var p = arguments[0];
        this.editable = p.editable;
        this.id = p.id;
        this.name = p.name;
        this.description = p.description;
        this.$scope = arguments[1];
    },
    _getDetailEl: function (p) {
        var k = '$' + p;
        if (!this.$el_detail[k])
            this.$el_detail[k] = this.$el_detail.find('#' + p);
        return this.$el_detail[k];
    },
    _addChange: function (k, c) {
        if (!this.editable)
            return;

        if (!this.changes)
            this.changes = {};

        c['ID'] = this.id;
        //只记录最后一次
        this.changes[k] = c;
        //统一增加时间戳
        this.changes[k]['CreateTime'] = new Date().toUTCString();

        debuger.info('new changelog for ' + k + ' of project#' + this.id, this.changes[k]);
    },
    _update: function (k, v) {
        if (this[k] == v)
            return false;
        this[k] = v;
        this._addChange(k, { 'Name': k, 'Value': v });
        return true;
    },
    renderDetail: function () {
        if (!this.$el_detail)
            this.$el_detail = $($('#tmp_detail_project').html());
        if (this.bind_detail)
            this.bind_detail(this.$el_detail, this);

        this.setDetail_Name(this.name);
        this.setDetail_Description(this.description);

        this.setDetail_Editable(this.editable);
        return this.$el_detail;
    },
    fixDetail: function () { },
    setName: function (v) {
        var base = this;
        this.$scope.$apply(function () { base._update('name', v); });
        this.setDetail_Name(v);
    },
    setDescription: function (v) {
        this._update('description', v);
        this.setDetail_Description(v);
    },
    setDetail_Name: function (v) {
        if (!this.$el_detail) return;
        this._getDetailEl('name').val(v);
    },
    setDetail_Description: function (v) {
        if (!this.$el_detail) return;
        this._getDetailEl('description').val(v);
    },
    setDetail_Editable: function (b) {
        if (!this.$el_detail) return;
        this._getDetailEl('name').attr('disabled', !this.editable);
        this._getDetailEl('description').attr('disabled', !this.editable);
    },
}