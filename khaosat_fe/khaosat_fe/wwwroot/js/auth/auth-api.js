(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Auth = Survey.Auth = Survey.Auth || {};

    Auth.Api = {
        login(credentials) {
            const url = (Survey.Urls || {}).login;
            if (!url) {
                return $.Deferred().reject({ message: Auth.Constants.message.missingEndpoint }).promise();
            }

            return $.ajax({
                url,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({ Username: credentials.username, Password: credentials.password })
            });
        }
    };
})(window, jQuery);
