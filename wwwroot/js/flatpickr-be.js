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
        // เปลี่ยน label "ปี" ที่ flatpickr render เป็น พ.ศ.
        const yearInput = instance.currentYearElement;
        if (!yearInput) return;
        const ceYear = parseInt(yearInput.value, 10);
        if (isNaN(ceYear)) return;
        // ใช้ wrapper text node แสดง พ.ศ. ทับ
        let beLabel = instance.calendarContainer.querySelector(".be-year-label");
        if (!beLabel) {
            beLabel = document.createElement("span");
            beLabel.className = "be-year-label";
            beLabel.style.marginLeft = "0.4em";
            beLabel.style.color = "#666";
            beLabel.style.fontSize = "0.9em";
            yearInput.parentNode.appendChild(beLabel);
        }
        beLabel.textContent = "(พ.ศ. " + (ceYear + 543) + ")";
    }
})();
