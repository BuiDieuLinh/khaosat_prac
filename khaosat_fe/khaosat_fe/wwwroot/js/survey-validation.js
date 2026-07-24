// survey-validation.js
window.validateSurveyInputs = function(code, name, startDate, endDate, elements) {
    if (!code || !name) {
        window.showToast("Vui lòng điền Mã khảo sát và Tên khảo sát!", "error");
        return false;
    }

    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
        window.showToast("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!", "error");
        return false;
    }

    if (!elements || elements.length === 0) {
        window.showToast("Vui lòng thêm ít nhất một câu hỏi!", "error");
        return false;
    }

    return true;
};
