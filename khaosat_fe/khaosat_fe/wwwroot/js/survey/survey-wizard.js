(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Common = window.Common = window.Common || {};

    let currentStep = 1;
    let treeInitialized = false;

    function showError(msg) {
        Common.Utils.showToast(msg, "error");
        return false;
    }

    function isPublicAccess() {
        const accessTypeBox = $("#surveyAccessType").dxSelectBox("instance");
        const val = accessTypeBox ? accessTypeBox.option("value") : 1;
        return parseInt(val) === 2;
    }

    function updateStep2State(isPublic) {
        const $ind2 = $("#stepIndicator2");
        if (!$ind2.length) return;

        if (isPublic) {
            $ind2.addClass("step-disabled");
            $ind2.attr("title", "Khảo sát công khai không cần chọn đối tượng nhận");
            $ind2.find(".step-sub").text("Bỏ qua khi Public");
            $ind2.find(".step-badge").html('<i class="dx-icon-close" style="font-size: 13px;"></i>');
            if (currentStep === 2) {
                Survey.Wizard.goToStep(1);
            }
        } else {
            $ind2.removeClass("step-disabled");
            $ind2.removeAttr("title");
            $ind2.find(".step-sub").text("Phạm vi gửi bài");
            $ind2.find(".step-badge").text("2");
        }
    }

    function renderPreview() {
        const $container = $("#previewContent");
        if (!$container.length) return;

        const isPublic = isPublicAccess();
        let targetText = "Toàn bộ công ty (Whole Company)";

        if (isPublic) {
            targetText = '<span class="badge bg-info-subtle text-info border border-info-subtle px-2 py-1 rounded">Công khai (Public Link - Không giới hạn đối tượng nội bộ)</span>';
        } else {
            const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;
            if (!isCompany) {
                const treeWidget = $("#targetTree").dxTreeView("instance");
                if (treeWidget) {
                    const selectedNodes = treeWidget.getSelectedNodes() || [];
                    const deptNames = [];
                    const posNames = [];
                    const processedDeptIds = new Set();

                    selectedNodes.forEach(node => {
                        const isDeptNode = !node.parent || !node.parent.itemData || !node.parent.key;
                        if (isDeptNode && node.selected === true) {
                            const dText = Survey.Wizard ? Survey.Wizard.getTreeNodeDisplayExpr(node.itemData) : (node.itemData.departmentName || node.itemData.DepartmentName);
                            const dId = node.itemData.id || node.itemData.Id;
                            if (dText) deptNames.push(dText);
                            if (dId) processedDeptIds.add(dId);
                        }
                    });

                    selectedNodes.forEach(node => {
                        const isPosNode = node.parent && node.parent.itemData && node.parent.key;
                        if (isPosNode && node.selected === true) {
                            const deptId = node.parent.itemData.id || node.parent.itemData.Id;
                            if (!processedDeptIds.has(deptId)) {
                                const pText = Survey.Wizard ? Survey.Wizard.getTreeNodeDisplayExpr(node.itemData) : (node.itemData.positionName || node.itemData.PositionName);
                                const dName = node.parent.itemData.departmentName || node.parent.itemData.DepartmentName;
                                if (pText) {
                                    posNames.push(dName ? `${pText} (${dName})` : pText);
                                }
                            }
                        }
                    });

                    const parts = [];
                    if (deptNames.length) parts.push(`<strong>Phòng ban:</strong> ${deptNames.join(", ")}`);
                    if (posNames.length) parts.push(`<strong>Chức vụ:</strong> ${posNames.join(", ")}`);
                    targetText = parts.join(" | ") || "Chưa chọn đối tượng";
                }
            }
        }

        const code = String($("#surveyCode").dxTextBox("instance") ? $("#surveyCode").dxTextBox("instance").option("value") || "" : "").trim();
        const name = String($("#surveyName").dxTextBox("instance") ? $("#surveyName").dxTextBox("instance").option("value") || "" : "").trim();
        const desc = String($("#surveyDescription").dxTextArea("instance") ? $("#surveyDescription").dxTextArea("instance").option("value") || "" : "").trim();
        const startDate = $("#surveyStartDate").dxDateBox("instance") ? $("#surveyStartDate").dxDateBox("instance").option("text") : "Chưa chọn";
        const endDate = $("#surveyEndDate").dxDateBox("instance") ? $("#surveyEndDate").dxDateBox("instance").option("text") : "Không giới hạn";
        const maxAttempts = $("#surveyMaxAttempts").dxNumberBox("instance") ? $("#surveyMaxAttempts").dxNumberBox("instance").option("value") : null;
        const statusVal = $("#surveyStatus").dxSelectBox("instance") ? $("#surveyStatus").dxSelectBox("instance").option("value") : 1;
        const statusText = statusVal === 1 
            ? '<span class="badge bg-success-subtle text-success border border-success-subtle px-3 py-1 rounded-pill">Đang mở (Published)</span>' 
            : '<span class="badge bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-1 rounded-pill">Lưu nháp (Draft)</span>';
        const accessTypeText = isPublic 
            ? '<span class="badge bg-info-subtle text-info border border-info-subtle px-3 py-1 rounded-pill">Public (Công khai qua Link)</span>' 
            : '<span class="badge bg-primary-subtle text-primary border border-primary-subtle px-3 py-1 rounded-pill">Internal (Nội bộ hệ thống)</span>';

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
                    <div class="card border-0 shadow-sm p-3 bg-light rounded-3" style="background-color: #ffffff !important;">
                        <div class="card-body">
                            <h5 class="fw-bold text-dark mb-3">Tổng quan cuộc khảo sát</h5>
                            <div class="row g-3 text-secondary small">
                                <div class="col-md-3"><strong>Mã khảo sát:</strong> <span class="text-dark fw-bold">${code}</span></div>
                                <div class="col-md-6"><strong>Tên khảo sát:</strong> <span class="text-dark fw-bold">${name}</span></div>
                                <div class="col-md-3"><strong>Loại truy cập:</strong> ${accessTypeText}</div>
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

        isPublic: function () {
            return isPublicAccess();
        },

        onAccessTypeChanged: function (e) {
            const isPublic = parseInt(e.value) === 2;
            updateStep2State(isPublic);
        },

        onWholeCompanyChanged: function (e) {
            const isChecked = e.value;
            const treeWidget = $("#targetTree").dxTreeView("instance");

            if (treeWidget) {
                treeWidget.option("disabled", isChecked);

                if (isChecked) {
                    treeWidget.unselectAll();
                }
            }

            if (isChecked) {
                $("#targetSpecificGroup").slideUp(200);
            } else {
                $("#targetSpecificGroup").slideDown(200);
            }
        },

        getTreeNodeDisplayExpr: function (item) {
            if (!item) return "";
            const dName = item.departmentName || item.DepartmentName;
            if (dName) {
                const dCode = item.departmentCode || item.DepartmentCode;
                return dCode ? `${dName} (${dCode})` : dName;
            }
            const pName = item.positionName || item.PositionName;
            if (pName) {
                const pCode = item.positionCode || item.PositionCode;
                return pCode ? `${pName} (${pCode})` : pName;
            }
            return "";
        },

        onTreeContentReady: function (e) {
            const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;
            const tree = e.component;

            if (isCompany) {
                tree.option("disabled", true);
            } else {
                tree.option("disabled", false);
            }

            if (!treeInitialized && window.Survey.initialTargets && window.Survey.initialTargets.length > 0) {
                treeInitialized = true;
                if (!isCompany) {
                    tree.unselectAll();
                    window.Survey.initialTargets.forEach(t => {
                        const type = t.targetType !== undefined ? t.targetType : t.TargetType;
                        const deptId = t.departmentId || t.DepartmentId;
                        const posId = t.positionId || t.PositionId;

                        if (type === 2 && deptId) {
                            tree.selectItem(deptId);
                        } else if (type === 3 && posId) {
                            tree.selectItem(posId);
                        }
                    });
                }
            }
        },

        validateStep: function (step) {
            if (step === 1) {
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

            if (step === 2) {
                if (isPublicAccess()) {
                    return true;
                }
                const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;
                if (!isCompany) {
                    const treeWidget = $("#targetTree").dxTreeView("instance");
                    const selectedNodes = treeWidget ? treeWidget.getSelectedNodes() : [];
                    if (!selectedNodes || selectedNodes.length === 0) {
                        return showError("Khi không chọn 'Toàn công ty', bạn phải chọn ít nhất một Phòng ban hoặc Chức vụ áp dụng!");
                    }
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

            const isPub = isPublicAccess();

            if (targetStep === 2 && isPub) {
                if (currentStep === 1) {
                    targetStep = 3;
                } else if (currentStep >= 3) {
                    targetStep = 1;
                } else {
                    return;
                }
            }

            if (targetStep > currentStep) {
                for (let s = currentStep; s < targetStep; s++) {
                    if (s === 2 && isPub) continue;
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
                if (i === 2 && isPub) {
                    $ind.removeClass("active completed").addClass("step-disabled");
                } else {
                    $ind.removeClass("active completed");
                    if (i === currentStep) {
                        $ind.addClass("active");
                    } else if (i < currentStep) {
                        $ind.addClass("completed");
                    }
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
            const isPub = isPublicAccess();
            if (currentStep === 1) {
                Survey.Wizard.goToStep(isPub ? 3 : 2);
            } else if (currentStep === 2) {
                Survey.Wizard.goToStep(3);
            } else if (currentStep === 3) {
                Survey.Wizard.goToStep(4);
            }
        },

        prevStep: function () {
            const isPub = isPublicAccess();
            if (currentStep === 4) {
                Survey.Wizard.goToStep(3);
            } else if (currentStep === 3) {
                Survey.Wizard.goToStep(isPub ? 1 : 2);
            } else if (currentStep === 2) {
                Survey.Wizard.goToStep(1);
            }
        },

        collectTargets: function () {
            const targets = [];
            if (isPublicAccess()) {
                return targets;
            }

            const isCompany = $("#isWholeCompany").dxCheckBox("instance") ? $("#isWholeCompany").dxCheckBox("instance").option("value") : true;

            if (isCompany) {
                targets.push({ targetType: 1, departmentId: null, positionId: null });
            } else {
                const treeWidget = $("#targetTree").dxTreeView("instance");
                if (treeWidget) {
                    const selectedNodes = treeWidget.getSelectedNodes() || [];
                    const processedDeptIds = new Set();

                    selectedNodes.forEach(node => {
                        const isDeptNode = !node.parent || !node.parent.itemData || !node.parent.key;
                        if (isDeptNode && node.selected === true) {
                            const deptId = node.itemData.id || node.itemData.Id;
                            if (deptId) {
                                targets.push({ targetType: 2, departmentId: deptId, positionId: null });
                                processedDeptIds.add(deptId);
                            }
                        }
                    });

                    selectedNodes.forEach(node => {
                        const isPosNode = node.parent && node.parent.itemData && node.parent.key;
                        if (isPosNode && node.selected === true) {
                            const deptId = node.parent.itemData.id || node.parent.itemData.Id;
                            if (!processedDeptIds.has(deptId)) {
                                const posId = node.itemData.id || node.itemData.Id;
                                if (posId) {
                                    targets.push({ targetType: 3, departmentId: deptId, positionId: posId });
                                }
                            }
                        }
                    });
                }
            }

            return targets;
        },

        initStepState: function () {
            updateStep2State(isPublicAccess());
        }
    };

})(window, jQuery);
