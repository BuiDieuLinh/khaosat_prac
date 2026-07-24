// auth-validation.js
window.validateLoginInputs = function(username, password) {
    if (!username || !password) {
        window.showToast("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "error", "loginToast");
        return false;
    }
    return true;
};
