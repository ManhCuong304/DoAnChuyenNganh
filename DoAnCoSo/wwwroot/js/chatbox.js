document.addEventListener("DOMContentLoaded", function () {
    fetch("/Post/GetFriends")
        .then(response => response.json())
        .then(friends => {
            const container = document.getElementById("chat-container");

            friends.forEach(friend => {
                // Tạo button mở chat
                const friendBox = document.createElement("div");
                friendBox.className = "user d-flex align-items-center mb-2";
                friendBox.style.cursor = "pointer";

                friendBox.innerHTML = `
                    <img src="${friend.image}" alt="avatar" class="user-info__img-1">
                    <div>
                        <div class="fw-bold">${friend.fullName}</div>
                        <div class="text-muted small">${friend.userName}</div>
                    </div>
                `;

                friendBox.addEventListener("click", () => {
                    openChatBox(friend);
                });

                container.appendChild(friendBox);
            });
        });
});


    chatContainer.appendChild(chatBox);

    fetch(`/messages/GetMessages/${currentUserId}/${friend.id}`)
        .then(res => res.json())
        .then(messages => {
            const messagesDiv = chatBox.querySelector(".chat-messages");
            messages.forEach(msg => {
                const div = document.createElement("div");
                div.textContent = msg.content;
                div.classList.add(msg.fromUserId === currentUserId ? "my-message" : "received-message");
                messagesDiv.appendChild(div);
            });
            messagesDiv.scrollTop = messagesDiv.scrollHeight;
        });
}

function GetFriends() {
    fetch("/home/getfriends")
        .then(res => res.json())
        .then(data => {
            const container = document.getElementById("friends-container");
            container.innerHTML = "";
            data.forEach(friend => {
                const div = document.createElement("div");
                div.classList.add("user", "d-flex", "align-items-center", "mb-2");
                div.innerHTML = `
                    <img src="${friend.image}" class="user-info__img-1">
                    <div>
                        <div class="fw-bold">${friend.fullName}</div>
                        <div class="text-muted small">${friend.userName}</div>
                    </div>
                `;
                div.onclick = () => openChatBox(friend);
                container.appendChild(div);
            });
        });
}

window.addEventListener("DOMContentLoaded", () => {
    GetFriends();
});
