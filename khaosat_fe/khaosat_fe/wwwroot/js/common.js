// common.js
window.Survey = window.Survey || {};
window.Survey.Utils = {
    showToast: function(message, type, toastId) {
        const selector = toastId ? `#${toastId}` : "#toastCreator, #toastDetail, #loginToast";
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
    }
};
