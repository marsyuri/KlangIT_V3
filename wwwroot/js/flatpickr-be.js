// Flatpickr wrapper สำหรับปฏิทินไทย พ.ศ.
// - input ที่ user เห็น: รูปแบบ "dd MMMMyy" เป็น พ.ศ. (เช่น "23 เมษายน 2569")
// - hidden input ที่ submit: ISO Gregorian "yyyy-MM-dd" (เช่น "2026-04-23")
//
// Usage: <input type="text" class="flatpickr-be" name="BorrowDate" value="2026-04-23" />
(function () {
    if (typeof flatpickr === "undefined") return;

    const thMonths = [
        "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน",
        "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"
    ];
    const thMonthsShort = [
        "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.",
        "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค."
    ];
    const thWeekdays = ["อา.", "จ.", "อ.", "พ.", "พฤ.", "ศ.", "ส."];

    const buddhistLocale = {
        weekdays: { shorthand: thWeekdays, longhand: thWeekdays },
        months:   { shorthand: thMonthsShort, longhand: thMonths },
        firstDayOfWeek: 0,
        rangeSeparator: " ถึง ",
        ordinal: function () { return ""; }
    };

    document.querySelectorAll(".flatpickr-be").forEach(function (el) {
        flatpickr(el, {
            locale: buddhistLocale,
            dateFormat: "Y-m-d",       // hidden value (ISO Gregorian) — submit to server
            altInput: true,
            altFormat: "j F",          // display ไม่มี year — จะแทนปี พ.ศ. ผ่าน onReady/onMonthChange/onYearChange ด้านล่าง
            allowInput: false,
            formatDate: function (date, format) {
                if (format === "Y-m-d") {
                    // ส่ง ISO Gregorian (ค.ศ.) ไป server
                    const y = date.getFullYear();
                    const m = String(date.getMonth() + 1).padStart(2, "0");
                    const d = String(date.getDate()).padStart(2, "0");
                    return y + "-" + m + "-" + d;
                }
                if (format === "j F") {
                    // โชว์ใน input แสดงปี พ.ศ. (+543)
                    const beYear = date.getFullYear() + 543;
                    return date.getDate() + " " + thMonths[date.getMonth()] + " " + beYear;
                }
                return date.toString();
            },
            onReady: applyBeYearLabel,
            onMonthChange: applyBeYearLabel,
            onYearChange: applyBeYearLabel
        });
    });

    function applyBeYearLabel(selectedDates, dateStr, instance) {
        // โชว์ปี พ.ศ. แทน ค.ศ. ใน header — readonly เพื่อกัน user พิมพ์ ค.ศ. ลงไป
        // ปุ่มลูกศร ↑↓ ยังเลื่อนปีได้เพราะ flatpickr ใช้ instance.currentYear ไม่อ่านจาก input.value
        const yearInput = instance.currentYearElement;
        if (!yearInput) return;
        const ceYear = instance.currentYear;
        if (typeof ceYear !== "number" || isNaN(ceYear)) return;

        yearInput.readOnly = true;
        yearInput.value = (ceYear + 543).toString();
        yearInput.title = "ค.ศ. " + ceYear;

        // เพิ่ม prefix "พ.ศ." ก่อน wrapper ของ year input (ครั้งเดียวเท่านั้น)
        const wrapper = yearInput.closest(".numInputWrapper");
        if (wrapper && !wrapper.previousElementSibling?.classList?.contains("be-year-prefix")) {
            const prefix = document.createElement("span");
            prefix.className = "be-year-prefix";
            prefix.textContent = "พ.ศ.";
            prefix.style.marginRight = "0.25em";
            prefix.style.fontWeight = "500";
            wrapper.parentNode.insertBefore(prefix, wrapper);
        }
    }
})();
