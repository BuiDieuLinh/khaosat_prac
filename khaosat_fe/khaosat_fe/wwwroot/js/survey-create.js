// survey-create.js
$(document).ready(function() {
    // Khởi tạo trạng thái ban đầu của danh sách câu hỏi
    if (window.toggleEmptyState) {
        window.toggleEmptyState();
    }
});

window.showImportPopup = function() {
    const popup = $("#importExcelPopup").dxPopup("instance");
    if (popup) {
        popup.show();
    }
};
