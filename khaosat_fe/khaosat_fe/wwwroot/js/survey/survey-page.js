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

    Survey.Page = {
        init() {
            Survey.Builder.init();
            initializeTargetPosition();
        }
    };

    $(function () { Survey.Page.init(); });
})(window, jQuery);
