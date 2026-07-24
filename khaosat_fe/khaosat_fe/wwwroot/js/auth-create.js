// auth-create.js
$(document).ready(function() {
    $("#usernameInput").dxTextBox({
        placeholder: "Nhập mã nhân viên hoặc email...",
        mode: "text",
        height: 45
    });

    $("#passwordInput").dxTextBox({
        placeholder: "Nhập mật khẩu...",
        mode: "password",
        height: 45
    });

    $("#loginBtn").dxButton({
        text: "Đăng Nhập",
        elementAttr: { class: "login-btn w-100" },
        height: 45,
        onClick: window.handleLogin
    });
});
