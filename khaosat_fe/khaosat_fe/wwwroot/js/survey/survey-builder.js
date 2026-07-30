(function (window, $) {
    "use strict";

    const Survey = window.Survey = window.Survey || {};
    const { selector: S, dataType: TYPE } = Survey.Constants;
    let questionCounter = 0;
    let optionCounter = 0;

    const typeOptions = [
        { id: TYPE.TEXT_BOX, text: "Nhập chữ tự do (Text)" },
        { id: TYPE.NUMBER, text: "Nhập số (Number)" },
        { id: TYPE.RADIO, text: "Chọn 1 đáp án (Radio)" },
        { id: TYPE.CHECKBOX, text: "Chọn nhiều đáp án (Checkbox)" },
        { id: TYPE.DATE_TIME, text: "Chọn ngày (Date)" },
        { id: TYPE.TEXT_AREA, text: "Nhập nội dung (TextArea)" }
    ];

    function isChoiceType(type) {
        return type === TYPE.RADIO || type === TYPE.CHECKBOX;
    }

    function normalizeType(type) {
        const normalized = String(type || TYPE.TEXT_BOX).toLowerCase();
        if (normalized === "select") return TYPE.RADIO;
        if (normalized === "checkbox" || normalized === "checkboxlist") return TYPE.CHECKBOX;
        if (normalized === "radio" || normalized === "radiolist") return TYPE.RADIO;
        if (normalized === "text" || normalized === "textbox" || normalized === "string") return TYPE.TEXT_BOX;
        if (normalized === "textarea") return TYPE.TEXT_AREA;
        if (normalized === "date" || normalized === "datetime") return TYPE.DATE_TIME;
        return type || TYPE.TEXT_BOX;
    }

    function escapeAttribute(value) {
        return String(value || "").replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    function getValue(source, camelName, pascalName, fallback) {
        if (!source) return fallback;
        return source[camelName] !== undefined ? source[camelName] : (source[pascalName] !== undefined ? source[pascalName] : fallback);
    }

    function questionHtml(id) {
        return `<div class="question-block builder-question-card border rounded-1 p-4 bg-white position-relative mb-3 shadow-sm" id="qBlock_${id}">
            <div class="builder-question-header d-flex align-items-center mb-3 pb-3 border-bottom">
                <div class="drag-handle builder-drag-handle me-3 text-muted" title="Kéo thả để sắp xếp"><i class="dx-icon-menu"></i></div>
                <h6 class="fw-bold text-dark mb-0"><i class="dx-icon-help-outline me-1"></i>Câu hỏi #${id}</h6>
                <button type="button" class="btn btn-outline-danger btn-sm ms-auto px-2 py-1" data-survey-action="remove-question" data-question-id="${id}"><i class="dx-icon-trash"></i> Xóa</button>
            </div>
            <div class="row g-3">
                <div class="col-md-4"><label class="form-label small">Tên trường dữ liệu <span class="text-danger">*</span></label><div id="qFieldName_${id}"></div></div>
                <div class="col-md-8"><label class="form-label small">Nhãn câu hỏi <span class="text-danger">*</span></label><div id="qCaption_${id}"></div></div>
                <div class="col-md-4"><label class="form-label small">Kiểu câu hỏi <span class="text-danger">*</span></label><div id="qDataType_${id}"></div></div>
                <div class="col-md-4"><label class="form-label small">Bắt buộc trả lời <span class="text-danger">*</span></label><div id="qRequired_${id}" class="mt-1"></div></div>
                <div class="col-md-4"><label class="form-label small">Gợi ý / hướng dẫn nhập</label><div id="qHelper_${id}"></div></div>
            </div>
            <div class="options-section builder-options-section border-top mt-3 pt-3" id="optionsSection_${id}">
                <div class="d-flex justify-content-between align-items-center mb-2"><h6 class="fw-bold text-primary mb-0">Danh sách đáp án lựa chọn</h6><button type="button" class="btn btn-outline-primary btn-sm" data-survey-action="add-option" data-question-id="${id}"><i class="dx-icon-add"></i> Thêm lựa chọn</button></div>
                <div class="options-container d-flex flex-column gap-2" id="optionsContainer_${id}"></div>
            </div>
        </div>`;
    }

    function optionHtml(questionId, value) {
        const id = `q_${questionId}_opt_${++optionCounter}`;
        return `<div class="option-row builder-option-row row g-2 align-items-center" id="${id}">
            <div class="col-auto drag-option-handle text-muted" title="Kéo thả để sắp xếp"><i class="dx-icon-menu"></i></div>
            <div class="col"><input type="text" class="form-control form-control-sm opt-text" value="${escapeAttribute(value)}" placeholder="Nhập nội dung đáp án..."></div>
            <div class="col-auto"><button type="button" class="btn btn-outline-danger btn-sm px-2 py-1" data-survey-action="remove-option" data-option-id="${id}" title="Xóa đáp án"><i class="dx-icon-trash"></i></button></div>
        </div>`;
    }

    function moveItem(event, $container, itemSelector) {
        const $item = $(event.itemElement);
        const $items = $container.children(itemSelector).not($item);
        if (event.toIndex <= 0) $container.prepend($item);
        else if (event.toIndex >= $items.length) $container.append($item);
        else $items.eq(event.toIndex).before($item);
    }

    function createWidgets(id, data) {
        const dataType = normalizeType(getValue(data, "dataType", "DataType", TYPE.TEXT_BOX));
        $(`#qFieldName_${id}`).dxTextBox({ placeholder: "Ví dụ: mucDoHaiLong", value: getValue(data, "fieldName", "FieldName", "") });
        $(`#qCaption_${id}`).dxTextBox({ placeholder: "Nhập nội dung câu hỏi...", value: getValue(data, "caption", "Caption", "") });
        $(`#qHelper_${id}`).dxTextBox({ placeholder: "Hướng dẫn cho người trả lời", value: getValue(data, "helper", "Helper", "") });
        $(`#qRequired_${id}`).dxRadioGroup({ items: [{ value: true, text: "Bắt buộc" }, { value: false, text: "Không bắt buộc" }], valueExpr: "value", displayExpr: "text", value: getValue(data, "required", "Required", true), layout: "horizontal" });
        $(`#qDataType_${id}`).dxSelectBox({ items: typeOptions, valueExpr: "id", displayExpr: "text", value: dataType, onValueChanged(event) { toggleOptions(id, event.value); } });
        toggleOptions(id, dataType);
    }

    function toggleOptions(questionId, dataType) {
        const $section = $(`#optionsSection_${questionId}`);
        if (!isChoiceType(dataType)) return $section.hide();
        $section.show();
        const $options = $(`#optionsContainer_${questionId}`);
        if (!$options.children(".option-row").length) addOption(questionId);
    }

    function setupOptionSortable(questionId) {
        const $container = $(`#optionsContainer_${questionId}`);
        $container.dxSortable({ filter: ".option-row", handle: ".drag-option-handle", itemOrientation: "vertical", dragDirection: "vertical", onReorder(event) { moveItem(event, $container, ".option-row"); } });
    }

    function addOption(questionId, option) {
        const $container = $(`#optionsContainer_${questionId}`);
        if (!$container.length) return;
        $container.append(optionHtml(questionId, getValue(option, "displayText", "DisplayText", "")));
    }

    function addQuestion(data) {
        const id = ++questionCounter;
        $(S.questionsContainer).append(questionHtml(id));
        createWidgets(id, data || {});
        const options = getValue(data, "options", "Options", []);
        if (Array.isArray(options) && options.length) {
            const $container = $(`#optionsContainer_${id}`).empty();
            options.forEach(function (option) { addOption(id, option); });
        }
        setupOptionSortable(id);
        toggleEmptyState();
    }

    function removeQuestion(questionId) { $(`#qBlock_${questionId}`).remove(); toggleEmptyState(); }
    function removeOption(optionId) { $(`#${optionId}`).remove(); }
    function toggleEmptyState() { $(S.emptyState).toggle(!$(S.questionsContainer).find(".question-block").length); }

    function getInstance(selector, widgetName) {
        const instance = $(selector)[widgetName]("instance");
        return instance || null;
    }

    function collectQuestions() {
        const elements = [];
        let error = null;
        $(S.questionsContainer).find(".question-block").each(function (index) {
            if (error) return false;
            const id = this.id.replace("qBlock_", "");
            const fieldName = getInstance(`#qFieldName_${id}`, "dxTextBox").option("value").trim();
            const caption = getInstance(`#qCaption_${id}`, "dxTextBox").option("value").trim();
            const dataType = getInstance(`#qDataType_${id}`, "dxSelectBox").option("value");
            const required = getInstance(`#qRequired_${id}`, "dxRadioGroup").option("value");
            const helper = getInstance(`#qHelper_${id}`, "dxTextBox").option("value").trim();
            if (!fieldName || !caption) { error = `Câu hỏi thứ ${index + 1} thiếu tên trường hoặc nhãn câu hỏi.`; return false; }
            const options = [];
            if (isChoiceType(dataType)) {
                $(this).find(".option-row").each(function (optionIndex) {
                    const text = $(this).find(".opt-text").val().trim();
                    if (!text) { error = `Đáp án thứ ${optionIndex + 1} của câu hỏi "${caption}" không được để trống.`; return false; }
                    options.push({ value: text, displayText: text, sortOrder: optionIndex, isDefault: false, isActive: true });
                });
                if (!error && !options.length) error = `Câu hỏi "${caption}" phải có ít nhất một đáp án.`;
            }
            if (error) return false;
            elements.push({ fieldName, sortOrder: index, configType: JSON.stringify({ DataType: isChoiceType(dataType) ? TYPE.SELECT : dataType, Caption: caption, DefaultValue: "", AllowNull: !required, IsMultiSelect: dataType === TYPE.CHECKBOX, Helper: helper, InputHelper: helper }), options });
        });
        if (error) Survey.Utils.showToast(error, "error");
        return error ? null : elements;
    }

    Survey.Element = Survey.Element || {};
    Object.assign(Survey.Element, { addQuestion, removeQuestion, addOption, removeOption, toggleEmptyState, gatherSurveyData: collectQuestions });

    Survey.Builder = {
        init() {
            const $container = $(S.questionsContainer);
            if (!$container.length) return;
            toggleEmptyState();
            $container.dxSortable({ filter: ".question-block", handle: ".drag-handle", itemOrientation: "vertical", dragDirection: "vertical", onReorder(event) { moveItem(event, $container, ".question-block"); } });
            $(document).off("click.surveyBuilder").on("click.surveyBuilder", "[data-survey-action]", function () {
                const $button = $(this);
                const action = $button.data("survey-action");
                if (action === "add-option") addOption($button.data("question-id"));
                if (action === "remove-question") removeQuestion($button.data("question-id"));
                if (action === "remove-option") removeOption($button.data("option-id"));
            });
        }
    };
})(window, jQuery);
