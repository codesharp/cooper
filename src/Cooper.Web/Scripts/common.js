//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

/////////////////////////////////////////////////////////////////////////////////////////
//debuger helper
var debuger = {};
debuger.console = window.console;
debuger.enable = typeof (console) != 'undefined' && console != null;
debuger.isProfileEnable = debuger.enable && false;
debuger.isDebugEnable = debuger.enable && true;
debuger.isInfoEnable = debuger.enable && true;
debuger.isWarnEnable = debuger.enable && true;
debuger.isErrorEnable = debuger.enable && true;
debuger.debug = function debug(m, o) { this.log(this.isDebugEnable, 'log', m, o); }
debuger.info = function info(m, o) { this.log(this.isInfoEnable, 'info', m, o); }
debuger.warn = function warn(m, o) { this.log(this.isWarnEnable, 'warn', m, o); }
debuger.error = function error(m, o) { this.log(this.isErrorEnable, 'error', m, o); }
debuger.log = function (b, f, m, o) { if (!b) return; this.console[f](m); if (o != undefined) this.console[f](o); }
debuger.assert = function (b) { if (this.enable) this.console.assert(b); }
debuger.profile = function (p) { if (this.isProfileEnable) this.console.profile(p); }
debuger.profileEnd = function (p) { if (this.isProfileEnable) this.console.profileEnd(p); }
/////////////////////////////////////////////////////////////////////////////////////////
//asserter
var asserter = { total: 0, fails: 0 };
asserter.clear = function () { this.total = this.fails = 0; }
asserter.isTrue = function (b) { this.total += 1; if (!b) this.fails += 1; };
asserter.result = function () { debuger[this.fails > 0 ? 'error' : 'warn']('asserter total: ' + this.total + ', fails: ' + this.fails + ''); }

/////////////////////////////////////////////////////////////////////////////////////////
//list obj,like array?
var List = function () { this._init.apply(this, arguments); }
List.prototype = {
    _init: function () {
        this.length = 0;
    },
    get: function (i) {
        if (i < 0 || i >= this.length) {
            debuger.error('index ' + i + ' out of range');
            return;
        }
        return this[i];
    },
    add: function (v) {
        this.length += 1;
        this.set(this.length - 1, v);
    },
    set: function (i, v) {
        if (i < 0 || i >= this.length) {
            debuger.error('index ' + i + ' out of range');
            return;
        }
        this[i.toString()] = typeof (v) == 'undefined' ? null : v;
    },
    removeAt: function (i) {
        if (typeof (this[i.toString()]) == 'undefined') return;
        this[i.toString()] = undefined;
        this.length -= 1;
    },
    insertBefore: function (i) { },
    insertAfter: function (i) { },
    insertAt: function (prev, next) { },
    indexOf: function (v) {
        if (v != undefined)
            for (var k in this)
                if (!isNaN(parseInt(k)) && this[k] == v)
                    return parseInt(k);
        return -1;
    }
};

/////////////////////////////////////////////////////////////////////////////////////////
//unittests
//(function () {
//    //List
//    var l = new List();
//    l.add(1);
//    asserter.isTrue(l.length == 1);
//    asserter.isTrue(l.get(0) == 1);
//    l.removeAt(0);
//    asserter.isTrue(l.length == 0);
//    asserter.result();

//})();

//TODO:引入一个js模板引擎
function render(t, d) {
    for (var k in d)
        t = t.replace(new RegExp('{' + k + '}', 'g'), d[k]);
    return t;
}