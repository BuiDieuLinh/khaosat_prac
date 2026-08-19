(function ($) {
    "use strict";

    var notifications = [];
    var list = $("#notificationList");
    var badge = $("#notificationBadge");
    var bellRingTimeout;

    function valueOf(notification, property) {
        return notification[property] ?? notification[property.charAt(0).toLowerCase() + property.slice(1)];
    }

    function formatDate(value) {
        if (!value) return "";

        var date = new Date(value);
        return Number.isNaN(date.getTime())
            ? ""
            : new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(date);
    }

    function updateBadge() {
        var unreadCount = notifications.filter(function (notification) {
            return !valueOf(notification, "IsRead");
        }).length;

        badge.text(unreadCount > 99 ? "99+" : unreadCount);
        badge.toggleClass("d-none", unreadCount === 0);
    }

    function renderNotifications() {
        list.empty();

        if (notifications.length === 0) {
            list.append($("<div>", { class: "text-center text-muted py-4", text: "Không có thông báo" }));
            updateBadge();
            return;
        }

        notifications.forEach(function (notification) {
            var isRead = valueOf(notification, "IsRead") === true;
            var item = $("<button>", {
                type: "button",
                class: "dropdown-item text-wrap border-bottom px-3 py-2" + (isRead ? "" : " bg-light")
            });
            var heading = $("<div>", { class: "d-flex justify-content-between align-items-start gap-2" });

            heading.append($("<span>", {
                class: "fw-semibold",
                text: valueOf(notification, "Title") || "Thông báo"
            }));
            if (!isRead) {
                heading.append($("<span>", { class: "badge rounded-pill bg-primary mt-1", text: "Mới" }));
            }

            item.append(heading);
            item.append($("<div>", { class: "small text-muted mt-1", text: valueOf(notification, "Message") || "" }));
            item.append($("<div>", { class: "small text-secondary mt-1", text: formatDate(valueOf(notification, "CreatedDate")) }));
            item.on("click", function () {
                var link = valueOf(notification, "Link");
                markAsRead(notification).always(function () {
                    if (link) window.location.assign(link);
                });
            });
            list.append(item);
        });

        updateBadge();
    }

    function markAsRead(notification) {
        if (valueOf(notification, "IsRead") === true) {
            return $.Deferred().resolve().promise();
        }

        return $.ajax({
            url: "/Notification/UpdateStatus?id=" + encodeURIComponent(valueOf(notification, "Id")),
            method: "PATCH"
        }).done(function () {
            notification.IsRead = true;
            notification.isRead = true;
            renderNotifications();
        });
    }

    function loadNotifications() {
        return $.getJSON("/Notification/GetNotificationsByUserId", { pageSize: 10 })
            .done(function (response) {
                notifications = Array.isArray(response) ? response : (response.data || response.Data || []);
                renderNotifications();
            })
            .fail(function () {
                notifications = [];
                renderNotifications();
            });
    }

    function receiveNotification(notification) {
        var notificationId = valueOf(notification, "Id");
        notifications = [notification].concat(notifications.filter(function (item) {
            return valueOf(item, "Id") !== notificationId;
        })).slice(0, 10);
        renderNotifications();
        ringBell();

        if (window.Common && window.Common.Utils) {
            window.Common.Utils.showToast(valueOf(notification, "Message") || "Bạn có thông báo mới.", "info");
        }
    }

    function ringBell() {
        var bell = $("#notificationBellIcon");
        if (bell.length === 0) return;

        clearTimeout(bellRingTimeout);
        bell.removeClass("notification-bell-ringing");
        void bell[0].offsetWidth;
        bell.addClass("notification-bell-ringing");
        bellRingTimeout = setTimeout(function () {
            bell.removeClass("notification-bell-ringing");
        }, 1800);
    }

    function connectNotificationHub() {
        var options = window.notificationHubOptions;
        if (!window.signalR || !options || !options.url || !options.accessToken) {
            return;
        }

        var connection = new signalR.HubConnectionBuilder()
            .withUrl(options.url, {
                accessTokenFactory: function () { return options.accessToken; }
            })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", receiveNotification);
        connection.start().catch(function (error) {
            console.warn("Không thể kết nối notification hub.", error);
        });
    }

    $(function () {
        $("#notificationDropdown").on("show.bs.dropdown", loadNotifications);
        $("#markAllAsReadBtn").on("click", function (event) {
            event.preventDefault();
            event.stopPropagation();

            var unreadNotifications = notifications.filter(function (notification) {
                return !valueOf(notification, "IsRead");
            });
            if (unreadNotifications.length > 0) {
                $.when.apply($, unreadNotifications.map(markAsRead)).always(loadNotifications);
            }
        });

        loadNotifications();
        connectNotificationHub();
    });
})(jQuery);
