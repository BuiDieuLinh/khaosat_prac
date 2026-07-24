// survey-element.js
window.questionCounter = 0;

window.toggleEmptyState = function() {
    const hasQuestions = $("#questionsContainer .question-block").length > 0;
    if (hasQuestions) {
        $("#emptyState").hide();
    } else {
        $("#emptyState").show();
    }
};

window.addQuestion = function() {
    window.questionCounter++;
    const qId = window.questionCounter;
    
    const html = `
        <div class="question-block border rounded-3 p-4 bg-white position-relative mb-2" id="qBlock_${qId}">
            <button type="button" class="btn btn-outline-danger btn-sm position-absolute top-0 end-0 mt-3 me-3" onclick="removeQuestion(${qId})">
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
                    <button type="button" class="btn btn-outline-primary btn-sm px-2 py-1" onclick="addOption(${qId})">
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
    window.toggleEmptyState();

    // Khởi tạo các Widget DevExtreme cho Câu hỏi này
    $(`#qFieldName_${qId}`).dxTextBox({
        placeholder: `Ví dụ: lyDoNghi, mucDoHaiLong...`,
        mode: "text"
    });

    $(`#qCaption_${qId}`).dxTextBox({
        placeholder: `Ví dụ: Nhập lý do nghỉ việc, Bạn có hài lòng không?...`
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
        value: "Text",
        onValueChanged: function(e) {
            const section = $(`#optionsSection_${qId}`);
            if (e.value === "Radio" || e.value === "Checkbox") {
                section.show();
                // Nếu chưa có option nào, tự động thêm 1 option cho người dùng
                if ($(`#optionsContainer_${qId} .option-row`).length === 0) {
                    window.addOption(qId);
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
        value: false,
        layout: "horizontal"
    });

    $(`#qHelper_${qId}`).dxTextBox({
        placeholder: `Ví dụ: Vui lòng nhập chi tiết, Chọn 1 lựa chọn phù hợp nhất...`
    });
};

window.removeQuestion = function(qId) {
    $(`#qBlock_${qId}`).remove();
    window.toggleEmptyState();
};

window.addOption = function(qId) {
    const container = $(`#optionsContainer_${qId}`);
    const optCount = container.find(".option-row").length + 1;
    const optRowId = `q_${qId}_opt_${optCount}`;

    const html = `
        <div class="option-row row g-2 align-items-center" id="${optRowId}">
            <div class="col-md-5">
                <input type="text" class="form-control form-control-sm opt-val" placeholder="Giá trị (Value) ví dụ: A, B, OK...">
            </div>
            <div class="col-md-5">
                <input type="text" class="form-control form-control-sm opt-text" placeholder="Nhãn hiển thị (DisplayText) ví dụ: Hài lòng...">
            </div>
            <div class="col-md-2">
                <button type="button" class="btn btn-outline-danger btn-sm w-100" onclick="removeOption('${optRowId}')">
                    <i class="dx-icon-trash"></i>
                </button>
            </div>
        </div>
    `;
    container.append(html);
};

window.removeOption = function(optRowId) {
    $(`#${optRowId}`).remove();
};

window.gatherSurveyData = function() {
    const elements = [];
    const questionBlocks = $("#questionsContainer .question-block");
    let hasError = false;

    questionBlocks.each(function(index) {
        const blockId = $(this).attr("id");
        const qId = blockId.split("_")[1];

        const fieldName = $(`#qFieldName_${qId}`).dxTextBox("instance").option("value");
        const caption = $(`#qCaption_${qId}`).dxTextBox("instance").option("value");
        const dataType = $(`#qDataType_${qId}`).dxSelectBox("instance").option("value");
        const required = $(`#qRequired_${qId}`).dxRadioGroup("instance").option("value");
        const helper = $(`#qHelper_${qId}`).dxTextBox("instance").option("value");

        if (!fieldName || !caption) {
            window.showToast(`Câu hỏi thứ ${index + 1} thiếu FieldName hoặc Caption!`, "error");
            hasError = true;
            return false; // Break loop
        }

        // Tạo ConfigType JSON dạng chuỗi
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
                window.showToast(`Câu hỏi "${caption}" dạng lựa chọn phải có ít nhất 1 đáp án!`, "error");
                hasError = true;
                return false;
            }

            optRows.each(function(optIdx) {
                const val = $(this).find(".opt-val").val();
                const text = $(this).find(".opt-text").val();

                if (!val || !text) {
                    window.showToast(`Đáp án thứ ${optIdx + 1} của câu hỏi "${caption}" không được để trống!`, "error");
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

window.gatherDetailAnswers = function(surveyElements) {
    const answers = [];
    for (let i = 0; i < surveyElements.length; i++) {
        const el = surveyElements[i];
        const widgetId = "#element_" + el.id;
        let value = null;

        if (el.hasOptions) {
            if (el.isMultiSelect) {
                const listWidget = $(widgetId).dxList("instance");
                if (listWidget) {
                    value = listWidget.option("selectedItemKeys"); 
                }
            } else {
                const radioWidget = $(widgetId).dxRadioGroup("instance");
                if (radioWidget) {
                    value = radioWidget.option("value");
                }
            }
        } else {
            const input = $(widgetId).dxTextBox("instance") || $(widgetId).dxNumberBox("instance");
            if (input) {
                value = input.option("value");
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
