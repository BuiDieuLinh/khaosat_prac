(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Common = window.Common = window.Common || {};
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
    Survey.Api = Survey.Api || {};
    Survey.Api.getErrorMessage = getErrorMessage;
    Survey.Api.saveSurvey = function (payload) {
        const urls = Survey.Urls || {};
        return Common.Utils.callApi(urls.saveSurvey || urls.createNested, urls.saveSurvey ? "PUT" : "POST", payload);
    };
    Survey.Api.submitResponse = function (payload) {
        return Common.Utils.callApi((Survey.Urls || {}).submit, "POST", payload);
    };
    Survey.Api.cloneSurvey = function (id) {
        const urls = Survey.Urls || {};
        const url = urls.clone ? urls.clone.replace("{id}", id) : `/Survey/Clone/${id}`;
        return Common.Utils.callApi(url, "POST");
    };
    Survey.Api.closeSurvey = function (id) {
        const urls = Survey.Urls || {};
        const url = urls.close ? urls.close.replace("{id}", id) : `/Survey/Close/${id}`;
        return Common.Utils.callApi(url, "PUT");
    };
})(window, jQuery);
