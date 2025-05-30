let test = document.getElementById("tester");

fetch("http://localhost:3102/", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ msg: "Hello from Electron!" }),
})
  .then((res) => res.text())
  .then((data) => (test.innerText = data));

function clicked() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: "bind@ab",
  });
}

function clickedf() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: "bind@vf",
  });
}
