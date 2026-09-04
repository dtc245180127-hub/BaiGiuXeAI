document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // LẤY CÁC THÀNH PHẦN CHATBOT
    // ==========================================

    const button =
        document.getElementById("ai-chatbot-button");

    const panel =
        document.getElementById("ai-chatbot-panel");

    const closeBtn =
        document.getElementById("ai-chatbot-close");

    const sendBtn =
        document.getElementById("ai-chatbot-send");

    const input =
        document.getElementById("ai-chatbot-input");

    const messages =
        document.getElementById("ai-chatbot-messages");


    // ==========================================
    // KHÔNG CÓ CHATBOT THÌ DỪNG
    // ==========================================

    if (
        !button ||
        !panel ||
        !closeBtn ||
        !sendBtn ||
        !input ||
        !messages
    ) {
        return;
    }


    // ==========================================
    // THÔNG TIN PHIÊN ĐĂNG NHẬP
    // ==========================================

    const role =
        panel.dataset.role || "";

    const name =
        panel.dataset.name || "guest";

    const sessionId =
        panel.dataset.sessionId || "";


    // ==========================================
    // KEY LƯU LỊCH SỬ CHAT
    // ==========================================

    const storageKey =
        sessionId
            ? `aiChatHistory_${sessionId}`
            : null;


    // ==========================================
    // ĐỌC LỊCH SỬ CHAT
    // ==========================================

    let chatHistory = [];

    if (storageKey) {

        try {

            chatHistory =
                JSON.parse(
                    sessionStorage.getItem(
                        storageKey
                    ) || "[]"
                );

            // Đảm bảo dữ liệu đọc được là mảng
            if (!Array.isArray(chatHistory)) {
                chatHistory = [];
            }

        }
        catch (error) {

            console.error(
                "Không đọc được lịch sử chatbot:",
                error
            );

            chatHistory = [];
        }
    }


    // ==========================================
    // TRẠNG THÁI LỜI CHÀO
    // ==========================================

    let greeted = false;


    // ==========================================
    // HIỂN THỊ TIN NHẮN
    // ==========================================

    function appendMessage(
        type,
        text
    ) {

        const div =
            document.createElement("div");

        div.className =
            `ai-message ${type}`;

        div.innerText =
            text;

        messages.appendChild(div);

        messages.scrollTop =
            messages.scrollHeight;
    }


    // ==========================================
    // LƯU LỊCH SỬ
    // ==========================================

    function saveHistory() {

        if (!storageKey) {
            return;
        }

        try {

            sessionStorage.setItem(
                storageKey,
                JSON.stringify(
                    chatHistory
                )
            );

        }
        catch (error) {

            console.error(
                "Không thể lưu lịch sử chatbot:",
                error
            );
        }
    }


    // ==========================================
    // HIỂN THỊ LỊCH SỬ CŨ
    // ==========================================

    function renderOldMessages() {

        messages.innerHTML = "";

        for (
            const item
            of chatHistory
        ) {

            appendMessage(
                item.role === "model"
                    ? "bot"
                    : "user",

                item.text
            );
        }
    }


    // ==========================================
    // LỜI CHÀO THEO VAI TRÒ
    // ==========================================

    function getGreetingByRole(
        role,
        name
    ) {

        const safeName =
            name && name.trim()
                ? ` ${name}`
                : "";

        const normalizedRole =
            (role || "")
                .trim()
                .toLowerCase();


        // --------------------------------------
        // QUẢN LÝ
        // --------------------------------------

        if (
            normalizedRole === "quanly" ||
            normalizedRole === "quản lý" ||
            normalizedRole === "admin"
        ) {

            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho quản lý hệ thống bãi xe. Tôi có thể hỗ trợ tra cứu nhanh, phân tích thông tin và hướng dẫn sử dụng hệ thống.`;
        }


        // --------------------------------------
        // NHÂN VIÊN
        // --------------------------------------

        if (
            normalizedRole === "nhanvien" ||
            normalizedRole === "nhân viên" ||
            normalizedRole === "employee"
        ) {

            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho nhân viên trong hệ thống quản lý bãi xe. Tôi có thể hỗ trợ tra cứu nhanh, giải thích chức năng và hướng dẫn thao tác trong hệ thống.`;
        }


        // --------------------------------------
        // KHÁCH HÀNG
        // --------------------------------------

        if (
            normalizedRole === "khachhang" ||
            normalizedRole === "khách hàng" ||
            normalizedRole === "customer"
        ) {

            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho khách hàng. Tôi có thể hỗ trợ giải đáp thông tin và hướng dẫn tra cứu trong hệ thống quản lý bãi xe.`;
        }


        // --------------------------------------
        // MẶC ĐỊNH
        // --------------------------------------

        return `Xin chào${safeName}! Tôi là trợ lý AI của hệ thống quản lý bãi xe. Tôi có thể hỗ trợ tra cứu nhanh và giải đáp thông tin cho bạn.`;
    }


    // ==========================================
    // HIỂN THỊ LỜI CHÀO
    // ==========================================

    function showGreetingIfNeeded() {

        if (greeted) {
            return;
        }


        // Nếu phiên hiện tại đã có lịch sử
        // thì không thêm lời chào mới.

        if (chatHistory.length > 0) {

            greeted = true;

            return;
        }


        const greeting =
            getGreetingByRole(
                role,
                name
            );


        appendMessage(
            "bot",
            greeting
        );


        chatHistory.push({

            role:
                "model",

            text:
                greeting

        });


        saveHistory();


        greeted = true;
    }


    // ==========================================
    // TRẠNG THÁI PANEL
    // ==========================================

    function openChatbot() {

        panel.classList.add(
            "chatbot-open"
        );

        panel.style.display =
            "flex";


        showGreetingIfNeeded();


        setTimeout(
            function () {

                input.focus();

                messages.scrollTop =
                    messages.scrollHeight;

            },
            50
        );
    }


    function closeChatbot() {

        panel.classList.remove(
            "chatbot-open"
        );

        panel.style.display =
            "none";
    }


    // ==========================================
    // KHỞI TẠO
    // ==========================================

    renderOldMessages();


    if (chatHistory.length > 0) {

        greeted = true;
    }


    // ==========================================
    // MỞ / ĐÓNG CHATBOT
    // ==========================================

    button.addEventListener(
        "click",
        function () {

            const isOpen =
                panel.classList.contains(
                    "chatbot-open"
                );


            if (isOpen) {

                closeChatbot();

            }
            else {

                openChatbot();
            }

        }
    );


    // ==========================================
    // ĐÓNG BẰNG NÚT X
    // ==========================================

    closeBtn.addEventListener(
        "click",
        function () {

            closeChatbot();

        }
    );


    // ==========================================
    // NÚT GỬI
    // ==========================================

    sendBtn.addEventListener(
        "click",
        sendMessage
    );


    // ==========================================
    // ENTER ĐỂ GỬI
    // ==========================================

    input.addEventListener(
        "keydown",
        function (e) {

            if (e.key === "Enter") {

                e.preventDefault();

                sendMessage();
            }

        }
    );


    // ==========================================
    // GỬI CÂU HỎI
    // ==========================================

    async function sendMessage() {

        const text =
            input.value.trim();


        // Không gửi nội dung rỗng

        if (!text) {
            return;
        }


        // --------------------------------------
        // HIỂN THỊ CÂU HỎI NGƯỜI DÙNG
        // --------------------------------------

        appendMessage(
            "user",
            text
        );


        input.value = "";


        // --------------------------------------
        // HIỂN THỊ ĐANG TRẢ LỜI
        // --------------------------------------

        const thinking =
            document.createElement("div");

        thinking.className =
            "ai-message bot";

        thinking.innerText =
            "Đang trả lời...";


        messages.appendChild(
            thinking
        );


        messages.scrollTop =
            messages.scrollHeight;


        // --------------------------------------
        // KHÓA NÚT TRONG KHI ĐANG GỬI
        // --------------------------------------

        sendBtn.disabled = true;

        input.disabled = true;


        try {

            // ==================================
            // GỌI CHATBOT BACKEND
            // ==================================

            const response =
                await fetch(
                    "/Chatbot/SendMessage",
                    {
                        method: "POST",

                        headers: {
                            "Content-Type":
                                "application/json"
                        },

                        body: JSON.stringify({

                            message:
                                text,

                            history:
                                chatHistory

                        })
                    }
                );


            // ==================================
            // ĐỌC JSON PHẢN HỒI
            // ==================================

            let data = {};

            try {

                data =
                    await response.json();

            }
            catch (jsonError) {

                console.error(
                    "Không đọc được JSON từ chatbot:",
                    jsonError
                );

                data = {};
            }


            // ==================================
            // XÓA ĐANG TRẢ LỜI
            // ==================================

            thinking.remove();


            // ==================================
            // XỬ LÝ LỖI
            // ==================================

            if (!response.ok) {

                let errorText =
                    data.error ||
                    "Có lỗi xảy ra khi gọi AI.";


                if (data.detail) {

                    const detailText =
                        typeof data.detail === "string"
                            ? data.detail
                            : JSON.stringify(
                                data.detail
                            );


                    // --------------------------
                    // AI QUÁ TẢI
                    // --------------------------

                    if (
                        detailText.includes(
                            "high demand"
                        )
                    ) {

                        errorText =
                            "AI đang quá tải tạm thời, bạn thử lại sau vài giây nhé.";
                    }


                    // --------------------------
                    // HẾT QUOTA
                    // --------------------------

                    else if (
                        detailText.includes(
                            "quota"
                        ) ||
                        detailText.includes(
                            "429"
                        )
                    ) {

                        errorText =
                            "AI tạm thời đã hết lượt sử dụng trong lúc này. Bạn thử lại sau hoặc kiểm tra quota Gemini.";
                    }


                    // --------------------------
                    // LỖI KHÁC
                    // --------------------------

                    else {

                        errorText +=
                            "\n" +
                            detailText;
                    }
                }


                appendMessage(
                    "bot",
                    errorText
                );


                console.error(
                    "Gemini error:",
                    data
                );


                return;
            }


            // ==================================
            // PHẢN HỒI THÀNH CÔNG
            // ==================================

            const botReply =
                data.reply ||
                "Không có phản hồi từ AI.";


            appendMessage(
                "bot",
                botReply
            );


            // ==================================
            // LƯU CÂU HỎI
            // ==================================

            chatHistory.push({

                role:
                    "user",

                text:
                    text

            });


            // ==================================
            // LƯU CÂU TRẢ LỜI
            // ==================================

            chatHistory.push({

                role:
                    "model",

                text:
                    botReply

            });


            // ==================================
            // CHỈ GIỮ 6 TIN NHẮN GẦN NHẤT
            // ==================================

            if (
                chatHistory.length > 6
            ) {

                chatHistory =
                    chatHistory.slice(
                        chatHistory.length - 6
                    );
            }


            // ==================================
            // LƯU SESSION STORAGE
            // ==================================

            saveHistory();

        }
        catch (error) {

            // ----------------------------------
            // XÓA TRẠNG THÁI ĐANG TRẢ LỜI
            // ----------------------------------

            thinking.remove();


            // ----------------------------------
            // HIỂN THỊ LỖI KẾT NỐI
            // ----------------------------------

            appendMessage(
                "bot",
                "Không kết nối được tới máy chủ."
            );


            console.error(
                "Chatbot error:",
                error
            );

        }
        finally {

            // ----------------------------------
            // MỞ LẠI INPUT + NÚT GỬI
            // ----------------------------------

            sendBtn.disabled = false;

            input.disabled = false;

            input.focus();
        }
    }

});