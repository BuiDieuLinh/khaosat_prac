(function (window) {
    "use strict";

    const Employee = window.Employee = window.Employee || {};

    Employee.Constants = {
        selector: {
            employeePopup: "#employeePopup",
            employeeForm: "#employeeForm",
            employeeGrid: "#employeeGrid"
        },
        message: {
            serverError: "Lỗi máy chủ."
        }
    };
})(window);
