const API_BASE = "http://localhost:5207/api";

let sessionId = null;

const chatLog = document.getElementById("chat-log");
const chatForm = document.getElementById("chat-form");
const messageInput = document.getElementById("message-input");
const quizBtn = document.getElementById("quiz-btn");
const lectureSummaryBtn = document.getElementById("lecture-summary-btn");

function addMessage(role, text, citations = []) {
  const container = document.createElement("div");
  container.className = `message ${role}`;
  container.textContent = text;

  if (citations.length) {
    const citationEl = document.createElement("small");
    citationEl.className = "citation";
    citationEl.textContent = citations
      .map((c) => `${c.lectureTitle} (Lecture ${c.lectureId}, Chunk ${c.chunkId})`)
      .join(" | ");
    container.appendChild(citationEl);
  }

  chatLog.appendChild(container);
  chatLog.scrollTop = chatLog.scrollHeight;
}

async function sendMessage(message) {
  addMessage("user", message);

  const response = await fetch(`${API_BASE}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ sessionId, message }),
  });

  if (!response.ok) {
    const text = await response.text();
    addMessage("assistant", `Error: ${text}`);
    return;
  }

  const payload = await response.json();
  sessionId = payload.sessionId;
  addMessage("assistant", payload.reply, payload.citations || []);
}

chatForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const message = messageInput.value.trim();
  if (!message) {
    return;
  }
  messageInput.value = "";
  await sendMessage(message);
});

quizBtn.addEventListener("click", async () => {
  await sendMessage("Create a 5-question quiz about CAPM using my lecture notes.");
});

lectureSummaryBtn.addEventListener("click", async () => {
  await sendMessage("Summarize lectureId 1 in bullet points.");
});
