(function (window) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};

    Survey.Constants = {
        selector: {
            surveyCode: "#surveyCode",
            surveyName: "#surveyName",
            surveyDescription: "#surveyDescription",
            surveyStartDate: "#surveyStartDate",
            surveyEndDate: "#surveyEndDate",
            surveyStatus: "#surveyStatus",
            surveyForm: "#surveyForm",
            questionsContainer: "#questionsContainer",
            emptyState: "#emptyState",
            importPopup: "#importExcelPopup",
            importFile: "#importExcelFile",
            importMode: "#importMode",
            targetDepartment: "#surveyTargetDepartment",
            targetPosition: "#surveyTargetPosition",
            employeeId: "#employeeId"
        },
        dataType: {
            TEXT_BOX: "TextBox",
            TEXT_AREA: "TextArea",
            NUMBER: "Number",
            DATE_TIME: "Datetime",
            RADIO: "Radio",
            CHECKBOX: "Checkbox",
            SELECT: "Select"
        },
        message: {
            serverError: "Lỗi máy chủ.",
            missingSurveyInfo: "Vui lòng điền Mã khảo sát và Tên khảo sát.",
            missingStartDate: "Vui lòng nhập ngày bắt đầu.",
            invalidDateRange: "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
            missingQuestion: "Vui lòng thêm ít nhất một câu hỏi!"
        }
    };
})(window);
