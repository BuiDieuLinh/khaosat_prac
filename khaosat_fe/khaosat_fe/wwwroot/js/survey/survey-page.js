(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Common = window.Common = window.Common || {};
    const { selector: S } = Survey.Constants || {};
    let isSaving = false;

    function widgetValue(selector, widgetName) {
        const instance = $(selector)[widgetName]("instance");
        return instance ? instance.option("value") : null;
    }

    function collectSurvey() {
        const startDate = Common.Utils.toLocalISOString(widgetValue(S.surveyStartDate, "dxDateBox"));
        const endDate = Common.Utils.toLocalISOString(widgetValue(S.surveyEndDate, "dxDateBox"));
        return {
            code: String(widgetValue(S.surveyCode, "dxTextBox") || "").trim(),
            name: String(widgetValue(S.surveyName, "dxTextBox") || "").trim(),
            description: String(widgetValue(S.surveyDescription, "dxTextArea") || "").trim(),
            startDate,
            endDate,
            status: widgetValue(S.surveyStatus, "dxSelectBox"),
            elements: Survey.Element.gatherSurveyData()
        };
    }

    function saveSurvey() {
        if (isSaving) return;
        const survey = collectSurvey();
        if (!survey.elements || !Survey.Validation.validateSurvey(survey)) return;

        isSaving = true;
        Survey.Api.saveSurvey(survey).done(function () {
            const isEdit = Boolean((Survey.Urls || {}).saveSurvey);
            Common.Utils.showToast(isEdit ? "Cập nhật khảo sát thành công!" : "Tạo khảo sát thành công!", "success");
            window.setTimeout(function () { window.location.assign(Survey.Urls.index); }, 1500);
        }).fail(function (xhr) {
            Common.Utils.showToast(Survey.Api.getErrorMessage(xhr), "error");
        }).always(function () {
            isSaving = false;
        });
    }

    function getResponseValue(element) {
        const $element = $(`#element_${element.id}`);
        if (!$element.length) return null;
        if (element.hasOptions) {
            const instance = $element.data("dxList") || $element.data("dxRadioGroup");
            return instance ? instance.option(element.isMultiSelect ? "selectedItemKeys" : "value") : null;
        }
        const widgets = ["dxTextBox", "dxNumberBox", "dxDateBox", "dxTextArea"];
        for (const widget of widgets) {
            const instance = $element.data(widget);
            if (instance) return instance.option("value");
        }
        return null;
    }

    function collectAnswers(elements) {
        const answers = [];
        (elements || []).forEach(function (element) {
            const value = getResponseValue(element);
            const values = Array.isArray(value) ? value : [value];
            values.filter(value => value !== null && value !== undefined && value !== "").forEach(function (value) {
                const option = (element.options || []).find(option => String(option.value) === String(value));
                answers.push({ elementId: element.id, optionId: option ? option.id : null, value: value instanceof Date ? Common.Utils.toLocalISOString(value) : String(value) });
            });
        });
        return answers;
    }

    function submitResponse() {
        const employeeId = $(S.employeeId).val();
        if (!employeeId) return Common.Utils.showToast("Không tìm thấy thông tin nhân viên đăng nhập!", "error", "toastDetail");
        if (!Survey.Validation.validateResponse(Survey.surveyElements || [])) return;

        Survey.Api.submitResponse({ surveyId: Survey.surveyId, employeeId, answers: collectAnswers(Survey.surveyElements) })
            .done(function () {
                Common.Utils.showToast("Gửi khảo sát thành công!", "success", "toastDetail");
                window.setTimeout(function () { window.location.assign(Survey.Urls.index); }, 1500);
            })
            .fail(function (xhr) { Common.Utils.showToast(`Lỗi gửi khảo sát: ${Survey.Api.getErrorMessage(xhr)}`, "error", "toastDetail"); });
    }

    function loadPositions(departmentId, currentPosId) {
        const positionWidget = $(S.targetPosition).dxSelectBox("instance");
        if (!positionWidget) return;

        if (!departmentId) {
            positionWidget.option("dataSource", []);
            positionWidget.option("value", null);
            positionWidget.option("disabled", true);
            return;
        }

        const url = (Survey.Urls || {}).getPositions || "/Department/GetPositions";
        $.get(url, { departmentId: departmentId })
            .done(function (positions) {
                positionWidget.option("disabled", false);
                positionWidget.option("dataSource", positions || []);
                if (currentPosId) {
                    positionWidget.option("value", currentPosId);
                }
            })
            .fail(function () {
                positionWidget.option("dataSource", []);
                positionWidget.option("disabled", true);
            });
    }

    function initializeTargetPosition() {
        const department = $(S.targetDepartment).dxSelectBox("instance");
        const position = $(S.targetPosition).dxSelectBox("instance");
        if (!department || !position) return;

        department.on("valueChanged", function (event) {
            position.option("value", null);
            loadPositions(event.value, null);
        });

        const currentDeptId = department.option("value");
        if (currentDeptId) {
            const currentPosId = position.option("value");
            loadPositions(currentDeptId, currentPosId);
        }
    }

    Survey.Event = Survey.Event || {};
    Survey.Event.saveSurvey = saveSurvey;
    Survey.Event.submitSurvey = submitResponse;
    Survey.Event.showImportPopup = function () {
        const popup = $(S.importPopup).dxPopup("instance");
        if (popup) popup.show();
    };

    Survey.Page = {
        init() {
            Survey.Builder.init();
            initializeTargetPosition();
        }
    };

    $(function () { Survey.Page.init(); });
})(window, jQuery);
