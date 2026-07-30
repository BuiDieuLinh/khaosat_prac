(function (window) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    Survey.Auth = Survey.Auth || {};
    Survey.Auth.Constants = {
        selector: {
            username: "#usernameInput",
            password: "#passwordInput",
            loginButton: "#loginBtn",
            errorMessage: "#errorMessage"
        },
        message: {
            missingCredentials: "Vui lòng nhập đầy đủ tài khoản và mật khẩu.",
            loginFailed: "Đăng nhập không thành công.",
            systemError: "Lỗi hệ thống khi đăng nhập.",
            missingEndpoint: "Không tìm thấy địa chỉ đăng nhập."
        }
    };
})(window);
