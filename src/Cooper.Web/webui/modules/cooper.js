/// Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/// <reference path="../../Content/angular/angular-1.0.1.min.js" />
/// <reference path="../../Content/jquery/jquery-1.7.2.min.js" />
/// <reference path="../../Scripts/common.js" />
/// <reference path="../../Scripts/lang.js" />
/// <reference path="../controllers/Main.js" />

"use strict"

var cooper = angular.module('cooper', []);

(function () {
    if (!window.url_root)
        url_root = '';
    if (!window.url_root_webui)
        url_root_webui = '/webui';

    cooper.value('lang', lang);

    cooper.value('tmp', {
        left: url_root_webui + '/left.htm',
        team_list: url_root_webui + '/team/list.htm',
        team_detail: url_root_webui + '/team/detail.htm',
        task_list: url_root_webui + '/task/list.htm',
        task_detail: url_root_webui + '/task/detail.htm',
        task_templates: url_root_webui + '/task/templates.htm'
    });

    var b = $.browser.msie && $.browser.version.indexOf('7.') >= 0;
    cooper.value('ie7',b);

    var prefix = false ? '/team#/t/' : '/t/';
    cooper.value('urls', {
        personal: url_root + '/per',
        account: url_root + '/account',
        team: function (t) { if (t) return url_root + prefix + t.id; },
        member: function (t, m) { if (t && m) return url_root + prefix + t.id + '/m/' + m.id; },
        project: function (t, p) { if (t && p) return url_root + prefix + t.id + '/p/' + p.id; }
    });

    //当前路由参数[obsolete]
    cooper.value('params', {
        taskFolderId: 0,
        teamId: currentTeamId,
        projectId: currentProjectId,
        memberId: currentMemberId
    });
    //当前用户
    cooper.value('account', currentAccount);

    cooper.config([
        '$routeProvider',
        '$locationProvider',
        function ($routeProvider, $locationProvider) {
            var url = url_root_webui + '/task/list.htm';
            $routeProvider.
            when('/t', {
                templateUrl: url,
                controller: MainCtrl
            }).
            when('/t/:teamId', {
                templateUrl: url,
                controller: MainCtrl
            }).
            when('/t/:teamId/p/:projectId', {
                templateUrl: url,
                controller: MainCtrl
            }).
            when('/t/:teamId/m/:memberId', {
                templateUrl: url,
                controller: MainCtrl
            }).
            otherwise({
                redirectTo: '/t/0'
            });
            $locationProvider.html5Mode(true);
        }
    ]);
})();