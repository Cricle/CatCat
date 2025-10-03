import {
  __publicField,
  nextTick
} from "./chunk-7NUVISC2.js";

// node_modules/@gtm-support/core/lib/index.js
var m = /^GTM-[0-9A-Z]+$/;
function s(r) {
  if (typeof r != "string" || !m.test(r)) {
    let t = String(r).toUpperCase().replace(/.*-|[^0-9A-Z]/g, ""), a = t.length === 0 ? "" : ` Did you mean 'GTM-${t}'?`;
    throw new Error(`'${r}' is not a valid GTM-ID (${m}).${a}`);
  }
}
function n(r, t = "dataLayer") {
  return r[t] || (r[t] = []), r[t];
}
function p(r, t) {
  let a = document, e = a.createElement("script"), o = (g) => {
    var _a;
    (_a = t.onReady) == null ? void 0 : _a.call(t, { id: r, script: e }), e.removeEventListener("load", o);
  };
  e.addEventListener("load", o);
  let i2 = t.dataLayerName ?? "dataLayer";
  if (n(window, i2).push({ event: "gtm.js", "gtm.start": (/* @__PURE__ */ new Date()).getTime() }), !r) return e;
  e.async = !t.defer, e.defer = !!(t.defer || t.compatibility), t.nonce && e.setAttribute("nonce", t.nonce), t.scriptType && (e.type = t.scriptType);
  let c2 = new URLSearchParams({ id: r, ...t.dataLayerName ? { l: t.dataLayerName } : {}, ...t.queryParams ?? {} }), d = t.source ?? "https://www.googletagmanager.com/gtm.js";
  e.src = `${d}?${c2}`;
  let l = t.parentElement ?? a.body;
  if (typeof (l == null ? void 0 : l.appendChild) != "function") throw new Error("parentElement must be a DOM element");
  return l.appendChild(e), e;
}
function y(r = "https://www.googletagmanager.com/gtm.js") {
  return Array.from(document.getElementsByTagName("script")).some((t) => t.src.includes(r));
}
var u = class {
  constructor(t) {
    __publicField(this, "id");
    __publicField(this, "options");
    __publicField(this, "scriptElements", []);
    __publicField(this, "isInBrowserContext", () => typeof window < "u");
    if (Array.isArray(t.id)) for (let a of t.id) typeof a == "string" ? s(a) : s(a.id);
    else s(t.id);
    this.id = t.id, this.options = { enabled: true, debug: false, loadScript: true, defer: false, compatibility: false, dataLayerName: "dataLayer", ...t }, delete this.options.id;
  }
  enabled() {
    return this.options.enabled ?? true;
  }
  enable(t = true, a) {
    if (this.options.enabled = t, this.isInBrowserContext() && t && !y(a) && this.options.loadScript) if (Array.isArray(this.id)) this.id.forEach((e) => {
      let o;
      typeof e == "string" ? o = p(e, { ...this.options }) : o = p(e.id, { ...this.options, queryParams: e.queryParams }), this.scriptElements.push(o);
    });
    else {
      let e = p(this.id, { ...this.options });
      this.scriptElements.push(e);
    }
  }
  debugEnabled() {
    return this.options.debug ?? false;
  }
  debug(t) {
    this.options.debug = t;
  }
  dataLayer() {
    return this.isInBrowserContext() && this.options.enabled ? n(window, this.options.dataLayerName) : false;
  }
  trackView(t, a, e = {}) {
    let o = this.isInBrowserContext() && (this.options.enabled ?? false);
    this.options.debug && console.log(`[GTM-Support${o ? "" : "(disabled)"}]: Dispatching TrackView`, { screenName: t, path: a }), o && n(window, this.options.dataLayerName).push({ ...e, event: this.options.trackViewEventProperty ?? "content-view", "content-name": a, "content-view-name": t });
  }
  trackEvent({ event: t, category: a = null, action: e = null, label: o = null, value: i2 = null, noninteraction: b = false, ...c2 } = {}) {
    let d = this.isInBrowserContext() && (this.options.enabled ?? false);
    this.options.debug && console.log(`[GTM-Support${d ? "" : "(disabled)"}]: Dispatching event`, { event: t, category: a, action: e, label: o, value: i2, ...c2 }), d && n(window, this.options.dataLayerName).push({ event: t ?? "interaction", target: a, action: e, "target-properties": o, value: i2, "interaction-type": b, ...c2 });
  }
  push(t) {
    let a = this.isInBrowserContext() && (this.options.enabled ?? false);
    this.options.debug && console.log(`[GTM-Support${a ? "" : "(disabled)"}]: Dispatching event`, t), a && n(window, this.options.dataLayerName).push(t);
  }
};

// node_modules/@gtm-support/vue-gtm/dist/index.js
var i;
function c(a, t = { id: "" }) {
  t = { trackOnNextTick: false, ...t }, i = new u(t), a.config.globalProperties.$gtm = i, i.isInBrowserContext() && (t.vueRouter && G(a, t.vueRouter, t.ignoredViews, t.trackOnNextTick, t.vueRouterAdditionalEventData), i.options.enabled && i.options.loadScript && (Array.isArray(t.id) ? t.id.forEach((r) => {
    if (typeof r == "string") p(r, t);
    else {
      let n2 = { ...t };
      r.queryParams != null && (n2.queryParams = { ...n2.queryParams, ...r.queryParams }), p(r.id, n2);
    }
  }) : p(t.id, t))), a.provide("gtm", t);
}
function G(a, t, r = [], n2, f = () => ({})) {
  function l(e, u2) {
    return e instanceof Error ? !!(e.type & u2) : false;
  }
  t.afterEach(async (e, u2, p2) => {
    var _a, _b, _c;
    if (typeof e.name != "string" || Array.isArray(r) && r.includes(e.name) || typeof r == "function" && r(e, u2)) return;
    let s2 = e.meta && typeof e.meta.gtm == "string" && e.meta.gtm ? e.meta.gtm : e.name;
    l(p2, 4) ? (i == null ? void 0 : i.debugEnabled()) && console.log(`[VueGtm]: '${s2}' not tracked due to navigation aborted`) : l(p2, 8) && (i == null ? void 0 : i.debugEnabled()) && console.log(`[VueGtm]: '${s2}' not tracked due to navigation cancelled`);
    let d = { ...await f(e, u2), ...(_a = e.meta) == null ? void 0 : _a.gtmAdditionalEventData }, o = ((_c = (_b = t.options) == null ? void 0 : _b.history) == null ? void 0 : _c.base) ?? "";
    o.endsWith("/") || (o += "/"), o += e.fullPath.startsWith("/") ? e.fullPath.substring(1) : e.fullPath, n2 ? nextTick(() => {
      i == null ? void 0 : i.trackView(s2, o, d);
    }) : i == null ? void 0 : i.trackView(s2, o, d);
  });
}
function N(a) {
  return { install: (t) => c(t, a) };
}
var v = { install: c };
var V = v;
function E() {
  return i;
}
export {
  u as GtmPlugin,
  u as GtmSupport,
  s as assertIsGtmId,
  N as createGtm,
  V as default,
  y as hasScript,
  p as loadScript,
  E as useGtm
};
//# sourceMappingURL=@gtm-support_vue-gtm.js.map
