// survey.js
window.Survey = window.Survey || {};
window.Survey.DataType = window.Survey.DataType || {};
window.Survey.Validation = window.Survey.Validation || {};
window.Survey.Element = window.Survey.Element || {};
window.Survey.Api = window.Survey.Api || {};
window.Survey.Event = window.Survey.Event || {};

window.Survey.DataType = {
    Text: "Text",
    Number: "Number",
    Date: "Date",
    DateTime: "DateTime",
    Select: "Select",
    Radio: "Radio",
    Checkbox: "Checkbox",
    Rating: "Rating",
    File: "File"
};

window.Survey.Validation.validateSurvey = function (code, name, startDate, endDate, elements) {
    if (!code || !name) {
        window.Survey.Utils.showToast("Vui lòng điền Mã khảo sát và Tên khảo sát!", "error");
        return false;
    }
    if (!startDate) {
        window.Survey.Utils.showToast("Vui lòng nhập ngày bắt đầu!", "error");
    }

    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
        window.Survey.Utils.showToast("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!", "error");
        return false;
    }

    if (!elements || elements.length === 0) {
        window.Survey.Utils.showToast("Vui lòng thêm ít nhất một câu hỏi!", "error");
        return false;
    }

    return true;
};

window.Survey.Validation.validateSurveyResponseAnswers = function (surveyElements) {
    if (!window.jQuery || !$.fn.validate) {
        return true;
    }

    let validator = $("#surveyForm").data("validator");
    if (!validator) {
        validator = $("#surveyForm").validate({
            ignore: [],
            errorClass: "text-danger small d-block mt-1",
            errorElement: "div",
            errorPlacement: function (error, element) {
                const widget = element.closest(".dx-widget");
                if (widget.length) {
                    error.insertAfter(widget);
                } else {
                    error.insertAfter(element);
                }
            },
            highlight: function (element) {
                const name = $(element).attr("name");
                const widget = $("#" + name);
                if (widget.length) {
                    widget.addClass("dx-invalid");
                } else {
                    $(element).closest(".dx-widget").addClass("dx-invalid");
                }
                $(element).closest(".question-block").addClass("dx-invalid-block");
            },
            unhighlight: function (element) {
                const name = $(element).attr("name");
                const widget = $("#" + name);
                if (widget.length) {
                    widget.removeClass("dx-invalid");
                } else {
                    $(element).closest(".dx-widget").removeClass("dx-invalid");
                }
                $(element).closest(".question-block").removeClass("dx-invalid-block");
            }
        });

        surveyElements.forEach(el => {
            const name = "element_" + el.id;
            const input = $(`[name="${name}"]`);
            if (input.length) {
                const rules = {};
                const messages = {};

                if (el.required) {
                    rules.required = true;
                    messages.required = "Không được để trống";
                }

                if (el.dataType === "Number") {
                    rules.number = true;
                    rules.min = 0;
                    messages.number = "Vui lòng nhập số hợp lệ";
                    messages.min = "Giá trị không được âm";
                }

                input.rules("add", {
                    ...rules,
                    messages: messages
                });
            }
        });
    }

    return $("#surveyForm").valid();
};

window.Survey.Element.questionCounter = 0;

window.Survey.Element.toggleEmptyState = function () {
    const hasQuestions = $("#questionsContainer .question-block").length > 0;
    if (hasQuestions) {
        $("#emptyState").hide();
    } else {
        $("#emptyState").show();
    }
};

window.Survey.Element.addQuestion = function (existingData) {
    window.Survey.Element.questionCounter++;
    const qId = window.Survey.Element.questionCounter;

    const html = `
        <div class="question-block border rounded-1 p-4 bg-white position-relative mb-3 shadow-sm" id="qBlock_${qId}">
            <button type="button" class="btn btn-outline-danger btn-sm position-absolute top-0 end-0 mt-3 me-3" onclick="window.Survey.Element.removeQuestion(${qId})">
                <i class="dx-icon-trash"></i> Xóa
            </button>
            
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label small">Tên trường dữ liệu (FieldName - viết liền, không dấu) <span class="text-danger">*</span></label>
                    <div id="qFieldName_${qId}"></div>
                </div>
                <div class="col-md-6">
                    <label class="form-label small">Nhãn câu hỏi hiển thị (Caption) <span class="text-danger">*</span></label>
                    <div id="qCaption_${qId}"></div>
                </div>
                <div class="col-md-4">
                    <label class="form-label small">Kiểu câu hỏi (DataType) <span class="text-danger">*</span></label>
                    <div id="qDataType_${qId}"></div>
                </div>
                <div class="col-md-4">
                    <label class="form-label small">Bắt buộc trả lời (Required)</label>
                    <div id="qRequired_${qId}" class="mt-1"></div>
                </div>
                <div class="col-md-4">
                    <label class="form-label small">Gợi ý / Hướng dẫn nhập</label>
                    <div id="qHelper_${qId}"></div>
                </div>
            </div>

            <!-- Khu vực tùy chọn đáp án (chỉ hiện khi kiểu là Radio hoặc Checkbox) -->
            <div class="options-section border-top mt-3 pt-3" id="optionsSection_${qId}" style="display: none;">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="fw-bold text-primary mb-0"><i class="dx-icon-bulletlist small me-1"></i> Danh sách đáp án lựa chọn</h6>
                    <button type="button" class="btn btn-outline-primary btn-sm px-2 py-1" onclick="window.Survey.Element.addOption(${qId})">
                        <i class="dx-icon-add small"></i> Thêm lựa chọn
                    </button>
                </div>
                <div class="options-container d-flex flex-column gap-2" id="optionsContainer_${qId}">
                    <!-- Các tùy chọn được thêm bằng JS ở đây -->
                </div>
            </div>
        </div>
    `;

    $("#questionsContainer").append(html);
    window.Survey.Element.toggleEmptyState();

    $(`#qFieldName_${qId}`).dxTextBox({
        placeholder: `Ví dụ: lyDoNghi, mucDoHaiLong...`,
        mode: "text",
        value: existingData ? existingData.fieldName : ""
    });

    $(`#qCaption_${qId}`).dxTextBox({
        placeholder: `Ví dụ: Nhập lý do nghỉ việc, Bạn có hài lòng không?...`,
        value: existingData ? existingData.caption : ""
    });

    $(`#qDataType_${qId}`).dxSelectBox({
        items: [
            { id: "Text", text: "Nhập chữ tự do (Text)" },
            { id: "Number", text: "Nhập số (Number)" },
            { id: "Radio", text: "Chọn 1 đáp án (Radio)" },
            { id: "Checkbox", text: "Chọn nhiều đáp án (Checkbox)" }
        ],
        valueExpr: "id",
        displayExpr: "text",
        value: existingData ? existingData.dataType : "Text",
        onValueChanged: function (e) {
            const section = $(`#optionsSection_${qId}`);
            if (e.value === "Radio" || e.value === "Checkbox") {
                section.show();
                if ($(`#optionsContainer_${qId} .option-row`).length === 0) {
                    window.Survey.Element.addOption(qId);
                }
            } else {
                section.hide();
            }
        }
    });

    $(`#qRequired_${qId}`).dxRadioGroup({
        items: [
            { value: true, text: "Bắt buộc (Yes)" },
            { value: false, text: "Không bắt buộc (No)" }
        ],
        valueExpr: "value",
        displayExpr: "text",
        value: existingData ? existingData.required : false,
        layout: "horizontal"
    });

    $(`#qHelper_${qId}`).dxTextBox({
        placeholder: `Ví dụ: Vui lòng nhập chi tiết, Chọn 1 lựa chọn phù hợp nhất...`,
        value: existingData ? existingData.helper : ""
    });

    if (existingData && (existingData.dataType === "Radio" || existingData.dataType === "Checkbox")) {
        $(`#optionsSection_${qId}`).show();
        if (existingData.options && existingData.options.length > 0) {
            existingData.options.forEach(opt => {
                window.Survey.Element.addOption(qId, opt);
            });
        }
    }
};

window.Survey.Element.removeQuestion = function (qId) {
    $(`#qBlock_${qId}`).remove();
    window.Survey.Element.toggleEmptyState();
};

window.Survey.Element.addOption = function (qId, existingOpt) {
    const container = $(`#optionsContainer_${qId}`);
    const optCount = container.find(".option-row").length + 1;
    const optRowId = `q_${qId}_opt_${optCount}`;

    const val = existingOpt ? existingOpt.value : "";
    const text = existingOpt ? existingOpt.displayText : "";

    const html = `
        <div class="option-row row g-2 align-items-center" id="${optRowId}">
            <div class="col-md-5">
                <input type="text" class="form-control form-control-sm opt-val" value="${val}" placeholder="Giá trị (Value) ví dụ: A, B, OK...">
            </div>
            <div class="col-md-5">
                <input type="text" class="form-control form-control-sm opt-text" value="${text}" placeholder="Nhãn hiển thị (DisplayText) ví dụ: Hài lòng...">
            </div>
            <div class="col-md-2">
                <button type="button" class="btn btn-outline-danger btn-sm w-100" onclick="window.Survey.Element.removeOption('${optRowId}')">
                    <i class="dx-icon-trash"></i>
                </button>
            </div>
        </div>
    `;
    container.append(html);
};

window.Survey.Element.removeOption = function (optRowId) {
    $(`#${optRowId}`).remove();
};

window.Survey.Element.gatherSurveyData = function () {
    const elements = [];
    const questionBlocks = $("#questionsContainer .question-block");
    let hasError = false;

    questionBlocks.each(function (index) {
        const blockId = $(this).attr("id");
        const qId = blockId.split("_")[1];

        const fieldName = $(`#qFieldName_${qId}`).dxTextBox("instance").option("value");
        const caption = $(`#qCaption_${qId}`).dxTextBox("instance").option("value");
        const dataType = $(`#qDataType_${qId}`).dxSelectBox("instance").option("value");
        const required = $(`#qRequired_${qId}`).dxRadioGroup("instance").option("value");
        const helper = $(`#qHelper_${qId}`).dxTextBox("instance").option("value");

        if (!fieldName || !caption) {
            window.Survey.Utils.showToast(`Câu hỏi thứ ${index + 1} thiếu FieldName hoặc Caption!`, "error");
            hasError = true;
            return false;
        }

        const config = {
            DataType: (dataType === "Radio" || dataType === "Checkbox") ? "Select" : dataType,
            Caption: caption,
            DefaultValue: "",
            AllowNull: !required,
            IsMultiSelect: dataType === "Checkbox",
            Helper: helper,
            InputHelper: helper
        };

        const options = [];
        if (dataType === "Radio" || dataType === "Checkbox") {
            const optRows = $(`#optionsContainer_${qId} .option-row`);
            if (optRows.length === 0) {
                window.Survey.Utils.showToast(`Câu hỏi "${caption}" dạng lựa chọn phải có ít nhất 1 đáp án!`, "error");
                hasError = true;
                return false;
            }

            optRows.each(function (optIdx) {
                const val = $(this).find(".opt-val").val();
                const text = $(this).find(".opt-text").val();

                if (!val || !text) {
                    window.Survey.Utils.showToast(`Đáp án thứ ${optIdx + 1} của câu hỏi "${caption}" không được để trống!`, "error");
                    hasError = true;
                    return false;
                }

                options.push({
                    value: val,
                    displayText: text,
                    sortOrder: optIdx,
                    isDefault: false,
                    isActive: true
                });
            });
        }

        if (hasError) return false;

        elements.push({
            fieldName: fieldName,
            sortOrder: index,
            configType: JSON.stringify(config),
            options: options
        });
    });

    if (hasError) return null;
    return elements;
};

window.Survey.Element.gatherDetailAnswers = function (surveyElements) {
    const answers = [];
    for (let i = 0; i < surveyElements.length; i++) {
        const el = surveyElements[i];
        const widgetId = "#element_" + el.id;
        const elDom = $(widgetId);
        if (!elDom.length) continue;

        let value = null;
        const listInstance = elDom.data("dxList");
        const radioInstance = elDom.data("dxRadioGroup");
        const textInstance = elDom.data("dxTextBox");
        const numberInstance = elDom.data("dxNumberBox");

        if (el.hasOptions) {
            if (el.isMultiSelect) {
                if (listInstance) {
                    value = listInstance.option("selectedItemKeys");
                }
            } else {
                if (radioInstance) {
                    value = radioInstance.option("value");
                }
            }
        } else {
            if (textInstance) {
                value = textInstance.option("value");
            } else if (numberInstance) {
                value = numberInstance.option("value");
            }
        }

        if (value !== null && value !== undefined && value !== "") {
            if (Array.isArray(value)) {
                value.forEach(val => {
                    const opt = el.options.find(o => o.value === val);
                    answers.push({
                        elementId: el.id,
                        optionId: opt ? opt.id : null,
                        value: val.toString()
                    });
                });
            } else {
                const opt = el.options.find(o => o.value === value);
                answers.push({
                    elementId: el.id,
                    optionId: opt ? opt.id : null,
                    value: value.toString()
                });
            }
        }
    }
    return answers;
};

// ==========================================
// 4. API (survey-api.js)
// ==========================================
window.Survey.Api.submitSurveyPayload = function (payload) {
    const isEdit = !!window.Survey.Urls.saveSurvey;
    $.ajax({
        url: window.Survey.Urls.saveSurvey || window.Survey.Urls.createNested,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            window.Survey.Utils.showToast(isEdit ? "Cập nhật khảo sát thành công!" : "Tạo khảo sát thành công!", "success");
            setTimeout(function () {
                window.location.href = window.Survey.Urls.index;
            }, 1500);
        },
        error: function (err) {
            let errorMsg = "Lỗi máy chủ";
            if (err.responseText) {
                try {
                    const parsed = JSON.parse(err.responseText);
                    if (parsed.message) {
                        errorMsg = parsed.message;
                    } else if (parsed.errors) {
                        errorMsg = Object.values(parsed.errors).flat().join("<br/>");
                    } else {
                        errorMsg = err.responseText;
                    }
                } catch (e) {
                    errorMsg = err.responseText;
                }
            }
            window.Survey.Utils.showToast(errorMsg, "error");
        }
    });
};

window.Survey.Api.submitSurveyResponse = function (payload) {
    $.ajax({
        url: window.Survey.Urls.submit,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            window.Survey.Utils.showToast("Gửi khảo sát thành công!", "success", "toastDetail");
            setTimeout(function () {
                window.location.href = window.Survey.Urls.index;
            }, 1500);
        },
        error: function (err) {
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
            window.Survey.Utils.showToast("Lỗi gửi khảo sát: " + cleanMsg, "error", "toastDetail");
        }
    });
};

window.Survey.Event.saveSurvey = function () {
    const code = $("#surveyCode").dxTextBox("instance").option("value");
    const name = $("#surveyName").dxTextBox("instance").option("value");
    const description = $("#surveyDescription").dxTextArea("instance").option("value");
    const startDate = $("#surveyStartDate").dxDateBox("instance").option("value");
    const endDate = $("#surveyEndDate").dxDateBox("instance").option("value");
    const status = $("#surveyStatus").dxSelectBox("instance").option("value");

    const elements = window.Survey.Element.gatherSurveyData();
    if (elements === null) return;

    const formattedStartDate = window.Survey.Utils.toLocalISOString(startDate);
    const formattedEndDate = window.Survey.Utils.toLocalISOString(endDate);

    const isValid = window.Survey.Validation.validateSurvey(code, name, formattedStartDate, formattedEndDate, elements);
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

    window.Survey.Api.submitSurveyPayload(payload);
};

window.Survey.Event.submitSurvey = function () {
    const employeeIdInput = $("#employeeId").val();
    if (!employeeIdInput) {
        window.Survey.Utils.showToast("Không tìm thấy thông tin nhân viên đăng nhập!", "error", "toastDetail");
        return;
    }

    const isValid = window.Survey.Validation.validateSurveyResponseAnswers(window.Survey.surveyElements);
    if (!isValid) return;

    const answers = window.Survey.Element.gatherDetailAnswers(window.Survey.surveyElements);
    const payload = {
        surveyId: window.Survey.surveyId,
        employeeId: employeeIdInput,
        answers: answers
    };

    window.Survey.Api.submitSurveyResponse(payload);
};

window.Survey.Event.showImportPopup = function () {
    const popup = $("#importExcelPopup").dxPopup("instance");
    if (popup) {
        popup.show();
    }
};

$(document).ready(function () {
    if ($("#questionsContainer").length > 0) {
        window.Survey.Element.toggleEmptyState();
    }

    if ($("#surveyTargetDepartment").length > 0 && $("#surveyTargetPosition").length > 0) {
        setTimeout(() => {
            const parentWidget = $("#surveyTargetDepartment").data("dxSelectBox");
            const childWidget = $("#surveyTargetPosition").data("dxSelectBox");

            if (parentWidget && childWidget) {
                const positionData = {
                    "IT": [
                        { Value: "dev", DisplayText: "Lập trình .NET (3)" },
                        { Value: "qa", DisplayText: "Kiểm thử (QA) (2)" },
                        { Value: "devops", DisplayText: "DevOps (1)" }
                    ],
                    "HR": [
                        { Value: "recruitment", DisplayText: "Tuyển dụng" },
                        { Value: "cb", DisplayText: "Lương & Phúc lợi (C&B)" },
                        { Value: "training", DisplayText: "Đào tạo" }
                    ],
                    "Sales": [
                        { Value: "sales_north", DisplayText: "Sales miền Bắc" },
                        { Value: "sales_south", DisplayText: "Sales miền Nam" }
                    ],
                    "All": [
                        { Value: "all", DisplayText: "Tất cả nhân sự" }
                    ]
                };

                parentWidget.on("valueChanged", function (e) {
                    const selectedDept = e.value;
                    const list = positionData[selectedDept] || [];

                    childWidget.option("value", null);
                    childWidget.option("dataSource", list);
                    childWidget.option("disabled", list.length === 0);
                });
            }
        }, 100);
    }
});
