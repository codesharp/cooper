/// Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/// <reference path="../../Content/angular/angular-1.0.1.min.js" />
/// <reference path="../../Content/jquery/jquery-1.7.2.min.js" />
/// <reference path="../../Scripts/common.js" />
/// <reference path="../../Scripts/lang.js" />

var cooper = angular.module('cooper', []);

cooper.value('lang', lang);

cooper.value('tmp', {
    left: 'left.htm',
    team_list: 'team_list.htm',
    team_detail: 'team_detail.htm',
    task_list: 'task_list.htm',
    task_detail: 'task_detail.htm',
    task_templates: 'task_templates.htm'
});