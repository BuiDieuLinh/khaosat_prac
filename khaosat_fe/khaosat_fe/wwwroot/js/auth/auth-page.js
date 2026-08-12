(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Auth = Survey.Auth = Survey.Auth || {};
    const SELECTOR = Auth.Constants.selector;
    const MESSAGE = Auth.Constants.message;

    Auth.Page = {
        isSubmitting: false,

        init() {
            this.cacheDom();
            if (!this.$username.length) return;
            this.initializeWidgets();
        },

        cacheDom() {
            this.$username = $(SELECTOR.username);
            this.$password = $(SELECTOR.password);
            this.$loginButton = $(SELECTOR.loginButton);
            this.$errorMessage = $(SELECTOR.errorMessage);
        },

        initializeWidgets() {
            this.$username.dxTextBox({
                placeholder: "Nhập mã nhân viên hoặc email...",
                height: 45,
                onEnterKey: () => this.submit()
            });
            this.$password.dxTextBox({
                placeholder: "Nhập mật khẩu...",
                mode: "password",
                height: 45,
                onEnterKey: () => this.submit()
            });
            this.$loginButton.dxButton({
                text: "Đăng nhập",
                elementAttr: { class: "login-btn w-100" },
                height: 45,
                onClick: () => this.submit()
            });
        },

        collectCredentials() {
            return {
                username: String(this.$username.dxTextBox("instance").option("value") || "").trim(),
                password: String(this.$password.dxTextBox("instance").option("value") || "")
            };
        },

        setSubmitting(isSubmitting) {
            this.isSubmitting = isSubmitting;
            const button = this.$loginButton.dxButton("instance");
            if (button) button.option("disabled", isSubmitting);
        },

        showError(message) {
            this.$errorMessage.text(message || MESSAGE.systemError).stop(true, true).fadeIn();
        },

        submit() {
            if (this.isSubmitting) return;
            const credentials = this.collectCredentials();
            if (!Auth.Validation.validateCredentials(credentials)) return;

            this.$errorMessage.hide();
            this.setSubmitting(true);
            Auth.Api.login(credentials).done(response => {
                if (!response || !response.success) {
                    this.showError((response && response.message) || MESSAGE.loginFailed);
                    return;
                }

                Common.Utils.showToast("Đăng nhập thành công!", "success");
                window.setTimeout(() => window.location.assign(Survey.Urls.surveyIndex), 1000);
            }).fail(xhr => {
                this.showError((xhr && xhr.message) || MESSAGE.systemError);
            }).always(() => this.setSubmitting(false));
        }
    };

    // Compatibility bridge for an existing DevExtreme OnClick expression, if one is added later.
    Auth.handleLogin = function () { Auth.Page.submit(); };
    $(function () { Auth.Page.init(); });
})(window, jQuery);
