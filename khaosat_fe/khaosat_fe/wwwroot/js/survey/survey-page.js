(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const Common = window.Common = window.Common || {};
    const { selector: S } = Survey.Constants || {};
    let isSaving = false;
    const isEdit = Boolean((Survey.Urls || {}).saveSurvey);

    function widgetValue(selector, widgetName) {
        const instance = $(selector)[widgetName]("instance");
        return instance ? instance.option("value") : null;
    }

    function collectSurvey() {
        const startDate = Common.Utils.toLocalISOString(widgetValue(S.surveyStartDate, "dxDateBox"));
        const endDate = Common.Utils.toLocalISOString(widgetValue(S.surveyEndDate, "dxDateBox"));
        const maxAttemptsVal = widgetValue("#surveyMaxAttempts", "dxNumberBox");
        const maxAttempts = (maxAttemptsVal !== null && maxAttemptsVal !== undefined && maxAttemptsVal !== "") ? parseInt(maxAttemptsVal) : null;
        const targets = Survey.Wizard ? Survey.Wizard.collectTargets() : [];

        const accessTypeVal = widgetValue("#surveyAccessType", "dxSelectBox");
        const accessType = accessTypeVal ? parseInt(accessTypeVal) : 1;
        const anonymousMode = widgetValue("#surveyAnonymousMode", "dxCheckBox") || false;

        return {
            code: String(widgetValue(S.surveyCode, "dxTextBox") || "").trim(),
            name: String(widgetValue(S.surveyName, "dxTextBox") || "").trim(),
            description: String(widgetValue(S.surveyDescription, "dxTextArea") || "").trim(),
            startDate,
            endDate,
            status: widgetValue(S.surveyStatus, "dxSelectBox"),
            maxAttempts,
            accessType,
            anonymousMode,
            targets,
            elements: Survey.Element.gatherSurveyData()
        };
    }

    function saveSurvey() {
        if (isSaving) return;

        if (Survey.Wizard) {
            if (!Survey.Wizard.validateStep(1) || !Survey.Wizard.validateStep(2) || !Survey.Wizard.validateStep(3)) {
                return;
            }
        }

        const survey = collectSurvey();
        if (!survey.elements || !Survey.Validation.validateSurvey(survey)) return;

        isSaving = true;
        Survey.Api.saveSurvey(survey).done(function () {
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
        if (!employeeId) return Common.Utils.showToast("Không tìm thấy thông tin nhân viên đăng nhập!", "error");
        if (!Survey.Validation.validateResponse(Survey.surveyElements || [])) return;

        Survey.Api.submitResponse({ surveyId: Survey.surveyId, employeeId, answers: collectAnswers(Survey.surveyElements) })
            .done(function () {
                Common.Utils.showToast("Gửi khảo sát thành công!", "success");
                window.setTimeout(function () { window.location.assign(Survey.Urls.index); }, 1500);
            })
            .fail(function (xhr) { Common.Utils.showToast(`Lỗi gửi khảo sát: ${Survey.Api.getErrorMessage(xhr)}`, "error"); });
    }

    let selectedExcelFile = null;

    function handleExcelUpload(e) {
        if (e && e.value && e.value.length > 0) {
            selectedExcelFile = e.value[0];
        } else {
            selectedExcelFile = null;
        }
    }

    function onImportPopupHidden() {
        selectedExcelFile = null;
        const uploader = $(S.importFile).dxFileUploader("instance");
        if (uploader) uploader.reset();
    }

    function downloadTemplate() {
        if (typeof ExcelJS === "undefined" || typeof saveAs === "undefined") {
            Common.Utils.showToast("Thư viện xuất Excel chưa sẵn sàng!", "error");
            return;
        }

        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet("Mau_Cau_Hoi_Khao_Sat");

        worksheet.columns = [
            { header: "STT", key: "stt", width: 8 },
            { header: "Mã trường (FieldName)", key: "fieldName", width: 25 },
            { header: "Nội dung câu hỏi (Caption)", key: "caption", width: 45 },
            { header: "Kiểu câu hỏi (DataType)", key: "dataType", width: 22 },
            { header: "Bắt buộc (Required)", key: "required", width: 18 },
            { header: "Gợi ý (Helper)", key: "helper", width: 30 },
            { header: "Danh sách đáp án (Options)", key: "options", width: 50 }
        ];

        worksheet.addRow({
            stt: 1,
            fieldName: "mucDoHaiLong",
            caption: "Đánh giá mức độ hài lòng của bạn về môi trường làm việc",
            dataType: "Radio",
            required: "Có",
            helper: "Chọn 1 đáp án phù hợp nhất",
            options: "1: Rất tốt; 2: Hài lòng; 3: Bình thường; 4: Chưa tốt"
        });

        worksheet.addRow({
            stt: 2,
            fieldName: "cheDoPhucLoi",
            caption: "Các chế độ đãi ngộ bạn mong muốn bổ sung",
            dataType: "Checkbox",
            required: "Không",
            helper: "Có thể chọn nhiều đáp án",
            options: "Bảo hiểm sức khỏe cao cấp; Phụ cấp du lịch; Thưởng hiệu suất"
        });

        worksheet.addRow({
            stt: 3,
            fieldName: "yKienDongGop",
            caption: "Ý kiến đóng góp khác cho công ty",
            dataType: "TextArea",
            required: "Không",
            helper: "Nhập phản hồi chi tiết nếu có",
            options: ""
        });

        const headerRow = worksheet.getRow(1);
        headerRow.font = { bold: true, color: { argb: "FFFFFF" } };
        headerRow.fill = {
            type: "pattern",
            pattern: "solid",
            fgColor: { argb: "1B6EC2" }
        };
        headerRow.alignment = { vertical: "middle", horizontal: "center" };

        workbook.xlsx.writeBuffer().then(function (buffer) {
            const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            saveAs(blob, "Mau_Cau_Hoi_Khao_Sat.xlsx");
        }).catch(function (err) {
            Common.Utils.showToast("Lỗi tải file mẫu: " + err.message, "error");
        });
    }

    function executeImport() {
        if (!selectedExcelFile) {
            Common.Utils.showToast("Vui lòng chọn tệp Excel để nhập!", "error");
            return;
        }

        if (typeof ExcelJS === "undefined") {
            Common.Utils.showToast("Thư viện đọc Excel chưa sẵn sàng!", "error");
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            const buffer = e.target.result;
            const workbook = new ExcelJS.Workbook();
            workbook.xlsx.load(buffer).then(function () {
                const worksheet = workbook.worksheets[0];
                if (!worksheet) {
                    Common.Utils.showToast("Tệp Excel không chứa dữ liệu!", "error");
                    return;
                }

                const questions = [];
                worksheet.eachRow(function (row, rowNumber) {
                    if (rowNumber === 1) return; // Skip header

                    const getVal = (colIndex) => {
                        const cell = row.getCell(colIndex);
                        if (!cell || cell.value === null || cell.value === undefined) return "";
                        if (typeof cell.value === "object") {
                            if (cell.value.result !== undefined) return String(cell.value.result).trim();
                            if (cell.value.text !== undefined) return String(cell.value.text).trim();
                            if (cell.value.richText && Array.isArray(cell.value.richText)) {
                                return cell.value.richText.map(t => t.text).join("").trim();
                            }
                        }
                        return String(cell.value).trim();
                    };

                    const fieldName = getVal(2);
                    const caption = getVal(3);
                    const dataTypeRaw = getVal(4);
                    const requiredRaw = getVal(5);
                    const helper = getVal(6);
                    const optionsRaw = getVal(7);

                    if (!fieldName && !caption) return;

                    const requiredLower = requiredRaw.toLowerCase();
                    const required = requiredLower === "có" || requiredLower === "yes" || requiredLower === "true" || requiredLower === "1";

                    let dataType = "TextBox";
                    const dtLower = dataTypeRaw.toLowerCase();
                    if (dtLower.includes("check") || dtLower.includes("nhiều")) dataType = "Checkbox";
                    else if (dtLower.includes("radio") || dtLower.includes("1") || dtLower.includes("một")) dataType = "Radio";
                    else if (dtLower.includes("number") || dtLower.includes("số")) dataType = "Number";
                    else if (dtLower.includes("date") || dtLower.includes("ngày")) dataType = "Datetime";
                    else if (dtLower.includes("area") || dtLower.includes("nội dung")) dataType = "TextArea";
                    else if (dtLower.includes("text") || dtLower.includes("chữ")) dataType = "TextBox";

                    const options = [];
                    if (optionsRaw) {
                        const rawItems = optionsRaw.split(/;\s*|\n+/);
                        rawItems.forEach(item => {
                            const trimmed = item.trim();
                            if (trimmed) {
                                const colonIdx = trimmed.indexOf(":");
                                let value = trimmed;
                                let displayText = trimmed;
                                if (colonIdx > -1) {
                                    value = trimmed.substring(0, colonIdx).trim();
                                    displayText = trimmed.substring(colonIdx + 1).trim();
                                }
                                options.push({ value: value || displayText, displayText: displayText || value });
                            }
                        });
                    }

                    questions.push({
                        fieldName: fieldName || `field_${rowNumber}`,
                        caption: caption || fieldName,
                        dataType,
                        required,
                        helper,
                        options
                    });
                });

                if (!questions.length) {
                    Common.Utils.showToast("Không tìm thấy câu hỏi hợp lệ trong tệp Excel!", "error");
                    return;
                }

                const radioWidget = $(S.importMode).dxRadioGroup("instance");
                const mode = radioWidget ? radioWidget.option("value") : "overwrite";

                if (mode === "overwrite") {
                    if (Survey.Element && Survey.Element.clearQuestions) {
                        Survey.Element.clearQuestions();
                    } else {
                        $(S.questionsContainer).empty();
                        Survey.Element.toggleEmptyState();
                    }
                }

                questions.forEach(q => {
                    Survey.Element.addQuestion(q);
                });

                Common.Utils.showToast(`Đã nhập thành công ${questions.length} câu hỏi từ Excel!`, "success");
                const popup = $(S.importPopup).dxPopup("instance");
                if (popup) popup.hide();
            }).catch(function (err) {
                Common.Utils.showToast("Lỗi xử lý file Excel: " + err.message, "error");
            });
        };

        reader.readAsArrayBuffer(selectedExcelFile);
    }

    function refreshGrid() {
        const $grid = $("#cardSurveys");
        if (!$grid.length) return;

        const instance = $grid.data("dxDataGrid") || $grid.data("dxCardView") || $grid.data("dxTileView");
        if (instance && typeof instance.refresh === "function") {
            instance.refresh();
        } else {
            window.location.reload();
        }
    }

    function cloneSurvey(id) {
        if (!id) return;
        Survey.Api.cloneSurvey(id)
            .done(function () {
                refreshGrid();
                Common.Utils.showToast("Đã tạo bản sao khảo sát thành công.", "success");
            })
            .fail(function (xhr) {
                Common.Utils.showToast(`Lỗi khi clone khảo sát: ${Survey.Api.getErrorMessage(xhr)}`, "error");
            });
    }

    function closeSurvey(id) {
        if (!id) return;
        DevExpress.ui.dialog.confirm(
            "Sau khi đóng, khảo sát sẽ không thể tiếp tục thực hiện.", "Bạn có chắc chắn muốn đóng khảo sát này?"
        ).done(function (result) {
            if (!result) return;
            Survey.Api.closeSurvey(id)
                .done(function () {
                    Common.Utils.showToast("Đã đóng khảo sát thành công.", "success");
                    refreshGrid();
                })
                .fail(function (xhr) {
                    Common.Utils.showToast(`Lỗi khi đóng khảo sát: ${Survey.Api.getErrorMessage(xhr)}`, "error");
                });
        });
    }

    function showSurveyMenu(element, id, status, isStartedVal, completedCountVal) {
        const isClosed = status === 2;
        const isStarted = Boolean(isStartedVal);
        const completedCount = parseInt(completedCountVal) || 0;
        const canEdit = !isClosed && !isStarted && completedCount === 0;

        let editTooltip = "";
        if (isClosed) {
            editTooltip = "Khảo sát đã được đóng.";
        } else if (completedCount > 0) {
            editTooltip = "Khảo sát đã có người tham gia nên không thể chỉnh sửa. Vui lòng nhân bản khảo sát nếu muốn thay đổi nội dung.";
        } else if (isStarted) {
            editTooltip = "Khảo sát đã bắt đầu nên không thể chỉnh sửa.";
        }

        const items = [
            {
                text: "Edit",
                icon: "edit",
                disabled: !canEdit,
                tooltip: editTooltip,
                onClick: function () {
                    if (canEdit) {
                        window.location.href = "/Survey/Edit/" + id;
                    }
                }
            },
            {
                text: "Clone Survey",
                icon: "copy",
                onClick: function () {
                    cloneSurvey(id);
                }
            }
        ];

        if (!isClosed) {
            items.push({
                text: "Close Survey",
                icon: "close",
                danger: true,
                onClick: function () {
                    closeSurvey(id);
                }
            });
        }

        let $menu = $("#surveyContextMenuContainer");
        if (!$menu.length) {
            $menu = $('<div id="surveyContextMenuContainer"></div>').appendTo("body");
        }

        const menuInstance = $menu.dxContextMenu({
            target: element,
            showEvent: "",
            dataSource: items,
            width: 170,
            itemTemplate: function (itemData) {
                const $item = $('<div class="d-flex align-items-center gap-2 py-1 px-2"></div>');
                if (itemData.icon) {
                    const iconClass = itemData.danger ? "text-danger" : (itemData.disabled ? "text-muted opacity-50" : "text-primary");
                    $item.append(`<i class="dx-icon-${itemData.icon} ${iconClass}"></i>`);
                }
                const textClass = itemData.danger ? "text-danger" : (itemData.disabled ? "text-muted opacity-50" : "text-dark");
                $item.append(`<span class="${textClass} fw-medium" style="font-size: 0.88rem;">${itemData.text}</span>`);

                if (itemData.disabled && itemData.tooltip) {
                    $item.attr("title", itemData.tooltip);
                    $item.attr("data-bs-toggle", "tooltip");
                    $item.attr("data-bs-placement", "left");
                }
                return $item;
            },
            onItemClick: function (e) {
                if (e.itemData && !e.itemData.disabled && typeof e.itemData.onClick === "function") {
                    e.itemData.onClick();
                }
            }
        }).dxContextMenu("instance");

        menuInstance.show();
    }

    Survey.Event = Survey.Event || {};
    Survey.Event.saveSurvey = saveSurvey;
    Survey.Event.submitSurvey = submitResponse;
    Survey.Event.showImportPopup = function () {
        const popup = $(S.importPopup).dxPopup("instance");
        if (popup) popup.show();
    };
    Survey.Event.downloadTemplate = downloadTemplate;
    Survey.Event.handleExcelUpload = handleExcelUpload;
    Survey.Event.executeImport = executeImport;
    Survey.Event.onImportPopupHidden = onImportPopupHidden;
    Survey.Event.cloneSurvey = cloneSurvey;
    Survey.Event.closeSurvey = closeSurvey;
    Survey.Event.showSurveyMenu = showSurveyMenu;

    window.cloneSurvey = cloneSurvey;
    window.closeSurvey = closeSurvey;
    window.showSurveyMenu = showSurveyMenu;

    Survey.Page = {
        init() {
            if (Survey.Builder && typeof Survey.Builder.init === "function") {
                Survey.Builder.init();
            }
            $('body').tooltip({
                selector: '[data-bs-toggle="tooltip"]',
                trigger: 'hover'
            });

            $(document).on('show.bs.dropdown', '.dropdown', function () {
                $(this).closest('.dx-row, .dx-data-row, .dx-card, .survey-row-card').addClass('survey-dropdown-open').css('z-index', 9999);
            });

            $(document).on('hide.bs.dropdown', '.dropdown', function () {
                $(this).closest('.dx-row, .dx-data-row, .dx-card, .survey-row-card').removeClass('survey-dropdown-open').css('z-index', '');
            });

            const codeEditor = $(S.surveyCode).dxTextBox("instance");

            if (isEdit && codeEditor) {
                codeEditor.option("disabled", true);
            }
        }
    };

    $(function () { Survey.Page.init(); });
})(window, jQuery);
