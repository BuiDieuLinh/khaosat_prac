// auth-event.js
window.handleLogin = function() {
    const username = $("#usernameInput").dxTextBox("instance").option("value");
    const password = $("#passwordInput").dxTextBox("instance").option("value");

    const isValid = window.validateLoginInputs(username, password);
    if (!isValid) return;

    $("#errorMessage").hide();

    const payload = {
        Username: username,
        Password: password
    };

    window.submitLogin(payload);
};
