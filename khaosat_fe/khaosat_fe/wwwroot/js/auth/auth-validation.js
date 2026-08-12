(function (window) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Auth = Survey.Auth = Survey.Auth || {};

    Auth.Validation = {
        validateCredentials(credentials) {
            if (credentials.username && credentials.password) {
                return true;
            }

            Common.Utils.showToast(Auth.Constants.message.missingCredentials, "error");
            return false;
        }
    };
})(window);
