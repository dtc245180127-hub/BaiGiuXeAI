document.addEventListener("DOMContentLoaded", function () {
    const button = document.getElementById("ai-chatbot-button");
    const panel = document.getElementById("ai-chatbot-panel");
    const closeBtn = document.getElementById("ai-chatbot-close");
    const sendBtn = document.getElementById("ai-chatbot-send");
    const input = document.getElementById("ai-chatbot-input");
    const messages = document.getElementById("ai-chatbot-messages");

    const role = panel.dataset.role || "";
    const name = panel.dataset.name || "guest";
    const storageKey = `aiChatHistory_${name}_${role}`;

    const sessionKey = `aiChatSessionStarted_${name}_${role}`;

    if (!sessionStorage.getItem(sessionKey)) {
        localStorage.removeItem(storageKey);
        sessionStorage.setItem(sessionKey, "true");
    }

    let chatHistory = JSON.parse(localStorage.getItem(storageKey) || "[]");
    let greeted = false;

    function appendMessage(type, text) {
        const div = document.createElement("div");
        div.className = `ai-message ${type}`;
        div.innerText = text;
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }

    function saveHistory() {
        localStorage.setItem(storageKey, JSON.stringify(chatHistory));
    }

    function renderOldMessages() {
        messages.innerHTML = "";
        for (const item of chatHistory) {
            appendMessage(item.role === "model" ? "bot" : "user", item.text);
        }
    }
    function clearCurrentChat() {
        chatHistory = [];
        localStorage.removeItem(storageKey);
        messages.innerHTML = "";
        greeted = false;
    }
    function getGreetingByRole(role, name) {
        const safeName = name && name.trim() ? ` ${name}` : "";
        const normalizedRole = (role || "").trim().toLowerCase();

        if (normalizedRole === "nhân viên" || normalizedRole === "nhanvien" || normalizedRole === "employee") {
            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho nhân viên trong hệ thống quản lý bãi xe. Tôi có thể hỗ trợ tra cứu nhanh, giải thích chức năng và hướng dẫn thao tác trong hệ thống.`;
        }

        if (normalizedRole === "khách hàng" || normalizedRole === "khachhang" || normalizedRole === "customer") {
            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho khách hàng. Tôi có thể hỗ trợ giải đáp thông tin và hướng dẫn tra cứu trong hệ thống quản lý bãi xe.`;
        }

        if (normalizedRole === "admin" || normalizedRole === "quản trị" || normalizedRole === "quantri") {
            return `Xin chào${safeName}! Tôi là trợ lý AI dành cho quản trị viên hệ thống bãi xe. Tôi có thể hỗ trợ giải thích chức năng quản lý, tra cứu nhanh và hướng dẫn sử dụng hệ thống.`;
        }

        return `Xin chào${safeName}! Tôi là trợ lý AI của hệ thống quản lý bãi xe. Tôi có thể hỗ trợ tra cứu nhanh và giải đáp thông tin cho bạn.`;
    }

    function showGreetingIfNeeded() {
        if (greeted) return;
        if (chatHistory.length > 0) return;

        const role = panel.dataset.role || "";
        const name = panel.dataset.name || "";
        const greeting = getGreetingByRole(role, name);

        appendMessage("bot", greeting);

        chatHistory.push({
            role: "model",
            text: greeting
        });

        saveHistory();
        greeted = true;
    }

    renderOldMessages();

    if (chatHistory.length > 0) {
        greeted = true;
    }

    button.addEventListener("click", function () {
        const isOpen = panel.style.display === "flex";
        panel.style.display = isOpen ? "none" : "flex";

        if (!isOpen) {
            showGreetingIfNeeded();
            input.focus();
        }
    });

    closeBtn.addEventListener("click", function () {
        panel.style.display = "none";
    });

    sendBtn.addEventListener("click", sendMessage);

    input.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            sendMessage();
        }
    });

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        appendMessage("user", text);
        input.value = "";

        const thinking = document.createElement("div");
        thinking.className = "ai-message bot";
        thinking.innerText = "Đang trả lời...";
        messages.appendChild(thinking);
        messages.scrollTop = messages.scrollHeight;

        try {
            const response = await fetch("/Chatbot/SendMessage", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message: text,
                    history: chatHistory
                })
            });

            const data = await response.json();
            thinking.remove();

            if (!response.ok) {
                let errorText = data.error || "Có lỗi xảy ra.";

                if (data.detail) {
                    const detailText = typeof data.detail === "string" ? data.detail : JSON.stringify(data.detail);

                    if (detailText.includes("high demand")) {
                        errorText = "AI đang quá tải tạm thời, bạn thử lại sau vài giây nhé.";
                    } else if (detailText.includes("quota") || detailText.includes("429")) {
                        errorText = "AI tạm thời đã hết lượt sử dụng trong lúc này. Bạn thử lại sau hoặc kiểm tra quota Gemini.";
                    } else {
                        errorText += "\n" + detailText;
                    }
                }

                appendMessage("bot", errorText);
                console.error("Gemini error:", data);
                return;
            }

            const botReply = data.reply || "Không có phản hồi từ AI.";
            appendMessage("bot", botReply);

            chatHistory.push({
                role: "user",
                text: text
            });

            chatHistory.push({
                role: "model",
                text: botReply
            });

            if (chatHistory.length > 6) {
                chatHistory = chatHistory.slice(chatHistory.length - 6);
            }

            saveHistory();
        } catch (error) {
            thinking.remove();
            appendMessage("bot", "Không kết nối được tới máy chủ.");
            console.error(error);
        }
    }
});