// auth.js
window.Survey = window.Survey || {};
window.Survey.Validation = window.Survey.Validation || {};
window.Survey.Auth = window.Survey.Auth || {};

// auth-validation.js
window.Survey.Validation.validateLogin = function(username, password) {
    if (!username || !password) {
        window.Survey.Utils.showToast("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "error", "loginToast");
        return false;
    }
    return true;
};

// auth-api.js
window.Survey.Auth.submitLogin = function(payload) {
    $.ajax({
        url: window.Survey.Urls.login,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function(res) {
            if (res.success) {
                window.Survey.Utils.showToast("Đăng nhập thành công!", "success", "loginToast");
                setTimeout(function() {
                    window.location.href = window.Survey.Urls.surveyIndex;
                }, 1000);
            } else {
                $("#errorMessage").text(res.message).fadeIn();
            }
        },
        error: function(err) {
            $("#errorMessage").text("Lỗi hệ thống khi đăng nhập.").fadeIn();
        }
    });
};

// auth-event.js
window.Survey.Auth.handleLogin = function() {
    const username = $("#usernameInput").dxTextBox("instance").option("value");
    const password = $("#passwordInput").dxTextBox("instance").option("value");

    const isValid = window.Survey.Validation.validateLogin(username, password);
    if (!isValid) return;

    $("#errorMessage").hide();

    const payload = {
        Username: username,
        Password: password
    };

    window.Survey.Auth.submitLogin(payload);
};

// auth-create.js
$(document).ready(function() {
    if ($("#usernameInput").length > 0) {
        $("#usernameInput").dxTextBox({
            placeholder: "Nhập mã nhân viên hoặc email...",
            mode: "text",
            height: 45
        });
    }

    if ($("#passwordInput").length > 0) {
        $("#passwordInput").dxTextBox({
            placeholder: "Nhập mật khẩu...",
            mode: "password",
            height: 45
        });
    }

    if ($("#loginBtn").length > 0) {
        $("#loginBtn").dxButton({
            text: "Đăng Nhập",
            elementAttr: { class: "login-btn w-100" },
            height: 45,
            onClick: window.Survey.Auth.handleLogin
        });
    }
});
