(function (window, $) {
    "use strict";

    const Employee = window.Employee = window.Employee || {};

    function getErrorMessage(xhr) {
        const responseText = xhr && xhr.responseText;
        if (!responseText) return "Lỗi máy chủ.";

        try {
            const response = JSON.parse(responseText);
            if (response.message) return response.message;
            if (response.errors) return Object.values(response.errors).flat().join(" ");
        } catch (_) {
            // A plain-text error is a valid API response.
        }
        return responseText;
    }

    Employee.Api = Employee.Api || {};
    Employee.Api.getErrorMessage = getErrorMessage;

    Employee.Api.saveEmployee = function (formData, url, method) {
        return Common.Utils.callApi(url, method || "POST", formData);
    };

    Employee.Api.deleteEmployee = function (payload) {
        const url = (window.Employee.Urls || {}).delete;
        return Common.Utils.callApi(url, "DELETE", payload);
    };
})(window, jQuery);
