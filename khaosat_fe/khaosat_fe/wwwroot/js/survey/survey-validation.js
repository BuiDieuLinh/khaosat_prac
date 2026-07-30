(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const { selector: S, message: MESSAGE } = Survey.Constants;

    function showError(message, toastId) {
        Survey.Utils.showToast(message, "error", toastId);
        return false;
    }

    Survey.Validation = Survey.Validation || {};

    Survey.Validation.validateSurvey = function (survey) {
        if (!survey.code || !survey.name) return showError(MESSAGE.missingSurveyInfo);
        if (!survey.startDate) return showError(MESSAGE.missingStartDate);
        if (survey.endDate && new Date(survey.endDate) < new Date(survey.startDate)) {
            return showError(MESSAGE.invalidDateRange);
        }
        if (!survey.elements || !survey.elements.length) return showError(MESSAGE.missingQuestion);
        return true;
    };

    Survey.Validation.validateResponse = function (surveyElements) {
        if (!$.fn.validate || !$(S.surveyForm).length) return true;

        const $form = $(S.surveyForm);
        let validator = $form.data("validator");
        if (!validator) {
            validator = $form.validate({
                ignore: [],
                errorClass: "text-danger small d-block mt-1",
                errorElement: "div",
                errorPlacement(error, element) {
                    error.insertAfter(element.closest(".dx-widget").length ? element.closest(".dx-widget") : element);
                },
                highlight(element) {
                    const $widget = $(`#${$(element).attr("name")}`);
                    ($widget.length ? $widget : $(element).closest(".dx-widget")).addClass("dx-invalid");
                    $(element).closest(".question-block").addClass("dx-invalid-block");
                },
                unhighlight(element) {
                    const $widget = $(`#${$(element).attr("name")}`);
                    ($widget.length ? $widget : $(element).closest(".dx-widget")).removeClass("dx-invalid");
                    $(element).closest(".question-block").removeClass("dx-invalid-block");
                }
            });

            (surveyElements || []).forEach(function (element) {
                const $input = $(`[name="element_${element.id}"]`);
                if (!$input.length) return;
                const rules = element.required ? { required: true } : {};
                if (element.dataType === "Number") Object.assign(rules, { number: true, min: 0 });
                $input.rules("add", Object.assign({}, rules, {
                    messages: { required: "Không được để trống", number: "Vui lòng nhập số hợp lệ", min: "Giá trị không được âm" }
                }));
            });
        }
        return $form.valid();
    };
})(window, jQuery);
