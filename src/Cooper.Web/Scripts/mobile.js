//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

///<reference path="lang.js" />
///<reference path="common.js" />
$(function () {
    $.mobile.page.prototype.options.domCache = false;

    $.ajaxSetup({ cache: false });

    bind();
    $(document).bind('pageload', function () { bind(); });

    function bind() {
        $('.link_account').unbind().click(function () { location.href = url_account; });

        $('#list a').unbind().click(function () {
            var id = $(this).attr('id');
            $.mobile.changePage('MobileDetail?id=' + id, { transition: 'slide' });
        });

        $('.link_home').unbind().click(function () {
            $.mobile.changePage('Mobile', { transition: "slide", direction: 'reverse' });
        });

        $('.link_append').unbind().click(function () {
            $.mobile.changePage('MobileDetail?_' + new Date().getTime(), { transition: 'slide' });
        });
        $('.link_save').unbind().click(function () {
            var arr = $("#edit form").serializeArray();
            var data = {};
            $.each(arr, function (i, n) { data[n['name']] = n['value']; });
            $.post(url_m_detail, data, function () {
                $.mobile.changePage('Mobile', { transition: "slide", direction: 'reverse' });
            });
        });
        $('.link_cancel').unbind().click(function () { $.mobile.changePage('Mobile', { transition: "slide", direction: 'reverse' }); });
    }
});