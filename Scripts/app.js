(function ($) {
    "use strict";

    function antiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    // Application root path (works whether the app is deployed at the site root
    // "/" or under a virtual directory like "/itehad/"). window.appRoot is set by
    // the layout via @Url.Content("~/"); fall back to "/" if it is missing.
    function appUrl(path) {
        var root = (window.appRoot || "/");
        if (root.charAt(root.length - 1) !== "/") { root += "/"; }
        return root + String(path).replace(/^\/+/, "");
    }

    // ------- Auto-show toast notifications -------
    $(function () {
        document.querySelectorAll("[data-auto-toast]").forEach(function (el) {
            bootstrap.Toast.getOrCreateInstance(el).show();
        });
    });

    // ------- Attendance: check-in / check-out time picker -------
    $(function () {
        var currentMode = null;
        var currentId = null;

        $(document).on("click", "[data-attendance-action]", function () {
            currentMode = $(this).data("attendance-action");
            currentId = $(this).data("attendance-id");

            var now = new Date();
            var hh = String(now.getHours()).padStart(2, "0");
            var mm = String(now.getMinutes()).padStart(2, "0");
            $("#attendanceTimeInput").val(hh + ":" + mm);
            $("#attendanceTimeModalTitle").text(currentMode === "checkin" ? "تسجيل دخول" : "تسجيل خروج");
        });

        $(document).on("click", "#attendanceTimeConfirm", function () {
            var time = $("#attendanceTimeInput").val();
            if (!time) return;

            if (currentMode === "checkin") {
                $("#checkInDriverId").val(currentId);
                $("#checkInTime").val(time);
                $("#checkInForm").trigger("submit");
            } else if (currentMode === "checkout") {
                $("#checkOutAttendanceId").val(currentId);
                $("#checkOutTime").val(time);
                $("#checkOutForm").trigger("submit");
            }
        });
    });

    // ------- Expense form: show driver/car field for fuel & maintenance categories -------
    $(function () {
        var $categorySelect = $("[data-toggle-driver-field]");
        var $driverWrap = $("[data-driver-field-wrap]");

        function toggleDriverField() {
            if (!$categorySelect.length) return;
            var selectedOption = $categorySelect.find("option:selected");
            var requiresDriver = selectedOption.data("requires-driver") == 1;
            $driverWrap.toggle(requiresDriver);
        }

        $categorySelect.on("change", toggleDriverField);
        toggleDriverField();
    });

    // ------- Sidebar (mobile) -------
    $(function () {
        $(document).on("click", "[data-sidebar-toggle]", function () {
            $(".sidebar").addClass("show");
            $(".sidebar-backdrop").addClass("show");
        });
        $(document).on("click", ".sidebar-backdrop", function () {
            $(".sidebar").removeClass("show");
            $(".sidebar-backdrop").removeClass("show");
        });
    });

    // ------- Trip form: show/hide days count -------
    $(function () {
        var $requestType = $("#RequestType");
        var $daysWrap = $("[data-days-count-wrap]");

        function toggleDaysCount() {
            if (!$requestType.length) return;
            var isMultiDay = $requestType.val() === "1";
            $daysWrap.toggle(isMultiDay);
        }

        $requestType.on("change", toggleDaysCount);
        toggleDaysCount();
    });

    // ------- Quick add: customer -------
    $(function () {
        $(document).on("click", "[data-quick-add-customer-submit]", function () {
            var $btn = $(this);
            var name = $("#quickCustomerName").val();
            var phone = $("#quickCustomerPhone").val();

            if (!name || !name.trim()) {
                $("#quickCustomerName").addClass("is-invalid");
                return;
            }

            $btn.prop("disabled", true);

            $.post(appUrl("Trips/CreateCustomerAjax"), {
                name: name,
                phone: phone,
                __RequestVerificationToken: antiForgeryToken()
            }).done(function (res) {
                if (res.success) {
                    var $select = $("#CustomerId");
                    var option = new Option(res.name, res.id, true, true);
                    $select.append(option).trigger("change");
                    $("#quickCustomerName").val("").removeClass("is-invalid");
                    $("#quickCustomerPhone").val("");
                    bootstrap.Modal.getOrCreateInstance(document.getElementById("quickAddCustomerModal")).hide();
                } else {
                    alert(res.message || "تعذّرت الإضافة");
                }
            }).fail(function () {
                alert("حدث خطأ أثناء إضافة الزبون");
            }).always(function () {
                $btn.prop("disabled", false);
            });
        });
    });

    // ------- Quick add: location (updates both from/to selects) -------
    $(function () {
        var targetSelectId = "FromLocationId";

        $(document).on("click", "[data-quick-add-location-open]", function () {
            targetSelectId = $(this).data("quick-add-location-open");
        });

        $(document).on("click", "[data-quick-add-location-submit]", function () {
            var $btn = $(this);
            var name = $("#quickLocationName").val();

            if (!name || !name.trim()) {
                $("#quickLocationName").addClass("is-invalid");
                return;
            }

            $btn.prop("disabled", true);

            $.post(appUrl("Trips/CreateLocationAjax"), {
                name: name,
                __RequestVerificationToken: antiForgeryToken()
            }).done(function (res) {
                if (res.success) {
                    ["FromLocationId", "ToLocationId"].forEach(function (id) {
                        var $select = $("#" + id);
                        if ($select.length) {
                            $select.append(new Option(res.name, res.id));
                        }
                    });
                    $("#" + targetSelectId).val(res.id).trigger("change");
                    $("#quickLocationName").val("").removeClass("is-invalid");
                    bootstrap.Modal.getOrCreateInstance(document.getElementById("quickAddLocationModal")).hide();
                } else {
                    alert(res.message || "تعذّرت الإضافة");
                }
            }).fail(function () {
                alert("حدث خطأ أثناء إضافة الموقع");
            }).always(function () {
                $btn.prop("disabled", false);
            });
        });
    });

    // ------- Quick add: expense category -------
    $(function () {
        $(document).on("click", "[data-quick-add-category-submit]", function () {
            var $btn = $(this);
            var name = $("#quickCategoryName").val();

            if (!name || !name.trim()) {
                $("#quickCategoryName").addClass("is-invalid");
                return;
            }

            $btn.prop("disabled", true);

            $.post(appUrl("Expenses/CreateCategoryAjax"), {
                name: name,
                __RequestVerificationToken: antiForgeryToken()
            }).done(function (res) {
                if (res.success) {
                    var $select = $("#CategoryId");
                    var option = new Option(res.name, res.id, true, true);
                    var requiresDriver = res.name === "سولار" || res.name === "صيانة";
                    $(option).attr("data-requires-driver", requiresDriver ? "1" : "0");
                    $select.append(option).trigger("change");
                    $("#quickCategoryName").val("").removeClass("is-invalid");
                    bootstrap.Modal.getOrCreateInstance(document.getElementById("quickAddCategoryModal")).hide();
                } else {
                    alert(res.message || "تعذّرت الإضافة");
                }
            }).fail(function () {
                alert("حدث خطأ أثناء إضافة التصنيف");
            }).always(function () {
                $btn.prop("disabled", false);
            });
        });
    });

    // ------- Searchable select (customer / locations) -------
    function enhanceSearchableSelect($select) {
        if ($select.data("enhanced")) return;
        $select.data("enhanced", true);

        var $wrap = $('<div class="searchable-select-box"></div>');
        var $input = $('<input type="text" class="form-control searchable-select-input" autocomplete="off" placeholder="اكتب للبحث...">');
        var $menu = $('<div class="searchable-select-menu"></div>');

        $select.before($wrap);
        $wrap.append($input).append($menu).append($select);
        $select.hide();

        function getOptions() {
            return $select.find("option").map(function () {
                return { value: $(this).val(), text: $(this).text() };
            }).get().filter(function (o) { return o.value !== ""; });
        }

        function syncInputFromSelect() {
            var selected = $select.find("option:selected").first();
            $input.val(selected.length && selected.val() !== "" ? selected.text() : "");
        }

        function renderMenu(filter) {
            var options = getOptions();
            var term = (filter || "").trim();
            var filtered = term
                ? options.filter(function (o) { return o.text.indexOf(term) !== -1; })
                : options;

            $menu.empty();
            if (filtered.length === 0) {
                $menu.append('<div class="empty">لا توجد نتائج</div>');
            } else {
                filtered.slice(0, 50).forEach(function (o) {
                    $('<div class="item"></div>').text(o.text).attr("data-value", o.value).appendTo($menu);
                });
            }
            $menu.addClass("show");
        }

        $input.on("focus", function () { renderMenu($input.val()); });
        $input.on("input", function () { renderMenu($input.val()); });

        $menu.on("click", ".item", function () {
            $select.val($(this).data("value")).trigger("change");
            $menu.removeClass("show");
        });

        $select.on("change", syncInputFromSelect);

        $(document).on("click", function (e) {
            if ($wrap.get(0) !== e.target && $wrap.has(e.target).length === 0) {
                $menu.removeClass("show");
            }
        });

        syncInputFromSelect();
    }

    $(function () {
        $(".js-searchable-select").each(function () {
            enhanceSearchableSelect($(this));
        });
    });

    // ------- Driver picker: فلترة + عدّاد المختارين -------
    // الفلترة بتقارن بدون حساسية لحالة الأحرف وبتشمل رقم السيارة كمان، لأن
    // أسماء السائقين مكتوبة إنجليزي بحالات مختلفة.
    function syncDriverPicker($picker) {
        if (!$picker.length) { return; }

        var term = $.trim(String($picker.find("[data-driver-filter]").val() || "")).toLowerCase();
        var shown = 0;
        $picker.find("[data-driver-row]").each(function () {
            var hay = String($(this).data("search") || $(this).text()).toLowerCase();
            var hit = !term || hay.indexOf(term) !== -1;
            $(this).toggle(hit);
            if (hit) { shown++; }
        });
        $picker.find("[data-driver-empty]").prop("hidden", shown > 0);

        var picked = $picker.find("input[name='DriverIds']:checked").length;
        $picker.find("[data-driver-count]").text(picked ? "مختار " + picked : "");
        $picker.find("[data-driver-clear]").prop("hidden", picked === 0);
    }

    $(function () {
        $(document).on("input", "[data-driver-filter]", function () {
            syncDriverPicker($(this).closest("[data-driver-picker]"));
        });

        $(document).on("change", "[data-driver-picker] input[name='DriverIds']", function () {
            syncDriverPicker($(this).closest("[data-driver-picker]"));
        });

        $(document).on("click", "[data-driver-clear]", function () {
            var $picker = $(this).closest("[data-driver-picker]");
            $picker.find("input[name='DriverIds']").prop("checked", false);
            syncDriverPicker($picker);
        });

        // Enter وما ضل ظاهر إلا سائق واحد = اختاره وفضّي الفلترة. بيوفّر
        // كبستين على كل رحلة لما يكونوا عم يدخّلوا رحلات ورا بعض.
        $(document).on("keydown", "[data-driver-filter]", function (e) {
            if (e.key !== "Enter") { return; }
            e.preventDefault();
            var $picker = $(this).closest("[data-driver-picker]");
            var $visible = $picker.find("[data-driver-row]:visible");
            if ($visible.length === 1) {
                var $box = $visible.find("input[name='DriverIds']");
                $box.prop("checked", !$box.prop("checked"));
                $(this).val("");
                syncDriverPicker($picker);
            }
        });

        $("[data-driver-picker]").each(function () { syncDriverPicker($(this)); });
    });

    // ------- Confirm before delete/settle actions -------
    $(function () {
        $(document).on("submit", "[data-confirm]", function (e) {
            var message = $(this).data("confirm");
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // ================= منتقي التاريخ: يوم/شهر/سنة دائمًا =================
    // كروم وإيدج بيعرضوا حقول date و datetime-local حسب لغة المتصفح، فبتطلع
    // شهر/يوم/سنة على الأجهزة المضبوطة إنجليزي أمريكي. الحل: نخفي الحقل الأصلي
    // — بيضل هو اللي بينبعث للسيرفر بصيغة ISO زي قبل بالضبط، فما في أي تغيير
    // على الباك-إند — ونحط مكانه حقل نصي بصيغة يوم/شهر/سنة مع تقويم من عندنا.

    // الشهور بالأرقام (1..12) مش بالأسماء — أسرع بالقراءة والاختيار.
    var DP_MONTHS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
    var DP_WEEKDAYS = ["أحد", "إثنين", "ثلاثاء", "أربعاء", "خميس", "جمعة", "سبت"];
    var DP_ICON = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" ' +
        'stroke-width="1.8" stroke-linecap="round"><rect x="3" y="5" width="18" height="16" rx="2"/>' +
        '<path d="M3 10h18M8 3v4M16 3v4"/></svg>';
    // أسهم SVG مش حروف: « و » بينعكسوا تلقائياً بسياق RTL فبيوقّع الاتجاه غلط.
    function dpChevron(dir) {
        return '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" ' +
            'stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="' +
            (dir > 0 ? "M15 5l-7 7 7 7" : "M9 5l7 7-7 7") + '"/></svg>';
    }

    function dpPad(n) { return (n < 10 ? "0" : "") + n; }

    function dpFromIso(v) {
        var m = /^(\d{4})-(\d{1,2})-(\d{1,2})(?:[T ](\d{1,2}):(\d{1,2}))?/.exec(v || "");
        return m ? new Date(+m[1], +m[2] - 1, +m[3], +(m[4] || 0), +(m[5] || 0)) : null;
    }
    function dpToIso(d, withTime) {
        var s = d.getFullYear() + "-" + dpPad(d.getMonth() + 1) + "-" + dpPad(d.getDate());
        return withTime ? s + "T" + dpPad(d.getHours()) + ":" + dpPad(d.getMinutes()) : s;
    }
    function dpToText(d, withTime) {
        var s = dpPad(d.getDate()) + "/" + dpPad(d.getMonth() + 1) + "/" + d.getFullYear();
        return withTime ? s + " " + dpPad(d.getHours()) + ":" + dpPad(d.getMinutes()) : s;
    }
    // بيقبل يوم/شهر/سنة بأي فاصل (/ أو - أو .) والوقت اختياري.
    function dpHasTime(s) { return /\d\s*:\s*\d/.test(s || ""); }
    function dpFromText(s) {
        var m = /^\s*(\d{1,2})\s*[\/\-.]\s*(\d{1,2})\s*[\/\-.]\s*(\d{2,4})(?:[\s,،]+(\d{1,2})\s*:\s*(\d{1,2}))?\s*$/.exec(s || "");
        if (!m) { return null; }
        var year = +m[3];
        if (year < 100) { year += 2000; }
        var d = new Date(year, +m[2] - 1, +m[1], +(m[4] || 0), +(m[5] || 0));
        if (d.getDate() !== +m[1] || d.getMonth() !== +m[2] - 1 || d.getFullYear() !== year) { return null; }
        return d;
    }

    var $dpPop = null, dpField = null, dpView = null;

    function dpSet(field, d) {
        field.orig.value = d ? dpToIso(d, field.withTime) : "";
        field.$text.val(d ? dpToText(d, field.withTime) : "");
        $(field.orig).trigger("change");
    }

    // النص المكتوب باليد: لو صالح بنعتمده، ولو لأ بنرجّع آخر قيمة صحيحة.
    function dpCommitText(field) {
        var raw = $.trim(field.$text.val());
        var prev = dpFromIso(field.orig.value);
        if (!raw) { dpSet(field, null); return; }
        var d = dpFromText(raw);
        if (!d) {
            field.$text.val(prev ? dpToText(prev, field.withTime) : "");
            return;
        }
        if (field.withTime && !dpHasTime(raw) && prev) {
            d.setHours(prev.getHours(), prev.getMinutes());
        }
        dpSet(field, d);
    }

    function dpBuild() {
        $dpPop = $(
            '<div class="dp-pop" hidden>' +
                '<div class="dp-bar">' +
                    '<button type="button" class="dp-nav" data-dp-step="-1" title="الشهر السابق">' + dpChevron(-1) + '</button>' +
                    '<select class="dp-sel dp-month" aria-label="الشهر"></select>' +
                    '<select class="dp-sel dp-year" aria-label="السنة"></select>' +
                    '<button type="button" class="dp-nav" data-dp-step="1" title="الشهر التالي">' + dpChevron(1) + '</button>' +
                '</div>' +
                '<table class="dp-grid"><thead><tr></tr></thead><tbody></tbody></table>' +
                '<div class="dp-time" hidden>' +
                    '<span>الوقت</span>' +
                    '<span class="dp-clock" dir="ltr">' +
                        '<input type="number" class="dp-hh" min="0" max="23" step="1" aria-label="الساعة">' +
                        '<b>:</b>' +
                        '<input type="number" class="dp-mm" min="0" max="59" step="1" aria-label="الدقيقة">' +
                    '</span>' +
                '</div>' +
                '<div class="dp-foot">' +
                    '<button type="button" class="dp-btn" data-dp-today>اليوم</button>' +
                    '<button type="button" class="dp-btn" data-dp-clear>مسح</button>' +
                    '<button type="button" class="dp-btn dp-btn-main" data-dp-done>تم</button>' +
                '</div>' +
            '</div>');

        var $head = $dpPop.find("thead tr");
        DP_WEEKDAYS.forEach(function (n) { $head.append($("<th>").text(n)); });
        var $mo = $dpPop.find(".dp-month");
        DP_MONTHS.forEach(function (n, i) { $mo.append($("<option>").val(i).text(n)); });

        $("body").append($dpPop);
        dpWire();
    }

    function dpRender() {
        if (!dpField) { return; }
        var sel = dpFromIso(dpField.orig.value);
        var y = dpView.getFullYear(), mo = dpView.getMonth();

        var $ys = $dpPop.find(".dp-year");
        var now = new Date().getFullYear();
        var lo = Math.min(now - 15, y), hi = Math.max(now + 5, y);
        if ($ys.data("lo") !== lo || $ys.data("hi") !== hi) {
            $ys.empty();
            for (var yy = hi; yy >= lo; yy--) { $ys.append($("<option>").val(yy).text(yy)); }
            $ys.data("lo", lo).data("hi", hi);
        }
        $ys.val(y);
        $dpPop.find(".dp-month").val(mo);

        var first = new Date(y, mo, 1);
        var start = new Date(y, mo, 1 - first.getDay());   // الأسبوع بيبدأ أحد
        var today = new Date(); today.setHours(0, 0, 0, 0);
        var $tb = $dpPop.find("tbody").empty();
        for (var w = 0; w < 6; w++) {
            var $tr = $("<tr>");
            for (var i = 0; i < 7; i++) {
                var cur = new Date(start.getFullYear(), start.getMonth(), start.getDate() + w * 7 + i);
                var $b = $("<button type='button'>").text(cur.getDate())
                    .attr("data-dp-pick", dpToIso(cur, false));
                if (cur.getMonth() !== mo) { $b.addClass("dp-out"); }
                if (cur.getTime() === today.getTime()) { $b.addClass("dp-today"); }
                if (sel && cur.getFullYear() === sel.getFullYear() &&
                    cur.getMonth() === sel.getMonth() && cur.getDate() === sel.getDate()) {
                    $b.addClass("dp-on");
                }
                $tr.append($("<td>").append($b));
            }
            $tb.append($tr);
        }

        $dpPop.find(".dp-time").prop("hidden", !dpField.withTime);
        if (dpField.withTime) {
            var t = sel || new Date();
            $dpPop.find(".dp-hh").val(dpPad(t.getHours()));
            $dpPop.find(".dp-mm").val(dpPad(t.getMinutes()));
        }
    }

    function dpPlace() {
        if (!dpField || $dpPop.prop("hidden")) { return; }
        var r = dpField.$text[0].getBoundingClientRect();
        var w = $dpPop.outerWidth(), h = $dpPop.outerHeight();
        var left = r.right - w;                       // محاذاة على اليمين لأن الواجهة RTL
        left = Math.max(8, Math.min(left, window.innerWidth - w - 8));
        var top = r.bottom + 6;
        if (top + h > window.innerHeight - 8) {
            top = r.top - h - 6 >= 8 ? r.top - h - 6 : Math.max(8, window.innerHeight - h - 8);
        }
        $dpPop.css({ top: top + "px", left: left + "px" });
    }

    function dpOpen(field) {
        if (field.orig.disabled || field.$text.prop("readonly")) { return; }
        if (!$dpPop) { dpBuild(); }
        dpField = field;
        var sel = dpFromIso(field.orig.value) || new Date();
        dpView = new Date(sel.getFullYear(), sel.getMonth(), 1);
        // جوّا مودال لازم البوب-أب يكون ابنه، وإلا Bootstrap بيسحب التركيز منه.
        var host = field.$text.closest(".modal-content")[0] || document.body;
        if ($dpPop[0].parentNode !== host) { host.appendChild($dpPop[0]); }
        dpRender();
        $dpPop.prop("hidden", false);
        dpPlace();
    }

    function dpClose() {
        if ($dpPop) { $dpPop.prop("hidden", true); }
        dpField = null;
    }

    // بياخد اليوم المضغوط ويلزقه مع الوقت المكتوب بخانات الساعة/الدقيقة.
    function dpPickedDate(iso) {
        var d = dpFromIso(iso);
        if (dpField.withTime) {
            var hh = parseInt($dpPop.find(".dp-hh").val(), 10);
            var mm = parseInt($dpPop.find(".dp-mm").val(), 10);
            d.setHours(isNaN(hh) ? 0 : Math.max(0, Math.min(23, hh)),
                       isNaN(mm) ? 0 : Math.max(0, Math.min(59, mm)));
        }
        return d;
    }

    function dpWire() {
        // منع فقدان التركيز لما نضغط جوّا البوب-أب.
        $dpPop.on("mousedown", function (e) {
            if (!$(e.target).is("input, select")) { e.preventDefault(); }
        });

        $dpPop.on("click", "[data-dp-step]", function () {
            dpView = new Date(dpView.getFullYear(), dpView.getMonth() + parseInt($(this).data("dp-step"), 10), 1);
            dpRender();
        });

        $dpPop.on("change", ".dp-month, .dp-year", function () {
            dpView = new Date(parseInt($dpPop.find(".dp-year").val(), 10),
                              parseInt($dpPop.find(".dp-month").val(), 10), 1);
            dpRender();
        });

        $dpPop.on("click", "[data-dp-pick]", function () {
            if (!dpField) { return; }
            var field = dpField;
            dpSet(field, dpPickedDate($(this).attr("data-dp-pick")));
            // مع الوقت بنضل فاتحين عشان يظبط الساعة؛ بدونه خلصنا.
            if (field.withTime) { dpView = dpFromIso(field.orig.value); dpRender(); }
            else { dpClose(); }
        });

        $dpPop.on("input", ".dp-hh, .dp-mm", function () {
            if (!dpField) { return; }
            var sel = dpFromIso(dpField.orig.value);
            if (sel) { dpSet(dpField, dpPickedDate(dpToIso(sel, false))); }
        });

        $dpPop.on("click", "[data-dp-today]", function () {
            if (!dpField) { return; }
            var field = dpField;
            var now = new Date();
            dpSet(field, field.withTime ? now : new Date(now.getFullYear(), now.getMonth(), now.getDate()));
            dpClose();
        });

        $dpPop.on("click", "[data-dp-clear]", function () {
            if (dpField) { dpSet(dpField, null); }
            dpClose();
        });

        $dpPop.on("click", "[data-dp-done]", dpClose);

        $(document).on("mousedown", function (e) {
            if (!dpField) { return; }
            if ($dpPop[0].contains(e.target) || dpField.$text[0] === e.target) { return; }
            dpClose();
        });
        $(document).on("keydown", function (e) { if (e.key === "Escape") { dpClose(); } });
        $(window).on("resize scroll", dpPlace);
        $(document).on("scroll", ".modal, .table-responsive", dpPlace);
    }

    function dpEnhance(el) {
        if (el.getAttribute("data-dp") === "on") { return; }
        el.setAttribute("data-dp", "on");

        var withTime = el.type === "datetime-local";
        var iso = el.value;

        var $text = $("<input>", {
            type: "text",
            "class": el.className,
            autocomplete: "off",
            inputmode: "numeric",
            placeholder: withTime ? "يوم/شهر/سنة  ساعة:دقيقة" : "يوم/شهر/سنة"
        });
        if (el.getAttribute("style")) { $text.attr("style", el.getAttribute("style")); }
        if (el.getAttribute("title")) { $text.attr("title", el.getAttribute("title")); }
        if (el.disabled) { $text.prop("disabled", true); }
        if (el.required) { $text.prop("required", true); }

        var $wrap = $('<div class="dp-wrap"></div>').insertBefore(el);
        $wrap.append($text).append('<span class="dp-icon" aria-hidden="true">' + DP_ICON + '</span>').append(el);

        el.type = "hidden";
        el.value = iso;                 // تغيير النوع ممكن يمسح القيمة، فبنرجّعها
        el.className = "dp-value";      // .form-control على حقل مخفي بتخليه يبان
        el.removeAttribute("style");

        var field = { orig: el, $text: $text, withTime: withTime };
        var d = dpFromIso(iso);
        if (d) { $text.val(dpToText(d, withTime)); }

        $text.on("focus click", function () { dpOpen(field); });
        $text.on("blur", function () { dpCommitText(field); });
        $text.on("keydown", function (e) {
            if (e.key === "Escape") { dpClose(); }
            else if (e.key === "Enter") { dpCommitText(field); dpClose(); }
        });
        // تحديث التقويم مباشرة وإحنا بنكتب، بس بدون ما نصلّح النص تحت إيد المستخدم.
        $text.on("input", function () {
            var typed = dpFromText($text.val());
            if (!typed) { return; }
            var prev = dpFromIso(el.value);
            if (withTime && !dpHasTime($text.val()) && prev) {
                typed.setHours(prev.getHours(), prev.getMinutes());
            }
            el.value = dpToIso(typed, withTime);
            if (dpField === field) {
                dpView = new Date(typed.getFullYear(), typed.getMonth(), 1);
                dpRender();
            }
        });
    }

    function dpScan(root) {
        $(root || document).find('input[type="date"], input[type="datetime-local"]').each(function () {
            dpEnhance(this);
        });
    }
    window.itehadDatePickers = dpScan;   // للمحتوى اللي بينزل بالأجاكس

    // حقول التاريخ صارت مخفية، لازم يضلوا داخل التحقق من جهة المتصفح.
    // بينفّذ قبل jquery.validate.unobtrusive لأن ترتيب الحِزم بالليّاوت بيحطنا آخر شي.
    if ($.validator) {
        $.validator.setDefaults({ ignore: ":hidden:not(.dp-value)" });
    }

    $(function () {
        dpScan(document);
        $(document).on("shown.bs.modal", ".modal", function () { dpScan(this); });
    });
})(jQuery);
