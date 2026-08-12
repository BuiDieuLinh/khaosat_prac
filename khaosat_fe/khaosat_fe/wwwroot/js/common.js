// common.js
window.Common = window.Common || {};
window.Common.Utils = {
    showToast: function(message, type) {
        const selector = "#globalToast";
        const toast = $(selector).first().dxToast("instance");
        if (toast) {
            toast.option({
                message: message,
                type: type
            });
            toast.show();
        } else {
            alert(message);
        }
    },

    toLocalISOString: function(dateVal) {
        if (!dateVal) return null;
        const dateObj = new Date(dateVal);
        if (isNaN(dateObj.getTime())) return null;
        const tzoffset = dateObj.getTimezoneOffset() * 60000;
        return (new Date(dateObj.getTime() - tzoffset)).toISOString().slice(0, -1);
    },

    clearError: function(elementId) {
        $(`#${elementId}`).removeClass("is-invalid");
        $(`#error_${elementId}`).text("").hide();
    },

    showInputError: function(elementId, message) {
        $(`#${elementId}`).addClass("is-invalid");
        $(`#error_${elementId}`).text(message).show();
    },

    focusInput: function(elementId) {
        $(`#${elementId}`).focus();
    },

    callApi: function (url, method, payload) {
        if (!url) {
            return $.Deferred().reject({ responseText: "Không tìm thấy địa chỉ API." }).promise();
        }

        const isFormData = payload instanceof FormData;
        const ajaxConfig = {
            url: url,
            method: method || "GET"
        };

        if (isFormData) {
            ajaxConfig.data = payload;
            ajaxConfig.processData = false;
            ajaxConfig.contentType = false;
        } else {
            ajaxConfig.contentType = "application/json";
            ajaxConfig.data = payload !== undefined && payload !== null ? JSON.stringify(payload) : null;
        }

        return $.ajax(ajaxConfig).fail(function(xhr, status, error) {
            if (xhr.status === 401) {
                location.href = "/401";
            }

            if (xhr.status === 403) {
                location.href = "/403";
            }
        });
    }
};

// Backward compatibility bridge
window.Survey = window.Survey || {};
window.Survey.Utils = window.Common.Utils;

