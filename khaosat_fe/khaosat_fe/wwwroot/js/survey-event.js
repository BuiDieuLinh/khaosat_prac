// survey-event.js
window.saveSurvey = function() {
    const code = $("#surveyCode").dxTextBox("instance").option("value");
    const name = $("#surveyName").dxTextBox("instance").option("value");
    const description = $("#surveyDescription").dxTextArea("instance").option("value");
    const startDate = $("#surveyStartDate").dxDateBox("instance").option("value");
    const endDate = $("#surveyEndDate").dxDateBox("instance").option("value");
    const status = $("#surveyStatus").dxSelectBox("instance").option("value");

    const elements = window.gatherSurveyData();
    if (elements === null) return;

    const formattedStartDate = window.toLocalISOString(startDate);
    const formattedEndDate = window.toLocalISOString(endDate);

    const isValid = window.validateSurveyInputs(code, name, formattedStartDate, formattedEndDate, elements);
    if (!isValid) return;

    const payload = {
        code: code,
        name: name,
        description: description,
        startDate: formattedStartDate,
        endDate: formattedEndDate,
        status: status,
        elements: elements
    };

    window.submitSurveyPayload(payload);
};

window.submitSurvey = function() {
    const employeeIdInput = $("#employeeId").val();
    if (!employeeIdInput) {
        window.showToast("Không tìm thấy thông tin nhân viên đăng nhập!", "error", "toastDetail");
        return;
    }

    const answers = window.gatherDetailAnswers(window.surveyElements);
    const payload = {
        surveyId: window.surveyId,
        employeeId: employeeIdInput,
        answers: answers
    };

    window.submitSurveyResponse(payload);
};
