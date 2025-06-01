const newScenarioName = document.getElementById("add-scenario-name");
const newScenarioWarning = document.getElementById("add-scenario-warning");
const scenarioHolder = document.getElementById("scenario-holder");
const scenarioPopup = document.getElementById("add-scenario-popup");
const currentScenarioName = document.getElementById("current-scenario-name");
const currentScenarioButton = document.getElementById("current-scenario-btn");
const bindHolder = document.getElementById("binds");

function getAllScenarios() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `all_scenario@any`,
  })
    .then((res) => res.text())
    .then((response) => {
      response.split(";").forEach((name) => {
        if (!name) return;
        const sc = document.createElement("div");
        sc.classList.add("scenario");
        sc.innerText = name;
        sc.addEventListener("click", () => selectScenario(sc));
        scenarioHolder.appendChild(sc);
      });
      if (scenarioHolder.children.length > 0) {
        selectScenario(scenarioHolder.children[0]);
      }
    });
}

window.addEventListener("DOMContentLoaded", () => {
  document
    .getElementById("add-scenario")
    .addEventListener("click", AddScenario);
  document
    .getElementById("add-scenario-done")
    .addEventListener("click", AddScenarioDone);
  document
    .getElementById("current-scenario-btn")
    .addEventListener("click", StartCurrentScenario);
  document
    .getElementById("add-bind-btn")
    .addEventListener("click", () =>
      AddBind(document.getElementById("add-bind-listen"))
    );
  getAllScenarios();
});

function AddScenario() {
  scenarioPopup.style.visibility = "visible";
}

function StartCurrentScenario() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `scenario@${currentScenarioName.innerText}`,
  });

  const pauseSrc = new URL("assets/pause.svg", window.location.href).href;
  const startSrc = new URL("assets/start.svg", window.location.href).href;
  currentScenarioButton.src =
    currentScenarioButton.src === pauseSrc ? startSrc : pauseSrc;
}

function AddScenarioDone() {
  const name = newScenarioName.value;
  if (name.length < 2 || name.length > 7) {
    newScenarioWarning.style.visibility = "visible";
    return;
  }

  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `add_scenario@${name}`,
  })
    .then((res) => res.text())
    .then((response) => {
      const finalName = response.split("@")[1];
      const sc = document.createElement("div");
      sc.classList.add("scenario");
      sc.innerText = finalName;
      sc.addEventListener("click", () => selectScenario(sc));
      scenarioHolder.appendChild(sc);
      scenarioPopup.style.visibility = "collapse";
      newScenarioWarning.style.visibility = "hidden";
      newScenarioName.value = "";
    });
}

function selectScenario(scenario) {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `stop_scenario@${currentScenarioName.innerText}`,
  });

  [...scenarioHolder.children].forEach((child) =>
    child.classList.remove("selected")
  );
  scenario.classList.add("selected");
  currentScenarioName.innerText = scenario.innerText;
  currentScenarioButton.src = new URL(
    "assets/start.svg",
    window.location.href
  ).href;

  [...bindHolder.children].forEach((child) => {
    if (child.id !== "add-bind") child.remove();
  });

  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `scenario_binds@${currentScenarioName.innerText}`,
  })
    .then((res) => res.text())
    .then((response) => {
      response.split("/").forEach((line) => {
        if (!line) return;
        const type = line.split("&")[1];
        if (type === "1") AddStringMap(line);
        else if (type === "0") AddKeyMap(line);
      });
    });
}

function AddKeyMap(line) {
  const map = document.createElement("div");
  map.classList.add("bind");
  bindHolder.appendChild(map);

  const [id, type, fromKeyCodeStr] = line.split("&");
  let fromKeyCode = parseInt(fromKeyCodeStr);
  let toKeys = (line.split(">")[1] || "")
    .split(";")
    .filter((k) => k)
    .map(Number);

  const fromKeyBtn = document.createElement("button");
  fromKeyBtn.classList.add("listen-key");
  fromKeyBtn.innerText = String.fromCharCode(fromKeyCode);
  map.appendChild(fromKeyBtn);

  let waiting = false;
  fromKeyBtn.addEventListener("click", () => {
    fromKeyBtn.innerText = "Press a key...";
    waiting = true;
  });

  const keyListener = (e) => {
    if (!waiting) return;
    waiting = false;
    fromKeyCode = e.key.toUpperCase().charCodeAt(0);
    fromKeyBtn.innerText = e.key.toUpperCase();
    sendUpdate();
  };
  window.addEventListener("keydown", keyListener);

  const toKeyContainer = document.createElement("div");
  toKeyContainer.classList.add("to-keys");
  map.appendChild(toKeyContainer);

  function renderToKeys() {
    toKeyContainer.innerHTML = "";
    toKeys.forEach((code, i) => {
      const keyBtn = document.createElement("button");
      keyBtn.innerText = String.fromCharCode(code);
      keyBtn.addEventListener("click", () => {
        toKeys.splice(i, 1);
        sendUpdate();
        renderToKeys();
      });
      toKeyContainer.appendChild(keyBtn);
    });

    const addBtn = document.createElement("button");
    addBtn.innerText = "+";
    addBtn.addEventListener("click", () => {
      addBtn.innerText = "Press key...";
      const handler = (e) => {
        const newCode = e.key.toUpperCase().charCodeAt(0);
        toKeys.push(newCode);
        sendUpdate();
        renderToKeys();
        addBtn.innerText = "+";
        window.removeEventListener("keydown", handler);
      };
      window.addEventListener("keydown", handler);
    });
    toKeyContainer.appendChild(addBtn);
  }

  function sendUpdate() {
    fetch("http://localhost:3102/", {
      method: "POST",
      headers: { "Content-Type": "text/plain" },
      body: `change_bind@${
        currentScenarioName.innerText
      }@${id}&${type}&${fromKeyCode}>${toKeys.join(";")}`,
    });
  }

  renderToKeys();
}

function AddStringMap(line) {
  const map = document.createElement("div");
  map.classList.add("bind");
  bindHolder.appendChild(map);

  const [id, type, fromKeyCodeStr] = line.split("&");
  let fromKeyCode = parseInt(fromKeyCodeStr);
  let toString = line.split(">")[1] || "";

  const fromKeyBtn = document.createElement("button");
  fromKeyBtn.classList.add("listen-key");
  fromKeyBtn.innerText = String.fromCharCode(fromKeyCode);
  map.appendChild(fromKeyBtn);

  const input = document.createElement("input");
  input.type = "text";
  input.placeholder = toString;
  input.classList.add("edit-to-value");
  input.value = toString;
  map.appendChild(input);

  input.addEventListener("change", () => {
    sendUpdate(input.value, fromKeyCode);
  });

  let waiting = false;
  fromKeyBtn.addEventListener("click", () => {
    fromKeyBtn.innerText = "Press a key...";
    waiting = true;
  });

  const keyListener = (e) => {
    if (!waiting) return;
    waiting = false;
    fromKeyCode = e.key.toUpperCase().charCodeAt(0);
    fromKeyBtn.innerText = e.key.toUpperCase();
    sendUpdate(input.value, fromKeyCode);
  };
  window.addEventListener("keydown", keyListener);

  function sendUpdate(val, key) {
    fetch("http://localhost:3102/", {
      method: "POST",
      headers: { "Content-Type": "text/plain" },
      body: `change_bind@${currentScenarioName.innerText}@${id}&${type}&${key}>${val}`,
    });
  }
}

function AddBind(button) {
  const type = document.getElementById("bind-type-selector").value;
  const originalText = button.innerText;
  button.innerText = "Press a key...";

  function onKeyPress(e) {
    window.removeEventListener("keydown", onKeyPress);
    const fromKeyCode = e.keyCode;
    const id = bindHolder.children.length - 1;
    const toKeys = "";
    const toString = "placeholder";
    button.innerText = originalText;

    const payload =
      type === "0"
        ? `change_bind@${currentScenarioName.innerText}@${id}&${type}&${fromKeyCode}>${toKeys}`
        : `change_bind@${currentScenarioName.innerText}@${id}&${type}&${fromKeyCode}>${toString}`;

    fetch("http://localhost:3102/", {
      method: "POST",
      headers: { "Content-Type": "text/plain" },
      body: payload,
    }).then(() => {
      if (type === "0") AddKeyMap(`${id}&${type}&${fromKeyCode}>${toKeys}`);
      else AddStringMap(`${id}&${type}&${fromKeyCode}>${toString}`);
    });
  }

  window.addEventListener("keydown", onKeyPress);
}
