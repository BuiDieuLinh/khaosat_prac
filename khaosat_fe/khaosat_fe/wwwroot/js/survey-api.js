// survey-api.js
window.submitSurveyPayload = function(payload) {
    $.ajax({
        url: window.surveyUrls.createNested,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function(res) {
            window.showToast("Tạo khảo sát thành công!", "success");
            setTimeout(function() {
                window.location.href = window.surveyUrls.index;
            }, 1500);
        },
        error: function(err) {
            window.showToast(err.responseText.errors || "Lỗi máy chủ", "error");
        }
    });
};

window.submitSurveyResponse = function(payload) {
    $.ajax({
        url: window.surveyUrls.submit,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function(response) {
            window.showToast("Gửi khảo sát thành công!", "success", "toastDetail");
            setTimeout(function() {
                window.location.href = window.surveyUrls.index;
            }, 1500);
        },
        error: function(err) {
            // Parse error message if JSON, else use text
            let cleanMsg = "Lỗi máy chủ";
            try {
                const errObj = JSON.parse(err.responseText);
                if (errObj && errObj.message) {
                    cleanMsg = errObj.message;
                }
            } catch (e) {
                if (err.responseText) {
                    cleanMsg = err.responseText;
                }
            }
            window.showToast("Lỗi gửi khảo sát: " + cleanMsg, "error", "toastDetail");
        }
    });
};
