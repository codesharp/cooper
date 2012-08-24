/// Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/// <reference path="../../Content/angular/angular-1.0.1.min.js" />
/// <reference path="../../Content/jquery/jquery-1.7.2.min.js" />
/// <reference path="../../Scripts/common.js" />
/// <reference path="../../Scripts/lang.js" />

"use strict"

function TeamIndexCtrl($scope, $rootScope, urls, lang) {
    //injections
    $rootScope.urls = urls;
    $rootScope.lang = lang;
    //基本参数
    $rootScope.teamMode = true;
    $rootScope.mini = false;
    $rootScope.full = false;
    setSize($scope);

    $scope.$on('$locationChangeStart', function (e, url) {
        debuger.debug('change to ' + url);
        url = url.toLowerCase();
        //临时处理部分需要直接跳转的地址
        if (url.indexOf('/account') >= 0)
            location.href = '/account';
        else if (url.indexOf('/per') >= 0)
            location.href = '/per';
    });
}
// 大小模式
function setSize($scope) {
    var b = $.browser.msie && $.browser.version.indexOf('7.') >= 0;
    if (!$scope.full && !$scope.mini) {
        $scope.class_tasklist = 'span6';
        $scope.class_taskdetail = b ? 'span3' : 'span4';
    }
    else if ($scope.full) {
        $scope.class_tasklist = 'span8';
        $scope.class_taskdetail = b ? 'span3' : 'span4';
    }
    else if ($scope.mini) {
        $scope.class_tasklist = '';
        $scope.class_taskdetail = 'hide';
    }
}
// *****************************************************
// Team/Personal模式Controller
// *****************************************************
function MainCtrl($scope, $rootScope, $http, $routeParams, $location, tmp, urls, lang, account) {
    debuger.debug('$routeParams', $routeParams);
    debuger.debug($routeParams.teamId);
    var p = $routeParams; 

    // *****************************************************
    // Personal
    // *****************************************************
    //TODO:获取taskfolder
    if (!$scope.teamMode) {
        return;
    }
    // *****************************************************
    // Team
    // *****************************************************
    $http.get('/team/getteams?_=' + new Date().getTime()).success(function (data, status, headers, config) {
        $('div.modal-backdrop').fadeOut(1000);
        debuger.assert(data);
        // *****************************************************
        // 设置rootScope 
        // *****************************************************
        //teams
        $rootScope.teams = data;
        debuger.debug('teams', $scope.teams);
        //team
        debuger.debug('current teamId=', p.teamId);
        $rootScope.team = findBy($scope.teams, 'id', p.teamId);
        debuger.debug('current team', $scope.team);
        //必须有一个team
        if (!$rootScope.team) {
            if ($scope.teams.length > 0)
                $location.path(urls.team($scope.teams[0]));
            return;
        }
        //project
        $rootScope.project = findBy($scope.team.projects, 'id', p.projectId);
        debuger.debug('projectId=', p.projectId);
        debuger.debug('project', $scope.project);
        //member
        $rootScope.member = findBy($scope.team.members, 'id', p.memberId);
        debuger.debug('memberId=', p.memberId);
        debuger.debug('member', $scope.member);
        //html.title
        if ($rootScope.project)
            $rootScope.title = $rootScope.project.name;
        else if ($rootScope.member)
            $rootScope.title = $rootScope.member.name;
        else if ($rootScope.team)
            $rootScope.title = $rootScope.team.name;
        $rootScope.title = $rootScope.title == $rootScope.team.name ? $rootScope.title : $rootScope.title + ' - ' + $rootScope.team.name;
        //data ready
        $rootScope.$broadcast('ready_team');
    });
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team list
function TeamListCtrl($scope, $rootScope, $http, $element) {
    $scope.showAddTeam = function () { $element.find('div.modal').modal('show'); }
    $scope.hideAddTeam = function () { $element.find('div.modal').modal('hide'); }
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.$on('ready_team', function () {
        if ($scope.teams.length == 0)
            $scope.showAddTeam();
    });
}
function TeamAddFormCtrl($scope, $element, $http, $location, urls) {
    var $form = $element;
    $scope.addTeam = function () {
        var t = {};
        $.each($form.serializeArray(), function (i, n) { t[n.name] = n.value; });
        if ($scope.teamAddForm.$valid) {
            $http.post('/team/CreateTeam', t).success(function (data, status, headers, config) {
                success($form);
                debuger.debug(data);
                t.id = eval('(' + data + ')');
                //$scope.teams = $.merge($scope.teams, [t]);
                $scope.hideAddTeam();
                $location.path(urls.team(t));
            });
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team detail
function TeamDetailCtrl($scope, $http, $element, $location, urls, account) {
    $scope.initTab = function () { $scope.tab = $scope.member ? 'm' : 'p'; }
    $scope.$on('ready_team', $scope.initTab);
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.memberUrl = function (m) { return m.accountId == account.id ? urls.team($scope.team) : urls.member($scope.team, m); }
    $scope.showModify = function () { $scope.tab2 = 's'; $element.find('div.modal').modal('show'); }
    $scope.showMembers = function () { $scope.tab2 = 'm'; $element.find('div.modal').modal('show'); }
    $scope.canRemove = function (m) { return m.accountId != account.id; }
    $scope.removeMember = function (m) {
        //不能删除当前用户所对应的member
        debuger.assert($scope.canRemove(m));
        $http.post('/team/DeleteMember', { teamId: $scope.team.id, memberId: m.id }).success(function () {
            $scope.team.members = $.grep($scope.team.members, function (n) { return n.id != m.id });
            //若删除的是当前member，跳转到team
            if ($scope.member && $scope.member.id == m.id)
                $location.path(urls.team($scope.team));
        });
    }
    $scope.addProject = function (n) {
        $http.post('/team/CreateProject', { teamId: $scope.team.id, name: n }).success(function (data, status, headers, config) {
            debuger.debug(data);
            var p = { name: n };
            p.id = eval('(' + data + ')');
            $location.path(urls.project($scope.team, p));
        });
    }
}
function TeamSettingsFormCtrl($scope, $element, $http) {
    var $form = $element;
    var prev_name = $scope.team ? $scope.team.name : '';

    $scope.updateTeam = function () {
        $.each($form.serializeArray(), function (i, n) { $scope.team[n.name] = n.value; });
        if ($scope.teamSettingsForm.$valid) {
            $http.put('/team/UpdateTeam', $scope.team).success(function (data) {
                prev_name = $scope.team.name;
                success($form);
            });
        }
        else
            $scope.team.name = prev_name;
    }
}
function TeamMembersFormCtrl($scope, $element, $http) {
    var $form = $element;
    $scope.addMember = function () {
        var m = {};
        $.each($form.serializeArray(), function (i, n) { m[n.name] = n.value; });
        if ($scope.formTeamMembers.$valid) {
            //email/member不能重复
            $scope.duplicate = findBy($scope.team.members, 'email', m.email) ? true : false;
            if ($scope.duplicate) return;

            m.teamId = $scope.team.id;
            $http.post('/team/CreateMember', m).success(function (data, status, headers, config) {
                debuger.debug(data);
                m.id = eval('(' + data + ')');
                $scope.team.members = $.merge($scope.team.members, [m]);
                success($form);
            }).error(function () { error($form); });
        }
    }
}
function error($e) {
    $e.find('div.alert-success').hide();
    $e.find('div.alert-error:eq(0)').show().fadeOut(4000);
}
function success($e) {
    $e.find('div.alert-error').hide();
    $e.find('div.alert-success').show().fadeOut(4000);
}
function findBy(a, k, v) {
    for (var i = 0; i < a.length; i++)
        if (a[i][k] == v)
            return a[i];
    return null;
}