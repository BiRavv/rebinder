function BackendAction(address, type) {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `${address}@${type}`,
  });
}

window.addEventListener("DOMContentLoaded", () => {
  document.getElementById("test-btn").addEventListener("click", () => {
    BackendAction("scenario", "one");
  });
});
