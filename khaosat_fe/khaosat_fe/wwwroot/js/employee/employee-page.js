(function (window, $) {
    "use strict";

    const Employee = window.Employee = window.Employee || {};
    const Common = window.Common = window.Common || {};

    let selectedEmployee = null;
    let isLoadingEmployee = false;
    window.departmentData = window.departmentData || [];

    function getUrls() {
        return window.Employee.Urls || {};
    }

    function addEmployee() {
        selectedEmployee = null;
        const popup = $(Employee.Constants.selector.employeePopup).dxPopup("instance");
        if (popup) popup.show();
        const form = $(Employee.Constants.selector.employeeForm).dxForm("instance");
        form.option("formData", {});
    }

    function editEmployee(e) {
        selectedEmployee = e.row.data;
        const popup = $(Employee.Constants.selector.employeePopup).dxPopup("instance");
        if (popup) popup.show();
    }

    function cancelEmployee() {
        const popup = $(Employee.Constants.selector.employeePopup).dxPopup("instance");
        if (popup) popup.hide();
    }

    function employeePopupShown() {
        const form = $(Employee.Constants.selector.employeeForm).dxForm("instance");
        if (!form) return;

        const positionEditor = form.getEditor("PositionId");
        const departmentEditor = form.getEditor("DepartmentId");

        isLoadingEmployee = true;
        if (departmentEditor) {
            departmentEditor.option("dataSource", window.departmentData || []);
        }

        if (!selectedEmployee) {
            form.resetValues();
            form.option("formData", {});
            if (positionEditor) positionEditor.option("dataSource", []);
            isLoadingEmployee = false;
            return;
        }

        const urls = getUrls();
        if (urls.positions && selectedEmployee.DepartmentId) {
            $.get(urls.positions, {
                departmentId: selectedEmployee.DepartmentId
            })
                .done(function (positions) {
                    if (positionEditor) positionEditor.option("dataSource", positions || []);
                    form.option("formData", { ...selectedEmployee });
                })
                .always(function () {
                    isLoadingEmployee = false;
                });
        } else {
            if (positionEditor) positionEditor.option("dataSource", []);
            form.option("formData", { ...selectedEmployee });
            isLoadingEmployee = false;
        }
    }

    function deleteEmployee(e) {
        DevExpress.ui.dialog.confirm(
            "Bạn có chắc muốn xóa nhân sự này?",
            "Xác nhận xóa"
        ).done(function (result) {
            if (!result) return;
            Employee.Api.deleteEmployee({ key: e.row.data.Id })
                .done(function () {
                    const grid = $(Employee.Constants.selector.employeeGrid).dxDataGrid("instance");
                    if (grid) grid.refresh();
                    Common.Utils.showToast("Xóa nhân viên thành công!", "success");
                })
                .fail(function (xhr) {
                    Common.Utils.showToast(Employee.Api.getErrorMessage(xhr), "error");
                });
        });
    }

    function departmentChanged(e) {
        if (isLoadingEmployee) return;

        const form = $(Employee.Constants.selector.employeeForm).dxForm("instance");
        if (!form) return;
        const positionEditor = form.getEditor("PositionId");
        if (!positionEditor) return;

        if (!e.value) {
            positionEditor.option("dataSource", []);
            positionEditor.option("value", null);
            return;
        }

        const urls = getUrls();
        if (urls.positions) {
            $.get(urls.positions, {
                departmentId: e.value
            }, function (positions) {
                if (isLoadingEmployee) return;
                positionEditor.option("dataSource", positions || []);
                positionEditor.option("value", null);
            });
        }
    }

    function saveEmployee() {
        const form = $(Employee.Constants.selector.employeeForm).dxForm("instance");
        if (!form) return;
        const data = form.option("formData") || {};

        const isUpdate = Boolean(selectedEmployee && selectedEmployee.Id);
        const urls = getUrls();
        const targetUrl = isUpdate ? urls.update : urls.create;
        const httpMethod = isUpdate ? "PUT" : "POST";

        const formData = new FormData();
        if (isUpdate) {
            formData.append("key", data.Id);
        }
        formData.append("values", JSON.stringify(data));

        Employee.Api.saveEmployee(formData, targetUrl, httpMethod)
            .done(function () {
                const popup = $(Employee.Constants.selector.employeePopup).dxPopup("instance");
                if (popup) popup.hide();

                const grid = $(Employee.Constants.selector.employeeGrid).dxDataGrid("instance");
                if (grid) grid.refresh();

                Common.Utils.showToast(isUpdate ? "Cập nhật nhân viên thành công!" : "Tạo nhân viên thành công!", "success");
            })
            .fail(function (xhr) {
                Common.Utils.showToast(Employee.Api.getErrorMessage(xhr), "error");
            });
    }

    // Export functions and variables to global scope for DevExtreme event handlers
    window.addEmployee = addEmployee;
    window.editEmployee = editEmployee;
    window.cancelEmployee = cancelEmployee;
    window.employeePopupShown = employeePopupShown;
    window.deleteEmployee = deleteEmployee;
    window.departmentChanged = departmentChanged;
    window.saveEmployee = saveEmployee;

    $(function () {
        const urls = getUrls();
        if (urls.departments) {
            $.get(urls.departments, function (data) {
                window.departmentData = data || [];
                const form = $(Employee.Constants.selector.employeeForm).dxForm("instance");
                if (form) {
                    const departmentEditor = form.getEditor("DepartmentId");
                    if (departmentEditor) {
                        departmentEditor.option("dataSource", window.departmentData);
                    }
                }
            });
        }
    });
})(window, jQuery);