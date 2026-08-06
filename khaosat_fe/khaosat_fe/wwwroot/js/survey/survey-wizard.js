(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Common = window.Common = window.Common || {};

    let currentStep = 1;

    function showError(msg) {
        Common.Utils.showToast(msg, "error");
        return false;
    }

    function renderPreview() {
        const $container = $("#previewContent");
        if (!$container.length) return;

        const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;
        let targetText = "Toàn bộ công ty (Whole Company)";
        if (!isCompany) {
            const depWidget = $("#targetDepartments").dxTagBox("instance");
            const posWidget = $("#targetPositions").dxTagBox("instance");
            const depItems = depWidget ? depWidget.option("selectedItems") || [] : [];
            const posItems = posWidget ? posWidget.option("selectedItems") || [] : [];

            const depNames = depItems.map(i => i.DepartmentName || i.name).join(", ");
            const posNames = posItems.map(i => i.PositionName || i.name).join(", ");

            const parts = [];
            if (depNames) parts.push(`<strong>Phòng ban:</strong> ${depNames}`);
            if (posNames) parts.push(`<strong>Chức vụ:</strong> ${posNames}`);
            targetText = parts.join(" | ") || "Không giới hạn cụ thể";
        }

        const code = String($("#surveyCode").dxTextBox("instance") ? $("#surveyCode").dxTextBox("instance").option("value") || "" : "").trim();
        const name = String($("#surveyName").dxTextBox("instance") ? $("#surveyName").dxTextBox("instance").option("value") || "" : "").trim();
        const desc = String($("#surveyDescription").dxTextArea("instance") ? $("#surveyDescription").dxTextArea("instance").option("value") || "" : "").trim();
        const startDate = $("#surveyStartDate").dxDateBox("instance") ? $("#surveyStartDate").dxDateBox("instance").option("text") : "Chưa chọn";
        const endDate = $("#surveyEndDate").dxDateBox("instance") ? $("#surveyEndDate").dxDateBox("instance").option("text") : "Không giới hạn";
        const maxAttempts = $("#surveyMaxAttempts").dxNumberBox("instance") ? $("#surveyMaxAttempts").dxNumberBox("instance").option("value") : null;
        const statusVal = $("#surveyStatus").dxSelectBox("instance") ? $("#surveyStatus").dxSelectBox("instance").option("value") : 1;
        const statusText = statusVal === 1 ? '<span class="badge bg-success-subtle text-success border border-success-subtle px-3 py-1 rounded-pill">Đang mở (Published)</span>' : '<span class="badge bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-1 rounded-pill">Lưu nháp (Draft)</span>';

        const elements = Survey.Element.gatherSurveyData() || [];

        let questionsPreviewHtml = "";
        if (!elements.length) {
            questionsPreviewHtml = '<div class="alert alert-warning">Chưa có câu hỏi nào.</div>';
        } else {
            elements.forEach((el, index) => {
                let config = {};
                try {
                    config = typeof el.configType === "string" ? JSON.parse(el.configType) : el.configType;
                } catch (e) { }

                const caption = config.Caption || el.fieldName;
                const requiredStr = config.AllowNull === false ? '<span class="text-danger">*</span>' : '';
                const helperStr = config.Helper ? `<p class="text-muted small mb-2">${config.Helper}</p>` : '';
                const dataType = config.DataType || "TextBox";

                let inputHtml = "";
                if (dataType === "Radio" || (dataType === "Select" && !config.IsMultiSelect)) {
                    inputHtml = '<div class="d-flex flex-column gap-2">';
                    (el.options || []).forEach(opt => {
                        inputHtml += `<div class="form-check">
                            <input class="form-check-input" type="radio" disabled>
                            <label class="form-check-label text-dark">${opt.displayText || opt.value}</label>
                        </div>`;
                    });
                    inputHtml += '</div>';
                } else if (dataType === "Checkbox" || (dataType === "Select" && config.IsMultiSelect)) {
                    inputHtml = '<div class="d-flex flex-column gap-2">';
                    (el.options || []).forEach(opt => {
                        inputHtml += `<div class="form-check">
                            <input class="form-check-input" type="checkbox" disabled>
                            <label class="form-check-label text-dark">${opt.displayText || opt.value}</label>
                        </div>`;
                    });
                    inputHtml += '</div>';
                } else if (dataType === "TextArea") {
                    inputHtml = '<textarea class="form-control" rows="3" disabled placeholder="Nhập câu trả lời..."></textarea>';
                } else if (dataType === "Number") {
                    inputHtml = '<input type="number" class="form-control" style="max-width: 250px;" disabled placeholder="Nhập số...">';
                } else if (dataType === "Datetime" || dataType === "Date") {
                    inputHtml = '<input type="date" class="form-control" style="max-width: 250px;" disabled>';
                } else {
                    inputHtml = '<input type="text" class="form-control" disabled placeholder="Nhập câu trả lời...">';
                }

                questionsPreviewHtml += `
                    <div class="card mb-3 border-0 shadow-sm p-3 rounded-3" style="background: #ffffff;">
                        <div class="card-body">
                            <h6 class="fw-bold text-dark mb-1">Câu ${index + 1}: ${caption} ${requiredStr}</h6>
                            ${helperStr}
                            <div class="mt-3">${inputHtml}</div>
                        </div>
                    </div>
                `;
            });
        }

        const previewHtml = `
            <div class="step-header-card d-flex align-items-center gap-3 mb-4">
                <div class="step-header-icon-box">
                    <i class="dx-icon-eyeopen fs-4"></i>
                </div>
                <div>
                    <h5 class="fw-bold text-dark mb-1">Xem trước & Xác nhận thông tin</h5>
                    <p class="text-muted small mb-0">Duyệt lại toàn bộ thông tin khảo sát trước khi lưu và phát hành.</p>
                </div>
            </div>

            <div class="row g-4 mb-4">
                <div class="col-md-12">
                    <div class="card border-0 shadow-sm p-3 bg-light rounded-3">
                        <div class="card-body">
                            <h5 class="fw-bold text-dark mb-3"><i class="dx-icon-info text-primary me-2"></i>Tổng quan cuộc khảo sát</h5>
                            <div class="row g-3 text-secondary small">
                                <div class="col-md-3"><strong>Mã khảo sát:</strong> <span class="text-dark fw-bold">${code}</span></div>
                                <div class="col-md-6"><strong>Tên khảo sát:</strong> <span class="text-dark fw-bold">${name}</span></div>
                                <div class="col-md-3"><strong>Trạng thái:</strong> ${statusText}</div>
                                <div class="col-md-3"><strong>Thời gian bắt đầu:</strong> <span class="text-dark">${startDate}</span></div>
                                <div class="col-md-3"><strong>Thời gian kết thúc:</strong> <span class="text-dark">${endDate}</span></div>
                                <div class="col-md-3"><strong>Số lần làm tối đa:</strong> <span class="text-dark fw-bold">${maxAttempts ? maxAttempts + " lần" : "Không giới hạn (Unlimited)"}</span></div>
                                <div class="col-md-12"><strong>Đối tượng áp dụng:</strong> <span class="text-dark">${targetText}</span></div>
                                ${desc ? `<div class="col-md-12"><strong>Mô tả:</strong> <span class="text-dark">${desc}</span></div>` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="preview-questions-section">
                <h5 class="fw-bold text-dark mb-3"><i class="dx-icon-card text-primary me-2"></i>Xem trước giao diện điền khảo sát (${elements.length} câu hỏi)</h5>
                ${questionsPreviewHtml}
            </div>
        `;

        $container.html(previewHtml);
    }

    Survey.Wizard = {
        getCurrentStep: function () {
            return currentStep;
        },

        onWholeCompanyChanged: function (e) {
            const isChecked = e.value;
            const $dep = $("#targetDepartments");
            const $pos = $("#targetPositions");

            if ($dep.length && $dep.dxTagBox("instance")) {
                const depBox = $dep.dxTagBox("instance");
                depBox.option("disabled", isChecked);
                if (isChecked) depBox.option("value", []);
            }
            if ($pos.length && $pos.dxTagBox("instance")) {
                const posBox = $pos.dxTagBox("instance");
                posBox.option("disabled", isChecked);
                if (isChecked) posBox.option("value", []);
            }

            if (isChecked) {
                $("#targetSpecificGroup").slideUp(200);
            } else {
                $("#targetSpecificGroup").slideDown(200);
            }
        },

        validateStep: function (step) {
            if (step === 1) {
                const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;
                if (!isCompany) {
                    const deps = $("#targetDepartments").dxTagBox("instance") ? $("#targetDepartments").dxTagBox("instance").option("value") || [] : [];
                    const poss = $("#targetPositions").dxTagBox("instance") ? $("#targetPositions").dxTagBox("instance").option("value") || [] : [];
                    if (deps.length === 0 && poss.length === 0) {
                        return showError("Khi không chọn 'Toàn công ty', bạn phải chọn ít nhất một Phòng ban hoặc Chức vụ áp dụng!");
                    }
                }
                return true;
            }

            if (step === 2) {
                const code = String($("#surveyCode").dxTextBox("instance") ? $("#surveyCode").dxTextBox("instance").option("value") || "" : "").trim();
                const name = String($("#surveyName").dxTextBox("instance") ? $("#surveyName").dxTextBox("instance").option("value") || "" : "").trim();
                const startDate = $("#surveyStartDate").dxDateBox("instance") ? $("#surveyStartDate").dxDateBox("instance").option("value") : null;
                const endDate = $("#surveyEndDate").dxDateBox("instance") ? $("#surveyEndDate").dxDateBox("instance").option("value") : null;
                const maxAttempts = $("#surveyMaxAttempts").dxNumberBox("instance") ? $("#surveyMaxAttempts").dxNumberBox("instance").option("value") : null;

                if (!code || !name) return showError("Vui lòng điền đầy đủ Mã khảo sát và Tên khảo sát.");
                if (!startDate) return showError("Vui lòng chọn Ngày bắt đầu khảo sát.");
                if (endDate && new Date(endDate) < new Date(startDate)) {
                    return showError("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
                }
                if (maxAttempts !== null && maxAttempts !== undefined && maxAttempts <= 0) {
                    return showError("Số lần khảo sát tối đa phải lớn hơn 0 hoặc để trống (Unlimited).");
                }
                return true;
            }

            if (step === 3) {
                const elements = Survey.Element.gatherSurveyData();
                if (!elements || !elements.length) {
                    return showError("Vui lòng thêm ít nhất một câu hỏi khảo sát!");
                }
                return true;
            }

            return true;
        },

        goToStep: function (targetStep) {
            if (targetStep < 1 || targetStep > 4) return;
             
            if (targetStep > currentStep) {
                for (let s = currentStep; s < targetStep; s++) {
                    if (!Survey.Wizard.validateStep(s)) {
                        return;
                    }
                }
            }

            currentStep = targetStep;

            if (currentStep === 4) {
                renderPreview();
            }
             
            $(".wizard-content-step").hide();
            $(`#wizardStep${currentStep}`).fadeIn(200);
             
            for (let i = 1; i <= 4; i++) {
                const $ind = $(`#stepIndicator${i}`);
                $ind.removeClass("active completed");
                if (i === currentStep) {
                    $ind.addClass("active");
                } else if (i < currentStep) {
                    $ind.addClass("completed");
                }
            }
             
            if (currentStep === 1) {
                $("#btnWizardBack").hide();
                $("#btnWizardNext").show();
                $("#btnWizardSave").hide();
            } else if (currentStep === 2) {
                $("#btnWizardBack").show();
                $("#btnWizardNext").show();
                $("#btnWizardSave").hide();
            } else if (currentStep === 3) {
                $("#btnWizardBack").show();
                $("#btnWizardNext").show();
                $("#btnWizardSave").hide();
            } else if (currentStep === 4) {
                $("#btnWizardBack").show();
                $("#btnWizardNext").hide();
                $("#btnWizardSave").show();
            }
        },

        nextStep: function () {
            Survey.Wizard.goToStep(currentStep + 1);
        },

        prevStep: function () {
            Survey.Wizard.goToStep(currentStep - 1);
        },

        collectTargets: function () {
            const targets = [];
            const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;

            if (isCompany) {
                targets.push({ targetType: 1, targetId: null });
            } else {
                const deps = $("#targetDepartments").dxTagBox("instance") ? $("#targetDepartments").dxTagBox("instance").option("value") || [] : [];
                const poss = $("#targetPositions").dxTagBox("instance") ? $("#targetPositions").dxTagBox("instance").option("value") || [] : [];

                deps.forEach(id => targets.push({ targetType: 2, targetId: id }));
                poss.forEach(id => targets.push({ targetType: 3, targetId: id }));
            }

            return targets;
        }
    };

})(window, jQuery);
