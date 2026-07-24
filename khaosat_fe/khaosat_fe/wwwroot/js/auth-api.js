// auth-api.js
window.submitLogin = function(payload) {
    $.ajax({
        url: window.authUrls.login,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function(res) {
            if (res.success) {
                window.showToast("Đăng nhập thành công!", "success", "loginToast");
                setTimeout(function() {
                    window.location.href = window.authUrls.surveyIndex;
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
