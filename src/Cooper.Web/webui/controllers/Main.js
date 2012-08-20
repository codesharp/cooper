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
        if (url.indexOf('/per') >= 0)
            location.href = '/per';
        else if (url.indexOf('/account') >= 0)
            location.href = '/account';
    });
    $rootScope.tmp = tmp;
    $rootScope.urls = urls;
    $rootScope.lang = lang;
    $rootScope.user = { id: 1, name: 'Xu Huang', email: 'wskyhx@gmail.com' };

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

    //    var team1 = {
    //        id: 1,
    //        name: 'Code# Team',
    //        projects: [
    //            { id: 1, name: 'NSF' },
    //            { id: 2, name: 'NTFE' },
    //            { id: 3, name: 'CooperWeb' }
    //        ],
    //        members: [
    //            { id: 1, name: 'Xu Huang', email: 'wskyhx@gmail.com' },
    //            { id: 2, name: 'Sunleepy', email: 'sunleepy@gmail.com' },
    //            { id: 3, name: 'Xuhua Tang', email: 'txh@gmail.com' }
    //        ]
    //    },
    //        team2 = angular.copy(team1),
    //        team3 = angular.copy(team1);
    //    team2.id = 2;
    //    team2.name = 'Ali-ENT';
    //    team3.id = 3;
    //    team3.name = 'NetShare';
    //    $scope.teams = [team1, team2, team3];
    //    debuger.debug('teams', $scope.teams);

    debuger.debug('$routeParams', $routeParams);
    debuger.debug('params', params);
    var p = $routeParams.teamId ? $routeParams : params;

    $http.get('/team/getteams').success(function (data, status, headers, config) {
        debuger.assert(data);
        $scope.teams = data;
        debuger.debug('teams', $scope.teams);

        // *****************************************************
        // 设置rootScope 必须有一个team
        // *****************************************************
        debuger.debug('current teamId=', p.teamId);
        $rootScope.team = findBy($scope.teams, 'id', p.teamId);
        debuger.debug('current team', $scope.team);
        if (!$rootScope.team) {
            if ($scope.teams.length > 0)
                location.href = urls.team($scope.teams[0]);
            return;
        }
        debuger.debug('current projectId=', p.projectId);
        $rootScope.project = findBy($scope.team.projects, 'id', p.projectId);
        debuger.debug('current project', $scope.project);
        debuger.debug('current memberId=', p.memberId);
        $rootScope.member = findBy($scope.team.members, 'id', p.memberId);
        debuger.debug('current member', $scope.member);
        //htmltitle
        if ($rootScope.project)
            $rootScope.title = $rootScope.project.name;
        else if ($rootScope.member)
            $rootScope.title = $rootScope.member.name;
        else if ($rootScope.team)
            $rootScope.title = $rootScope.team.name;
        $rootScope.title = $rootScope.title == $rootScope.team.name ? $rootScope.title : $rootScope.title + ' - ' + $rootScope.team.name;
    });
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team list
function TeamListCtrl($scope, $http, $element) {
    $scope.showAddTeam = function () { $element.find('div.modal').modal('show'); }
    $scope.hideAddTeam = function () { $element.find('div.modal').modal('hide'); }
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.checkTeams = function () {
        if (!$scope.teams || $scope.teams && $scope.teams.length == 0)
            $scope.showAddTeam();
    }
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
                $scope.teams = $.merge($scope.teams, [t]);
                $scope.hideAddTeam();
                $location.path(urls.team(t));
            });
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team detail
function TeamDetailCtrl($scope, $http, $element, $location, urls) {
    $scope.initTab = function () { $scope.tab = $scope.member ? 'm' : 'p'; }
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.showModify = function () { $scope.tab2 = 's'; $element.find('div.modal').modal('show'); }
    $scope.showMembers = function () { $scope.tab2 = 'm'; $element.find('div.modal').modal('show'); }
    $scope.removeMember = function (id) {
        debuger.assert(id != $scope.user.id);
        $http.post('/team/DeleteMember', { teamId: $scope.team.id, memberId: id }).success(function () {
            $scope.team.members = $.grep($scope.team.members, function (n) { return n.id != id });
            //若删除的是当前member，跳转到team
            if ($scope.member && $scope.member.id == id)
                $location.path(urls.team($scope.team));
        });
    }
    $scope.addProject = function (n) {
        $http.post('/team/CreateProject', { teamId: $scope.team.id, name: n }).success(function (data, status, headers, config) {
            debuger.debug(data);
            var p = { name: n };
            p.id = eval('(' + data + ')');
            //$scope.team.projects = $.merge($scope.team.projects, [p]);
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