/// Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/// <reference path="../../Content/angular/angular-1.0.1.min.js" />
/// <reference path="../../Content/jquery/jquery-1.7.2.min.js" />
/// <reference path="../../Scripts/common.js" />
/// <reference path="../../Scripts/lang.js" />

'use strict';

function MainCtrl($scope, $rootScope, $http, $routeParams, tmp, urls, lang, params) {
    //临时处理部分需要直接跳转的地址
    $scope.$on('$locationChangeStart', function (e, url) {
        debuger.debug(url);
        if (url.indexOf('/per') >= 0) {
            e.preventDefault()
            location.href = '/per';
        }
        else if (url.indexOf('/account') >= 0)
            location.href = '/account';
    });
    $rootScope.tmp = tmp;
    $rootScope.urls = urls;
    $rootScope.lang = lang;
    $rootScope.user = { id: 1, name: 'Xu Huang', email: 'wskyhx@gmail.com' };

    var team1 = {
        id: 1,
        name: 'Code# Team',
        projects: [
            { id: 1, name: 'NSF' },
            { id: 2, name: 'NTFE' },
            { id: 3, name: 'CooperWeb' }
        ],
        members: [
            { id: 1, name: 'Xu Huang', email: 'wskyhx@gmail.com' },
            { id: 2, name: 'Sunleepy', email: 'sunleepy@gmail.com' },
            { id: 3, name: 'Xuhua Tang', email: 'txh@gmail.com' }
        ]
    },
        team2 = angular.copy(team1),
        team3 = angular.copy(team1);
    team2.id = 2;
    team2.name = 'Ali-ENT';
    team3.id = 3;
    team3.name = 'NetShare';
    $scope.teams = [team1, team2, team3];
    debuger.debug('teams', $scope.teams);

    debuger.debug('$routeParams', $routeParams);
    debuger.debug('params', params);
    var p = $routeParams.teamId ? $routeParams : params;

    debuger.debug('current teamId=', p.teamId);
    $rootScope.team = $scope.teams[parseInt(p.teamId) - 1];
    debuger.debug('current team', $scope.team);

    //当前必须有一个team
    debuger.assert($rootScope.team);
    if (!$rootScope.team) {
        if ($scope.teams.length > 0)
            location.href = urls.team($scope.teams[0]);
        return;
    }

    debuger.debug('current projectId=', p.projectId);
    $rootScope.project = isNaN(parseInt(p.projectId)) ? null : $scope.team.projects[parseInt(p.projectId) - 1];
    debuger.debug('current project', $scope.project);

    debuger.debug('current memberId=', p.memberId);
    $rootScope.member = isNaN(parseInt(p.memberId)) ? null : $scope.team.members[parseInt(p.memberId) - 1];
    debuger.debug('current member', $scope.member);

    //大小模式
    $scope.mini = false;
    $scope.full = false;
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

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team list
function TeamListCtrl($scope, $http, $element) {
    $scope.showAddTeam = function () { $element.find('div.modal').modal('show'); }
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.currentId = 0;
    if ($scope.teams.length == 0)
        $scope.showAddTeam();
}
function TeamAddFormCtrl($scope, $element) {
    var $form = $element;
    $scope.addTeam = function () {
        var t = {};
        $.each($form.serializeArray(), function (i, n) { t[n.name] = n.value; });
        if ($scope.teamAddForm.$valid) {
            $scope.teams = $.merge($scope.teams, [t]);
            success($form);
            //TODO:切换到新增的team
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team detail
function TeamDetailCtrl($scope, $http, $element) {
    if (!$scope.tab)
        $scope.tab = $scope.member ? 'm' : 'p';
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.showModify = function () { $scope.tab2 = 's'; $element.find('div.modal').modal('show'); }
    $scope.showMembers = function () { $scope.tab2 = 'm'; $element.find('div.modal').modal('show'); }
    $scope.removeMember = function (id) {
        debuger.assert(id != $scope.user.id);
        $scope.team.members = $.grep($scope.team.members, function (n) { return n.id != id });
    }
    $scope.addProject = function (n) {
        $scope.team.projects = $.merge($scope.team.projects, [{ id: 10, name: n}]);
        //TODO:切换到新建项目
    }
}
function TeamSettingsFormCtrl($scope, $element) {
    var $form = $element;
    var prev_name = $scope.team ? $scope.team.name : '';

    $scope.updateTeam = function () {
        $.each($form.serializeArray(), function (i, n) { $scope.team[n.name] = n.value; });
        if ($scope.teamSettingsForm.$valid) {
            prev_name = $scope.team.name;
            success($form);
        }
        else
            $scope.team.name = prev_name;
    }
}
function TeamMembersFormCtrl($scope, $element) {
    var $form = $element;
    $scope.addMember = function () {
        var m = {};
        $.each($form.serializeArray(), function (i, n) { m[n.name] = n.value; });
        if ($scope.formTeamMembers.$valid) {
            $scope.team.members = $.merge($scope.team.members, [m]);
            success($form);
        }
    }
}
function error($e) {
    $e.find('div.alert-success').hide();
    $e.find('div.alert-error').show().fadeOut(3000);
}
function success($e) {
    $e.find('div.alert-error').hide();
    $e.find('div.alert-success').show().fadeOut(3000);
}