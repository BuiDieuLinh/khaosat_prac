(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const MESSAGE = Survey.Constants.message;

    function getErrorMessage(xhr) {
        const responseText = xhr && xhr.responseText;
        if (!responseText) return MESSAGE.serverError;

        try {
            const response = JSON.parse(responseText);
            if (response.message) return response.message;
            if (response.errors) return Object.values(response.errors).flat().join(" ");
        } catch (_) {
            // A plain-text error is a valid API response.
        }
        return responseText;
    }

    function send(url, method, payload) {
        if (!url) {
            return $.Deferred().reject({ responseText: "Không tìm thấy địa chỉ API." }).promise();
        }

        return $.ajax({
            url: url,
            method: method,
            contentType: "application/json",
            data: JSON.stringify(payload)
        });
    }

    Survey.Api = Survey.Api || {};
    Survey.Api.getErrorMessage = getErrorMessage;
    Survey.Api.saveSurvey = function (payload) {
        const urls = Survey.Urls || {};
        return send(urls.saveSurvey || urls.createNested, urls.saveSurvey ? "PUT" : "POST", payload);
    };
    Survey.Api.submitResponse = function (payload) {
        return send((Survey.Urls || {}).submit, "POST", payload);
    };
})(window, jQuery);
