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

    // ------- Driver checklist filter -------
    $(function () {
        $(document).on("input", "[data-driver-filter]", function () {
            var term = $(this).val().trim();
            $(this).closest("[data-driver-checklist]").find("[data-driver-row]").each(function () {
                var name = $(this).text();
                $(this).toggle(!term || name.indexOf(term) !== -1);
            });
        });
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
})(jQuery);
