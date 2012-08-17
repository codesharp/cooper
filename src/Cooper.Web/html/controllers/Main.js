/// Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/// <reference path="../../Content/angular/angular-1.0.1.min.js" />
/// <reference path="../../Content/jquery/jquery-1.7.2.min.js" />
/// <reference path="../../Scripts/common.js" />
/// <reference path="../../Scripts/lang.js" />

function MainCtrl($scope, $http, tmp, lang) {
    $scope.lang = lang;
    $scope.tmp = tmp;
    $scope.url_team = function (t) { return '/t/' + t.id; }
    $scope.url_member = function (t, m) { return '/t/' + t.id + '/m/' + m.id; }
    $scope.url_project = function (t, p) { return '/t/' + t.id + '/p/' + p.id; }

    $scope.user = { id: 1, name: 'Xu Huang', email: 'wskyhx@gmail.com' };
    $scope.team = {
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
    };
    $scope.teams = [{ id: 1, name: 'Code# Team' }, { id: 2, name: 'Ali-ENT'}];

    $scope.project = $scope.team.projects[0];

    //大小模式
    $scope.mini = false;
    $scope.full = false;
    if (!$scope.full && !$scope.mini) {
        $scope.class_tasklist = 'span6';
        $scope.class_taskdetail = 'span4';
    }
    else if ($scope.full) {
        $scope.class_tasklist = 'span8';
        $scope.class_taskdetail = 'span4';
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
    $scope.showModify = function () {
        $element.find('div.modal').modal('show').find('a[href="#settings"]').click();
    }
    $scope.showMembers = function () {
        $element.find('div.modal').modal('show').find('a[href="#members"]').click();
    }
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
    var prev_name = $scope.team.name;

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