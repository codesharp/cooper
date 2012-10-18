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
        $('div#cover').fadeOut();
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
        //TODO:为支持陌生人访问，在没有在team列表中获取到team时，单独获取p.teamId信息

        //必须有一个team
        if (!$rootScope.team) {
            if ($scope.teams.length > 0)
                $location.path(urls.teamPath($scope.teams[0]));
            else
                $rootScope.$broadcast('no_team');
            return;
        }
        //member
        $rootScope.member = findBy($scope.team.members, 'id', p.memberId);
        debuger.debug('memberId=', p.memberId);
        debuger.debug('member', $scope.member);
        //currentMember
        $rootScope.currentMember = findBy($scope.team.members, 'accountId', account.id);
        debuger.debug('currentMember=', $rootScope.currentMember);

        //TODO:controller结构要再做梳理
        //project
        var projects = [$scope.team.projects.length];
        //HACK:转换为为UI抽象的Project
        for (var i = 0; i < $scope.team.projects.length; i++)
            projects[i] = new Project($scope.team.projects[i], $rootScope);
        $scope.team.projects = projects;
        $rootScope.project = findBy($scope.team.projects, 'id', p.projectId);
        //可编辑性
        if ($rootScope.project)
            $rootScope.project.editable = $rootScope.currentMember != null;
        debuger.debug('projectId=', p.projectId);
        debuger.debug('project', $scope.project);
        //tag
        $rootScope.tag = p.tag;
        //html.title
        if ($rootScope.project)
            $rootScope.title = $rootScope.project.name;
        else if ($rootScope.member)
            $rootScope.title = $rootScope.member.name;
        else if($rootScope.tag)
            $rootScope.title = $rootScope.tag;
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
    $scope.hideAddTeam = function () { $element.find('input').val(''); $element.find('div.modal').modal('hide'); }
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.$on('no_team', function () { $scope.showAddTeam(); });
}
function TeamAddFormCtrl($scope, $element, $http, $location, urls, account) {
    var $form = $element;
    //默认使用当前用户的账号信息填充member
    $scope.memberName = account.name;
    $scope.memberEmail = account.email;
    $scope.errorClass = function (b) { return $scope.teamAddForm.$dirty && b ? 'error' : ''; }
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
                $location.path(urls.teamPath(t));
            });
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////
//team detail
function TeamDetailCtrl($scope, $http, $element, $location, urls, lang, account, ie7) {
    $scope.ie7 = ie7;
    $scope.initTab = function () {
        if ($scope.member)
            $scope.tab = 'm'
        else if ($scope.tag)
            $scope.tab = 't';
        else
            $scope.tab = 'p';
    }
    $scope.$on('ready_team', $scope.initTab);
    $scope.activeClass = function (b) { return b ? 'active' : ''; }
    $scope.memberUrl = function (m) { return m.accountId == account.id ? urls.team($scope.team) : urls.member($scope.team, m); }
    $scope.showModify = function () { $scope.tab2 = 's'; $element.find('div.modal').modal('show'); }
    $scope.showMembers = function () { $scope.tab2 = 'm'; $element.find('div.modal').modal('show'); }
    $scope.canRemove = function (m) { return m.accountId != account.id; }
    $scope.removeMember = function (m) {
        //不能删除当前用户所对应的member
        debuger.assert($scope.canRemove(m));

        if (!confirm(lang.confirm_delete_member)) return;

        $http.post('/team/DeleteMember', { teamId: $scope.team.id, memberId: m.id }).success(function () {
            $scope.team.members = $.grep($scope.team.members, function (n) { return n.id != m.id });
            //若删除的是当前member，跳转到team
            if ($scope.member && $scope.member.id == m.id)
                $location.path(urls.teamPath($scope.team));
        });
    }
    $scope.addProject = function (n) {
        $http.post('/team/CreateProject', { teamId: $scope.team.id, name: n }).success(function (data, status, headers, config) {
            debuger.debug(data);
            var p = { name: n };
            p.id = eval('(' + data + ')');
            $location.path(urls.projectPath($scope.team, p));
        });
    }
    //处理project名称变更
    //TODO:迁移到task sync之外的同步设计中
    //obsolete:已经增加project详情功能
    var timer;
    $element.bind('keyup', function (e) {
        var $e = $(e.target);
        if (!$e.attr('contenteditable') || !$scope.project)
            return;
        if (!$scope.currentMember) {
            $e.html($scope.project.name);
            return;
        }
        var name = $e.text();
        if (name == '') {
            //$e.html($scope.project.name);
            return;
        }
        if (timer)
            clearTimeout(timer);
        timer = setTimeout(function () {
            $http.put('/team/UpdateProject', { teamId: $scope.team.id, projectId: $scope.project.id, name: name }).success(function (data) {
                debuger.debug('project update=' + data);
                $scope.project.name = name;
            });
        }, 500);
    });
    //任务搜索
    $element.find('input#txtSearch').typeahead({
        source: [''],
        matcher: function (item) {
            var base = this;

            //HACK:How to cancel $http promise?
            //https://github.com/angular/angular.js/issues/1159
            $http.post('/team/search', { teamId: $scope.team.id, key: this.query }).success(function (data, status, headers, config) {
                for (var i = 0; i < data.length; i++)
                    data[i] = $.toJSON(data[i]);
                base.render(data);
            });
            
            /*if (this['timer'])
                clearTimeout(this['timer']);
            this['timer'] = setTimeout(function () {
                //tag
                //member
                //project
                //by key task
                //single task combo top 5
                //base.render(['a', 'b', 'c', 'd']);
                base.render([
                    $.toJSON({ tag: base.query }),
                    $.toJSON({ id: 1, member: base.query }),
                    $.toJSON({ id: 1, project: base.query }),
                    $.toJSON(base.query),
                    $.toJSON({ id: 1, task: base.query, snapshot: base.query }),
                    $.toJSON({ id: 2, task: base.query, snapshot: base.query })
                ]);
            }, 200);*/
            return true;
        },
        highlighter: function (item) {
            if (item == '')
                return '<i class="icon-refresh"></i> loading...';
            item = $.parseJSON(item);
            if (item.tag)
                return '<i class="icon-tag"></i> ' + highlight(item.tag, this.query);
            else if (item.member)
                return '<i class="icon-user"></i> ' + highlight(item.member, this.query);
            else if (item.project)
                return '<i class="icon-flag"></i> ' + highlight(item.project, this.query);
            else if (item.task)
                return '<div style="border-top:1px solid #ccc;padding-top:5px;">\
                    \<i class="icon-inbox"></i> '
                   + highlight(item.task, this.query)
                   + '<br> ' + highlight(item.snapshot, this.query) + '</div>';
            else
                return '<i class="icon-search"></i> ' + highlight(item, this.query);
        },
        sorter: function (items) {
            return items;
        },
        updater: function (item) {
            debuger.debug(item);
            item = $.parseJSON(item);
            if (item.tag)
                $scope.$apply(function () { $location.path(urls.tagPath($scope.team, item.tag)); });
            else if (item.member)
                $scope.$apply(function () { $location.path(urls.memberPath($scope.team, item)); });
            else if (item.project)
                $scope.$apply(function () { $location.path(urls.projectPath($scope.team, item)); });
            else if (item.task)
                $scope.$apply(function () { $location.path(urls.taskPath($scope.team, item)); });
            else
                $scope.$apply(function () { $location.path(urls.keyPath($scope.team, item)); });

            return this.query;
        }
    });
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
    $scope.errorClass = function (b) { return $scope.teamMembersForm.$dirty && b ? 'error' : ''; }
    $scope.addMember = function () {
        var m = {};
        $.each($form.serializeArray(), function (i, n) { m[n.name] = n.value; });
        if ($scope.teamMembersForm.$valid) {
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
function TeamMemberProfileFormCtrl($scope, $element, $http) {
    var $form = $element;
    $scope.errorClass = function (b) { return $scope.teamMemberProfileForm.$dirty && b ? 'error' : ''; }
    $scope.updateMemberProfile = function () {
        if ($scope.teamMemberProfileForm.$valid) {
            if ($scope.currentMember.name == $scope.memberName) {
                success($form);
                return;
            }
            $http.put('/team/UpdateMember', {
                teamId: $scope.team.id,
                memberId: $scope.currentMember.id,
                name: $scope.memberName
            }).success(function (data, status, headers, config) {
                $scope.currentMember.name = $scope.memberName;
                success($form);
            }).error(function () { error($form); });
        }
    }
    $scope.reset = function () {
        if (!$scope.currentMember) return;
        $scope.memberName = $scope.currentMember.name;
        $scope.memberEmail = $scope.currentMember.email;
    }
    $scope.$on('ready_team', $scope.reset);
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
function highlight(item, k) {
    try{
        return item.replace(new RegExp('(' + k + ')', 'ig'), function ($1, match) {
            return '<strong>' + match + '</strong>'
        });
    } catch (e)
    {
        return item;
    }
}